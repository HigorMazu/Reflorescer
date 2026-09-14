using Godot;

namespace Joguim.Combat
{
    [GlobalClass]
    public partial class Hurtbox : Area2D
    {
        [Export] public bool Active = true;
        [Export] public bool KnockbackResistant = false;

        [Signal] public delegate void DamageReceivedEventHandler(int damage, Vector2 knockbackDirection);
        [Signal] public delegate void HitEventHandler(Variant damageInfo);

        private Health _health;
        private bool _healthSearched = false;

        public override void _Ready()
        {
            Monitoring = true;
            Monitorable = true;
            FindHealth();
        }

        private void FindHealth()
        {
            if (_health != null) return;

            var p = GetParent();
            while (p != null)
            {
                _health = p.GetNodeOrNull<Health>("Health");
                if (_health != null) return;
                p = p.GetParent();
            }

            _healthSearched = true;
            if (_health == null)
                GD.PrintErr($"Hurtbox {GetPath()}: Health não encontrado em nenhum ancestral.");
        }

        public void ReceiveDamage(DamageInfo damageInfo)
        {
            if (!Active || damageInfo == null) return;

            if (_health == null && !_healthSearched) FindHealth();
            if (_health == null) return;

            if (_health.IsDead) return;

            _health.TakeDamage(damageInfo);

            EmitSignal("DamageReceived", damageInfo.Damage, damageInfo.KnockbackDirection);
        }

        public void SetOwner(Node2D owner)
        {
            Owner = owner;
            _health = null;
            _healthSearched = false;
            FindHealth();
        }
    }
}
