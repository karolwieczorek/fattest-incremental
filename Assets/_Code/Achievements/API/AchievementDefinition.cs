using System;
using UnityEngine;

namespace FattestInc.Achievements.API {
    public enum AchievementCondition {
        // Income per second reaches the last progression tier (Globe).
        ReachLastProgressionTier,
        // Every factory is at its max level.
        MaxAllFactories,
    }

    [Serializable]
    public class AchievementDefinition {
        // Stored in saves; never rename an id once released.
        public string id;
        public string title;
        [TextArea] public string description;
        public Sprite icon;
        public AchievementCondition condition;
    }
}
