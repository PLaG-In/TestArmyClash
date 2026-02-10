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
        [SerializeField] private Transform _groundPlane;
        [SerializeField] private Vector3 _battlefieldSize = new Vector3(30, 0, 20);
        
        [Header("Team Spawn Zones")]
        [SerializeField] private Transform _team1SpawnZone;
        [SerializeField] private Transform _team2SpawnZone;
        [SerializeField] private Material _team1ZoneMaterial;
        [SerializeField] private Material _team2ZoneMaterial;
        
        [Header("Camera")]
        [SerializeField] private Camera _battleCamera;
        [SerializeField] private Vector3 _cameraPosition = new Vector3(0, 15, -10);
        [SerializeField] private Vector3 _cameraRotation = new Vector3(45, 0, 0);
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem _battleStartEffect;
        [SerializeField] private ParticleSystem _victoryEffect;

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
            if (_groundPlane != null)
            {
                _groundPlane.localScale = new Vector3(
                    _battlefieldSize.x / 10f,
                    1,
                    _battlefieldSize.z / 10f
                );
            }
        }

        private void SetupCamera()
        {
            if (_battleCamera == null)
                _battleCamera = Camera.main;

            if (_battleCamera != null)
            {
                _battleCamera.transform.position = _cameraPosition;
                _battleCamera.transform.eulerAngles = _cameraRotation;
            }
        }

        private void SetupSpawnZones()
        {
            // Team 1 spawn zone (left)
            if (_team1SpawnZone != null)
            {
                _team1SpawnZone.position = new Vector3(-_config.armySpacing, 0, 0);
                if (_team1ZoneMaterial != null)
                {
                    var renderer = _team1SpawnZone.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = _team1ZoneMaterial;
                    }
                }
            }

            // Team 2 spawn zone (right)
            if (_team2SpawnZone != null)
            {
                _team2SpawnZone.position = new Vector3(_config.armySpacing, 0, 0);
                if (_team2ZoneMaterial != null)
                {
                    var renderer = _team2SpawnZone.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = _team2ZoneMaterial;
                    }
                }
            }
        }

        /// <summary>
        /// Play battle start visual effect
        /// </summary>
        public void PlayBattleStartEffect()
        {
            if (_battleStartEffect != null)
            {
                _battleStartEffect.Play();
            }
        }

        /// <summary>
        /// Play victory effect at winning team's position
        /// </summary>
        public void PlayVictoryEffect(Core.Team winningTeam)
        {
            if (_victoryEffect != null)
            {
                Vector3 position = winningTeam == Core.Team.Team1
                    ? _team1SpawnZone.position
                    : _team2SpawnZone.position;
                
                _victoryEffect.transform.position = position + Vector3.up * 2f;
                _victoryEffect.Play();
            }
        }

        /// <summary>
        /// Get spawn position for team (used by BattleManager)
        /// </summary>
        public Vector3 GetTeamSpawnCenter(Core.Team team)
        {
            return team == Core.Team.Team1
                ? _team1SpawnZone.position
                : _team2SpawnZone.position;
        }

        /// <summary>
        /// Draw battlefield boundaries in editor
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, _battlefieldSize);

            // Draw spawn zones
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(new Vector3(-5, 0, 0), new Vector3(3, 0.1f, 10));
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(5, 0, 0), new Vector3(3, 0.1f, 10));
        }
    }
}
