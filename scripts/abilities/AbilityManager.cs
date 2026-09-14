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
            // Prototype: libera DoubleJump imediatamente para testar plataformas
            CallDeferred(MethodName.UnlockPrototypeAbilities);
        }

        private void UnlockPrototypeAbilities()
        {
            UnlockAbility(AbilityId.DoubleJump);
            // Dash pode ser liberado depois facilmente
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
