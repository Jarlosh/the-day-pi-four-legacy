namespace Game.Client
{
    public interface IInteractable
    {
        bool CanInteract();
        void Interact();
        string GetDescriptionKey();
    }
}