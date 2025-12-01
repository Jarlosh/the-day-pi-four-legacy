using Interactables;
using Interactables.Events;
using Tools.Structure;
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
        
        private GameObject _lastHoveredObject;
        private IInteractable _lastInteractable;

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

        private void Update()
        {
            if (!Raycast(out var hit))
            {
                Unselect();
                return;
            }

            var go = hit.collider.gameObject;
            if (_lastHoveredObject == go)
            {
                return;
            }

            _lastHoveredObject = go;

            if (!hit.collider.gameObject.TryGetComponent<IInteractable>(out var interactable))
            {
                Unselect();
                return;
            }
            
            Select(interactable);
        }

        private void Unselect()
        {
            Select(null);
        }

        private void Select(IInteractable interactable)
        {
            if (_lastInteractable != interactable)
            {
                var last = _lastInteractable;
                _lastInteractable = interactable;
                EventBus.Instance.Publish(new OnSelectionChangedEvent(_lastInteractable, last));
            }
        }

        private void TryInteract()
        {
            if(_lastInteractable != null)
            {
                InteractSystem.Instance.OnInteractInput(_lastInteractable);
            }
        }

        private bool Raycast(out RaycastHit hit)
        {
            return Physics.Raycast(_camera.transform.position, _camera.transform.forward, out hit, _castDistance, _mask);
        }
    }
}