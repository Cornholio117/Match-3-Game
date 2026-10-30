# EXAMPLES.md — Reference templates

Copy the shape of these templates. They follow RULES.md exactly; when in doubt, match them.

---

## E1. MonoBehaviour template (view / component)

```csharp
using UnityEngine;

namespace Game.Runtime.Example
{
    public sealed class ExampleView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpriteRenderer _renderer;

        [Header("Settings")]
        [Tooltip("Scale applied while selected, relative to normal size.")]
        [SerializeField, Min(1f)] private float _selectedScale = 1.15f;

        [Header("Debug")]
        [SerializeField] private bool _debugMode;

        private Vector3 _baseScale;

        private void Awake()
        {
            if (_renderer == null)
            {
                Debug.LogError($"[{nameof(ExampleView)}] Missing SpriteRenderer on {name}", this);
                enabled = false;
                return;
            }

            _baseScale = transform.localScale;
        }

        public void SetSelected(bool selected)
        {
            transform.localScale = selected ? _baseScale * _selectedScale : _baseScale;

            if (_debugMode)
                GameLog.Info(nameof(ExampleView), $"{name} selected={selected}");
        }

        private void OnDrawGizmos()
        {
            if (!_debugMode) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}
```

## E2. Plain C# model with events (Game.Core)

```csharp
using System;

namespace Game.Core.Level
{
    /// <summary>Tracks the remaining moves for the current level.</summary>
    public sealed class MovesModel
    {
        public event Action<int> MovesChanged;
        public event Action OutOfMoves;

        public int Remaining { get; private set; }

        public MovesModel(int startingMoves)
        {
            if (startingMoves <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingMoves));

            Remaining = startingMoves;
        }

        public void Consume()
        {
            if (Remaining == 0) return;

            Remaining--;
            MovesChanged?.Invoke(Remaining);

            if (Remaining == 0)
                OutOfMoves?.Invoke();
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            Remaining += amount;
            MovesChanged?.Invoke(Remaining);
        }
    }
}
```

## E3. EditMode test for E2

```csharp
using Game.Core.Level;
using NUnit.Framework;

namespace Game.Tests.EditMode.Level
{
    public sealed class MovesModelTests
    {
        [Test]
        public void Consume_LastMove_RaisesOutOfMoves()
        {
            var model = new MovesModel(1);
            var raised = false;
            model.OutOfMoves += () => raised = true;

            model.Consume();

            Assert.That(model.Remaining, Is.EqualTo(0));
            Assert.That(raised, Is.True);
        }

        [Test]
        public void Consume_AtZero_DoesNotGoNegative()
        {
            var model = new MovesModel(1);
            model.Consume();

            model.Consume();

            Assert.That(model.Remaining, Is.EqualTo(0));
        }
    }
}
```

## E4. View subscribing to a model (event lifecycle)

```csharp
using Game.Core.Level;
using TMPro;
using UnityEngine;

namespace Game.Runtime.UI
{
    public sealed class MovesCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;

        private MovesModel _model;

        // Called by the presenter / bootstrap. No Find, no singleton.
        public void Bind(MovesModel model)
        {
            Unsubscribe();
            _model = model;
            Subscribe();
            HandleMovesChanged(_model.Remaining);
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (_model == null || !isActiveAndEnabled) return;
            _model.MovesChanged += HandleMovesChanged;
        }

        private void Unsubscribe()
        {
            if (_model == null) return;
            _model.MovesChanged -= HandleMovesChanged;
        }

        private void HandleMovesChanged(int remaining)
        {
            // SetText with an argument avoids allocating a new string each update.
            _label.SetText("{0}", remaining);
        }
    }
}
```

## E5. GameLog — logging that is removed from release builds

Add `ENABLE_LOGS` to Project Settings → Player → Scripting Define Symbols for development builds only.

```csharp
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Game.Runtime
{
    public static class GameLog
    {
        // [Conditional] removes the call *and its string argument* when ENABLE_LOGS is not defined.
        [Conditional("ENABLE_LOGS"), Conditional("UNITY_EDITOR")]
        public static void Info(string system, string message) => Debug.Log($"[{system}] {message}");

        [Conditional("ENABLE_LOGS"), Conditional("UNITY_EDITOR")]
        public static void Warn(string system, string message) => Debug.LogWarning($"[{system}] {message}");

        // Errors are always logged — they feed crash reporting.
        public static void Error(string system, string message) => Debug.LogError($"[{system}] {message}");
    }
}
```

Note: `GameLog` lives in `Game.Runtime`. Code in `Game.Core` that must log receives an `ILogger`-style
interface instead, so Core stays free of `UnityEngine`.

## E6. Pooled object

`PieceView` (not shown) is a MonoBehaviour that implements `IPoolable` and resets its visuals in `OnDespawned`.

```csharp
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Board
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    public sealed class PiecePool : MonoBehaviour
    {
        [SerializeField] private PieceView _prefab;
        [Tooltip("8x8 board + one full refill wave.")]
        [SerializeField, Min(1)] private int _prewarmCount = 128;

        private ObjectPool<PieceView> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<PieceView>(
                createFunc: () => Instantiate(_prefab, transform),
                actionOnGet: p => { p.gameObject.SetActive(true); p.OnSpawned(); },
                actionOnRelease: p => { p.OnDespawned(); p.gameObject.SetActive(false); },
                actionOnDestroy: p => Destroy(p.gameObject),
                collectionCheck: Application.isEditor,
                defaultCapacity: _prewarmCount,
                maxSize: _prewarmCount * 2);

            Prewarm();
        }

        public PieceView Get() => _pool.Get();
        public void Release(PieceView piece) => _pool.Release(piece);

        private void Prewarm()
        {
            var temp = new PieceView[_prewarmCount]; // one-time allocation at load, not per frame
            for (var i = 0; i < temp.Length; i++) temp[i] = _pool.Get();
            for (var i = 0; i < temp.Length; i++) _pool.Release(temp[i]);
        }
    }
}
```

## E7. State machine

```csharp
namespace Game.Core.Flow
{
    public interface IState
    {
        void Enter();
        void Exit();
    }

    public sealed class StateMachine
    {
        public IState Current { get; private set; }

        public void ChangeState(IState next)
        {
            if (next == Current) return;

            Current?.Exit();
            Current = next;
            Current?.Enter();
        }
    }
}
```

## E8. Async loading with cancellation (Unity 6 Awaitable)

```csharp
using System.Threading;
using UnityEngine;

namespace Game.Runtime.Loading
{
    public sealed class LevelLoader : MonoBehaviour
    {
        public async Awaitable LoadLevelAsync(int levelIndex, CancellationToken ct)
        {
            // destroyCancellationToken stops the work if this object is destroyed mid-load.
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);

            await Awaitable.NextFrameAsync(linked.Token);
            // … load level data via IAssetLoader, build the board, then return.
        }
    }
}
```

## E9. Creating or fixing a scene hierarchy

When asked to create or tidy a scene:
1. List current root objects.
2. Create any missing separator from R4 (exact name, tag `EditorOnly`, zero transform, no components).
3. Reorder root objects so each sits directly after its section separator, in R4 order.
4. Report every object you moved: `Moved "X" from <old position> to section <Section>`.
5. Do not rename or delete objects without asking.

Example result:
```
==========General==========
Bootstrap
==========Camera==========
Main Camera
==========Board==========
BoardView
PiecePool
==========UI==========
HUD Canvas
EventSystem
```
