using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Simulation.API;
using UnityEngine;
using XCharts.Runtime;

namespace FattestInc.Simulation.Implementation {
    public class SimulationTypeStackedChartView : ChartViewBase {
        [SerializeField] FactoriesReferencer factoriesReferencer;
        [SerializeField] float clicksPerSecond;

        [Header("References")] [SerializeField]
        LineChart chart;

        protected override void SimulationGenerated(SimulationResult result) {
            ShowStackedCumulativeByFactory(factoriesReferencer.Factories, result, clicksPerSecond);
        }

        public void ShowStackedCumulativeByFactory(
            IReadOnlyList<IFactoryData> factories,
            SimulationResult result,
            double manualClicksPerSecond) {
            if (chart == null)
                return;
            if (factories == null || factories.Count == 0)
                return;
            if (result.Snapshots == null || result.Snapshots.Count == 0)
                return;

            var cumulative = SimulationAggregation.BuildCumulativeByFactory(
                factories, result, manualClicksPerSecond, useIdInSeriesName: true);

            // X labels
            var xLabels = result.Snapshots.Select(s => s.TimeSeconds.ToString()).ToList();

            // Safe reset (keeps series config intact)
            chart.ClearData();

            // chart.EnsureChartComponent<Title>().text = "Cumulative Energy by Generator (Stacked)";
            // chart.EnsureChartComponent<Legend>().show = true;

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Category;
            xAxis.data = xLabels;

            var yAxis = chart.EnsureChartComponent<YAxis>();
            yAxis.type = Axis.AxisType.Value;

            // Plot order matters visually (bottom->top). Commonly: sort by final contribution.
            var ordered = cumulative
                .OrderBy(kv => kv.Value[kv.Value.Length - 1]) // smallest first => bottom
                .ToList();

            var keep = new HashSet<string>();

            foreach (var kv in ordered) {
                var seriesName = kv.Key;
                var arr = kv.Value;

                keep.Add(seriesName);

                var serie = chart.GetOrCreateLineSerie(seriesName); // your extension
                // serie.showSymbol = false;
                serie.lineType = LineType.Normal;

                // Stacked area setup
                serie.stack = "total";
                // serie.areaStyle.show = true;

                for (int i = 0; i < arr.Length; i++)
                    chart.AddData(seriesName, arr[i]);
            }

            // Remove demo/unused series safely (your extension)
            chart.RemoveAllSeriesExcept(keep);

            chart.RefreshChart();
        }
    }
}