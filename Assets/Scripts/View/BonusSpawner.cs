using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using ArmyClash.Core;

namespace ArmyClash.View
{
    /// <summary>
    /// Periodically spawns bonus pickups on the battlefield.
    /// Uses a simple pool (List) of BonusPickup objects.
    /// Integrates with BattleState - spawns only during active battle.
    /// </summary>
    public class BonusSpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private BonusConfig _bonusConfig;

        [Header("Prefab")]
        [Tooltip("Prefab with BonusPickup component. If null, creates a sphere automatically.")]
        [SerializeField] private GameObject _bonusPrefab;

        // ─── Injected ─────────────────────────────────────────────────────────────
        private BattleState _battleState;
        private BattleManager _battleManager;

        // ─── Pool ────────────────────────────────────────────────────────────────
        private readonly List<BonusPickup> _pool = new List<BonusPickup>();
        private int _activeCount;

        // ─── State ───────────────────────────────────────────────────────────────
        private bool _isSpawning;

        [Inject]
        public void Construct(BattleState battleState, BattleManager battleManager)
        {
            _battleState = battleState;
            _battleManager = battleManager;
        }

        private void Start()
        {
            if (_bonusConfig == null)
            {
                Debug.LogError("[BonusSpawner] BonusConfig is not assigned!");
                return;
            }

            _battleState.OnBattleStarted += OnBattleStarted;
            _battleState.OnBattleStopped += OnBattleStoppped;
            _battleManager.OnBattleCleared += OnBattleCleared;

            // Pre-warm pool
            for (int i = 0; i < _bonusConfig.maxBonuses; i++)
            {
                _pool.Add(CreatePickup());
            }
        }

        // ─── Events ──────────────────────────────────────────────────────────────

        private void OnBattleStarted()
        {
            _isSpawning = true;
            StartCoroutine(SpawnLoop());
        }

        private void OnBattleStoppped()
        {
            _isSpawning = false;
            DeactivateAll();
        }

        private void OnBattleCleared()
        {
            _isSpawning = false;
            DeactivateAll();
        }

        // ─── Spawn loop ──────────────────────────────────────────────────────────

        private IEnumerator SpawnLoop()
        {
            // Short delay so battle starts before first bonus appears
            yield return new WaitForSeconds(3f);

            while (_isSpawning)
            {
                TrySpawnBonus();
                yield return new WaitForSeconds(_bonusConfig.spawnInterval);
            }
        }

        private void TrySpawnBonus()
        {
            // Count currently active pickups
            _activeCount = 0;
            foreach (var p in _pool.Where(p => p.gameObject.activeSelf))
            {
                _activeCount++;
            }

            if (_activeCount >= _bonusConfig.maxBonuses) return;

            // Get from pool
            BonusPickup pickup = GetFromPool();
            if (pickup == null) return;

            // Random position on battlefield
            Vector3 pos = new Vector3(
                Random.Range(_bonusConfig.spawnAreaX.x, _bonusConfig.spawnAreaX.y),
                1.0f,  // float slightly above ground
                Random.Range(_bonusConfig.spawnAreaZ.x, _bonusConfig.spawnAreaZ.y)
            );

            pickup.transform.position = pos;
            pickup.gameObject.SetActive(true);
            pickup.Initialize(_bonusConfig.GetRandom(), _bonusConfig);
        }

        // ─── Pool ────────────────────────────────────────────────────────────────

        private BonusPickup GetFromPool()
        {
            foreach (var p in _pool)
            {
                if (!p.gameObject.activeSelf)
                    return p;
            }
            var extra = CreatePickup();
            _pool.Add(extra);
            return extra;
        }

        private BonusPickup CreatePickup()
        {
            var go = Instantiate(_bonusPrefab, transform);

            go.name = "BonusPickup";
            go.SetActive(false);

            if (!go.TryGetComponent<BonusPickup>(out var pickup))
            {
                pickup = go.AddComponent<BonusPickup>();
            }

            return pickup;
        }

        private void DeactivateAll()
        {
            foreach (var p in _pool)
            {
                p.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_battleState != null)
            {
                _battleState.OnBattleStarted -= OnBattleStarted;
                _battleState.OnBattleStopped -= OnBattleStoppped;
            }

            if (_battleManager != null)
            {
                _battleManager.OnBattleCleared -= OnBattleCleared;
            }
        }
    }
}
