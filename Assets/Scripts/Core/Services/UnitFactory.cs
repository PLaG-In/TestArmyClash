using System;

namespace ArmyClash.Core
{
    /// <summary>
    /// Factory service for creating units with random or specified properties
    /// </summary>
    public class UnitFactory : IUnitFactory
    {
        private readonly GameConfig _config;
        private readonly System.Random _random;

        public UnitFactory(GameConfig config)
        {
            _config = config;
            _random = new System.Random();
        }

        public Unit CreateRandomUnit(Team team)
        {
            UnitShape shape = GetRandomEnum<UnitShape>();
            UnitSize size = GetRandomEnum<UnitSize>();
            UnitColor color = GetRandomEnum<UnitColor>();

            return CreateUnit(shape, size, color, team);
        }

        public Unit CreateUnit(UnitShape shape, UnitSize size, UnitColor color, Team team)
        {
            UnitStats stats = _config.CalculateStats(shape, size, color);
            return new Unit(shape, size, color, team, stats);
        }

        private T GetRandomEnum<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(_random.Next(values.Length));
        }
    }

    public interface IUnitFactory
    {
        Unit CreateRandomUnit(Team team);
        Unit CreateUnit(UnitShape shape, UnitSize size, UnitColor color, Team team);
    }
}
