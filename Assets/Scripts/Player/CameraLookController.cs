using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
    public class CameraLookController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private InputActionReference _cameraInputAction;

        [SerializeField] private float _sensitivityX = 1f;
        [SerializeField] private float _sensitivityY = 1f;
        [SerializeField] private float _minPitch = -45f;
        [SerializeField] private float _maxPitch = 45f;

        private Vector2 _lookInput;

        private float _yAxisRotation;
        private float _xAxisRotation;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            InitState();
            
            _cameraInputAction.action.performed += OnInputPerformed;
            _cameraInputAction.action.canceled += OnInputCancelled;
        }

        private void OnDestroy()
        {
            _cameraInputAction.action.performed -= OnInputPerformed;
            _cameraInputAction.action.canceled -= OnInputCancelled;
        }

        private void InitState()
        {
            _yAxisRotation = transform.rotation.eulerAngles.y;

            if (_cameraTransform != null)
            {
                _xAxisRotation = _cameraTransform.localRotation.eulerAngles.x;
            }
        }

        private void OnInputPerformed(InputAction.CallbackContext ctx)
        {
            _lookInput = ctx.ReadValue<Vector2>();
        }

        private void OnInputCancelled(InputAction.CallbackContext _)
        {
            _lookInput = Vector2.zero;
        }

        private void Update()
        {
            var mouseX = _lookInput.x * _sensitivityX * Time.deltaTime;
            var mouseY = _lookInput.y * _sensitivityY * Time.deltaTime;

            _yAxisRotation += mouseX;
            _xAxisRotation -= mouseY;
            _xAxisRotation = Mathf.Clamp(_xAxisRotation, _minPitch, _maxPitch);

            transform.rotation = Quaternion.Euler(0f, _yAxisRotation, 0f);
            
            if (_cameraTransform != null)
            {
                _cameraTransform.localRotation = Quaternion.Euler(_xAxisRotation, 0f, 0f);
            }
        }
    }
}