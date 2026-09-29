using Godot;
using System.Collections.Generic;
using Joguim.Core;

namespace Joguim.Abilities
{
    public partial class AbilityManager : Node
    {
        public static AbilityManager Instance { get; private set; }

        private HashSet<AbilityId> _unlockedAbilities = new();

        public override void _Ready()
        {
            Instance = this;
            // Habilidades iniciais da demo: DoubleJump já vem liberado.
            // Entram direto no conjunto, sem emitir AbilityUnlocked (não é um desbloqueio de gameplay).
            _unlockedAbilities.Add(AbilityId.DoubleJump);

            EventBus.Instance.BossDefeated += OnBossDefeated;
        }

        // Progressão da demo: derrotar o Korrag libera o Dash.
        // UnlockAbility ignora repetição, então um BossDefeated duplicado não desbloqueia duas vezes.
        private void OnBossDefeated(string bossId)
        {
            if (bossId == "boss_javali") UnlockAbility(AbilityId.Dash);
        }

        public bool HasAbility(AbilityId abilityId)
        {
            return _unlockedAbilities.Contains(abilityId);
        }

        public void UnlockAbility(AbilityId abilityId)
        {
            if (_unlockedAbilities.Contains(abilityId)) return;

            _unlockedAbilities.Add(abilityId);
            EventBus.Instance.EmitSignal("AbilityUnlocked", abilityId.ToString());
            GD.Print($"Ability unlocked: {abilityId}");
        }

        public List<AbilityId> GetAllUnlockedAbilities()
        {
            return new List<AbilityId>(_unlockedAbilities);
        }

        public void SetUnlockedAbilities(List<AbilityId> abilities)
        {
            _unlockedAbilities.Clear();
            foreach (var ability in abilities)
            {
                _unlockedAbilities.Add(ability);
            }
        }
    }
}
