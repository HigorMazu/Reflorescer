using Godot;

namespace Joguim.Companion
{
    [GlobalClass]
    public partial class FaiscaLightController : Node
    {
        [Export] public float BaseRadius = 128.0f;
        [Export] public float BaseIntensity = 1.0f;
        [Export] public Color BaseColor = new Color(1.0f, 0.95f, 0.6f);
        [Export] public float PulseSpeed = 2.0f;
        [Export] public float PulseAmplitude = 0.15f;
        [Export] public NodePath LightPath;

        private PointLight2D _light;
        private float _baseTextureScale;

        public override void _Ready()
        {
            _light = GetNodeOrNull<PointLight2D>(LightPath);

            if (_light != null)
            {
                _light.Color = BaseColor;
                _light.TextureScale = BaseRadius / 64f;
                _baseTextureScale = _light.TextureScale;
            }
        }

        public override void _Process(double delta)
        {
            if (_light == null) return;

            float pulse = 1.0f + Mathf.Sin(Time.GetTicksMsec() / 1000.0f * PulseSpeed) * PulseAmplitude;
            _light.TextureScale = _baseTextureScale * pulse;
        }

        public void SetRadius(float radius)
        {
            if (_light != null)
            {
                _baseTextureScale = radius / 64f;
            }
        }

        public void SetIntensity(float intensity)
        {
            if (_light != null)
            {
                _light.TextureScale = _baseTextureScale * intensity;
            }
        }

        public void SetColor(Color color)
        {
            if (_light != null)
            {
                _light.Color = color;
            }
        }

        public void Flash(float duration = 0.3f, float intensityMultiplier = 2.0f)
        {
            if (_light == null) return;

            var tween = CreateTween();
            tween.TweenProperty(_light, "texture_scale", _baseTextureScale * intensityMultiplier, duration / 2f);
            tween.TweenProperty(_light, "texture_scale", _baseTextureScale, duration / 2f);
        }
    }
}
