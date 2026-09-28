using Godot;
using Joguim.Core;

namespace Joguim.World
{
    public partial class TransitionTrigger : Area2D
    {
        [Export] public string TargetScene = "";
        // Nome do spawn point (Marker2D no grupo "SpawnPoints") na cena de destino. Vazio = spawn padrão da cena.
        [Export] public string TargetArea = "";
        [Export] public Vector2 SpawnOffset = Vector2.Zero;

        public override void _Ready()
        {
            AreaEntered += OnAreaEntered;
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area.GetParent().IsInGroup("Player"))
            {
                TransitionToTarget();
            }
        }

        private void TransitionToTarget()
        {
            if (string.IsNullOrEmpty(TargetScene))
            {
                GD.PrintErr("TransitionTrigger: TargetScene not set.");
                return;
            }

            if (ResourceLoader.Exists(TargetScene))
            {
                SceneManager.Instance.LoadSceneAtSpawn(TargetScene, TargetArea, SpawnOffset);
            }
            else
            {
                GD.PrintErr($"TransitionTrigger: Scene not found at {TargetScene}");
            }
        }
    }
}
