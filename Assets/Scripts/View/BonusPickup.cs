using System.Collections;
using UnityEngine;
using ArmyClash.Core;
using ArmyClash.Utilities;

namespace ArmyClash.View
{
    /// <summary>
    /// A physical bonus pickup on the battlefield.
    /// - Uses a trigger collider so units collect it on overlap
    /// - Animates: bobs up/down, rotates, pulses scale
    /// - Despawns after lifetime expires
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BonusPickup : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private TrailRenderer _trail;

        // runtime data set by spawner
        private BonusEntry _entry;
        private BonusConfig _config;
        private Material _material;
        private bool _collected;
        private float _spawnTime;

        // animation
        private Vector3 _basePosition;
        private float _bobOffset;

        // ─── Initialise ──────────────────────────────────────────────────────────

        public void Initialize(BonusEntry entry, BonusConfig config)
        {
            _entry = entry;
            _config = config;
            _collected = false;
            _spawnTime = Time.time;
            _basePosition = transform.position;

            // Random bob phase so pickups don't all move in sync
            _bobOffset = Random.Range(0f, Mathf.PI * 2f);

            // Make sure collider is a trigger
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;

            // Disable Rigidbody gravity if present (pickup floats)
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) { rb.useGravity = false; rb.isKinematic = true; }

            ApplyVisuals();

            StartCoroutine(LifetimeCoroutine());
        }

        private void ApplyVisuals()
        {
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            if (_renderer != null)
            {
                _material = _renderer.material;
                _material.color = _entry.color;
                // Emissive glow
                _material.EnableKeyword("_EMISSION");
                _material.SetColor("_EmissionColor", _entry.color * 0.6f);
                _renderer.material = _material;
            }

            if (_trail != null)
            {
                _trail.startColor = _entry.color;
                _trail.endColor = new Color(_entry.color.r, _entry.color.g, _entry.color.b, 0);
            }
        }

        // ─── Animation ───────────────────────────────────────────────────────────

        private void Update()
        {
            if (_collected) return;

            float t = Time.time;

            // Bob up and down
            float bob = Mathf.Sin(t * 2f + _bobOffset) * 0.25f;
            transform.position = _basePosition + Vector3.up * bob;

            // Spin
            transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);

            // Pulse scale based on remaining lifetime
            float life = _config.bonusLifetime;
            float elapsed = t - _spawnTime;
            float remaining = life - elapsed;

            // Blink fast when about to disappear (last 3 seconds)
            if (remaining < 3f)
            {
                float blink = Mathf.PingPong(t * 8f, 1f);
                transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.2f, blink);
            }
            else
            {
                float pulse = 1f + Mathf.Sin(t * 4f + _bobOffset) * 0.1f;
                transform.localScale = Vector3.one * pulse;
            }
        }

        // ─── Collection ──────────────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            if (_collected) return;

            // Only units can collect
            UnitView unitView = other.GetComponent<UnitView>();
            if (unitView == null || unitView.Model == null || !unitView.Model.IsAlive)
                return;

            Collect(unitView);
        }

        private void Collect(UnitView collector)
        {
            _collected = true;

            // Apply effect to unit
            ApplyEffect(collector.Model);

            // Visual feedback
            collector.PlayAttack(); // reuse pulse animation

            StartCoroutine(CollectEffect());
        }

        private void ApplyEffect(Unit unit)
        {
            switch (_entry.type)
            {
                case BonusType.AttackBoost:
                    unit.ApplyBuff(new BonusEffect(BonusType.AttackBoost, _entry.value, _entry.duration));
                    Debug.Log($"[Bonus] {unit.Team} unit got ATK +{_entry.value} for {_entry.duration}s");
                    break;

                case BonusType.SpeedBoost:
                    unit.ApplyBuff(new BonusEffect(BonusType.SpeedBoost, _entry.value, _entry.duration));
                    Debug.Log($"[Bonus] {unit.Team} unit got Speed +{_entry.value} for {_entry.duration}s");
                    break;

                case BonusType.HealthRestore:
                    unit.Heal(_entry.value);
                    Debug.Log($"[Bonus] {unit.Team} unit healed {_entry.value} HP");
                    break;

                case BonusType.Shield:
                    unit.ApplyBuff(new BonusEffect(BonusType.Shield, 0, _entry.duration));
                    Debug.Log($"[Bonus] {unit.Team} unit got Shield for {_entry.duration}s");
                    break;

                case BonusType.Berserker:
                    unit.ApplyBuff(new BonusEffect(BonusType.Berserker, _entry.value, _entry.duration));
                    Debug.Log($"[Bonus] {unit.Team} unit went Berserker! ATK +{_entry.value}");
                    break;
            }
        }

        // ─── Coroutines ──────────────────────────────────────────────────────────

        private IEnumerator CollectEffect()
        {
            // Burst scale up then shrink to nothing
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 start = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(start, Vector3.one * 2f, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            ReturnToPool();
        }

        private IEnumerator LifetimeCoroutine()
        {
            yield return new WaitForSeconds(_config.bonusLifetime);

            if (!_collected)
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            if (_material != null)
            {
                _material.SafeDestroy();
                _material = null;
            }
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                _material.SafeDestroy();
            }
        }
    }
}
