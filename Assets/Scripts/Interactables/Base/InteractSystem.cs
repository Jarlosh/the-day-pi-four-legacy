using Tools.Structure;

namespace Interactables
{
    public class InteractSystem: Singleton<InteractSystem>
    {
        public void OnInteractInput(IInteractable interactable)
        {
            if (interactable.CanInteract())
            {
                interactable.Interact();
            }
        }
    }
}