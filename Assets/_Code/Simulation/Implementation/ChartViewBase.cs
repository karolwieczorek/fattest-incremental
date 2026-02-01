using FattestInc.Simulation.API;
using UnityEngine;

namespace FattestInc.Simulation.Implementation {
    public abstract class ChartViewBase : MonoBehaviour {
        [SerializeField] SimulationRunner simulationRunner;
        
        void OnEnable() {
            simulationRunner.OnSimulationGenerated += SimulationGenerated;
        }

        void OnDisable() {
            simulationRunner.OnSimulationGenerated -= SimulationGenerated;
        }

        protected abstract void SimulationGenerated(SimulationResult result);
    }
}