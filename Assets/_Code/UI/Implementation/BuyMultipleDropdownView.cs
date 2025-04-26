using System;
using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using Hypnagogia.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.UI.Implementation {
    public class BuyMultipleDropdownView : MonoBehaviour {
        [SerializeField] TMP_Dropdown dropdown;

        [HInject] BuyMultipleReferencer buyMultipleReferencer;
        [HInject] BuyMultipleDataStore buyMultipleDataStore;

        readonly List<BuyMultipleOptionData> options = new();

        void OnEnable() {
            options.Clear();
            foreach (var data in buyMultipleReferencer.BuyMultipleDataList) {
                options.Add(new BuyMultipleOptionData() {
                    data = data,
                    text = data.label
                });
            }
            dropdown.options = options.Cast<TMP_Dropdown.OptionData>().ToList();
            dropdown.onValueChanged.AddListener(OnValueChanged);
            dropdown.value = 0;
        }

        void OnDisable() {
            dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        void OnValueChanged(int selectedIndex) {
            buyMultipleDataStore.SetBuyMultipleData(options[selectedIndex].data);
        }

        class BuyMultipleOptionData : TMP_Dropdown.OptionData {
            public BuyMultipleData data;
        }
    }
}