using UnityEngine;
using Zenject;

namespace ArmyClash.Core
{
    /// <summary>
    /// Controls camera movement and zoom
    /// Allows player to pan and zoom during battle
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _panSpeed = 20f;
        [SerializeField] private float _zoomSpeed = 10f;
        [SerializeField] private float _rotationSpeed = 100f;

        [Header("Limits")]
        [SerializeField] private Vector2 _panLimitX = new Vector2(-20f, 20f);
        [SerializeField] private Vector2 _panLimitZ = new Vector2(-20f, 20f);
        [SerializeField] private Vector2 _zoomLimit = new Vector2(5f, 30f);

        [Header("Input")]
        [SerializeField] private bool _enableEdgePanning = true;
        [SerializeField] private float _edgePanningThreshold = 20f;
        [SerializeField] private bool _enableKeyboardControls = true;
        [SerializeField] private bool _enableMouseControls = true;

        private Vector3 _targetPosition;
        private float _targetZoom;
        private bool _isDragging;
        private Vector3 _lastMousePosition;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            _targetPosition = transform.position;
            _targetZoom = _camera.orthographicSize;
        }

        private void Update()
        {
            HandleKeyboardInput();
            HandleMouseInput();
            HandleEdgePanning();
            HandleZoom();

            UpdateCameraPosition();
        }

        private void HandleKeyboardInput()
        {
            if (!_enableKeyboardControls) return;

            Vector3 movement = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                movement.z += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                movement.z -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                movement.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                movement.x += 1;

            if (movement != Vector3.zero)
            {
                _targetPosition += movement.normalized * _panSpeed * Time.deltaTime;
                ClampPosition();
            }
        }

        private void HandleMouseInput()
        {
            if (!_enableMouseControls) return;

            // Middle mouse button drag
            if (Input.GetMouseButtonDown(2))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(2))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                _targetPosition -= new Vector3(delta.x, 0, delta.y) * 0.05f;
                _lastMousePosition = Input.mousePosition;
                ClampPosition();
            }
        }

        private void HandleEdgePanning()
        {
            if (!_enableEdgePanning) return;

            Vector3 movement = Vector3.zero;
            Vector3 mousePos = Input.mousePosition;

            if (mousePos.x < _edgePanningThreshold)
                movement.x -= 1;
            if (mousePos.x > Screen.width - _edgePanningThreshold)
                movement.x += 1;
            if (mousePos.y < _edgePanningThreshold)
                movement.z -= 1;
            if (mousePos.y > Screen.height - _edgePanningThreshold)
                movement.z += 1;

            if (movement != Vector3.zero)
            {
                _targetPosition += movement.normalized * _panSpeed * Time.deltaTime;
                ClampPosition();
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                _targetZoom -= scroll * _zoomSpeed;
                _targetZoom = Mathf.Clamp(_targetZoom, _zoomLimit.x, _zoomLimit.y);
            }
        }

        private void UpdateCameraPosition()
        {
            // Smooth movement
            transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * 5f);

            // Smooth zoom (for orthographic camera)
            if (_camera.orthographic)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetZoom, Time.deltaTime * 5f);
            }
            else
            {
                // For perspective camera, move closer/further
                Vector3 pos = transform.position;
                pos.y = Mathf.Lerp(pos.y, _targetZoom, Time.deltaTime * 5f);
                transform.position = pos;
            }
        }

        private void ClampPosition()
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _panLimitX.x, _panLimitX.y);
            _targetPosition.z = Mathf.Clamp(_targetPosition.z, _panLimitZ.x, _panLimitZ.y);
        }

        /// <summary>
        /// Focus camera on a specific position
        /// </summary>
        public void FocusOn(Vector3 position, float zoom = -1)
        {
            _targetPosition = position;
            ClampPosition();

            if (zoom > 0)
            {
                _targetZoom = Mathf.Clamp(zoom, _zoomLimit.x, _zoomLimit.y);
            }
        }

        /// <summary>
        /// Reset camera to default position
        /// </summary>
        public void ResetCamera()
        {
            _targetPosition = new Vector3(0, transform.position.y, -10);
            _targetZoom = 15f;
        }

        /// <summary>
        /// Enable/disable camera controls
        /// </summary>
        public void SetControlsEnabled(bool enabled)
        {
            _enableKeyboardControls = enabled;
            _enableMouseControls = enabled;
            _enableEdgePanning = enabled;
        }
    }
}