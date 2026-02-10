using UnityEngine;
using Zenject;
using ArmyClash.Core;

namespace ArmyClash.View
{
    /// <summary>
    /// Visual representation of a unit - MonoBehaviour attached to GameObject
    /// </summary>
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _healthBarTransform;

        private Unit _model;
        private GameConfig _config;

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
            // Set color
            if (_renderer != null)
            {
                _renderer.material.color = _config.GetColorValue(_model.Color);
            }

            // Set scale based on size
            transform.localScale = Vector3.one * _config.GetSizeScale(_model.Size);
        }

        private void Update()
        {
            if (_model != null && _model.IsAlive)
            {
                // Sync position from model
                _model.Position = transform.position;
            }
        }

        private void UpdateHealthBar(float currentHP)
        {
            if (_healthBarTransform != null)
            {
                float healthPercent = currentHP / _model.Stats.HP;
                _healthBarTransform.localScale = new Vector3(healthPercent, 1, 1);
            }
        }

        private void OnUnitDeath(Unit unit)
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_model != null)
            {
                _model.OnHealthChanged -= UpdateHealthBar;
                _model.OnDeath -= OnUnitDeath;
            }
        }

        public class Factory : PlaceholderFactory<Unit, UnitView>
        {
        }
    }
}
