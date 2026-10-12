using Godot;

namespace Joguim.Player
{
    /// <summary>
    /// Small movement accents that preserve Kairo's production sprite while making
    /// wall grip and dash readable during play.
    /// </summary>
    public partial class MovementStateEffects : Node2D
    {
        [Export(PropertyHint.Range, "0,2,1")] public int PreviewState { get; set; }

        public string DisplayedState { get; private set; } = "none";
        public float ContactSide { get; private set; } = 1f;

        private PlayerController _player;
        private float _time;
        private Vector2 _appliedWallOffset;
        private float _appliedWallRotation;
        private Vector2 _wallClawContact;
        private Vector2 _wallBootContact;
        public Vector2 WallPoseOffset => _appliedWallOffset;
        public float WallPoseRotation => _appliedWallRotation;
        // Contact points in player-local space, useful to keep visual effects
        // on the actual hand and front boot as the whole sprite leans.
        public Vector2 WallClawContact => _wallClawContact;
        public Vector2 WallBootContact => _wallBootContact;

        public override void _Ready()
        {
            _player = GetParentOrNull<PlayerController>();
            QueueRedraw();
        }

        public override void _Process(double delta)
        {
            _time += (float)delta;
            string nextState = PreviewState switch
            {
                1 => "wall_slide",
                2 => "dash",
                _ when _player?.IsDashing == true => "dash",
                _ when _player?.IsWallSliding == true => "wall_slide",
                _ => "none"
            };

            if (nextState != DisplayedState)
            {
                DisplayedState = nextState;
                if (nextState == "wall_slide") _time = 0f;
            }

            // The previous marks were behind the body and disappeared at game scale.
            // Only wall contact moves in front; dash keeps its approved layer.
            ZIndex = DisplayedState == "wall_slide" ? 2 : -2;
            ContactSide = _player?.Visual?.Scale.X < 0 ? -1f : 1f;
            if (PreviewState == 0 && _player?.IsOnWall() == true)
                ContactSide = -Mathf.Sign(_player.GetWallNormal().X);

            // The jump frame's forward claw is near source (135, 74), while its
            // bent front boot is nearer (92, 128). Pivoting the whole pose
            // against the wall brings both to the trunk; the sword stays in hand.
            Vector2 desiredOffset = DisplayedState == "wall_slide"
                ? new Vector2(ContactSide * (12f + Mathf.Sin(_time * 7f) * .5f), 0)
                : Vector2.Zero;
            float desiredRotation = DisplayedState == "wall_slide"
                ? -ContactSide * (.65f + Mathf.Sin(_time * 7f) * .012f)
                : 0f;
            if (_player?.Visual != null)
            {
                _player.Visual.Position += desiredOffset - _appliedWallOffset;
                _player.Visual.Rotation += desiredRotation - _appliedWallRotation;
            }
            _appliedWallOffset = desiredOffset;
            _appliedWallRotation = desiredRotation;

            if (DisplayedState == "wall_slide" && _player?.Visual != null)
            {
                // The wall-slide art reuses Kairo's airborne frame. These are the
                // hand and bent-front-boot centers after AnimatedSprite2D's source
                // offset/scale. Transform through Visual so the anchors follow the
                // lean and mirror precisely instead of floating beside the body.
                _wallClawContact = _player.Visual.Transform * new Vector2(27f, -28f);
                _wallBootContact = _player.Visual.Transform * new Vector2(18f, -2f);
            }
            else
            {
                _wallClawContact = Vector2.Zero;
                _wallBootContact = Vector2.Zero;
            }

            Visible = DisplayedState != "none";
            if (Visible) QueueRedraw();
        }

        public override void _Draw()
        {
            float facing = _player?.Visual?.Scale.X < 0 ? -1f : 1f;
            float pulse = 0.78f + Mathf.Sin(_time * 18f) * 0.12f;

            if (DisplayedState == "dash")
            {
                Color streak = new(0.53f, 0.83f, 0.66f, pulse);
                float behind = -facing;
                for (int i = 0; i < 3; i++)
                {
                    float y = -48f + i * 13f;
                    float drift = Mathf.PosMod(_time * 52f + i * 7f, 9f);
                    Vector2 start = new(behind * (18f + drift + i * 6f), y);
                    Vector2 end = new(behind * (7f + drift), y + 1.5f);
                    DrawLine(start, end, streak, 2f);
                }
                DrawCircle(new Vector2(behind * 12f, -7f), 3.5f, new Color(0.55f, 0.42f, 0.24f, pulse * 0.75f));
                DrawCircle(new Vector2(behind * 21f, -5f), 2f, new Color(0.66f, 0.53f, 0.31f, pulse * 0.55f));
            }
            else if (DisplayedState == "wall_slide")
            {
                float wallSide = ContactSide;
                // Both accents originate at the real claw and boot anchors,
                // so the pose reads as a grip rather than a sideways fall.
                Vector2 claw = _wallClawContact;
                Vector2 boot = _wallBootContact;
                for (int i = 0; i < 3; i++)
                {
                    Vector2 talon = claw + new Vector2(0, (i - 1) * 3.4f);
                    DrawLine(talon + new Vector2(-wallSide * 5f, -1.8f),
                        talon + new Vector2(wallSide * 2.5f, 2.5f),
                        new Color(.96f, .86f, .56f, pulse), 1.65f);
                }
                DrawLine(boot + new Vector2(-wallSide * 7f, 1.5f), boot,
                    new Color(.20f, .16f, .10f, .96f), 4.5f);
                DrawLine(boot + new Vector2(-wallSide * 6f, .3f), boot,
                    new Color(.80f, .64f, .35f, pulse), 1.7f);

                // Bark chips and leaves tumble from the two points of contact.
                // Their staggered ages give a continuous fall without a particle
                // node or a new texture sheet.
                for (int i = 0; i < 14; i++)
                {
                    float age = Mathf.PosMod(_time * 1.7f + i * .079f, 1f);
                    bool isLeaf = i % 4 == 0;
                    Vector2 origin = i % 3 == 0 ? boot : claw;
                    float drift = -wallSide * (3.5f + age * (9f + i % 3 * 3f));
                    Vector2 p = origin + new Vector2(drift + Mathf.Sin(_time * 5f + i) * 1.6f,
                        age * age * (30f + i % 4 * 4f));
                    float alpha = 1f - age;
                    if (isLeaf)
                    {
                        float size = (5.4f - age * 1.2f) * alpha;
                        Vector2[] leaf =
                        {
                            p + new Vector2(-wallSide * size, 0),
                            p + new Vector2(-wallSide * size * .15f, -size * .62f),
                            p + new Vector2(wallSide * size, 0),
                            p + new Vector2(-wallSide * size * .15f, size * .62f)
                        };
                        DrawColoredPolygon(leaf, new Color(.63f, .88f, .30f, alpha * .95f));
                        DrawLine(p + new Vector2(-wallSide * size * .8f, 0),
                            p + new Vector2(wallSide * size * .55f, 0),
                            new Color(.92f, .94f, .53f, alpha * .88f), 1f);
                    }
                    else
                    {
                        float size = (4.1f - age * 1.7f) * alpha;
                        DrawLine(p + new Vector2(wallSide * size, -size * .45f),
                            p + new Vector2(-wallSide * size, size * .6f),
                            new Color(.88f, .62f, .31f, alpha * .94f), 2.2f);
                    }
                }
            }
        }
    }
}
