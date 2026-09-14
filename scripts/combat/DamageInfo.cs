using Godot;

namespace Joguim.Combat
{
    public class DamageInfo
    {
        public int Damage { get; set; }
        public Vector2 KnockbackDirection { get; set; }
        public float KnockbackForce { get; set; }
        public Node2D Source { get; set; }

        public DamageInfo(int damage, Vector2 knockbackDirection, float knockbackForce, Node2D source)
        {
            Damage = damage;
            KnockbackDirection = knockbackDirection;
            KnockbackForce = knockbackForce;
            Source = source;
        }
    }
}
