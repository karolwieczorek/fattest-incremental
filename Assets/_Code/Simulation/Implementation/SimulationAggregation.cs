using System.Collections.Generic;
using System.Linq;
using FattestInc.Economy.API;
using FattestInc.Simulation.API;

namespace FattestInc.Simulation.Implementation {
    public static class SimulationAggregation {

        public static Dictionary<string, double[]> BuildCumulativeByFactory(
            IReadOnlyList<IFactoryData> factories,
            SimulationResult result,
            double manualClicksPerSecond,
            bool useIdInSeriesName = true) {
            var byId = factories.ToDictionary(f => f.FactoryId, f => f);

            int n = result.Snapshots.Count;

            // Stable series keys for *every factory in your data list*
            var keysByFactoryId = new Dictionary<string, string>(factories.Count);
            foreach (var f in factories) {
                var key = useIdInSeriesName
                    ? $"{f.FactoryName} ({f.FactoryId})"
                    : f.FactoryName;

                keysByFactoryId[f.FactoryId] = key;
            }

            // Allocate cumulative arrays
            var cumulative = keysByFactoryId.Values
                .Distinct()
                .ToDictionary(k => k, _ => new double[n]);

            // Running totals per factory (by FactoryId)
            var running = factories.ToDictionary(f => f.FactoryId, _ => 0.0);

            for (int i = 0; i < n; i++) {
                var snap = result.Snapshots[i];

                // For each factory in this snapshot, compute produced this second and accumulate
                foreach (var lvl in snap.Levels) {
                    if (!byId.TryGetValue(lvl.FactoryId, out var f))
                        continue;

                    int level = lvl.Level;
                    if (level <= 0)
                        continue;

                    double perSecond;
                    if (f.FactoryType == FactoryType.Clicker) {
                        // manual clicks drive clicker production
                        perSecond = f.GetValueForLevel(level) * manualClicksPerSecond;
                    }
                    else {
                        var vpt = f.GetValueForLevel(level);
                        var dur = f.GetDurationForLevel(level);
                        perSecond = dur > 0 ? (double) vpt / dur : 0.0;
                    }

                    running[lvl.FactoryId] += perSecond;

                    var key = keysByFactoryId[lvl.FactoryId];
                    cumulative[key][i] = running[lvl.FactoryId];
                }

                // IMPORTANT: factories that didn't appear in snapshot.Levels (shouldn't happen in your sim)
                // would need carry-forward values. Your snapshots include all factories, so we're fine.
            }

            return cumulative;
        }

        public static Dictionary<string, double[]> BuildEpsByFactory(
            IReadOnlyList<IFactoryData> factories,
            SimulationResult result,
            double manualClicksPerSecond,
            bool useIdInSeriesName = true)
        {
            var byId = factories.ToDictionary(f => f.FactoryId, f => f);

            int n = result.Snapshots.Count;

            var seriesNameByFactoryId = factories
                .Select((f, index) => new
                {
                    f.FactoryId,
                    Name = useIdInSeriesName
                        ? $"[{index}] {f.FactoryName} ({f.FactoryId})"
                        : $"[{index}] {f.FactoryName}"
                })
                .ToDictionary(x => x.FactoryId, x => x.Name);

            // Allocate eps arrays
            var eps = seriesNameByFactoryId.Values
                .Distinct()
                .ToDictionary(name => name, _ => new double[n]);

            for (int t = 0; t < n; t++)
            {
                var snap = result.Snapshots[t];

                foreach (var lvl in snap.Levels)
                {
                    if (!byId.TryGetValue(lvl.FactoryId, out var f))
                        continue;

                    int level = lvl.Level;
                    double perSecond = 0.0;

                    if (level > 0)
                    {
                        if (f.FactoryType == FactoryType.Clicker)
                        {
                            perSecond = f.GetValueForLevel(level) * manualClicksPerSecond;
                        }
                        else
                        {
                            var vpt = f.GetValueForLevel(level);
                            var dur = f.GetDurationForLevel(level);
                            perSecond = dur > 0 ? (double)vpt / dur : 0.0;
                        }
                    }

                    var seriesName = seriesNameByFactoryId[lvl.FactoryId];
                    eps[seriesName][t] = perSecond;
                }
            }

            return eps;
        }
    }
}