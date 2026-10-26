using Godot;

namespace Joguim.UI
{
    // Fixed footprint below health; shape and text distinguish states without relying on color.
    public partial class SwordStatusIndicator : Control
    {
        public bool HasSword { get; private set; }
        private Label _state;
        private bool _initialized;
        private float _pulse;
        private Tween _transition;
        private Label _key;

        public override void _Ready()
        {
            CustomMinimumSize = new Vector2(156, 32);
            MouseFilter = MouseFilterEnum.Ignore;
            _state = new Label { Position = new Vector2(35, 5), MouseFilter = MouseFilterEnum.Ignore };
            _state.AddThemeFontSizeOverride("font_size", 13);
            AddChild(_state);
            _key = new Label { Position = new Vector2(122, 6), Size = new Vector2(30, 18), HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = MouseFilterEnum.Ignore };
            _key.AddThemeFontSizeOverride("font_size", 12);
            _key.AddThemeColorOverride("font_color", new Color("c6cbb9"));
            AddChild(_key);
            InputHints.Changed += RefreshKey;
            RefreshKey();
            SetState(HasSword, false);
        }

        public override void _ExitTree() => InputHints.Changed -= RefreshKey;

        // Tecla da espada no dispositivo atual (Q no teclado, LB/L1/L no controle)
        private void RefreshKey()
        {
            if (_key != null) _key.Text = InputHints.Label("toggle_sword");
        }

        public void SetState(bool active, bool animate = true)
        {
            bool changed = _initialized && HasSword != active;
            HasSword = active;
            _initialized = true;
            if (_state != null)
            {
                _state.Text = active ? "ATIVA" : "GUARDADA";
                _state.AddThemeColorOverride("font_color", new Color(active ? "d5f1aa" : "d5cbb2"));
            }
            _transition?.Kill();
            _pulse = changed && animate ? 1 : 0;
            if (_pulse > 0)
            {
                _transition = CreateTween();
                _transition.TweenMethod(Callable.From<float>(value => { _pulse = value; QueueRedraw(); }), 1f, 0f, .28);
            }
            QueueRedraw();
        }

        public override void _Draw()
        {
            var accent = new Color(HasSword ? "8fbd65" : "938875");
            var panel = new StyleBoxFlat
            {
                BgColor = new Color("15231feb"), BorderColor = accent.Lerp(Colors.White, _pulse * .45f),
                BorderWidthLeft = 2, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
                CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5, CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5
            };
            DrawStyleBox(panel, new Rect2(Vector2.Zero, new Vector2(156, 32)));
            DrawRect(new Rect2(126, 6, 23, 20), new Color("33433a"));
            if (HasSword)
            {
                DrawColoredPolygon(new[] { new Vector2(15, 20), new Vector2(18, 10), new Vector2(28, 5), new Vector2(25, 15), new Vector2(18, 23) }, accent);
                DrawLine(new Vector2(17, 21), new Vector2(25, 9), new Color("e5f2c3"), 1);
                DrawLine(new Vector2(12, 19), new Vector2(20, 25), new Color("b9a67c"), 2);
                DrawLine(new Vector2(16, 23), new Vector2(12, 28), new Color("b9a67c"), 3);
            }
            else
            {
                DrawStyleBox(new StyleBoxFlat { BgColor = new Color("425a3d"), CornerRadiusTopLeft = 3, CornerRadiusBottomRight = 3 }, new Rect2(11, 10, 17, 15));
                for (int i = 0; i < 3; i++)
                    DrawLine(new Vector2(12, 12 + i * 5), new Vector2(27, 10 + i * 5), accent, 2);
            }
        }
    }
}
