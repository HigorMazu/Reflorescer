using Godot;

namespace Joguim.Interaction
{
    public interface IInteractable
    {
        void Interact(Node2D interactor);
        string GetInteractionPrompt();
        bool CanInteract();
    }
}
