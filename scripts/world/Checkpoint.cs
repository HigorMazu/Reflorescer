using Godot;
using Joguim.Core;
using Joguim.Interaction;
using Joguim.Save;

namespace Joguim.World
{
    public partial class Checkpoint : Area2D, IInteractable
    {
        [Export] public string CheckpointId = "checkpoint_01";
        [Export] public bool Activated = false;
        [Export] public NodePath SpritePath;
        [Export] public NodePath ActivationEffectPath;
        // Ativa sozinha quando o Kairo passa por ela (sem precisar do E). Usado na bandeira única do Dossel Vivo
        // (decisão de 11/10/2026): garante que ninguém pule a bandeira e volte pro começo da demo ao morrer.
        [Export] public bool AutoActivateOnTouch = false;

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

            // Voltar pra cena (ou renascer nela) não "desativa" a bandeira: o estado vem do save da sessão
            if (SaveManager.Instance != null && SaveManager.Instance.IsCheckpointActivated(CheckpointId))
            {
                Activated = true;
            }

            if (AutoActivateOnTouch) BodyEntered += OnBodyEntered;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (!Activated && body.IsInGroup("Player")) Interact(body);
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
