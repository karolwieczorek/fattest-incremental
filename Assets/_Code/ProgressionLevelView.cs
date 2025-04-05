using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc {
    public class ProgressionLevelView : MonoBehaviour {
        [SerializeField] Image icon;
        [SerializeField] TMP_Text label;

        public void Init(ProgressionLevelsData.LevelData levelData) {
            icon.sprite = levelData.sprite;
            label.text = levelData.name;
        }
    }
}