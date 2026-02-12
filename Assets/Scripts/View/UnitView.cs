using System;
using UnityEngine;
using Zenject;
using ArmyClash.Core;
using ArmyClash.Utilities;
using System.Collections;
using TMPro;

namespace ArmyClash.View
{
    /// <summary>
    /// Visual representation with autonomous movement and collision-based attacks
    /// Each unit moves independently towards its target
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform healthBarTransform;
        [SerializeField] private TMP_Text teamNumber;

        private Unit _model;
        private GameConfig _config;
        private Color _baseColor;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private BattleState _battleState;

        // Attack tracking
        private float _lastAttackTime;
        private UnitView _currentTarget;

        public Unit Model => _model;
        public Rigidbody Rigidbody => _rigidbody;

        [Inject]
        public void Construct(GameConfig config, BattleState battleState)
        {
            _config = config;
            _battleState = battleState;
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        public void Initialize(Unit model)
        {
            _model = model;
            _model.OnHealthChanged += UpdateHealthBar;
            _model.OnDeath += OnUnitDeath;

            SetupVisuals();
            SetupCollider();
            UpdateHealthBar(_model.CurrentHP);

            _rigidbody.isKinematic = true;
        }

        private void Start()
        {
            _battleState.OnBattleStarted += OnBattleStarted;
        }

        private void OnBattleStarted()
        {
            _rigidbody.isKinematic = false;
        }

        private void SetupCollider()
        {
            if (_model == null) return;

            gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_UNIT);
            gameObject.tag = _model.Team == Team.Team1 ? Constants.TAG_TEAM1 : Constants.TAG_TEAM2;

            _rigidbody.mass = _model.Size == UnitSize.Big ? 2f : 1f;
        }

        private void SetupVisuals()
        {
            _baseColor = _config.GetColorValue(_model.Color);
            _renderer.material.color = _baseColor;

            float scale = _model.Size == UnitSize.Small
                ? Constants.SMALL_UNIT_SCALE
                : Constants.BIG_UNIT_SCALE;
            transform.localScale = Vector3.one * scale;

            if (healthBarTransform != null)
            {
                Vector3 offset = healthBarTransform.localPosition;
                offset.y = Constants.HEALTHBAR_OFFSET;
                healthBarTransform.localPosition = offset;
            }

            teamNumber.text = $"{_model.Team}";
        }

        private void Update()
        {
            if (_battleState.IsBattleActive && _model is { IsAlive: true })
            {
                Vector3 pos = transform.position;
                _model.Position = pos;

                float healthPercent = _model.CurrentHP / _model.Stats.HP;
                if (healthPercent < 0.3f)
                {
                    float pulse = Mathf.PingPong(Time.time * 2f, 1f);
                    Color pulsedColor = Color.Lerp(_baseColor, Color.white, pulse * 0.3f);
                    _renderer.material.color = pulsedColor;
                }
                else
                {
                    _renderer.material.color = _baseColor;
                }

                if (healthBarTransform != null && Camera.main != null)
                {
                    healthBarTransform.LookAt(Camera.main.transform);
                    healthBarTransform.Rotate(0, 180, 0);
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_battleState.IsBattleActive || _model is not { IsAlive: true })
                return;

            if (_model.Target is { IsAlive: true })
            {
                Vector3 targetPosition = _model.Target.Position;
                Vector3 currentPosition = transform.position;
                
                // Calculate direction on XZ plane
                Vector3 direction = targetPosition - currentPosition;
                direction.y = 0;
                direction.Normalize();
                
                // Apply velocity
                Vector3 targetVelocity = direction * _model.Stats.Speed;
                Vector3 currentVelocity = _rigidbody.linearVelocity;
                currentVelocity.y = 0;
                
                _rigidbody.linearVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * 5f);
                
                // Rotate to face target
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
                }
            }
            else
            {
                // No target - stop
                Vector3 vel = _rigidbody.linearVelocity;
                vel.x = 0;
                vel.z = 0;
                _rigidbody.linearVelocity = vel;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            UnitView enemyView = collision.gameObject.GetComponent<UnitView>();
            if (enemyView != null && enemyView.Model != null && _model != null)
            {
                if (enemyView.Model.Team != _model.Team && enemyView.Model.IsAlive)
                {
                    _currentTarget = enemyView;

                    if (CanAttack())
                    {
                        AttackTarget(enemyView);
                    }
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            UnitView enemyView = collision.gameObject.GetComponent<UnitView>();
            if (enemyView != null && enemyView.Model != null && _model != null)
            {
                if (enemyView.Model.Team != _model.Team && enemyView.Model.IsAlive)
                {
                    _currentTarget = enemyView;
                    if (CanAttack())
                    {
                        AttackTarget(enemyView);
                    }
                }
                else
                {
                    // Same team - push away
                    Vector3 separation = transform.position - collision.transform.position;
                    separation.y = 0;

                    if (separation.magnitude < Constants.UNIT_PERSONAL_SPACE && separation.magnitude > 0.01f)
                    {
                        Vector3 force = separation.normalized * 1f;
                        force.y = 0;
                        _rigidbody.AddForce(force, ForceMode.VelocityChange);
                    }
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            UnitView enemyView = collision.gameObject.GetComponent<UnitView>();
            if (enemyView == _currentTarget)
            {
                _currentTarget = null;
            }
        }

        private bool CanAttack()
        {
            float attackCooldown = _model.Stats.AtkSpeed;
            return Time.time - _lastAttackTime >= attackCooldown;
        }

        private void AttackTarget(UnitView targetView)
        {
            if (targetView == null || targetView.Model == null || !targetView.Model.IsAlive)
            {
                _currentTarget = null;
                return;
            }

            targetView.Model.TakeDamage(_model.Stats.ATK);
            _lastAttackTime = Time.time;

            PlayAttack();
            targetView.PlayHitReaction();
        }

        private void UpdateHealthBar(float currentHP)
        {
            if (healthBarTransform == null) return;

            float healthPercent = Mathf.Clamp01(currentHP / _model.Stats.HP);

            Vector3 scale = healthBarTransform.localScale;
            scale.x = healthPercent;
            healthBarTransform.localScale = scale;

            var healthBarRenderer = healthBarTransform.GetComponent<Renderer>();

            Color healthColor = healthPercent switch
            {
                > 0.6f => Constants.COLOR_HEALTH_HIGH,
                > 0.3f => Constants.COLOR_HEALTH_MID,
                _ => Constants.COLOR_HEALTH_LOW
            };

            healthBarRenderer.material.color = healthColor;
        }

        private void OnUnitDeath(Unit unit)
        {
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.isKinematic = true;
            }
            StopCoroutine(HitFlash());
            StopCoroutine(AttackPulse());
        }

        public void PlayAttack()
        {
            StopCoroutine(AttackPulse());
            if (gameObject.activeSelf)
            {
                StartCoroutine(AttackPulse());
            }
        }

        public void PlayHitReaction()
        {
            StopCoroutine(HitFlash());
            if (gameObject.activeSelf)
            {
                StartCoroutine(HitFlash());
            }
        }

        private IEnumerator AttackPulse()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * 1.15f;

            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.Sin(t * Mathf.PI));
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private IEnumerator HitFlash()
        {
            if (_renderer == null) yield break;

            Color originalColor = _renderer.material.color;
            _renderer.material.color = Color.red;

            yield return new WaitForSeconds(0.1f);

            _renderer.material.color = originalColor;
        }

        private void OnDestroy()
        {
            _model.OnHealthChanged -= UpdateHealthBar;
            _model.OnDeath -= OnUnitDeath;

            _battleState.OnBattleStarted -= OnBattleStarted;
        }
    }
}
