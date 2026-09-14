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
            }
            else
            {
                Value = value;
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
