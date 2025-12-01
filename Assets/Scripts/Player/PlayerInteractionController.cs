using Interactables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
    public class PlayerInteractionController: MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private InputActionReference _inputAction;
        [SerializeField] private float _castDistance = 5;
        [SerializeField] private LayerMask _mask;
        

        private void Awake()
        {
            _inputAction.action.performed += OnInputPerformed;
        }

        private void OnDestroy()
        {
            _inputAction.action.performed -= OnInputPerformed;
        }

        private void OnInputPerformed(InputAction.CallbackContext _)
        {
            TryInteract();
        }

        private void TryInteract()
        {
            if (!Raycast(out var hit))
            {
                return;
            }

            if (hit.collider.gameObject.TryGetComponent<IInteractable>(out var interactable))
            {
                // todo: use eventbus
                InteractSystem.Instance.OnInteractInput(interactable);
            }
        }

        private bool Raycast(out RaycastHit hit)
        {
            return Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _castDistance, _mask);
        }
    }
}