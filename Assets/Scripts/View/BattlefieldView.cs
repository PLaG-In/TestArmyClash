using UnityEngine;
using Zenject;

namespace ArmyClash.View
{
    /// <summary>
    /// Manages the battlefield visual setup - ground plane, boundaries, camera position
    /// </summary>
    public class BattlefieldView : MonoBehaviour
    {
        [Header("Battlefield Settings")]
        [SerializeField] private Transform groundPlane;
        [SerializeField] private Vector3 battlefieldSize = new Vector3(30, 0, 20);
        
        [Header("Team Spawn Zones")]
        [SerializeField] private Transform team1SpawnZone;
        [SerializeField] private Transform team2SpawnZone;
        [SerializeField] private Material team1ZoneMaterial;
        [SerializeField] private Material team2ZoneMaterial;
        
        [Header("Camera")]
        [SerializeField] private Camera battleCamera;
        [SerializeField] private Vector3 cameraPosition = new Vector3(0, 15, -10);
        [SerializeField] private Vector3 cameraRotation = new Vector3(45, 0, 0);
      
        private Core.GameConfig _config;

        [Inject]
        public void Construct(Core.GameConfig config)
        {
            _config = config;
        }

        private void Start()
        {
            SetupBattlefield();
            SetupCamera();
            SetupSpawnZones();
        }

        private void SetupBattlefield()
        {
            if (groundPlane != null)
            {
                groundPlane.localScale = new Vector3(
                    battlefieldSize.x / 10f,
                    1,
                    battlefieldSize.z / 10f
                );
            }
        }

        private void SetupCamera()
        {
            if (battleCamera == null)
                battleCamera = Camera.main;

            if (battleCamera != null)
            {
                battleCamera.transform.position = cameraPosition;
                battleCamera.transform.eulerAngles = cameraRotation;
            }
        }

        private void SetupSpawnZones()
        {
            // Team 1 spawn zone (left)
            if (team1SpawnZone != null)
            {
                team1SpawnZone.position = new Vector3(-_config.armySpacing, 0, 0);
                if (team1ZoneMaterial != null)
                {
                    var renderer = team1SpawnZone.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = team1ZoneMaterial;
                    }
                }
            }

            // Team 2 spawn zone (right)
            if (team2SpawnZone != null)
            {
                team2SpawnZone.position = new Vector3(_config.armySpacing, 0, 0);
                if (team2ZoneMaterial != null)
                {
                    var renderer = team2SpawnZone.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = team2ZoneMaterial;
                    }
                }
            }
        }

        /// <summary>
        /// Get spawn position for team (used by BattleManager)
        /// </summary>
        public Vector3 GetTeamSpawnCenter(Core.Team team)
        {
            return team == Core.Team.Team1
                ? team1SpawnZone.position
                : team2SpawnZone.position;
        }

        /// <summary>
        /// Draw battlefield boundaries in editor
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, battlefieldSize);

            // Draw spawn zones
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(new Vector3(-5, 0, 0), new Vector3(3, 0.1f, 10));
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(5, 0, 0), new Vector3(3, 0.1f, 10));
        }
    }
}
