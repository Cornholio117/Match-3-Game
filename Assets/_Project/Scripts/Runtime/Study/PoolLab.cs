using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Study
{
    // Study lab (week 2, Wednesday): spawn and remove a batch of objects every frame, either with
    // Instantiate/Destroy or with Unity's ObjectPool. Compare both in the Profiler.
    public sealed class PoolLab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject _prefab;

        [Header("Settings")]
        [Tooltip("How many objects are spawned and removed every frame. A big cascade is roughly 20-40.")]
        [SerializeField, Min(1)] private int _countPerFrame = 50;

        [Header("Mode (change while playing and watch the Profiler)")]
        [SerializeField] private bool _run;
        [SerializeField] private bool _usePool;

        private readonly List<GameObject> _active = new List<GameObject>();
        private ObjectPool<GameObject> _pool;

        private void Awake()
        {
            if (_prefab == null)
            {
                GameLog.Error(nameof(PoolLab), $"Missing prefab on {name}");
                enabled = false;
                return;
            }

            _pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_prefab, transform),
                actionOnGet: item => item.SetActive(true),
                actionOnRelease: item => item.SetActive(false),
                actionOnDestroy: item => Destroy(item),
                defaultCapacity: _countPerFrame,
                maxSize: _countPerFrame * 2);
        }

        // Update is fine here: the lab exists to produce this work every frame.
        private void Update()
        {
            if (!_run) return;

            for (var i = 0; i < _active.Count; i++)
            {
                if (_usePool)
                    _pool.Release(_active[i]);
                else
                    Destroy(_active[i]);
            }

            _active.Clear();

            for (var i = 0; i < _countPerFrame; i++)
            {
                var item = _usePool ? _pool.Get() : Instantiate(_prefab, transform);
                _active.Add(item);
            }
        }
    }
}
