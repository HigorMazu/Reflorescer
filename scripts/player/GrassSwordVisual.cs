using Godot;

namespace Joguim.Player
{
    // One consistent sword sprite follows Kairo across neutral and attack poses.
    public partial class GrassSwordVisual : Node2D
    {
        private PlayerController _player;
        private AnimatedSprite2D _blade;
        private Sprite2D _runHandOverlay;
        private Tween _transition;
        public float Extension { get; private set; }
        public Vector2 GripPosition { get; private set; }

        public override void _Ready()
        {
            _player = GetNode<PlayerController>("../../../..");
            _blade = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            _runHandOverlay = GetNode<Sprite2D>("../../../RunHandOverlay");
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
            // Extend the blade from the closed fist without widening it.
            _blade.Scale = new Vector2(.65f + .35f * value, 1.4f * Mathf.Max(.001f, value));
            _blade.Modulate = new Color(1, 1, 1, Mathf.Min(1, value * 4));
            UpdatePose();
        }

        public override void _Process(double delta) => UpdatePose();

        private void UpdatePose()
        {
            if (_player?.Sprite == null) return;
            string state = _player.Sprite.Animation;
            _blade.Visible = Extension > .001f && state != "dead" && state != "hurt";
            // The sprite is cropped above the guard: the visible blade starts at
            // Kairo's closed fist, while the grip remains inside the character art.
            _blade.Offset = new Vector2(-5.5f, -30f);
            int frame = _player.Sprite.Frame;
            // Kairo's original hand and arm cover the hidden grip. Only the blade
            // that extends beyond the silhouette remains visible to the player.
            bool runningWithSword = state == "run" && _blade.Visible;
            ZIndex = runningWithSword ? 0 : -1;
            _runHandOverlay.Visible = runningWithSword;
            if (runningWithSword)
            {
                // Restore only the original fist and forearm above the blade.
                // The blade itself can then remain readable in front of the tail.
                _runHandOverlay.Texture = _player.Sprite.SpriteFrames.GetFrameTexture("run", frame);
                _runHandOverlay.RegionRect = new Rect2(62, 84, 40, 51);
                _runHandOverlay.Position = new Vector2(-14, -23);
            }
            Vector2 hand = state switch
            {
                // Kairo's anatomical right hand: the closed fist on the left side of his right-facing sprites.
                // These coordinates are the exact centers of the fist in the 180x170 source frames
                // after the character sprite's (-45, -65) offset and 0.5 scale are applied.
                "idle" => frame == 0 ? new Vector2(-27, -13.5f) : new Vector2(-26, -12.5f),
                "run" => frame == 0 ? new Vector2(-4, -5.5f) : new Vector2(-5, -15),
                "dash" => frame == 0 ? new Vector2(-5, -21.5f) : new Vector2(-6, -18),
                "jump" => new Vector2(-18, -27),
                "wall_slide" => frame == 0 ? new Vector2(-18, -27) : new Vector2(1, -10),
                "fall" => new Vector2(1, -10),
                "attack" => frame switch
                {
                    0 => new Vector2(-12, -11.5f),
                    1 => new Vector2(-24, -7.5f),
                    _ => new Vector2(36.5f, -13)
                },
                _ => new Vector2(-27, -13.5f)
            };
            float armRotation = _player.RightArm.Rotation;
            GripPosition = hand;
            _player.RightArm.Position = hand - (_player.RightHand.Position + Position).Rotated(armRotation);
            float swordAngle = state switch
            {
                "attack" => frame switch
                {
                    0 => 2.05f,
                    1 => 1.55f,
                    _ => 1.55f
                },
                // The running fist swings low; keep the blade below it instead
                // of crossing the tail and exposing a floating grip.
                "run" => 4.35f,
                "dash" => 4.05f,
                "fall" => 2.25f,
                _ => Mathf.Pi
            };
            Rotation = swordAngle - armRotation;
        }
    }
}
