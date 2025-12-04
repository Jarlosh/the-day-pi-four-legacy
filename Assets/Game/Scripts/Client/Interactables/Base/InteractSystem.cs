using Game.Shared;

namespace Game.Client
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