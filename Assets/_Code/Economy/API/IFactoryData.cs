using UnityEngine;

namespace FattestInc.Economy.API {
    public interface IFactoryInfo {
        string FactoryId { get; }
        string FactoryName { get; }
        Sprite Icon { get; }
        FactoryType FactoryType { get; }
        int StartingLevel { get; }
    }

    public interface IFactoryData : IFactoryInfo, IFactoryLevelsData {
        ulong GetValueForLevel(int factoryLevel);
        int GetDurationForLevel(int factoryLevel);
        bool HasNextLevel(int factoryLevel);
    }
} 