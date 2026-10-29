using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Progression.API;
using FattestInc.Simulation.API;
using UnityEngine;
using UnityEngine.Events;

namespace FattestInc.Simulation.Implementation {
    public class SimulationRunner : MonoBehaviour {
        [SerializeField] FactoriesReferencer factoriesReferencer;
        [SerializeField] UnlockingFactoriesData unlockingFactoriesData;
        [Tooltip("Optional. Adds time-to-tier to the pacing report.")]
        [SerializeField] ProgressionLevelsData progressionLevelsData;
        [SerializeField] PurchaseStrategy strategy = PurchaseStrategy.BestPayback;
        [SerializeField] bool logReport = true;

        SimulationResult lastResult;
        public event UnityAction<SimulationResult> OnSimulationGenerated;

        public SimulationResult Result => lastResult;
        public SimulationReport Report { get; private set; }

        public void RunSimulation(int duration, double clicks, double startingEnergy, bool buyOne) {
            if (factoriesReferencer == null || factoriesReferencer.Factories == null || factoriesReferencer.Factories.Count == 0) {
                Debug.LogError("SimulationWindow: Missing FactoriesReferencer or no factories set.", this);
                return;
            }

            var factories = factoriesReferencer.Factories.ToList();
            var sim = new EconomySimulator();
            var result = sim.Run(factories, duration, clicks, buyOne, startingEnergy, unlockingFactoriesData, strategy);
            lastResult = result;

            var tiers = progressionLevelsData != null
                ? progressionLevelsData.LevelsList.Select(l => (l.name, (double)l.value)).ToList()
                : null;
            Report = SimulationReport.Build(result, factories, tiers);
            if (logReport)
                Debug.Log($"Simulation pacing report ({strategy}, {clicks} clicks/s)\n{Report.ToText()}", this);

            OnSimulationGenerated?.Invoke(lastResult);
        }
    }
}
