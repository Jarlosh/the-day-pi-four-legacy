using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private InputActionReference _moveInputAction;
        
        private bool _isGrounded;
        private Vector2 _moveInput;

        private void Awake()
        {
            _moveInputAction.action.performed += OnMovePerformed;
            _moveInputAction.action.canceled += OnMoveCanceled;
        }

        private void OnDestroy()
        {
            _moveInputAction.action.performed -= OnMovePerformed;
            _moveInputAction.action.canceled -= OnMoveCanceled;
        }

        private void OnMoveCanceled(InputAction.CallbackContext _)
        {
            _moveInput = Vector2.zero;
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
        }

        private void Update()
        {
            if (_moveInput.sqrMagnitude == 0)
            {
                return;
            }
            
            var direction = transform.right * _moveInput.x + transform.forward * _moveInput.y;
            _characterController.Move(direction * (_moveSpeed * Time.deltaTime));
        }
    }
}