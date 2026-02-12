namespace ArmyClash.Utilities
{
    /// <summary>
    /// Project-wide constants and configuration values
    /// Centralized location for magic numbers and strings
    /// </summary>
    public static class Constants
    {
        // === COMBAT CONSTANTS ===
        
        /// <summary>
        /// Minimum distance between units to avoid overlap
        /// </summary>
        public const float UNIT_PERSONAL_SPACE = 2f;

        // === VISUAL CONSTANTS ===
        
        /// <summary>
        /// Height offset for floating health bars
        /// </summary>
        public const float HEALTHBAR_OFFSET = 1.5f;

        /// <summary>
        /// Scale multiplier for small units
        /// </summary>
        public const float SMALL_UNIT_SCALE = 0.75f;

        /// <summary>
        /// Scale multiplier for big units
        /// </summary>
        public const float BIG_UNIT_SCALE = 1.5f;

        // === PERFORMANCE CONSTANTS ===
        
        /// <summary>
        /// How often to update target selection (in frames)
        /// </summary>
        public const int TARGET_UPDATE_INTERVAL = 5;

        /// <summary>
        /// Maximum units per team (hard limit)
        /// </summary>
        public const int MAX_UNITS_PER_TEAM = 100;

        // === LAYER NAMES ===
        
        public const string LAYER_UNIT = "Unit";
        public const string LAYER_GROUND = "Ground";
        public const string LAYER_UI = "UI";

        // === TAG NAMES ===

        public const string TAG_TEAM1 = "Team1";
        public const string TAG_TEAM2 = "Team2";

        // === COLORS ===

        public static readonly UnityEngine.Color COLOR_HEALTH_HIGH = UnityEngine.Color.green;
        public static readonly UnityEngine.Color COLOR_HEALTH_MID = UnityEngine.Color.yellow;
        public static readonly UnityEngine.Color COLOR_HEALTH_LOW = UnityEngine.Color.red;
    }
}
