using UnityEngine;

namespace Game.Runtime.Config
{
    /// <summary>Designer-editable settings for one level. Read-only at runtime; live state belongs in models.</summary>
    [CreateAssetMenu(menuName = "Match3/Level Config", fileName = "SO_Level_01")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Header("Rules")]
        [Tooltip("Moves the player gets in this level.")]
        [SerializeField, Min(1)] private int _moves = 20;

        [Tooltip("Score needed to win this level.")]
        [SerializeField, Min(1)] private int _targetScore = 1000;

        public int Moves => _moves;
        public int TargetScore => _targetScore;
    }
}
