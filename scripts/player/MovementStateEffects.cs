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

        private PlayerController _player;
        private float _time;

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
                DisplayedState = nextState;

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
                Color scrape = new(0.82f, 0.68f, 0.39f, pulse);
                float wallSide = facing;
                for (int i = 0; i < 3; i++)
                {
                    float y = -40f + i * 9f + Mathf.Sin(_time * 15f + i) * 2f;
                    Vector2 contact = new(wallSide * 14f, y);
                    DrawLine(contact, contact + new Vector2(-wallSide * 5f, -3f), scrape, 1.7f);
                    DrawCircle(contact + new Vector2(-wallSide * 2f, 4f), 1.6f, scrape);
                }
            }
        }
    }
}
