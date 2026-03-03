using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services
{
    public class SoundService : ISoundService
    {
        private readonly IPacketService _packetService;

        public SoundService(IPacketService packetService)
        {
            _packetService = packetService;
        }

        public void PlaySound(ushort soundId)
        {
            // 0x54: cmd(1) mode(1) itemID(2) unk(1) x(2) y(2) z(2)
            byte[] data = new byte[12];
            data[0] = 0x54;
            data[1] = 0x01; // mode (repeat?)
            data[2] = (byte)(soundId >> 8);
            data[3] = (byte)soundId;
            // unk(1) x(2) y(2) z(2) - leave as 0 for global/ambient
            
            _packetService.SendToClient(data);
        }

        public void PlayMusic(ushort musicId)
        {
            // 0x6D: cmd(1) musicId(2)
            byte[] data = new byte[3];
            data[0] = 0x6D;
            data[1] = (byte)(musicId >> 8);
            data[2] = (byte)musicId;
            
            _packetService.SendToClient(data);
        }

        public void StopMusic()
        {
            PlayMusic(0xFFFF);
        }
    }
}
