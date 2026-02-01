using System.Collections.Generic;
using System.Linq;
using FattestInc.Simulation.API;
using UnityEngine;
using XCharts.Runtime;

namespace FattestInc.Simulation.Implementation {
    public sealed class SimulationChartView : MonoBehaviour {
        [SerializeField] SimulationRunner simulationRunner;
        
        [Header("References")] [SerializeField]
        private LineChart chart;

        [Header("Options")] [SerializeField] bool showAllFactories = true;
        [SerializeField] List<string> onlyFactoryIds = new();
        
        void OnEnable() {
            simulationRunner.OnSimulationGenerated += SimulationGenerated;
        }

        void OnDisable() {
            simulationRunner.OnSimulationGenerated -= SimulationGenerated;
        }

        void SimulationGenerated(SimulationResult result) {
            // ClearAll(chart);
            ShowLevels(result);
            // ShowUpgradeEvents(result);
        }

        public void ShowLevels(SimulationResult result) {
            if (chart == null)
                return;
            if (result.Snapshots == null || result.Snapshots.Count == 0)
                return;

            // Collect which factories exist (from first snapshot)
            var factoryIds = result.Snapshots[0].Levels.Select(l => l.FactoryId).ToList();

            if (!showAllFactories && onlyFactoryIds != null && onlyFactoryIds.Count > 0)
                factoryIds = factoryIds.Where(id => onlyFactoryIds.Contains(id)).ToList();

            // Prepare X axis labels (time in seconds)
            var xLabels = new List<string>(result.Snapshots.Count);
            for (int i = 0; i < result.Snapshots.Count; i++)
                xLabels.Add(result.Snapshots[i].TimeSeconds.ToString());

            // Reset chart
            chart.ClearData();
            chart.EnsureChartComponent<Title>().text = "Factory Levels (Upgrades) Over Time";

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Category;
            xAxis.data = xLabels;

            var yAxis = chart.EnsureChartComponent<YAxis>();
            yAxis.type = Axis.AxisType.Value;
            yAxis.minMaxType = Axis.AxisMinMaxType.Default;

            // Make sure legend exists
            chart.EnsureChartComponent<Legend>().show = true;

            // Build a fast lookup per snapshot: FactoryId -> Level
            // (Snapshots are per second so it’s fine; if you go huge, optimize.)
            var perTimeLevels = new List<Dictionary<string, int>>(result.Snapshots.Count);
            foreach (var s in result.Snapshots)
                perTimeLevels.Add(s.Levels.ToDictionary(l => l.FactoryId, l => l.Level));

            // Add a series for each factory
            foreach (var id in factoryIds) {
                // Find display name from snapshot 0
                var name = result.Snapshots[0].Levels.First(l => l.FactoryId == id).FactoryName;

                var serie = chart.AddSerie<Line>(name);
                // Optional: make it look like upgrades (steps)
                // In XCharts, step line is usually supported on Line serie:
                serie.lineType = LineType.StepStart; // if your XCharts version supports it

                for (int t = 0; t < result.Snapshots.Count; t++) {
                    int lvl = 0;
                    if (perTimeLevels[t].TryGetValue(id, out var v))
                        lvl = v;
                    chart.AddData(name, lvl);
                }
            }

            chart.RefreshChart();
        }

        public void ShowUpgradeEvents(SimulationResult result)
        {
            if (chart == null) return;
            if (result.Snapshots == null || result.Snapshots.Count < 2) return;

            chart.ClearData();
            chart.EnsureChartComponent<Title>().text = "Upgrade Events Over Time";

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Category;
            xAxis.data = result.Snapshots.Select(s => s.TimeSeconds.ToString()).ToList();

            chart.EnsureChartComponent<Legend>().show = true;

            var perSecondSerie = chart.AddSerie<Line>("Upgrades/sec");
            var cumulativeSerie = chart.AddSerie<Line>("Cumulative upgrades");

            int cumulative = 0;

            // Precompute dicts for quick compare
            var dicts = result.Snapshots
                .Select(s => s.Levels.ToDictionary(l => l.FactoryId, l => l.Level))
                .ToList();

            // t=0 has no "previous"
            chart.AddData("Upgrades/sec", 0);
            chart.AddData("Cumulative upgrades", 0);

            for (int t = 1; t < dicts.Count; t++)
            {
                int upgradesThisSecond = 0;
                foreach (var kv in dicts[t])
                {
                    var id = kv.Key;
                    var now = kv.Value;
                    dicts[t - 1].TryGetValue(id, out var prev);
                    if (now > prev) upgradesThisSecond += (now - prev);
                }

                cumulative += upgradesThisSecond;
                chart.AddData("Upgrades/sec", upgradesThisSecond);
                chart.AddData("Cumulative upgrades", cumulative);
            }

            chart.RefreshChart();
        }
        
        public static void ClearAll(LineChart chart)
        {
            if (chart == null) return;

            // Remove all series (gets rid of preview legend + lines)
            if (chart.series != null)
                chart.series.Clear();

            // Clear data points too (harmless even if series were removed)
            chart.ClearData();

            // Clear x-axis category labels if you use them
            var xAxis = chart.GetChartComponent<XAxis>();
            if (xAxis != null && xAxis.data != null)
                xAxis.data.Clear();

            chart.RefreshChart();
        }
    }
}