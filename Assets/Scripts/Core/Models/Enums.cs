namespace ArmyClash.Core
{
    /// <summary>
    /// All game enumerations in one place for easy access
    /// </summary>

    /// <summary>
    /// Unit shape types - determines visual representation and base stats
    /// </summary>
    public enum UnitShape
    {
        Cube,
        Sphere
        // Easy to extend: Pyramid, Cylinder, etc.
    }

    /// <summary>
    /// Unit size variants - affects HP and visual scale
    /// </summary>
    public enum UnitSize
    {
        Small,
        Big
        // Easy to extend: Medium, Huge, Tiny, etc.
    }

    /// <summary>
    /// Unit color types - affects stats and visual appearance
    /// </summary>
    public enum UnitColor
    {
        Blue,
        Green,
        Red
        // Easy to extend: Purple, Yellow, Orange, etc.
    }

    /// <summary>
    /// Team identifier for battle sides
    /// </summary>
    public enum Team
    {
        Team1,
        Team2
        // Could extend to: Team3, Team4 for multi-team battles
    }
}
