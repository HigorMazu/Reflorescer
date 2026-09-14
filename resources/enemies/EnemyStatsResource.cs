using Godot;

namespace Joguim.Resources
{
    [GlobalClass]
    public partial class EnemyStatsResource : Resource
    {
        [Export] public int MaxHealth = 50;
        [Export] public float MoveSpeed = 80.0f;
        [Export] public int AttackDamage = 10;
        [Export] public float AttackRange = 30.0f;
        [Export] public float DetectionRange = 150.0f;
        [Export] public float PatrolSpeed = 40.0f;
        [Export] public float ChaseSpeed = 100.0f;
        [Export] public float AttackCooldown = 1.0f;
        [Export] public float Gravity = 980.0f;
        [Export] public float KnockbackResistance = 0.2f;
        [Export] public int ExperienceReward = 10;
    }
}
