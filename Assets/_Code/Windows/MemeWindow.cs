using FattestInc.Windows.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.Windows {
    public class MemeWindow : SimpleWindow {
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] Image image;
        [SerializeField] TMP_Text descriptionLabel;
    }
}