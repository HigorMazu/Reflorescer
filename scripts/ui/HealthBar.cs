using Godot;

namespace Joguim.UI
{
    [GlobalClass]
    public partial class HealthBar : TextureProgressBar
    {
        [Export] public float TweenDuration = 0.3f;
        [Export] public bool AnimateOnDamage = true;

        private Tween _tween;

        public override void _Ready()
        {
            if (_tween != null && _tween.IsValid())
            {
                _tween.Kill();
            }
        }

        public override void _Draw()
        {
            var size = Size;
            var frame = new StyleBoxFlat
            {
                BgColor = new Color("17241f"), BorderColor = new Color("9db772"),
                BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2,
                CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5
            };
            DrawStyleBox(frame, new Rect2(Vector2.Zero, size));
            var inset = new Rect2(4, 4, Mathf.Max(0, size.X - 8), Mathf.Max(0, size.Y - 8));
            DrawRect(inset, new Color("0b1512"));
            float ratio = MaxValue <= 0 ? 0 : (float)(Value / MaxValue);
            DrawRect(new Rect2(inset.Position, new Vector2(inset.Size.X * ratio, inset.Size.Y)), new Color("8bad55"));
            for (float x = inset.Position.X + 24; x < inset.End.X; x += 24)
                DrawLine(new Vector2(x, inset.Position.Y + 1), new Vector2(x, inset.End.Y - 1), new Color(0.08f, 0.14f, 0.11f, .65f), 1);
        }

        public void SetValue(int value)
        {
            if (AnimateOnDamage && _tween != null && _tween.IsValid())
            {
                _tween.Kill();
            }

            if (AnimateOnDamage)
            {
                _tween = CreateTween();
                _tween.TweenProperty(this, "value", (float)value, TweenDuration);
                _tween.TweenCallback(Callable.From(QueueRedraw));
            }
            else
            {
                Value = value;
                QueueRedraw();
            }
        }

        public void SetMaxValue(int maxValue)
        {
            MaxValue = maxValue;
        }

        public void SetValues(int current, int max)
        {
            MaxValue = max;
            SetValue(current);
        }
    }
}
