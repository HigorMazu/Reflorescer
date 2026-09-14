using Godot;

namespace Joguim.Resources
{
    [GlobalClass]
    public partial class BossPhaseResource : Resource
    {
        [Export] public string PhaseName = "";
        [Export] public float HealthThreshold = 0.5f;
        [Export] public float AttackCooldown = 1.5f;
        [Export] public float MoveSpeed = 100.0f;
        [Export] public int Damage = 20;
        [Export] public string AnimationPrefix = "phase1_";
        [Export] public bool IsEnraged = false;
    }
}
