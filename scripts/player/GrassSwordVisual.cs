using Godot;

namespace Joguim.Player
{
    // One consistent sword sprite follows Kairo across neutral and attack poses.
    public partial class GrassSwordVisual : Node2D
    {
        private PlayerController _player;
        private AnimatedSprite2D _blade;
        private Tween _transition;
        public float Extension { get; private set; }
        public Vector2 GripPosition { get; private set; }

        public override void _Ready()
        {
            _player = GetNode<PlayerController>("../../../..");
            _blade = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public void SetActive(bool active, bool animate)
        {
            _transition?.Kill();
            if (!animate) SetExtension(active ? 1 : 0);
            else
            {
                _transition = CreateTween();
                _transition.TweenMethod(Callable.From<float>(SetExtension), Extension, active ? 1f : 0f, .24)
                    .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            }
        }

        private void SetExtension(float value)
        {
            Extension = value;
            _blade.Scale = new Vector2(.65f + .35f * value, Mathf.Max(.001f, value));
            _blade.Modulate = new Color(1, 1, 1, Mathf.Min(1, value * 4));
            UpdatePose();
        }

        public override void _Process(double delta) => UpdatePose();

        private void UpdatePose()
        {
            if (_player?.Sprite == null) return;
            string state = _player.Sprite.Animation;
            _blade.Visible = Extension > .001f && state != "dead" && state != "hurt";
            int frame = _player.Sprite.Frame;
            Vector2 hand = state switch
            {
                // Kairo's anatomical right hand: the closed fist on the left side of his right-facing sprites.
                // These coordinates are the exact centers of the fist in the 180x170 source frames
                // after the character sprite's (-45, -65) offset and 0.5 scale are applied.
                "idle" => frame == 0 ? new Vector2(-25.5f, 0) : new Vector2(-24, -1.5f),
                "run" => frame == 0 ? new Vector2(-4.5f, -5.5f) : new Vector2(-6.5f, -8.5f),
                "jump" => new Vector2(-17.5f, -16),
                "fall" => new Vector2(5, -5),
                "attack" => frame switch
                {
                    0 => new Vector2(-6, -5),
                    1 => new Vector2(16, -8),
                    _ => new Vector2(40, -15)
                },
                _ => new Vector2(-25.5f, 0)
            };
            float armRotation = _player.RightArm.Rotation;
            GripPosition = hand;
            _player.RightArm.Position = hand - (_player.RightHand.Position + Position).Rotated(armRotation);
            float swordAngle = state == "attack" ? frame switch
            {
                0 => 2.05f,
                1 => 1.55f,
                _ => 1.55f
            } : Mathf.Pi;
            Rotation = swordAngle - armRotation;
        }
    }
}
