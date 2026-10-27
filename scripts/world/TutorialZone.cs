using Godot;
using Joguim.UI;

namespace Joguim.World
{
    /// <summary>
    /// Trecho do cenário que mostra uma dica no topo da tela enquanto o Kairo está dentro (26/10/2026).
    /// Ex.: Keys = "K", Text = "atacar com a Espada de Grama". Usar collision_mask 2 (Player).
    /// </summary>
    public partial class TutorialZone : Area2D
    {
        [Export] public string Keys = "";
        [Export(PropertyHint.MultilineText)] public string Text = "";

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        private TutorialOverlay Overlay => GetTree().GetFirstNodeInGroup(TutorialOverlay.GroupName) as TutorialOverlay;

        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("Player")) Overlay?.ShowBanner(Name, Keys, Text);
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("Player")) Overlay?.HideBanner(Name);
        }
    }
}
