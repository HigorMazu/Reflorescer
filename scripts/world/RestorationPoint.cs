using Godot;
using Joguim.Core;
using Joguim.Interaction;
using Joguim.Save;

namespace Joguim.World
{
    // Ponto de restauração do bioma (ODS 15). Separado do Checkpoint de propósito:
    // restaurar não é salvar, e cada um tem seu próprio evento e persistência.
    public partial class RestorationPoint : Area2D, IInteractable
    {
        [Export] public string PointId = "restoration_01";
        [Export] public bool Restored = false;
        // "Antes/depois": nós que somem e nós que aparecem ao restaurar (placeholder hoje, arte do Higor depois)
        [Export] public Godot.Collections.Array<NodePath> DegradedVisuals = new();
        [Export] public Godot.Collections.Array<NodePath> RestoredVisuals = new();
        [Export] public Color ParticleColor = new Color(0.45f, 0.85f, 0.35f);

        public const string GroupName = "RestorationPoints";

        private const float FadeDuration = 0.6f;

        public override void _Ready()
        {
            AddToGroup(GroupName);

            // Voltar pra cena não "desrestaura": o estado vem do save da sessão
            if (SaveManager.Instance != null && SaveManager.Instance.IsPointRestored(PointId))
            {
                Restored = true;
            }
            ApplyVisualState(animate: false);
        }

        public void Interact(Node2D interactor)
        {
            if (Restored) return;

            Restored = true;
            GD.Print($"RestorationPoint {PointId} restaurado.");
            ApplyVisualState(animate: true);
            SpawnRestoreParticles();
            AudioManager.Instance?.PlaySfx("restoration.wav");
            EventBus.Instance?.EmitSignal(EventBus.SignalName.AreaRestored, PointId);
        }

        // Usado ao carregar um save: aplica o estado sem efeito e sem emitir evento
        public void SetRestoredWithoutEffect()
        {
            Restored = true;
            ApplyVisualState(animate: false);
        }

        public string GetInteractionPrompt()
        {
            return Restored ? "Área restaurada" : "Restaurar";
        }

        public bool CanInteract()
        {
            return !Restored;
        }

        private void ApplyVisualState(bool animate)
        {
            foreach (var path in DegradedVisuals)
            {
                if (GetNodeOrNull<CanvasItem>(path) is not CanvasItem degraded) continue;
                if (!animate)
                {
                    degraded.Visible = !Restored;
                    continue;
                }
                var tween = CreateTween();
                tween.TweenProperty(degraded, "modulate:a", 0.0f, FadeDuration);
                tween.TweenCallback(Callable.From(() => degraded.Visible = false));
            }

            foreach (var path in RestoredVisuals)
            {
                if (GetNodeOrNull<CanvasItem>(path) is not CanvasItem restored) continue;
                restored.Visible = Restored;
                if (!animate) continue;

                // brota de baixo pra cima
                restored.Modulate = new Color(restored.Modulate, 0f);
                var tween = CreateTween().SetParallel();
                tween.TweenProperty(restored, "modulate:a", 1.0f, FadeDuration);
                if (restored is Control control)
                {
                    control.PivotOffset = new Vector2(control.Size.X / 2f, control.Size.Y);
                    control.Scale = new Vector2(1f, 0f);
                    tween.TweenProperty(control, "scale", Vector2.One, FadeDuration)
                        .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                }
                else if (restored is Node2D node2D)
                {
                    Vector2 finalScale = node2D.Scale;
                    node2D.Scale = new Vector2(finalScale.X, 0f);
                    tween.TweenProperty(node2D, "scale", finalScale, FadeDuration)
                        .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                }
            }
        }

        private void SpawnRestoreParticles()
        {
            var particles = new CpuParticles2D
            {
                OneShot = true,
                Emitting = false,
                Amount = 28,
                Lifetime = 0.9,
                Explosiveness = 0.85f,
                Direction = Vector2.Up,
                Spread = 55f,
                InitialVelocityMin = 70f,
                InitialVelocityMax = 150f,
                Gravity = new Vector2(0, 220f),
                ScaleAmountMin = 2f,
                ScaleAmountMax = 4f,
                Color = ParticleColor,
                Position = new Vector2(0, -8f)
            };
            AddChild(particles);
            particles.Emitting = true;
            GetTree().CreateTimer(particles.Lifetime + 0.2).Timeout += () =>
            {
                if (IsInstanceValid(particles)) particles.QueueFree();
            };
        }
    }
}
