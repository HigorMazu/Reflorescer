using Godot;
using System;

namespace Joguim.Combat
{
    [GlobalClass]
    public partial class CombatController : Node
    {
        [Export] public NodePath HealthPath;
        [Export] public NodePath HitboxPath;
        [Export] public NodePath HurtboxPath;

        public Health Health { get; private set; }
        public Hitbox Hitbox { get; private set; }
        public Hurtbox Hurtbox { get; private set; }

        [Signal] public delegate void CombatReadyEventHandler();

        public override void _Ready()
        {
            Health = GetNodeOrNull<Health>(HealthPath);
            Hitbox = GetNodeOrNull<Hitbox>(HitboxPath);
            Hurtbox = GetNodeOrNull<Hurtbox>(HurtboxPath);

            if (Health != null)
            {
                Health.Died += OnHealthDied;
            }

            EmitSignal("CombatReady");
        }

        public void EnableHitbox()
        {
            if (Hitbox == null) return;
            Hitbox.ResetHitList();
            Hitbox.Active = true;
            Hitbox.Monitoring = true;
            Hitbox.Monitorable = true;
            Hitbox.CheckOverlappingHits();
        }

        public void DisableHitbox()
        {
            if (Hitbox == null) return;
            Hitbox.Active = false;
            Hitbox.ResetHitList();
        }

        public void EnableHurtbox()
        {
            if (Hurtbox != null) Hurtbox.Active = true;
        }

        public void DisableHurtbox()
        {
            if (Hurtbox != null) Hurtbox.Active = false;
        }

        public void ApplyKnockback(Vector2 direction, float force)
        {
            var body = Owner as CharacterBody2D;
            if (body == null) return;

            body.Velocity = direction.Normalized() * force;
        }

        protected virtual void OnHealthDied()
        {
            // Override in derived classes
        }

        public void SetHitboxDamage(int damage)
        {
            if (Hitbox != null) Hitbox.SetDamage(damage);
        }
    }
}
