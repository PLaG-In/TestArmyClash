using UnityEngine;
using Zenject;
using ArmyClash.Core;
using ArmyClash.Utilities;

namespace ArmyClash.View
{
    /// <summary>
    /// Visual representation of a unit
    /// Uses Constants and Extensions from Utilities
    /// </summary>
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _healthBarTransform;

        private Unit _model;
        private GameConfig _config;
        private Color _baseColor;

        /// <summary>
        /// Public accessor for the unit model
        /// </summary>
        public Unit Model => _model;

        [Inject]
        public void Construct(GameConfig config)
        {
            _config = config;
        }

        public void Initialize(Unit model)
        {
            _model = model;
            _model.OnHealthChanged += UpdateHealthBar;
            _model.OnDeath += OnUnitDeath;

            SetupVisuals();
            UpdateHealthBar(_model.CurrentHP);
        }

        private void SetupVisuals()
        {
            // Get or create renderer
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            // Create and set material
            if (_renderer != null)
            {
                _baseColor = _config.GetColorValue(_model.Color);
                _renderer.material.color = _baseColor;
            }

            // Set scale based on size using Constants
            float scale = _model.Size == UnitSize.Small 
                ? Constants.SMALL_UNIT_SCALE 
                : Constants.BIG_UNIT_SCALE;
            transform.localScale = Vector3.one * scale;

            // Position health bar using Constants
            if (_healthBarTransform != null)
            {
                Vector3 offset = _healthBarTransform.localPosition;
                offset.y = Constants.HEALTHBAR_OFFSET;
                _healthBarTransform.localPosition = offset;
            }
        }

        private void Update()
        {
            if (_model != null && _model.IsAlive)
            {
                // Pulse effect when low health
                float healthPercent = _model.CurrentHP / _model.Stats.HP;
                if (healthPercent < 0.3f)
                {
                    // Pulse between base color and brighter version
                    float pulse = Mathf.PingPong(Time.time * 2f, 1f);
                    Color pulsedColor = Color.Lerp(_baseColor, Color.white, pulse * 0.3f);
                    _renderer.material.color = pulsedColor;
                }
                else
                {
                    _renderer.material.color = _baseColor;
                }
            }
        }

        private void UpdateHealthBar(float currentHP)
        {
            if (_healthBarTransform == null) return;

            float healthPercent = Mathf.Clamp01(currentHP / _model.Stats.HP);
            
            // Update scale
            Vector3 scale = _healthBarTransform.localScale;
            scale.x = healthPercent;
            _healthBarTransform.localScale = scale;

            // Update color based on health using color interpolation
            var healthBarRenderer = _healthBarTransform.GetComponent<Renderer>();
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
            // Death is handled by BattleViewManager
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnHealthChanged -= UpdateHealthBar;
                _model.OnDeath -= OnUnitDeath;
            }
        }

        private void OnMouseDown()
        {
            // Handle unit selection click
            Debug.Log($"[UnitView] Clicked on {_model.Team} {_model.Color} {_model.Shape}");
        }
    }
}
