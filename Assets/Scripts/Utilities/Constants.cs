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
        /// Melee attack range in units
        /// </summary>
        public const float MELEE_RANGE = 1f;
        
        /// <summary>
        /// Minimum distance between units to avoid overlap
        /// </summary>
        public const float UNIT_PERSONAL_SPACE = 0.5f;

        /// <summary>
        /// Maximum targeting distance (optimization)
        /// </summary>
        public const float MAX_TARGET_RANGE = 50f;

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

        /// <summary>
        /// Spatial grid cell size for optimization
        /// </summary>
        public const float SPATIAL_GRID_CELL_SIZE = 5f;

        // === LAYER NAMES ===
        
        public const string LAYER_UNIT = "Unit";
        public const string LAYER_GROUND = "Ground";
        public const string LAYER_UI = "UI";

        // === TAG NAMES ===
        
        public const string TAG_PLAYER = "Player";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_TEAM1 = "Team1";
        public const string TAG_TEAM2 = "Team2";

        // === SCENE NAMES ===
        
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_BATTLE = "BattleScene";
        public const string SCENE_SETTINGS = "Settings";

        // === ANIMATION PARAMETERS ===
        
        public const string ANIM_ATTACK = "Attack";
        public const string ANIM_DEATH = "Death";
        public const string ANIM_IDLE = "Idle";
        public const string ANIM_WALK = "Walk";

        // === AUDIO EVENT NAMES ===
        
        public const string AUDIO_ATTACK_HIT = "AttackHit";
        public const string AUDIO_DEATH = "UnitDeath";
        public const string AUDIO_BATTLE_START = "BattleStart";
        public const string AUDIO_VICTORY = "Victory";

        // === UI CONSTANTS ===
        
        /// <summary>
        /// Default fade duration for UI transitions
        /// </summary>
        public const float UI_FADE_DURATION = 0.3f;

        /// <summary>
        /// Tooltip display delay
        /// </summary>
        public const float TOOLTIP_DELAY = 0.5f;

        // === GAME SETTINGS ===
        
        /// <summary>
        /// Default time scale values
        /// </summary>
        public const float TIME_SCALE_NORMAL = 1f;
        public const float TIME_SCALE_FAST = 2f;
        public const float TIME_SCALE_VERY_FAST = 3f;
        public const float TIME_SCALE_PAUSED = 0f;

        // === COLORS ===
        
        public static readonly UnityEngine.Color COLOR_TEAM1 = new UnityEngine.Color(0.2f, 0.4f, 1f); // Blue
        public static readonly UnityEngine.Color COLOR_TEAM2 = new UnityEngine.Color(1f, 0.3f, 0.2f); // Red
        public static readonly UnityEngine.Color COLOR_NEUTRAL = UnityEngine.Color.gray;
        public static readonly UnityEngine.Color COLOR_HEALTH_HIGH = UnityEngine.Color.green;
        public static readonly UnityEngine.Color COLOR_HEALTH_MID = UnityEngine.Color.yellow;
        public static readonly UnityEngine.Color COLOR_HEALTH_LOW = UnityEngine.Color.red;

        // === PATHS ===
        
        /// <summary>
        /// Resources path for unit prefabs
        /// </summary>
        public const string RESOURCES_UNITS = "Units/";

        /// <summary>
        /// Resources path for UI prefabs
        /// </summary>
        public const string RESOURCES_UI = "UI/";

        /// <summary>
        /// Save data directory
        /// </summary>
        public const string SAVE_DIRECTORY = "/SaveData/";

        // === PREFERENCES KEYS ===
        
        public const string PREF_MASTER_VOLUME = "MasterVolume";
        public const string PREF_SFX_VOLUME = "SFXVolume";
        public const string PREF_MUSIC_VOLUME = "MusicVolume";
        public const string PREF_QUALITY_LEVEL = "QualityLevel";
        public const string PREF_UNITS_PER_TEAM = "UnitsPerTeam";
    }
}
