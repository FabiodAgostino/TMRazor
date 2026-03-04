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
        private const int IDX_IN_RECV = 0;  // server→client: plugin writes, Razor reads
        private const int IDX_IN_SEND = 2;  // client→server: plugin writes, Razor reads

        // PluginHeader field offsets (x64, from TestPlugin analysis):
        private const int OFFSET_ON_RECV       = 16;
        private const int OFFSET_ON_SEND       = 24;
        private const int OFFSET_ON_INITIALIZE = 64;

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

                Marshal.WriteIntPtr(plugin, OFFSET_ON_INITIALIZE, Marshal.GetFunctionPointerForDelegate(_onInit));
                Marshal.WriteIntPtr(plugin, OFFSET_ON_RECV,       Marshal.GetFunctionPointerForDelegate(_onRecv));
                Marshal.WriteIntPtr(plugin, OFFSET_ON_SEND,       Marshal.GetFunctionPointerForDelegate(_onSend));

                WriteLog(string.Format("Installed. PID={0} map={1} mutexCreated={2}", pid, mapName, created));
            }
            catch (Exception ex)
            {
                WriteLog("CRITICAL ERROR in Install2: " + ex);
            }
        }

        private static void DoOnInitialize()
        {
            WriteLog("OnInitialize — TmClient ready");
        }

        private static bool DoOnRecv(ref byte[] data, ref int length)
        {
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
        private static void WritePacket(int bufIdx, byte[] data, int length)
        {
            if (_view == null || _commMutex == null || data == null || length <= 0) return;
            if (length > data.Length) length = data.Length;

            long baseOff = (long)bufIdx * SHARED_BUFF_STRIDE;

            if (!_commMutex.WaitOne(50)) return;
            try
            {
                int buffLen   = _view.ReadInt32(baseOff);       // current queued bytes
                int buffStart = _view.ReadInt32(baseOff + 4);   // read cursor

                // When buffer is fully consumed, reset cursor to 0 (mirrors HandleComm behaviour)
                if (buffLen == 0)
                {
                    buffStart = 0;
                    _view.Write(baseOff + 4, 0);
                }

                int writeOffset = buffStart + buffLen;

                // If the new packet doesn't fit at the current write position, try to compact
                if (writeOffset + length > SHARED_BUFF_SIZE)
                {
                    if (buffLen > 0 && buffLen + length <= SHARED_BUFF_SIZE)
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
                        WriteLog(string.Format("WARNING: buffer[{0}] full ({1}/{2}), packet 0x{3:X2} dropped",
                            bufIdx, buffLen, SHARED_BUFF_SIZE, data[0]));
                        return;
                    }
                }

                _view.WriteArray(baseOff + 8 + writeOffset, data, 0, length);
                _view.Write(baseOff, buffLen + length); // update Length
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
