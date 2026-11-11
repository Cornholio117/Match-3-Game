namespace Game.Core.Services
{
    /// <summary>Stand-in used in the Editor and in tests until the real audio service exists. Plays nothing.</summary>
    public sealed class NullAudioService : IAudioService
    {
        public void PlaySfx(string id)
        {
            // Intentionally empty: no sound is played.
        }

        public void PlayMusic(string id)
        {
            // Intentionally empty: no music is played.
        }

        public void StopMusic()
        {
            // Intentionally empty: nothing to stop.
        }
    }
}
