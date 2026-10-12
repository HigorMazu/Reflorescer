using Godot;
using Joguim.Bosses;

namespace Joguim.Art
{
    /// <summary>
    /// The first two attack atlas cells face right while the remaining Korrag
    /// art faces left. Correct those cells only, retaining the boss facing rule.
    /// </summary>
    public partial class KorragAttackOrientation : AnimatedSprite2D
    {
        private BossBase _boss;

        public override void _Ready()
        {
            _boss = GetParentOrNull<BossBase>();
            FrameChanged += ApplyFacing;
            AnimationChanged += ApplyFacing;
            ApplyFacing();
        }

        public override void _Process(double delta) => ApplyFacing();

        private void ApplyFacing()
        {
            bool baseFlip = _boss != null
                ? (_boss.SpriteFacesLeft ? _boss.FacingX > 0 : _boss.FacingX < 0)
                : false;
            bool rightFacingAtlasFrame = Animation == "attack" && Frame < 2;
            FlipH = baseFlip ^ rightFacingAtlasFrame;
        }
    }
}
