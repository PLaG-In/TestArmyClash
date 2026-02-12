using UnityEngine;
using Zenject;
using ArmyClash.Core;

namespace ArmyClash.Core
{
    public class CameraController : MonoBehaviour
    {
        [Header("Camera Reference")]
        [SerializeField] private Camera _camera;

        [Header("Movement")]
        [SerializeField] private float _panSpeed = 20f;
        [SerializeField] private float _mouseDragSpeed = 0.5f;
        [SerializeField] private float _touchDragSpeed = 0.05f;
        [SerializeField] private float _edgePanSpeed = 15f;
        [SerializeField] private float _edgePanThreshold = 20f;
        [SerializeField] private float _smoothTime = 0.15f;

        [Header("Zoom")]
        [SerializeField] private float _scrollZoomSpeed = 5f;
        [SerializeField] private float _pinchZoomSpeed = 0.05f;
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 30f;

        [Header("Rotation")]
        [SerializeField] private bool _enableRotation = true;
        [SerializeField] private float _keyRotateSpeed = 100f;
        [SerializeField] private float _touchRotateSpeed = 0.3f;

        [Header("Boundaries")]
        [SerializeField] private Vector2 _limitX = new Vector2(-25f, 25f);
        [SerializeField] private Vector2 _limitZ = new Vector2(-25f, 25f);
        [SerializeField] private Vector2 _heightLimit = new Vector2(5f, 30f);

        [Header("Input Toggles")]
        [SerializeField] private bool _enableKeyboard = true;
        [SerializeField] private bool _enableMouseDrag = true;
        [SerializeField] private bool _enableEdgePan = true;
        [SerializeField] private bool _enableScrollZoom = true;

        // --- state ---
        private Vector3 _targetPos;
        private float _targetZoom;
        private float _targetRotY;
        private bool _controlsEnabled;

        // smoothing
        private Vector3 _posVel;
        private float _zoomVel;
        private float _rotVel;

        // desktop drag
        private bool _isDragging;
        private Vector3 _lastMousePos;

        // touch previous frame
        private float _prevPinchDist;
        private float _prevTouchAngle;
        private Vector2 _prevMidpoint;

        private BattleState _battleState;

        [Inject]
        public void Construct(BattleState battleState)
        {
            _battleState = battleState;
        }

        private void Awake()
        {
            _targetPos = transform.position;
            _targetRotY = transform.eulerAngles.y;
            _targetZoom = _camera.orthographic
                ? _camera.orthographicSize
                : transform.position.y;
        }

        private void Start()
        {
            _battleState.OnBattleStarted += OnBattleStarted;
            _battleState.OnBattleStopped += OnBattleStopped;
        }

        private void Update()
        {
            if (!_controlsEnabled) return;

            // On actual devices: touch only. In Editor / standalone: both.
            bool hasMouse = Input.mousePresent;
            bool hasTouch = Input.touchSupported && Input.touchCount > 0;

            if (hasMouse && !hasTouch)
            {
                HandleKeyboard();
                HandleMouseDrag();
                HandleEdgePan();
                HandleScrollZoom();
                HandleKeyRotation();
                HandleResetKey();
            }
            else
            {
                HandleTouch();
            }

            ApplySmoothing();
        }

        private void OnBattleStarted()
        {
            SetControlsEnabled(true);
        }

        private void OnBattleStopped()
        {
            SetControlsEnabled(false);
        }

        // ================================================================
        // DESKTOP INPUT
        // ================================================================

        private void HandleKeyboard()
        {
            if (!_enableKeyboard) return;

            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dir += Vector3.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dir += Vector3.back;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dir += Vector3.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir += Vector3.right;
            if (dir == Vector3.zero) return;

            dir = Quaternion.Euler(0, _targetRotY, 0) * dir.normalized;
            _targetPos += dir * _panSpeed * Time.deltaTime;
            Clamp();
        }

        private void HandleMouseDrag()
        {
            if (!_enableMouseDrag) return;

            // Middle mouse or Right mouse
            bool pressed = Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1);
            bool released = Input.GetMouseButtonUp(2) || Input.GetMouseButtonUp(1);

            if (pressed) { _isDragging = true; _lastMousePos = Input.mousePosition; }
            if (released) { _isDragging = false; }

            if (!_isDragging) return;

            Vector3 delta = Input.mousePosition - _lastMousePos;
            Vector3 move = new Vector3(-delta.x, 0, -delta.y) * _mouseDragSpeed;
            move = Quaternion.Euler(0, _targetRotY, 0) * move;
            _targetPos += move;
            _lastMousePos = Input.mousePosition;
            Clamp();
        }

        private void HandleEdgePan()
        {
            if (!_enableEdgePan) return;

            Vector3 dir = Vector3.zero;
            Vector3 mp = Input.mousePosition;
            if (mp.x < _edgePanThreshold) dir += Vector3.left;
            if (mp.x > Screen.width - _edgePanThreshold) dir += Vector3.right;
            if (mp.y < _edgePanThreshold) dir += Vector3.back;
            if (mp.y > Screen.height - _edgePanThreshold) dir += Vector3.forward;
            if (dir == Vector3.zero) return;

            dir = Quaternion.Euler(0, _targetRotY, 0) * dir.normalized;
            _targetPos += dir * _edgePanSpeed * Time.deltaTime;
            Clamp();
        }

        private void HandleScrollZoom()
        {
            if (!_enableScrollZoom) return;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll == 0) return;
            _targetZoom -= scroll * _scrollZoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
        }

        private void HandleKeyRotation()
        {
            if (!_enableRotation) return;
            float dir = 0;
            if (Input.GetKey(KeyCode.Q)) dir = -1;
            if (Input.GetKey(KeyCode.E)) dir = 1;
            if (dir == 0) return;
            _targetRotY += dir * _keyRotateSpeed * Time.deltaTime;
            _targetRotY = Mathf.Repeat(_targetRotY, 360f);
        }

        private void HandleResetKey()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Home))
                ResetCamera();
        }

        // ================================================================
        // MOBILE TOUCH INPUT
        // ================================================================

        private void HandleTouch()
        {
            int count = Input.touchCount;

            if (count == 0)
            {
                ResetTouchState();
                return;
            }

            if (count == 1)
            {
                HandleSingleTouch();
                ResetTouchState();   // clear multi-touch memory
            }
            else
            {
                HandleMultiTouch();
            }
        }

        private void HandleSingleTouch()
        {
            Touch t = Input.GetTouch(0);
            if (t.phase != TouchPhase.Moved) return;

            Vector3 move = new Vector3(-t.deltaPosition.x, 0, -t.deltaPosition.y) * _touchDragSpeed;
            move = Quaternion.Euler(0, _targetRotY, 0) * move;
            _targetPos += move;
            Clamp();
        }

        private void HandleMultiTouch()
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 mid = (t0.position + t1.position) * 0.5f;
            float dist = Vector2.Distance(t0.position, t1.position);
            float angle = Mathf.Atan2(
                t1.position.y - t0.position.y,
                t1.position.x - t0.position.x) * Mathf.Rad2Deg;

            bool isFirstFrame = _prevPinchDist == 0;

            if (!isFirstFrame)
            {
                // Pinch-to-zoom
                float deltaDist = dist - _prevPinchDist;
                _targetZoom -= deltaDist * _pinchZoomSpeed;
                _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);

                // Two-finger pan
                Vector2 midDelta = mid - _prevMidpoint;
                Vector3 move = new Vector3(-midDelta.x, 0, -midDelta.y) * _touchDragSpeed;
                move = Quaternion.Euler(0, _targetRotY, 0) * move;
                _targetPos += move;
                Clamp();

                // Twist-to-rotate
                if (_enableRotation)
                {
                    float deltaAngle = Mathf.DeltaAngle(_prevTouchAngle, angle);
                    _targetRotY -= deltaAngle * _touchRotateSpeed;
                    _targetRotY = Mathf.Repeat(_targetRotY, 360f);
                }
            }

            _prevPinchDist = dist;
            _prevTouchAngle = angle;
            _prevMidpoint = mid;
        }

        private void ResetTouchState()
        {
            _prevPinchDist = 0;
            _prevTouchAngle = 0;
            _prevMidpoint = Vector2.zero;
        }

        // ================================================================
        // SMOOTHING
        // ================================================================

        private void ApplySmoothing()
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, _targetPos, ref _posVel, _smoothTime);

            float newY = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, _targetRotY, ref _rotVel, _smoothTime);
            Vector3 e = transform.eulerAngles;
            e.y = newY;
            transform.eulerAngles = e;

            if (_camera.orthographic)
            {
                _camera.orthographicSize = Mathf.SmoothDamp(
                    _camera.orthographicSize, _targetZoom, ref _zoomVel, _smoothTime);
            }
            else
            {
                Vector3 p = transform.position;
                p.y = Mathf.SmoothDamp(p.y, _targetZoom, ref _zoomVel, _smoothTime);
                transform.position = p;
                _targetPos.y = p.y;
            }
        }

        private void Clamp()
        {
            _targetPos.x = Mathf.Clamp(_targetPos.x, _limitX.x, _limitX.y);
            _targetPos.z = Mathf.Clamp(_targetPos.z, _limitZ.x, _limitZ.y);
            if (!_camera.orthographic)
                _targetPos.y = Mathf.Clamp(_targetPos.y, _heightLimit.x, _heightLimit.y);
        }

        // ================================================================
        // PUBLIC API
        // ================================================================

        public void FocusOn(Vector3 position, float? zoom = null)
        {
            _targetPos = position; Clamp();
            if (zoom.HasValue) _targetZoom = Mathf.Clamp(zoom.Value, _minZoom, _maxZoom);
        }

        public void FocusOnBattlefield() => FocusOn(Vector3.zero, 15f);

        public void ResetCamera()
        {
            _targetPos = new Vector3(0, transform.position.y, -10f);
            _targetZoom = 15f;
            _targetRotY = 0f;
        }

        public void SetControlsEnabled(bool enabled) { _controlsEnabled = enabled; _isDragging = false; }
        public void SetZoom(float z) => _targetZoom = Mathf.Clamp(z, _minZoom, _maxZoom);
        public void SetRotation(float deg) => _targetRotY = Mathf.Repeat(deg, 360f);

        // ================================================================
        // GIZMOS
        // ================================================================

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var a = new Vector3(_limitX.x, 0, _limitZ.x);
            var b = new Vector3(_limitX.y, 0, _limitZ.x);
            var c = new Vector3(_limitX.y, 0, _limitZ.y);
            var d = new Vector3(_limitX.x, 0, _limitZ.y);
            Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d); Gizmos.DrawLine(d, a);
        }

        // ================================================================
        // DESTROY
        // ================================================================

        private void OnDestroy()
        {
            _battleState.OnBattleStarted -= OnBattleStarted;
            _battleState.OnBattleStopped -= OnBattleStopped;
        }
    }
}