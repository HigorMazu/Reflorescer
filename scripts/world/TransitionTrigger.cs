using Godot;
using Joguim.Core;

namespace Joguim.World
{
    public partial class TransitionTrigger : Area2D
    {
        [Export] public string TargetScene = "";
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
                SceneManager.Instance.LoadScene(TargetScene);
            }
            else
            {
                GD.PrintErr($"TransitionTrigger: Scene not found at {TargetScene}");
            }
        }
    }
}
