using Game.Core.Board;
using Game.Core.Level;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Runtime.Config
{
    /// <summary>Designer-editable settings for one level. Read-only at runtime; live state belongs in models.</summary>
    [CreateAssetMenu(menuName = "Match3/Level Config", fileName = "SO_Level_01")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Header("Board")]
        [Tooltip("Columns on the board.")]
        [SerializeField, Min(3)] private int _width = 8;

        [Tooltip("Rows on the board.")]
        [SerializeField, Min(3)] private int _height = 8;

        [Tooltip("How many gem colours are in play (3 to 7). Fewer colours make matches easier.")]
        [SerializeField, Range(BoardModel.MinColorCount, BoardModel.MaxColorCount)] private int _colorCount = 5;

        [Header("Rules")]
        [Tooltip("Moves the player gets in this level.")]
        [SerializeField, Min(1)] private int _moves = 20;

        [Tooltip("How the level is won: reach a score, or collect gems of one colour.")]
        [SerializeField] private LevelGoalType _goalType = LevelGoalType.Score;

        [Tooltip("The score to reach, or how many gems to collect, depending on the goal type.")]
        [FormerlySerializedAs("_targetScore")]
        [SerializeField, Min(1)] private int _goalAmount = 1000;

        [Tooltip("The colour to collect. Only used when the goal type is Collect Color; it must be one of the colours in play.")]
        [SerializeField] private PieceColor _goalColor = PieceColor.Green;

        public int Width => _width;
        public int Height => _height;
        public int ColorCount => _colorCount;
        public int Moves => _moves;
        public LevelGoalType GoalType => _goalType;
        public int GoalAmount => _goalAmount;
        public PieceColor GoalColor => _goalColor;
    }
}
