using System.Collections.Generic;
using System.Linq;
using FattestInc.Simulation.API;
using UnityEngine;
using XCharts.Runtime;

namespace FattestInc.Simulation.Implementation {
    public class SimulationTotalEpsChartView  : ChartViewBase {
        [Header("References")] [SerializeField]
        LineChart chart;
        
        
        // [SerializeField]  
        string seriesName = "Energy";

        protected override void SimulationGenerated(SimulationResult result) {
            ShowTotalEps(result);
        }

        public void ShowTotalEps(SimulationResult result)
        {
            if (chart == null) return;
            if (result.Snapshots == null || result.Snapshots.Count == 0) return;

            // X axis: time in seconds
            var xLabels = result.Snapshots.Select(s => s.TimeSeconds.ToString()).ToList();

            chart.ClearData();

            chart.EnsureChartComponent<Title>().text = "Energy Per Second (Total)";
            chart.EnsureChartComponent<Legend>().show = true;

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Category;
            xAxis.data = xLabels;

            chart.EnsureChartComponent<YAxis>().type = Axis.AxisType.Value;

            // Reuse or create series
            var serie = chart.GetOrCreateLineSerie(seriesName);
            // serie.showSymbol = false;
            serie.lineType = LineType.Normal; // Smooth if you prefer

            for (int i = 0; i < result.Snapshots.Count; i++)
                chart.AddData(seriesName, result.Snapshots[i].EnergyPerSecond);

            // Keep only this series
            chart.RemoveAllSeriesExcept(new HashSet<string> { seriesName });

            chart.RefreshChart();
        }
    }
}