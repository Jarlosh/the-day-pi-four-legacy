namespace Game.Client
{
    public record OnSelectionChangedEvent(IInteractable Interactable, IInteractable Previous)
    {
    }
}