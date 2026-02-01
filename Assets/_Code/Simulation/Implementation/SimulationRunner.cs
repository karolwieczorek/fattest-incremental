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
        
        SimulationResult lastResult;
        public event UnityAction<SimulationResult> OnSimulationGenerated;

        public SimulationResult Result => lastResult;

        public void RunSimulation(int duration, double clicks, double startingEnergy, bool buyOne) {
            if (factoriesReferencer == null || factoriesReferencer.Factories == null || factoriesReferencer.Factories.Count == 0) {
                Debug.LogError("SimulationWindow: Missing FactoriesReferencer or no factories set.", this);
                return;
            }

            var sim = new EconomySimulator();
            var result = sim.Run(factoriesReferencer.Factories.ToList(), duration, clicks, buyOne, startingEnergy, unlockingFactoriesData);
            lastResult = result;
            
            OnSimulationGenerated?.Invoke(lastResult);
        }
    }
}