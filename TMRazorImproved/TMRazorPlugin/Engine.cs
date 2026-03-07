using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using CUO_API;

namespace Assistant
{
    /// <summary>
    /// cuoapi plugin loaded by TmClient.
    /// Creates named shared memory + mutex so that TMRazorImproved (in a separate process)
    /// can read all UO network packets without DLL injection.
    /// </summary>
    public static class Engine
    {
        // Static references: prevent GC from collecting delegates while TmClient holds native function pointers.
        private static OnPacketSendRecv _onRecv;
        private static OnPacketSendRecv _onSend;
        private static OnInitialize     _onInit;
        private static OnTick           _onTick;

        // Plugin pointer saved so OnInitialize can read host-provided Send function
        private static IntPtr _plugin;

        // Host-provided function: plugin calls this to inject a C→S packet
        private static OnPacketSendRecv _sendToServer;

        // Host-provided function: plugin calls this to inject a S→C packet (fake server packet)
        private static OnPacketSendRecv _recvToClient;

        // Tick counter for throttled logging
        private static int _tickCount;

        // Cached player serial from 0x1B LoginConfirm — used to re-inject a synthetic
        // LoginConfirm when the STC buffer is first drained (allows UI reconnect to work
        // even when the original 0x1B was already consumed by a previous session).
        private static uint _cachedPlayerSerial = 0;

        // Cached full skills packet (0x3A type=0x02) — re-injected on reconnect
        // so the UI gets skill data without needing a fresh login.
        private static byte[] _cachedSkillsPacket = null;

        private static MemoryMappedFile         _mmf;
        private static MemoryMappedViewAccessor _view;
        private static Mutex                    _commMutex;

        // Must match Crypt.h: SHARED_BUFF_SIZE and Buffer struct layout (#pragma pack(1))
        private const int  SHARED_BUFF_SIZE   = 524288;
        private const int  SHARED_BUFF_STRIDE = 8 + SHARED_BUFF_SIZE; // 524296 bytes per Buffer

        // sizeof(SharedMemory) with pack(1):
        //   4 * Buffer(524296) = 2097184
        //   + TitleBar[1024]=1024, ForceDisconn=1, AllowDisconn=1, TotalSend=4, TotalRecv=4,
        //     PacketTable[256*2]=512, DataPath[256]=256, DeathMsg[16]=16, Position[3*4]=12,
        //     CheatKey[16]=16, AllowNegotiate=1, AuthBits[8]=8, IsHaxed=1, ServerIP=4,
        //     ServerPort=2, UOVersion[16]=16  => 1878
        //   Total = 2099062
        private const long SHARED_MEM_SIZE = 2099062L + 4096L; // slight padding for safety

        // Buffer indices (match PacketService layout: _inRecv=0, _outRecv=1, _inSend=2, _outSend=3)
        private const int IDX_IN_RECV  = 0;  // server→client: plugin writes, Razor reads
        private const int IDX_IN_SEND  = 2;  // client→server: plugin writes, Razor reads
        private const int IDX_OUT_RECV = 1;  // server→client: Razor writes, plugin reads and feeds via Recv()
        private const int IDX_OUT_SEND = 3;  // client→server: Razor writes, plugin reads and sends via Send()

        // PluginHeader field offsets (x64, from cuoapi.dll PluginHeader struct reflection):
        //   int ClientVersion                  → 0
        //   IntPtr HWND                        → 8
        //   IntPtr OnRecv                      → 16   ← plugin registers callback
        //   IntPtr OnSend                      → 24   ← plugin registers callback
        //   IntPtr OnHotkeyPressed             → 32
        //   IntPtr OnMouse                     → 40
        //   IntPtr OnPlayerPositionChanged     → 48
        //   IntPtr OnClientClosing             → 56
        //   IntPtr OnInitialize                → 64   ← plugin registers callback
        //   IntPtr OnConnected                 → 72
        //   IntPtr OnDisconnected              → 80
        //   IntPtr OnFocusGained               → 88
        //   IntPtr OnFocusLost                 → 96
        //   IntPtr GetUOFilePath               → 104  ← host provides
        //   IntPtr Recv                        → 112  ← host provides: inject S→C packet
        //   IntPtr Send                        → 120  ← host provides: inject C→S packet
        //   IntPtr GetPacketLength             → 128
        //   IntPtr GetPlayerPosition           → 136
        //   IntPtr CastSpell                   → 144
        //   IntPtr GetStaticImage              → 152
        //   IntPtr Tick                        → 160  ← plugin registers callback
        //   IntPtr RequestMove                 → 168
        //   IntPtr SetTitle                    → 176
        private const int OFFSET_ON_RECV       = 16;
        private const int OFFSET_ON_SEND       = 24;
        private const int OFFSET_ON_INITIALIZE = 64;
        private const int OFFSET_RECV          = 112; // host Recv: inject S→C packet
        private const int OFFSET_SEND          = 120; // host Send: inject C→S packet
        private const int OFFSET_ON_TICK       = 160;

        /// <summary>Entry point called by TmClient via reflection.</summary>
        public static void Install(IntPtr plugin)
        {
            // Register AssemblyResolve BEFORE JIT-compiling Install2 (which references CUO_API types).
            // This ensures the CUO_API assembly loaded by TmClient is reused — identical types, no mismatch.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            Install2(plugin);
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            string name = new AssemblyName(e.Name).Name;
            // First: return already-loaded assembly from TmClient's AppDomain
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                if (string.Equals(asm.GetName().Name, name, StringComparison.OrdinalIgnoreCase))
                    return asm;
            // Fallback: load from plugin directory
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            string path = Path.Combine(dir, name + ".dll");
            return File.Exists(path) ? Assembly.LoadFile(path) : null;
        }

        private static void Install2(IntPtr plugin)
        {
            try
            {
                int    pid       = System.Diagnostics.Process.GetCurrentProcess().Id;
                string mapName   = string.Format("UONetSharedFM_{0:x}", pid);
                string mutexName = string.Format("UONetSharedCOMM_{0:x}", pid);

                // Create the named shared memory that TMRazorImproved's Crypt.dll will open.
                // All bytes are zero-initialized by the OS.
                _mmf  = MemoryMappedFile.CreateOrOpen(mapName, SHARED_MEM_SIZE, MemoryMappedFileAccess.ReadWrite);
                _view = _mmf.CreateViewAccessor(0, SHARED_MEM_SIZE, MemoryMappedFileAccess.ReadWrite);

                bool created;
                _commMutex = new Mutex(false, mutexName, out created);

                // Pin delegates so GC can't collect them
                _onInit = new OnInitialize(DoOnInitialize);
                _onRecv = new OnPacketSendRecv(DoOnRecv);
                _onSend = new OnPacketSendRecv(DoOnSend);
                _onTick = new OnTick(DoOnTick);

                IntPtr pInit = Marshal.GetFunctionPointerForDelegate(_onInit);
                IntPtr pRecv = Marshal.GetFunctionPointerForDelegate(_onRecv);
                IntPtr pSend = Marshal.GetFunctionPointerForDelegate(_onSend);
                IntPtr pTick = Marshal.GetFunctionPointerForDelegate(_onTick);

                _plugin = plugin;

                Marshal.WriteIntPtr(plugin, OFFSET_ON_INITIALIZE, pInit);
                Marshal.WriteIntPtr(plugin, OFFSET_ON_RECV,       pRecv);
                Marshal.WriteIntPtr(plugin, OFFSET_ON_SEND,       pSend);
                Marshal.WriteIntPtr(plugin, OFFSET_ON_TICK,       pTick);

                WriteLog(string.Format(
                    "Installed. PID={0} map={1} mutexCreated={2} pInit=0x{3:X} pRecv=0x{4:X} pSend=0x{5:X} pTick=0x{6:X}",
                    pid, mapName, created, pInit.ToInt64(), pRecv.ToInt64(), pSend.ToInt64(), pTick.ToInt64()));
            }
            catch (Exception ex)
            {
                WriteLog("CRITICAL ERROR in Install2: " + ex);
            }
        }

        private static void DoOnInitialize()
        {
            WriteLog("OnInitialize — TmClient ready");
            // Read host-provided Send function pointer (inject C→S packet)
            if (_plugin != IntPtr.Zero)
            {
                IntPtr recvPtr = Marshal.ReadIntPtr(_plugin, OFFSET_RECV);
                if (recvPtr != IntPtr.Zero)
                {
                    _recvToClient = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>(recvPtr);
                    WriteLog(string.Format("Recv function acquired at 0x{0:X}", recvPtr.ToInt64()));
                }
                else WriteLog("WARNING: Recv function pointer is null");

                IntPtr sendPtr = Marshal.ReadIntPtr(_plugin, OFFSET_SEND);
                if (sendPtr != IntPtr.Zero)
                {
                    _sendToServer = Marshal.GetDelegateForFunctionPointer<OnPacketSendRecv>(sendPtr);
                    WriteLog(string.Format("Send function acquired at 0x{0:X}", sendPtr.ToInt64()));
                }
                else WriteLog("WARNING: Send function pointer is null — SendToServer will not work");
            }
        }

        private static void DoOnTick()
        {
            _tickCount++;
            FlushOutRecv();
            FlushOutSend();
            if (_tickCount % 300 == 1) // log every ~5s at 60fps
                WriteLog(string.Format("[Tick] tick#{0}", _tickCount));
        }

        /// <summary>
        /// Reads packets written by TMRazorImproved into buffer IDX_OUT_RECV (buffer 1)
        /// and feeds them into ClassicUO as fake S→C packets via the host Recv() function.
        /// </summary>
        private static void FlushOutRecv()
        {
            if (_view == null || _commMutex == null || _recvToClient == null) return;

            long baseOff = (long)IDX_OUT_RECV * SHARED_BUFF_STRIDE;

            byte[] snapshot = null;
            int snapshotLen = 0;

            if (!_commMutex.WaitOne(5)) return;
            try
            {
                int buffLen = _view.ReadInt32(baseOff);
                if (buffLen >= LENGTH_PREFIX)
                {
                    int buffStart = _view.ReadInt32(baseOff + 4);
                    snapshot = new byte[buffLen];
                    _view.ReadArray(baseOff + 8 + buffStart, snapshot, 0, buffLen);
                    snapshotLen = buffLen;
                    _view.Write(baseOff,     0);
                    _view.Write(baseOff + 4, 0);
                }
            }
            finally { _commMutex.ReleaseMutex(); }

            if (snapshot == null || snapshotLen < LENGTH_PREFIX) return;

            int pos = 0;
            while (pos + LENGTH_PREFIX <= snapshotLen)
            {
                int packetLen = snapshot[pos] | (snapshot[pos + 1] << 8) | (snapshot[pos + 2] << 16) | (snapshot[pos + 3] << 24);
                pos += LENGTH_PREFIX;
                if (packetLen <= 0 || pos + packetLen > snapshotLen) break;

                byte[] pkt = new byte[packetLen];
                Array.Copy(snapshot, pos, pkt, 0, packetLen);
                pos += packetLen;

                int length = packetLen;
                _recvToClient(ref pkt, ref length);
            }
        }

        /// <summary>
        /// Reads packets written by TMRazorImproved into buffer IDX_OUT_SEND (buffer 3)
        /// and calls the host Send function to inject them into the C→S stream.
        /// Format: same length-prefix protocol as WritePacket ([4-byte LE length][data]).
        /// </summary>
        private static void FlushOutSend()
        {
            if (_view == null || _commMutex == null || _sendToServer == null) return;

            long baseOff = (long)IDX_OUT_SEND * SHARED_BUFF_STRIDE;

            byte[] snapshot = null;
            int snapshotLen = 0;

            if (!_commMutex.WaitOne(5)) return;
            try
            {
                int buffLen = _view.ReadInt32(baseOff);
                if (buffLen >= LENGTH_PREFIX)
                {
                    int buffStart = _view.ReadInt32(baseOff + 4);
                    snapshot = new byte[buffLen];
                    _view.ReadArray(baseOff + 8 + buffStart, snapshot, 0, buffLen);
                    snapshotLen = buffLen;
                    _view.Write(baseOff,     0); // Length = 0
                    _view.Write(baseOff + 4, 0); // Start = 0
                }
            }
            finally { _commMutex.ReleaseMutex(); }

            if (snapshot == null || snapshotLen < LENGTH_PREFIX) return;

            WriteLog(string.Format("FlushOutSend: found {0} bytes in buffer 3", snapshotLen));

            int pos = 0;
            while (pos + LENGTH_PREFIX <= snapshotLen)
            {
                int packetLen = snapshot[pos] | (snapshot[pos + 1] << 8) | (snapshot[pos + 2] << 16) | (snapshot[pos + 3] << 24);
                pos += LENGTH_PREFIX;
                if (packetLen <= 0 || pos + packetLen > snapshotLen) break;

                byte[] pkt = new byte[packetLen];
                Array.Copy(snapshot, pos, pkt, 0, packetLen);
                pos += packetLen;

                WriteLog(string.Format("FlushOutSend: calling Send id=0x{0:X2} len={1}", pkt[0], packetLen));
                int length = packetLen;
                bool result = _sendToServer(ref pkt, ref length);
                WriteLog(string.Format("FlushOutSend: Send returned {0}", result));
            }
        }

        private static bool DoOnRecv(ref byte[] data, ref int length)
        {
            // Cache player serial from LoginConfirm (0x1B)
            if (length >= 5 && data[0] == 0x1B)
            {
                uint serial = ((uint)data[1] << 24) | ((uint)data[2] << 16) | ((uint)data[3] << 8) | data[4];
                if (serial != 0)
                    _cachedPlayerSerial = serial;
            }
            // Cache full skills list (0x3A type 0x00 or 0x02) for re-injection on reconnect
            if (length >= 5 && data[0] == 0x3A && (data[3] == 0x00 || data[3] == 0x02))
            {
                _cachedSkillsPacket = new byte[length];
                Array.Copy(data, _cachedSkillsPacket, length);
            }
            WritePacket(IDX_IN_RECV, data, length);
            return true; // true = pass packet through to client
        }

        private static bool DoOnSend(ref byte[] data, ref int length)
        {
            WritePacket(IDX_IN_SEND, data, length);
            return true; // true = pass packet through to server
        }

        /// <summary>
        /// Appends <paramref name="length"/> bytes of <paramref name="data"/> to the
        /// shared Buffer at index <paramref name="bufIdx"/>.
        /// Buffer layout (matches Crypt.h Buffer struct with pack(1)):
        ///   [0..3]  = Length (int)  — bytes currently queued
        ///   [4..7]  = Start  (int)  — read cursor into Buff
        ///   [8..]   = Buff[524288]
        /// </summary>
        // Each entry in the buffer is: [4-byte LE length][packet bytes]
        // The consumer (PacketService) reads the prefix to know the packet size without
        // needing GetPacketLength / Crypt.dll's StaticPacketTable.
        private const int LENGTH_PREFIX = 4;

        private static void WritePacket(int bufIdx, byte[] data, int length)
        {
            if (_view == null || _commMutex == null || data == null || length <= 0) return;
            if (length > data.Length) length = data.Length;

            long baseOff  = (long)bufIdx * SHARED_BUFF_STRIDE;
            int  writeSize = LENGTH_PREFIX + length; // prefix + payload

            if (!_commMutex.WaitOne(50)) return;
            try
            {
                int buffLen   = _view.ReadInt32(baseOff);       // current queued bytes
                int buffStart = _view.ReadInt32(baseOff + 4);   // read cursor

                // When buffer is fully consumed, reset cursor to 0
                if (buffLen == 0)
                {
                    buffStart = 0;
                    _view.Write(baseOff + 4, 0);

                    // Re-inject cached 0x1B (LoginConfirm) as the first STC packet when the
                    // buffer is empty. This allows the UI to re-establish player identity on
                    // reconnect even when the original 0x1B was already consumed by a previous
                    // session. HandleLoginConfirm only needs cmd(1)+serial(4), rest is padding.
                    if (bufIdx == IDX_IN_RECV && _cachedPlayerSerial != 0)
                    {
                        byte[] synth = new byte[37]; // standard 0x1B size
                        synth[0] = 0x1B;
                        synth[1] = (byte)(_cachedPlayerSerial >> 24);
                        synth[2] = (byte)(_cachedPlayerSerial >> 16);
                        synth[3] = (byte)(_cachedPlayerSerial >> 8);
                        synth[4] = (byte)(_cachedPlayerSerial);
                        byte[] synthPrefix = BitConverter.GetBytes(synth.Length);
                        _view.WriteArray(baseOff + 8,              synthPrefix, 0, LENGTH_PREFIX);
                        _view.WriteArray(baseOff + 8 + LENGTH_PREFIX, synth,    0, synth.Length);
                        buffLen = LENGTH_PREFIX + synth.Length;

                        // Also re-inject cached skills packet (0x3A type=0x02) if available
                        var skills = _cachedSkillsPacket;
                        if (skills != null)
                        {
                            byte[] skillsPrefix = BitConverter.GetBytes(skills.Length);
                            _view.WriteArray(baseOff + 8 + buffLen,              skillsPrefix, 0, LENGTH_PREFIX);
                            _view.WriteArray(baseOff + 8 + buffLen + LENGTH_PREFIX, skills,    0, skills.Length);
                            buffLen += LENGTH_PREFIX + skills.Length;
                        }

                        _view.Write(baseOff, buffLen);
                    }
                }

                int writeOffset = buffStart + buffLen;

                // If the new entry doesn't fit at the current write position, try to compact
                if (writeOffset + writeSize > SHARED_BUFF_SIZE)
                {
                    if (buffLen > 0 && buffLen + writeSize <= SHARED_BUFF_SIZE)
                    {
                        // Move live data to the front of the buffer
                        byte[] temp = new byte[buffLen];
                        _view.ReadArray(baseOff + 8 + buffStart, temp, 0, buffLen);
                        _view.WriteArray(baseOff + 8, temp, 0, buffLen);
                        _view.Write(baseOff + 4, 0); // Start = 0
                        buffStart   = 0;
                        writeOffset = buffLen;
                    }
                    else
                    {
                        // Buffer full — drop this packet to avoid stalling the game
                        return;
                    }
                }

                // Write 4-byte little-endian length prefix then packet data
                byte[] prefix = BitConverter.GetBytes(length);
                _view.WriteArray(baseOff + 8 + writeOffset,              prefix, 0, LENGTH_PREFIX);
                _view.WriteArray(baseOff + 8 + writeOffset + LENGTH_PREFIX, data, 0, length);
                _view.Write(baseOff, buffLen + writeSize); // update Length
            }
            finally
            {
                _commMutex.ReleaseMutex();
            }
        }

        private static void WriteLog(string msg)
        {
            try
            {
                string line = string.Format("[{0:HH:mm:ss.fff}] [TMRazorPlugin] {1}{2}",
                    DateTime.Now, msg, Environment.NewLine);
                string dir  = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".";
                File.AppendAllText(Path.Combine(dir, "plugin.log"), line);
            }
            catch { }
        }
    }
}
