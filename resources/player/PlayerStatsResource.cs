using Godot;

namespace Joguim.Resources
{
    [GlobalClass]
    public partial class PlayerStatsResource : Resource
    {
        [Export] public int MaxHealth = 100;
        [Export] public float MoveSpeed = 200.0f;
        [Export] public float Acceleration = 1500.0f;
        [Export] public float Deceleration = 1500.0f;
        [Export] public float AirAcceleration = 1000.0f;
        [Export] public float JumpVelocity = -400.0f;
        // Variante "espada desativada": +velocidade, +pulo, sem ataque
        [Export] public float MoveSpeedNoSword = 240.0f;
        [Export] public float JumpVelocityNoSword = -520.0f;
        [Export] public float Gravity = 980.0f;
        [Export] public float FallGravityMultiplier = 1.5f;
        [Export] public float MaxFallSpeed = 600.0f;
        [Export] public int AttackDamage = 25;
        [Export] public float AttackCooldown = 0.4f;
        [Export] public float KnockbackResistance = 0.3f;
        [Export] public float InvulnerabilityDuration = 1.0f;
        // Dash (desbloqueado ao derrotar o Korrag): impulso horizontal curto, com i-frames durante o impulso
        [Export] public float DashSpeed = 520.0f;
        [Export] public float DashDuration = 0.16f;
        [Export] public float DashCooldown = 0.6f;
        // Parede: segurar A/D contra a parede no ar desliza devagar; pular empurra pro lado oposto
        [Export] public float WallSlideSpeed = 90.0f;
        [Export] public float WallJumpHorizontalSpeed = 260.0f;
        [Export] public float WallJumpInputLock = 0.15f;
    }
}
