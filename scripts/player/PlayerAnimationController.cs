using Godot;

namespace Joguim.Player
{
    [GlobalClass]
    public partial class PlayerAnimationController : Node
    {
        [Export] public NodePath AnimationPlayerPath;
        [Export] public NodePath SpritePath;
        [Export] public NodePath SwordPath;

        private AnimationPlayer _animationPlayer;
        private AnimatedSprite2D _sprite;
        private AnimatedSprite2D _sword;

        public override void _Ready()
        {
            _animationPlayer = GetNodeOrNull<AnimationPlayer>(AnimationPlayerPath);
            _sprite = GetNodeOrNull<AnimatedSprite2D>(SpritePath);
            _sword = GetNodeOrNull<AnimatedSprite2D>(SwordPath);
        }

        public void PlayAnimation(string animationName)
        {
            if (_animationPlayer != null && _animationPlayer.HasAnimation(animationName))
            {
                _animationPlayer.Play(animationName);
            }
            else if (_sprite != null && _sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(animationName))
            {
                _sprite.Play(animationName);
            }
        }

        public void SetFlipH(bool flipH)
        {
            if (_sprite != null) _sprite.FlipH = flipH;
        }

        public void SetSwordVisible(bool visible)
        {
            if (_sword != null) _sword.Visible = visible;
        }
    }
}
