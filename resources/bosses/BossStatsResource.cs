using Godot;

namespace Joguim.Resources
{
    [GlobalClass]
    public partial class BossStatsResource : Resource
    {
        [Export] public int MaxHealth = 500;
        [Export] public string BossName = "Boss";
        [Export] public float Gravity = 980.0f;
        [Export] public float KnockbackResistance = 0.5f;
        [Export] public BossPhaseResource[] Phases;
    }
}
