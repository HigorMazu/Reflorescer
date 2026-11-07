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
        // Portal fechado enquanto esse chefe estiver vivo (ex.: não sair no meio da luta com o Korrag). Vazio = sempre aberto.
        [Export] public string LockedUntilBossDefeated = "";
        [Export] public NodePath VisualPath = "PortalVisual";

        private CanvasItem _visual;
        // Hitbox da transição (06/11/2026): o portal reage ao CORPO do Kairo (CharacterBody2D, 24×40), não à
        // Hurtbox (22×66, vai 26 px acima da cabeça) — com ela trocava cedo, e no Dossel o spawn vindo do
        // Igarapé caía dentro do portal de volta. Quem nasce dentro do portal só dispara depois de sair dele.
        private const float ArmDelay = 0.25f;
        private float _armTimer = ArmDelay;
        private bool _waitForExit = false;
        private bool _transitioning = false;

        public bool IsLocked => !string.IsNullOrEmpty(LockedUntilBossDefeated)
            && Save.SaveManager.Instance?.IsBossDefeated(LockedUntilBossDefeated) != true;

        public override void _Ready()
        {
            _visual = GetNodeOrNull<CanvasItem>(VisualPath);
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
            if (!string.IsNullOrEmpty(LockedUntilBossDefeated) && EventBus.Instance != null)
                EventBus.Instance.BossDefeated += OnBossDefeated;
            UpdateVisual();
        }

        public override void _ExitTree()
        {
            if (!string.IsNullOrEmpty(LockedUntilBossDefeated) && EventBus.Instance != null)
                EventBus.Instance.BossDefeated -= OnBossDefeated;
        }

        // O SaveManager registra a derrota no mesmo sinal; adiado pra ler o estado já atualizado
        private void OnBossDefeated(string bossId) => CallDeferred(MethodName.OnUnlocked);

        // Abriu com o Kairo já parado dentro do portal: BodyEntered não dispara de novo, então confere aqui
        private void OnUnlocked()
        {
            UpdateVisual();
            if (!IsLocked && _armTimer <= 0 && !_waitForExit && PlayerInside()) RequestTransition();
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_armTimer <= 0) return;
            _armTimer -= (float)delta;
            // Fim da carência: se o Kairo nasceu (ou foi teleportado pro spawn) dentro do portal, espera ele sair
            if (_armTimer <= 0 && PlayerInside()) _waitForExit = true;
        }

        private bool PlayerInside()
        {
            foreach (var body in GetOverlappingBodies())
                if (body.IsInGroup("Player")) return true;
            return false;
        }

        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("Player")) _waitForExit = false;
        }

        private void UpdateVisual()
        {
            if (_visual != null) _visual.Modulate = new Color(1, 1, 1, IsLocked ? 0.25f : 1f);
        }

        private void OnBodyEntered(Node2D body)
        {
            if (!body.IsInGroup("Player") || IsLocked || _armTimer > 0 || _waitForExit) return;
            RequestTransition();
        }

        private void RequestTransition()
        {
            if (_transitioning) return;
            _transitioning = true;
            // Adiado: trocar de cena dentro do callback de física remove CollisionObjects no meio dele (erro do Godot)
            CallDeferred(MethodName.TransitionToTarget);
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
