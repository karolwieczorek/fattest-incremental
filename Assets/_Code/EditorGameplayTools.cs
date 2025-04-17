using System;
using Hypnagogia.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FattestInc {
    public class EditorGameplayTools : MonoBehaviour {
        [HInject] ResourceFactoriesHelper resourceFactoriesHelper;

        [Button]
        void IncrementMinutes(int minutes) {
            resourceFactoriesHelper.TickAllFactories((float)TimeSpan.FromMinutes(minutes).TotalSeconds);
        }

        [Button]
        void IncrementSeconds(float seconds) {
            resourceFactoriesHelper.TickAllFactories(seconds);
        }
    }
}