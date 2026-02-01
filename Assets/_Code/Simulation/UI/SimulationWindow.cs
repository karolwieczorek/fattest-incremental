using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using FattestInc.Simulation.API;
using FattestInc.Simulation.Implementation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FattestInc.Simulation.UI {
    public class SimulationWindow : MonoBehaviour {
        [Header("Inputs")]
        [SerializeField] TMP_InputField durationSecondsInput;
        [SerializeField] TMP_InputField manualClicksPerSecondInput;
        [SerializeField] TMP_InputField startingEnergyInput;
        [SerializeField] Toggle buyOnePerSecondToggle;
        [SerializeField] Button runButton;

        [Header("Scrubber")]
        [SerializeField] Slider timeSlider;
        [SerializeField] TMP_Text timeLabel;

        [Header("Outputs")]
        [SerializeField] TMP_Text energyLabel;
        [SerializeField] TMP_Text epsLabel;
        [SerializeField] TMP_Text factoriesLabel;

        [Header("Data")] 
        [SerializeField] SimulationRunner simulationRunner;
        [SerializeField] FactoriesReferencer factoriesReferencer;
        
        [Header("Optional")]
        [SerializeField] UnlockingFactoriesData unlockingFactoriesData;


        void OnEnable() {
            if (runButton != null)
                runButton.onClick.AddListener(RunSimulation);
            if (timeSlider != null)
                timeSlider.onValueChanged.AddListener(OnTimeChanged);
            simulationRunner.OnSimulationGenerated += SimulationGenerated;
        }

        void OnDisable() {
            if (runButton != null)
                runButton.onClick.RemoveListener(RunSimulation);
            if (timeSlider != null)
                timeSlider.onValueChanged.RemoveListener(OnTimeChanged);
            simulationRunner.OnSimulationGenerated -= SimulationGenerated;
        }

        void RunSimulation() {
            int duration = ParseInt(durationSecondsInput, 60);
            double clicks = ParseDouble(manualClicksPerSecondInput, 10);
            double startingEnergy = ParseDouble(startingEnergyInput, 10);
            bool buyOne = buyOnePerSecondToggle == null || buyOnePerSecondToggle.isOn;

            if (factoriesReferencer == null || factoriesReferencer.Factories == null || factoriesReferencer.Factories.Count == 0) {
                Debug.LogError("SimulationWindow: Missing FactoriesReferencer or no factories set.", this);
                return;
            }
            
            simulationRunner.RunSimulation(duration, clicks, startingEnergy, buyOne);

            if (timeSlider != null) {
                timeSlider.minValue = 0;
                timeSlider.maxValue = Mathf.Max(0, duration - 1);
                timeSlider.wholeNumbers = true;
                timeSlider.value = 0;
            }
        }
        
        

        void SimulationGenerated(SimulationResult result) {
            ShowSnapshot(0);
        }

        void OnTimeChanged(float value) {
            ShowSnapshot((int)value);
        }

        void ShowSnapshot(int index) {
            var lastResult = simulationRunner.Result;
            if (lastResult.Snapshots == null || lastResult.Snapshots.Count == 0)
                return;
            index = Mathf.Clamp(index, 0, lastResult.Snapshots.Count - 1);
            var s = lastResult.Snapshots[index];

            if (timeLabel != null) timeLabel.text = $"t = {s.TimeSeconds}s";
            if (energyLabel != null) energyLabel.text = $"Energy: {s.Energy:0.##}";
            if (epsLabel != null) epsLabel.text = $"EPS: {s.EnergyPerSecond:0.##}";

            if (factoriesLabel != null) {
                factoriesLabel.text = string.Join("\n", s.Levels.Select(l => $"{l.FactoryName} ({l.FactoryId}): L{l.Level}"));
            }
        }

        static int ParseInt(TMP_InputField field, int fallback) {
            if (field == null || string.IsNullOrWhiteSpace(field.text)) return fallback;
            if (int.TryParse(field.text, out var v)) return v;
            return fallback;
        }

        static double ParseDouble(TMP_InputField field, double fallback) {
            if (field == null || string.IsNullOrWhiteSpace(field.text)) return fallback;
            if (double.TryParse(field.text, out var v)) return v;
            return fallback;
        }
    }
} 