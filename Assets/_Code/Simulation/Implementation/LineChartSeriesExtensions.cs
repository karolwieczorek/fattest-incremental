using System.Collections.Generic;
using System.Linq;
using XCharts.Runtime;

namespace FattestInc.Simulation.Implementation {
    public static class LineChartSeriesExtensions {
        public static Line GetOrCreateLineSerie(this LineChart chart, string name)
        {
            if (chart == null) return null;

            // Find by name (authoritative identity)
            var existing = chart.series.FirstOrDefault(s => s.serieName == name);
            if (existing != null)
            {
                if (existing is Line line)
                {
                    // Reuse: clear only data
                    line.data.Clear();
                    return line;
                }

                // Same name but wrong type → remove safely, then recreate
                chart.RemoveSerieByName(name);
            }

            // Create new Line serie
            return chart.AddSerie<Line>(name);
        }
        
        public static void RemoveSerieByName(this LineChart chart, string name)
        {
            if (chart == null) return;

            // Prefer name-based API if available
            try
            {
                chart.RemoveSerie(name);
                return;
            }
            catch
            {
                // Fallback: find index internally (implementation detail)
            }

            for (int i = 0; i < chart.series.Count; i++)
            {
                if (chart.series[i].serieName == name)
                {
                    chart.RemoveSerie(i);
                    return;
                }
            }
        }
        
        public static void RemoveAllSeriesExcept(this LineChart chart, HashSet<string> keepNames)
        {
            if (chart == null) return;

            var toRemove = chart.series
                .Where(s => !keepNames.Contains(s.serieName))
                .Select(s => s.serieName)
                .ToList();

            foreach (var name in toRemove)
                chart.RemoveSerieByName(name);
        }

    }
}