using Godot;

namespace Joguim.World
{
    /// <summary>Animated passage marker for the forest/desert border.</summary>
    public partial class DesertPortalVisual : Node2D
    {
        [Export] public bool ForestSide = false;
        private static readonly Texture2D Passage = GD.Load<Texture2D>(
            "res://assets/environment/desert/desert_transition_side_v2.png");
        private float _time;

        public override void _Ready()
        {
            Material = new ShaderMaterial
            {
                Shader = GD.Load<Shader>("res://assets/shaders/transition_edge.gdshader")
            };
        }

        public override void _Process(double delta)
        {
            _time += (float)delta;
            QueueRedraw();
        }

        public override void _Draw()
        {
            Color light = ForestSide ? new Color("9eb46a") : new Color("e1a04e");
            if (ForestSide) DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
            else DrawSetTransform(Vector2.Zero, 0f, new Vector2(-1f, 1f));
            DrawTextureRect(Passage, new Rect2(-88, -82, 176, 132), false,
                ForestSide ? new Color(0.66f, 0.67f, 0.52f) : new Color(0.9f, 0.84f, 0.72f));
            DrawSetTransform(Vector2.Zero, 0f, Vector2.One);

            // A narrow animated veil leaves Kairo visible while clearly marking the passage.
            float breathe = .5f + .5f * Mathf.Sin(_time * 2.1f);
            DrawEllipse(new Vector2(0, 4), new Vector2(10, 32),
                new Color(light, .06f + breathe * .04f), 2f);

            // Sand grains travel upward; on the forest side a few green motes soften the seam.
            for (int i = 0; i < 8; i++)
            {
                float phase = _time * (.7f + i * .035f) + i * .82f;
                float y = 42 - Mathf.PosMod(phase * 18f, 82f);
                float x = Mathf.Sin(phase * 1.7f) * (7 + i % 3 * 4);
                Color grain = ForestSide && i % 3 == 0 ? new Color("9fc16d") : new Color("e6a44c");
                DrawCircle(new Vector2(x, y), 1.1f + (i % 2) * .45f, new Color(grain, .55f));
            }

        }

        private void DrawEllipse(Vector2 center, Vector2 radii, Color color, float width)
        {
            Vector2[] points = new Vector2[41];
            for (int i = 0; i <= 40; i++)
            {
                float angle = i * Mathf.Tau / 40f;
                points[i] = center + new Vector2(Mathf.Cos(angle) * radii.X, Mathf.Sin(angle) * radii.Y);
            }
            DrawPolyline(points, color, width, true);
        }
    }
}
