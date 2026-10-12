using Godot;

namespace Joguim.World
{
    /// <summary>Forest checkpoint rooted on the terrain baseline (local y = 0).</summary>
    public partial class CheckpointTotemVisual : Node2D
    {
        public bool Activated { get; private set; }
        private float _time;
        private float _activationFlash;

        public void SetActivated(bool active, bool animate)
        {
            Activated = active;
            _activationFlash = active && animate ? 1f : 0f;
            QueueRedraw();
        }

        public override void _Process(double delta)
        {
            _time += (float)delta;
            _activationFlash = Mathf.Max(0f, _activationFlash - (float)delta * 1.3f);
            if (Activated || _activationFlash > 0f) QueueRedraw();
        }

        public override void _Draw()
        {
            float glow = Activated ? .38f + Mathf.Sin(_time * 3.8f) * .12f : 0f;
            float burst = _activationFlash * _activationFlash;
            if (Activated)
            {
                DrawCircle(new Vector2(0, -28), 20 + burst * 10,
                    new Color(.42f, .91f, .49f, glow * .17f + burst * .15f));
                DrawCircle(new Vector2(0, -28), 11 + burst * 4,
                    new Color(.72f, .96f, .57f, glow * .23f + burst * .22f));
            }

            // Low stone socket and three roots bind the marker to the ground.
            Poly(new Color("1c2019"), new Vector2(-17, 0), new Vector2(-14, -9),
                new Vector2(13, -9), new Vector2(17, 0));
            Poly(new Color("4f4935"), new Vector2(-14, -2), new Vector2(-10, -12),
                new Vector2(10, -12), new Vector2(14, -2));
            Poly(new Color("75804a"), new Vector2(-15, -8), new Vector2(-12, -11),
                new Vector2(12, -11), new Vector2(15, -8));
            DrawLine(new Vector2(-9, -3), new Vector2(-20, 0), new Color("3d3928"), 3);
            DrawLine(new Vector2(8, -3), new Vector2(20, 0), new Color("3d3928"), 3);

            // Carved bark with a dark opening; the sprout is the saved-state cue.
            Poly(new Color("201d17"), new Vector2(-8, -10), new Vector2(-9, -47),
                new Vector2(-4, -52), new Vector2(5, -51), new Vector2(9, -44), new Vector2(8, -10));
            Poly(new Color("725438"), new Vector2(-6, -12), new Vector2(-7, -45),
                new Vector2(-3, -49), new Vector2(4, -48), new Vector2(7, -43), new Vector2(6, -12));
            DrawLine(new Vector2(-4, -44), new Vector2(-3, -15), new Color("a57b48"), 2);
            DrawLine(new Vector2(5, -39), new Vector2(4, -14), new Color("42301f"), 2);
            Poly(new Color("282a1d"), new Vector2(-4, -34), new Vector2(0, -39),
                new Vector2(4, -34), new Vector2(4, -24), new Vector2(0, -21), new Vector2(-4, -24));

            Color stem = Activated ? new Color("b2ed79") : new Color("86724a");
            Color leaf = Activated ? new Color("62c470") : new Color("5b633e");
            DrawLine(new Vector2(0, -24), new Vector2(0, -34), stem, 2);
            if (Activated)
            {
                Poly(leaf, new Vector2(-1, -30), new Vector2(-9, -37), new Vector2(-11, -42),
                    new Vector2(-5, -41), new Vector2(0, -35));
                Poly(new Color("8de28a"), new Vector2(1, -30), new Vector2(10, -38),
                    new Vector2(11, -43), new Vector2(5, -41), new Vector2(0, -35));
                DrawCircle(new Vector2(0, -37), 2.5f, new Color("e8f6a5"));
                DrawCircle(new Vector2(-11, -38), 1.3f, new Color(.88f, 1f, .67f, glow));
                DrawCircle(new Vector2(11, -41), 1.3f, new Color(.88f, 1f, .67f, glow));
            }
            else
            {
                Poly(leaf, new Vector2(-1, -30), new Vector2(-8, -32),
                    new Vector2(-8, -28), new Vector2(-2, -27));
                Poly(leaf, new Vector2(1, -30), new Vector2(8, -32),
                    new Vector2(8, -28), new Vector2(2, -27));
            }

            // Activation sparks emerge once, then the soft living glow remains.
            if (_activationFlash > 0f)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = i * Mathf.Tau / 6f;
                    Vector2 from = new(Mathf.Cos(angle) * 9, -30 + Mathf.Sin(angle) * 9);
                    Vector2 to = new(Mathf.Cos(angle) * (13 + burst * 12),
                        -30 + Mathf.Sin(angle) * (13 + burst * 12));
                    DrawLine(from, to, new Color(.91f, 1f, .65f, _activationFlash), 2);
                }
            }
        }

        private void Poly(Color color, params Vector2[] points) => DrawColoredPolygon(points, color);
    }
}
