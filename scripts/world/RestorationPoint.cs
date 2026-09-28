using Godot;
using Joguim.Interaction;

namespace Joguim.World
{
    // Ponto de restauração do bioma (ODS 15). Separado do Checkpoint de propósito:
    // restaurar não é salvar, e cada um tem seu próprio evento e persistência.
    public partial class RestorationPoint : Area2D, IInteractable
    {
        [Export] public string PointId = "restoration_01";
        [Export] public bool Restored = false;

        public const string GroupName = "RestorationPoints";

        public override void _Ready()
        {
            AddToGroup(GroupName);
        }

        public void Interact(Node2D interactor)
        {
            if (Restored) return;

            Restored = true;
            GD.Print($"RestorationPoint {PointId} restaurado.");
            // C02-T2: efeito visual/de mundo entra aqui
            // C02-T3: evento AreaRestored no EventBus + persistência no save entram aqui
        }

        public string GetInteractionPrompt()
        {
            return Restored ? "Área restaurada" : "Restaurar";
        }

        public bool CanInteract()
        {
            return !Restored;
        }
    }
}
