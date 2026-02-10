using UnityEngine;
using UnityEngine.UI;

namespace ArmyClash.View
{
    /// <summary>
    /// Visual representation of unit health bar
    /// Can use either Transform scale or UI Image fill
    /// </summary>
    public class HealthBarView : MonoBehaviour
    {
        [Header("Display Type")]
        [SerializeField] private HealthBarType _barType = HealthBarType.Transform;
        
        [Header("Transform-based (World Space)")]
        [SerializeField] private Transform _barTransform;
        [SerializeField] private SpriteRenderer _barRenderer;
        [SerializeField] private Color _healthyColor = Color.green;
        [SerializeField] private Color _damagedColor = Color.yellow;
        [SerializeField] private Color _criticalColor = Color.red;
        
        [Header("UI-based (Canvas)")]
        [SerializeField] private Image _fillImage;
        
        [Header("Settings")]
        [SerializeField] private bool _hideWhenFull = true;
        [SerializeField] private Vector3 _offset = new Vector3(0, 1.5f, 0);

        private float _maxHealth;
        private Camera _mainCamera;

        private enum HealthBarType
        {
            Transform,  // Uses scale on a Transform (world space)
            UIImage     // Uses Image.fillAmount (UI canvas)
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        /// <summary>
        /// Initialize health bar with max HP value
        /// </summary>
        public void Initialize(float maxHealth)
        {
            _maxHealth = maxHealth;
            UpdateHealthBar(maxHealth);
        }

        /// <summary>
        /// Update health bar based on current HP
        /// </summary>
        public void UpdateHealthBar(float currentHealth)
        {
            float healthPercent = Mathf.Clamp01(currentHealth / _maxHealth);

            switch (_barType)
            {
                case HealthBarType.Transform:
                    UpdateTransformBar(healthPercent);
                    break;
                case HealthBarType.UIImage:
                    UpdateUIBar(healthPercent);
                    break;
            }

            // Hide bar if at full health
            if (_hideWhenFull && healthPercent >= 1f)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        private void UpdateTransformBar(float healthPercent)
        {
            if (_barTransform != null)
            {
                Vector3 scale = _barTransform.localScale;
                scale.x = healthPercent;
                _barTransform.localScale = scale;
            }

            // Update color based on health
            if (_barRenderer != null)
            {
                _barRenderer.color = GetHealthColor(healthPercent);
            }
        }

        private void UpdateUIBar(float healthPercent)
        {
            if (_fillImage != null)
            {
                _fillImage.fillAmount = healthPercent;
                _fillImage.color = GetHealthColor(healthPercent);
            }
        }

        private Color GetHealthColor(float healthPercent)
        {
            if (healthPercent > 0.6f)
                return _healthyColor;
            else if (healthPercent > 0.3f)
                return _damagedColor;
            else
                return _criticalColor;
        }

        private void LateUpdate()
        {
            // Billboard effect - always face camera (for world space bars)
            if (_barType == HealthBarType.Transform && _mainCamera != null)
            {
                transform.LookAt(transform.position + _mainCamera.transform.rotation * Vector3.forward,
                                _mainCamera.transform.rotation * Vector3.up);
            }
        }

        /// <summary>
        /// Set position offset relative to unit
        /// </summary>
        public void SetOffset(Vector3 offset)
        {
            _offset = offset;
            transform.localPosition = _offset;
        }
    }
}
