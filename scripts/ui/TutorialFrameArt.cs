using Godot;

namespace Joguim.UI
{
    // Decorative drawing stays in the card margins and never receives input.
    public partial class TutorialFrameArt : Control
    {
        private float _time;
        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            Resized += QueueRedraw;
        }

        public override void _Process(double delta)
        {
            if (!IsVisibleInTree()) return;
            _time += (float)delta;
            QueueRedraw();
        }

        private void Leaf(Vector2 basePoint, Vector2 tip, Color color)
        {
            Vector2 middle = (basePoint + tip) * .5f;
            Vector2 normal = (tip - basePoint).Orthogonal().Normalized() * 4;
            DrawColoredPolygon(new[] { basePoint, middle + normal, tip, middle - normal }, color);
            DrawLine(basePoint, tip, new Color("a3a66a"), .7f, true);
        }

        public override void _Draw()
        {
            Color vine = new("78804d");
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side < 0 ? 12 : Size.X - 12;
                foreach (float end in new[] { -1f, 1f })
                {
                    float y = end < 0 ? 12 : Size.Y - 12;
                    Vector2 corner = new(x, y);
                    DrawPolyline(new[] { corner + new Vector2(0, -end * 76), corner,
                        corner + new Vector2(-side * 90, 0) }, new Color(.58f, .57f, .32f, .55f), 1f, true);
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 stem = corner + new Vector2(-side * (12 + i * 15), 0);
                        Leaf(stem, stem + new Vector2(-side * 12, -end * (9 + i % 2 * 4)), vine);
                        Vector2 vertical = corner + new Vector2(0, -end * (14 + i * 14));
                        Leaf(vertical, vertical + new Vector2(-side * 9, -end * 12), vine);
                    }
                }
                // Small botanical flourishes frame the title, away from the text.
                Vector2 start = new(Size.X * .5f + side * 150, 76);
                DrawLine(start, start + new Vector2(side * 105, 0), vine, 1f, true);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 stem = start + new Vector2(side * (22 + i * 19), 0);
                    Leaf(stem, stem + new Vector2(side * 13, -8), vine);
                    Leaf(stem, stem + new Vector2(side * 13, 8), new Color("4b6241"));
                }
                for (int i = 0; i < 4; i++)
                {
                    Vector2 p = new(x + Mathf.Sin(_time * .5f + i) * 3,
                        120 + Mathf.PosMod(i * 83 + _time * (3 + i), Mathf.Max(1, Size.Y - 220)));
                    float glow = .25f + .2f * Mathf.Sin(_time * 1.6f + i * 2);
                    DrawCircle(p, 4, new Color(.7f, .8f, .35f, glow * .12f));
                    DrawCircle(p, 1.1f, new Color(.85f, .9f, .5f, glow));
                }
            }
        }
    }
}
