using Game.Core.Services;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>Composition root: the one place that creates the services and hands them to the rest of the game.</summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        public ISaveService Save { get; private set; }
        public IAudioService Audio { get; private set; }
        public IAnalyticsService Analytics { get; private set; }

        // Awake is the right place: services must exist before any other script reads them in Start.
        private void Awake()
        {
            Save = new NullSaveService();
            Audio = new NullAudioService();
            Analytics = new NullAnalyticsService();

            if (_debugMode)
                GameLog.Info(nameof(Bootstrap), "Services created (null implementations).");
        }
    }
}
