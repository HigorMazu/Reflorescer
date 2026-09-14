using Godot;
using System.Collections.Generic;

namespace Joguim.Combat
{
    [GlobalClass]
    public partial class Hitbox : Area2D
    {
        [Export] public int Damage = 10;
        [Export] public float KnockbackForce = 200.0f;
        [Export] public bool Active = true;

        [Signal] public delegate void HitLandedEventHandler(Node2D target, int damage);

        private Node2D _owner;
        private HashSet<Hurtbox> _alreadyHit = new();

        public override void _Ready()
        {
            _owner = GetParent<Node2D>();
            Monitoring = true;
            Monitorable = true;
            AreaEntered += OnAreaEntered;
        }

        private void OnAreaEntered(Area2D area) => TryHit(area);

        private void TryHit(Area2D area)
        {
            if (!Active) return;
            if (area is not Hurtbox hurtbox) return;
            if (_alreadyHit.Contains(hurtbox)) return;

            var hurtOwner = hurtbox.GetParent() as Node ?? hurtbox.Owner as Node;
            var myOwner = _owner ?? GetParent<Node2D>();
            if (hurtOwner == myOwner) return;

            Vector2 knockbackDir = myOwner is Node2D owner2D
                ? (hurtbox.GlobalPosition - owner2D.GlobalPosition).Normalized()
                : Vector2.Right;

            var damageInfo = new DamageInfo(Damage, knockbackDir, KnockbackForce, myOwner as Node2D);
            hurtbox.ReceiveDamage(damageInfo);
            _alreadyHit.Add(hurtbox);

            GD.Print($"[Hitbox] {myOwner?.Name} -> {hurtOwner?.Name}: {Damage} dmg");
            EmitSignal("HitLanded", hurtbox.Owner, Damage);
        }

        public void CheckOverlappingHits()
        {
            if (!Active) return;
            foreach (var area in GetOverlappingAreas())
                TryHit(area);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (!Active) return;
            foreach (var area in GetOverlappingAreas())
                TryHit(area);
        }

        public void ResetHitList() => _alreadyHit.Clear();

        public void SetDamage(int damage) => Damage = damage;

        public void SetOwner(Node2D owner) => _owner = owner;
    }
}
