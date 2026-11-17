using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Board
{
    /// <summary>Reuses gem objects instead of creating and destroying them while the game runs.</summary>
    public sealed class PiecePool : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PieceView _prefab;

        [Header("Settings")]
        [Tooltip("Gems created in advance: an 8x8 board (64) plus a full refill wave, with room to spare.")]
        [SerializeField, Min(1)] private int _prewarmCount = 128;

        private ObjectPool<PieceView> _pool;

        private void Awake()
        {
            if (_prefab == null)
            {
                GameLog.Error(nameof(PiecePool), $"Missing prefab on {name}");
                enabled = false;
                return;
            }

            _pool = new ObjectPool<PieceView>(
                createFunc: () => Instantiate(_prefab, transform),
                actionOnGet: piece => { piece.gameObject.SetActive(true); piece.OnSpawned(); },
                actionOnRelease: piece => { piece.OnDespawned(); piece.gameObject.SetActive(false); },
                // Unity resets pools when Play mode ends, after the scene has already destroyed the gems.
                actionOnDestroy: piece => { if (piece != null) Destroy(piece.gameObject); },
                collectionCheck: Application.isEditor,
                defaultCapacity: _prewarmCount,
                maxSize: _prewarmCount * 2);

            Prewarm();
        }

        public PieceView Get()
        {
            return _pool.Get();
        }

        public void Release(PieceView piece)
        {
            _pool.Release(piece);
        }

        // Done once at load, so the first cascade does not have to create gems mid-animation.
        private void Prewarm()
        {
            var temp = new PieceView[_prewarmCount];
            for (var i = 0; i < temp.Length; i++)
                temp[i] = _pool.Get();

            for (var i = 0; i < temp.Length; i++)
                _pool.Release(temp[i]);
        }
    }
}
