namespace Interactables.Events
{
    public record OnSelectionChangedEvent(IInteractable Interactable, IInteractable Previous);
}