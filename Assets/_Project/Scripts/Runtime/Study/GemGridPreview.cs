using UnityEngine;

namespace Game.Runtime.Study
{
    // Device test (week 3, Thursday): shows an 8x8 grid of gem sprites so the atlas, the camera fit and the
    // safe area can be checked on a real phone. Throwaway: the real board view comes in week 4.
    public sealed class GemGridPreview : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The gem sprites to cycle through, one per colour.")]
        [SerializeField] private Sprite[] _gems;

        [Header("Settings")]
        [SerializeField, Min(1)] private int _width = 8;
        [SerializeField, Min(1)] private int _height = 8;

        private void Awake()
        {
            if (_gems == null || _gems.Length == 0)
            {
                GameLog.Error(nameof(GemGridPreview), $"No gem sprites assigned on {name}");
                enabled = false;
                return;
            }

            // Built once at start (not every frame), so there is no repeated spawning to pool.
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var cell = new GameObject($"Gem_{x}_{y}");
                    cell.transform.SetParent(transform, false);
                    cell.transform.localPosition = new Vector3(x, y, 0f);

                    // Neighbours differ by 1 across and by 2 up, so no row or column of three can match.
                    var renderer = cell.AddComponent<SpriteRenderer>();
                    renderer.sprite = _gems[(x + 2 * y) % _gems.Length];
                }
            }
        }
    }
}
