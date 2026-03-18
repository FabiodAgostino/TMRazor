namespace TMRazorImproved.Shared.Interfaces
{
    public interface ISoundService
    {
        void PlaySound(ushort soundId, int x = 0, int y = 0, int z = 0);
        void PlayMusic(ushort musicId);
        void StopMusic();

        /// <summary>Imposta il volume del processo client UO (0.0 = silenzio, 1.0 = massimo).</summary>
        void SetVolume(float volume);

        /// <summary>Restituisce il volume corrente del processo client UO (0.0-1.0). Ritorna 1.0 se non disponibile.</summary>
        float GetVolume();
    }
}
