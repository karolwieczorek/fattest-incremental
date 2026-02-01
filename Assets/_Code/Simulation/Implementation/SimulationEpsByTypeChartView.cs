using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Simulation.API;
using UnityEngine;
using XCharts.Runtime;

namespace FattestInc.Simulation.Implementation {
    public class SimulationEpsByTypeChartView : ChartViewBase {
        [SerializeField] FactoriesReferencer factoriesReferencer;
        [SerializeField] float clicksPerSecond;

        [Header("References")] [SerializeField]
        LineChart chart;

        protected override void SimulationGenerated(SimulationResult result) {
            ShowEpsByFactoryStacked(factoriesReferencer.Factories, result, clicksPerSecond);
        }
        
        void ShowEpsByFactoryStacked(
            IReadOnlyList<IFactoryData> factories,
            SimulationResult result,
            double manualClicksPerSecond)
        {
            if (chart == null) return;
            if (factories == null || factories.Count == 0) return;
            if (result.Snapshots == null || result.Snapshots.Count == 0) return;

            var epsByFactory = SimulationAggregation.BuildEpsByFactory(
                factories, result, manualClicksPerSecond, useIdInSeriesName: false);

            var xLabels = result.Snapshots.Select(s => s.TimeSeconds.ToString()).ToList();

            chart.ClearData();
            // chart.EnsureChartComponent<Title>().text = "Earnings Per Second by Generator (Stacked)";
            // chart.EnsureChartComponent<Legend>().show = true;

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Category;
            xAxis.data = xLabels;

            chart.EnsureChartComponent<YAxis>().type = Axis.AxisType.Value;

            // Stack order: small contributors at bottom, big at top
            // var ordered = epsByFactory
            //     .OrderBy(kv => kv.Value.Last()) // or Sum() / Max()
            //     .ToList();

            var ordered = epsByFactory;

            var keep = new HashSet<string>();

            foreach (var kv in ordered)
            {
                var seriesName = kv.Key;
                var arr = kv.Value;

                keep.Add(seriesName);

                var serie = chart.GetOrCreateLineSerie(seriesName); // your extension
                // serie.showSymbol = false;
                serie.lineType = LineType.Normal;

                // stacked area
                serie.stack = "total";
                // serie.areaStyle.show = true;

                for (int i = 0; i < arr.Length; i++)
                    chart.AddData(seriesName, arr[i]);
            }

            chart.RemoveAllSeriesExcept(keep); // your extension
            chart.RefreshChart();
        }
    }
}