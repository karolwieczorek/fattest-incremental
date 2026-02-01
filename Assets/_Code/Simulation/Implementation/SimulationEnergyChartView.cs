using System.Collections.Generic;
using System.Linq;
using FattestInc.Simulation.API;
using UnityEngine;
using XCharts.Runtime;

namespace FattestInc.Simulation.Implementation {
    public class SimulationEnergyChartView : ChartViewBase {
        [Header("References")] [SerializeField]
        LineChart chart;
        
        
        // [SerializeField]  
        string seriesName = "Energy";

        protected override void SimulationGenerated(SimulationResult result) {
            ShowTotalEarned(result);
        }
        
        public void ShowTotalEarned(SimulationResult result)
        {
            if (chart == null) return;
            if (result.Snapshots == null || result.Snapshots.Count == 0) return;

            var xLabels = result.Snapshots.Select(s => s.TimeSeconds.ToString()).ToList();

            chart.ClearData();

            // chart.EnsureChartComponent<Title>().text = "Cumulative Earned Energy";
            // chart.EnsureChartComponent<Legend>().show = true;

            var xAxis = chart.EnsureChartComponent<XAxis>();
            xAxis.type = Axis.AxisType.Category;
            xAxis.data = xLabels;

            chart.EnsureChartComponent<YAxis>().type = Axis.AxisType.Value;

            var serie = chart.GetOrCreateLineSerie(seriesName);
            // serie.showSymbol = false;
            serie.lineType = LineType.Smooth; // or Normal

            for (int i = 0; i < result.Snapshots.Count; i++)
                chart.AddData(seriesName, result.Snapshots[i].EnergyTotalEarned);

            chart.RemoveAllSeriesExcept(new HashSet<string> { seriesName });
            chart.RefreshChart();
        }
    }
}