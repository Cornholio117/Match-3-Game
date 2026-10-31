using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Runtime.Study
{
    // Study lab (week 1, Monday): every snippet below allocates garbage on purpose.
    // Find each one in the Profiler's GC Alloc column, then rewrite it alloc-free.
    public sealed class AllocationLab : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _label;

        [Header("Snippets (tick one at a time and watch GC Alloc in the Profiler)")]
        [SerializeField] private bool _runStringConcat;
        [SerializeField] private bool _runNewList;
        [SerializeField] private bool _runLoopCount;

        private readonly int[] _scores = { 5, 12, 7, 30, 18, 3, 25, 9 };
        private readonly List<int> _high = new List<int>();
        private int _frameCount;

        // The result is exposed so the compiler does not warn about an unused value.
        public int LastCount { get; private set; }

        private void Awake()
        {
            if (_label == null)
            {
                GameLog.Error(nameof(AllocationLab), $"Missing TMP_Text label on {name}");
                enabled = false;
            }
        }

        // Update is fine here: this lab is meant to produce work every frame.
        private void Update()
        {
            _frameCount++;

            if (_runStringConcat)
                BuildLabel();

            if (_runNewList)
                CountHighScoresWithNewList();

            if (_runLoopCount)
                CountHighScoresWithLoop();
        }

        // Bad 1: string concatenation creates a new string every call. Rewrite this one yourself.
        private void BuildLabel()
        {
            _label.SetText("Frame: {0}", _frameCount);
        }

        // Fixed 2: the list is created once and cleared each call, so nothing new reaches the GC.
        private void CountHighScoresWithNewList()
        {
            _high.Clear();
            for (var i = 0; i < _scores.Length; i++)
            {
                if (_scores[i] > 10)
                    _high.Add(_scores[i]);
            }

            LastCount = _high.Count;
        }

        // Fixed 3: a plain loop with a local counter allocates nothing (LINQ cost 284 B per call).
        private void CountHighScoresWithLoop()
        {
            var count = 0;
            for (var i = 0; i < _scores.Length; i++)
            {
                if (_scores[i] > 10)
                    count++;
            }

            LastCount = count;
        }
    }
}
