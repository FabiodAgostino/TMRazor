namespace TMRazorImproved.Shared.Interfaces
{
    public interface ISoundService
    {
        void PlaySound(ushort soundId);
        void PlayMusic(ushort musicId);
        void StopMusic();
    }
}
