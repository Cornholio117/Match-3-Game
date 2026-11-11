namespace Game.Core.Services
{
    /// <summary>Plays sounds and music by name, so game code never touches audio sources.</summary>
    public interface IAudioService
    {
        void PlaySfx(string id);

        void PlayMusic(string id);

        void StopMusic();
    }
}
