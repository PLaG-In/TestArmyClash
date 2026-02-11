using UnityEngine;
using Zenject;
using ArmyClash.Core;
using ArmyClash.Utilities;
using System.Collections;

namespace ArmyClash.View
{
    /// <summary>
    /// Visual representation of a unit with physics
    /// Fixed: units stay on ground, can attack despite colliders
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        [SerializeField] private Transform healthBarTransform;

        private Unit _model;
        private GameConfig _config;
        private Color _baseColor;
        private Rigidbody _rigidbody;
        private Collider _collider;

        public Unit Model => _model;
        public Rigidbody Rigidbody => _rigidbody;

        // Attack cooldown tracking
        private float _lastAttackTime;
        private UnitView _currentCollisionTarget;

        [Inject]
        public void Construct(GameConfig config)
        {
            _config = config;
        }

        private void Awake()
        {
            SetupPhysics();
        }

        public void Initialize(Unit model)
        {
            _model = model;
            _model.OnHealthChanged += UpdateHealthBar;
            _model.OnDeath += OnUnitDeath;

            SetupVisuals();
            SetupCollider();
            UpdateHealthBar(_model.CurrentHP);
        }

        private void SetupPhysics()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }

            _rigidbody.mass = 1f;
            _rigidbody.linearDamping = 5f;
            _rigidbody.angularDamping = 10f;
//            _rigidbody.useGravity = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY | 
                                    RigidbodyConstraints.FreezeRotationX | 
                                    RigidbodyConstraints.FreezeRotationZ;
        }

        private void SetupCollider()
        {
            if (_model == null) return;

            float scale = _model.Size == UnitSize.Small 
                ? Constants.SMALL_UNIT_SCALE 
                : Constants.BIG_UNIT_SCALE;

            // Adjust mass based on size
            if (_rigidbody != null)
            {
                _rigidbody.mass = _model.Size == UnitSize.Big ? 2f : 1f;
            }
        }

        private void SetupVisuals()
        {
            if (renderer == null)
            {
                renderer = GetComponent<Renderer>();
            }

            if (renderer != null)
            {
                _baseColor = _config.GetColorValue(_model.Color);
                renderer.material.color = _baseColor;
            }

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
        }

        private void Update()
        {
            if (_model != null && _model.IsAlive)
            {
                // Sync model position from physics (only X and Z, Y is frozen)
                Vector3 pos = transform.position;
                pos.y = 0.5f; // Keep at ground level
                _model.Position = pos;
                transform.position = pos; // Enforce ground level

                // Visual health effect
                float healthPercent = _model.CurrentHP / _model.Stats.HP;
                if (healthPercent < 0.3f)
                {
                    float pulse = Mathf.PingPong(Time.time * 2f, 1f);
                    Color pulsedColor = Color.Lerp(_baseColor, Color.white, pulse * 0.3f);
                    renderer.material.color = pulsedColor;
                }
                else
                {
                    renderer.material.color = _baseColor;
                }

                // Make health bar face camera
                if (healthBarTransform != null && Camera.main != null)
                {
                    healthBarTransform.LookAt(Camera.main.transform);
                    healthBarTransform.Rotate(0, 180, 0);
                }
                if (_currentCollisionTarget != null && CanAttack())
                {
                    AttackTarget(_currentCollisionTarget);
                }
            }
        }

        private void FixedUpdate()
        {
            // Only move if battle is active
            if (_model == null || !_model.IsAlive)
                return;

            if (_model.Target != null && _model.Target.IsAlive)
            {
                Vector3 targetPosition = _model.Target.Position;
                Vector3 currentPosition = transform.position;
                
                // Calculate 2D distance (ignore Y)
                float distance = currentPosition.Distance2D(targetPosition);

                // Move if not in attack range
                float attackRange = Constants.MELEE_RANGE + 1f;
                
                if (distance > attackRange)
                {
                    // Calculate direction on XZ plane only
                    Vector3 direction = targetPosition - currentPosition;
                    direction.y = 0;
                    direction.Normalize();
                    
                    // Apply velocity only on XZ plane
                    Vector3 targetVelocity = direction * _model.Stats.Speed;
                    Vector3 currentVelocity = _rigidbody.linearVelocity;
                    currentVelocity.y = 0; // No vertical movement
                    
                    _rigidbody.linearVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * 5f);
                }
                else
                {
                    // Stop when in range - zero velocity on XZ plane
                    Vector3 vel = _rigidbody.linearVelocity;
                    vel.x = 0;
                    vel.z = 0;
                    _rigidbody.linearVelocity = vel;
                }
            }
            else
            {
                // No target - stop moving
                Vector3 vel = _rigidbody.linearVelocity;
                vel.x = 0;
                vel.z = 0;
                _rigidbody.linearVelocity = vel;
            }

            // Extra safety: force Y position
            Vector3 pos = transform.position;
            if (Mathf.Abs(pos.y - 0.5f) > 0.1f)
            {
                pos.y = 0.5f;
                transform.position = pos;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            UnitView enemyView = collision.gameObject.GetComponent<UnitView>();
            if (enemyView != null && enemyView.Model != null && _model != null)
            {
                // Check if enemy team
                if (enemyView.Model.Team != _model.Team && enemyView.Model.IsAlive)
                {
                    _currentCollisionTarget = enemyView;

                    // Try immediate attack
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
                    _currentCollisionTarget = enemyView;
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
            if (enemyView == _currentCollisionTarget)
            {
                _currentCollisionTarget = null;
            }
        }

        private void UpdateHealthBar(float currentHP)
        {
            if (healthBarTransform == null) return;

            float healthPercent = Mathf.Clamp01(currentHP / _model.Stats.HP);
            
            Vector3 scale = healthBarTransform.localScale;
            scale.x = healthPercent;
            healthBarTransform.localScale = scale;

            var healthBarRenderer = healthBarTransform.GetComponent<Renderer>();
            if (healthBarRenderer != null)
            {
                Color healthColor;
                if (healthPercent > 0.6f)
                    healthColor = Constants.COLOR_HEALTH_HIGH;
                else if (healthPercent > 0.3f)
                    healthColor = Constants.COLOR_HEALTH_MID;
                else
                    healthColor = Constants.COLOR_HEALTH_LOW;
                
                healthBarRenderer.material.color = healthColor;
            }
        }

        private void OnUnitDeath(Unit unit)
        {            
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.isKinematic = true;
            }
        }

        public void PlayAttack()
        {
            StartCoroutine(AttackPulse());
        }

        public void PlayHitReaction()
        {
            StartCoroutine(HitFlash());
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
                _currentCollisionTarget = null;
                return;
            }

            // Deal damage
            targetView.Model.TakeDamage(_model.Stats.ATK);
            _lastAttackTime = Time.time;

            // Visual/audio feedback
            PlayAttack();
            targetView.PlayHitReaction();
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
            if (renderer == null) yield break;

            Color originalColor = renderer.material.color;
            renderer.material.color = Color.red;
            
            yield return new WaitForSeconds(0.1f);
            
            if (renderer != null)
            {
                renderer.material.color = originalColor;
            }
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnHealthChanged -= UpdateHealthBar;
                _model.OnDeath -= OnUnitDeath;
            }
        }
    }
}
