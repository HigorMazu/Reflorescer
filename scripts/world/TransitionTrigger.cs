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

        public bool IsLocked => !string.IsNullOrEmpty(LockedUntilBossDefeated)
            && Save.SaveManager.Instance?.IsBossDefeated(LockedUntilBossDefeated) != true;

        public override void _Ready()
        {
            _visual = GetNodeOrNull<CanvasItem>(VisualPath);
            AreaEntered += OnAreaEntered;
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
        private void OnBossDefeated(string bossId) => CallDeferred(MethodName.UpdateVisual);

        private void UpdateVisual()
        {
            if (_visual != null) _visual.Modulate = new Color(1, 1, 1, IsLocked ? 0.25f : 1f);
        }

        private void OnAreaEntered(Area2D area)
        {
            if (IsLocked) return;
            if (area.GetParent().IsInGroup("Player"))
            {
                // Adiado: trocar de cena dentro do callback de física remove CollisionObjects no meio dele (erro do Godot)
                CallDeferred(MethodName.TransitionToTarget);
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
