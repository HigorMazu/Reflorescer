using Godot;
using Joguim.Core;
using Joguim.Interaction;

namespace Joguim.World
{
    public partial class Checkpoint : Area2D, IInteractable
    {
        [Export] public string CheckpointId = "checkpoint_01";
        [Export] public bool Activated = false;
        [Export] public NodePath SpritePath;
        [Export] public NodePath ActivationEffectPath;

        private AnimatedSprite2D _sprite;
        private GpuParticles2D _activationEffect;

        public override void _Ready()
        {
            _sprite = GetNodeOrNull<AnimatedSprite2D>(SpritePath);
            _activationEffect = GetNodeOrNull<GpuParticles2D>(ActivationEffectPath);

            if (_activationEffect != null)
            {
                _activationEffect.Visible = false;
            }

            AddToGroup("Checkpoints");
        }

        public void Interact(Node2D interactor)
        {
            if (Activated) return;

            Activated = true;

            if (_sprite != null)
            {
                _sprite.Play("activate");
            }

            if (_activationEffect != null)
            {
                _activationEffect.Visible = true;
                _activationEffect.Restart();
            }

            EventBus.Instance.EmitSignal(
                "CheckpointActivated",
                GlobalPosition,
                CheckpointId
            );

            AudioManager.Instance?.PlaySfx("checkpoint_activate.wav");
        }

        public string GetInteractionPrompt()
        {
            return Activated ? "Checkpoint ativado" : "Ativar checkpoint";
        }

        // Checkpoint já ativado não oferece interação (o Interact já ignorava; agora o prompt também some)
        public bool CanInteract()
        {
            return !Activated;
        }

        public void Deactivate()
        {
            Activated = false;

            if (_sprite != null)
            {
                _sprite.Play("idle");
            }

            if (_activationEffect != null)
            {
                _activationEffect.Visible = false;
            }
        }
    }
}
