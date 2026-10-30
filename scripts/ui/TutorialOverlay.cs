using Godot;
using Joguim.Core;
using Joguim.Save;

namespace Joguim.UI
{
    /// <summary>
    /// Tutorial pra quem nunca jogou (26/10/2026): tela de abertura com a história e os controles no começo
    /// de um Novo jogo, e faixas de dica no topo da tela quando o Kairo entra numa <see cref="Joguim.World.TutorialZone"/>.
    /// O HUD cria este nó; as zonas o encontram pelo grupo.
    /// </summary>
    public partial class TutorialOverlay : Control
    {
        public const string GroupName = "TutorialOverlay";

        private Control _intro;
        private PanelContainer _banner;
        private Label _bannerKey;
        private Label _bannerText;
        private string _bannerOwner = "";
        private string _bannerKeysTemplate = "";
        private string _bannerTextTemplate = "";
        // Textos com {acao}, refeitos quando o jogador troca entre teclado e controle
        private readonly System.Collections.Generic.List<(Label label, string template)> _dynamic = new();
        private Tween _bannerTween;

        public bool IsIntroOpen => _intro != null && _intro.Visible;
        public bool IsBannerVisible => _banner != null && _banner.Visible && _banner.Modulate.A > 0.01f;
        public string BannerText => _bannerText?.Text ?? "";
        public string BannerKeys => _bannerKey?.Text ?? "";

        public override void _Ready()
        {
            AddToGroup(GroupName);
            ProcessMode = ProcessModeEnum.Always;
            MouseFilter = MouseFilterEnum.Ignore;
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            BuildBanner();
            Resized += LayoutBanner;
            InputHints.Changed += RefreshHints;
            Input.JoyConnectionChanged += OnJoyConnectionChanged;
            CallDeferred(MethodName.CheckPendingIntro);
        }

        public override void _ExitTree()
        {
            InputHints.Changed -= RefreshHints;
            Input.JoyConnectionChanged -= OnJoyConnectionChanged;
        }

        private void OnJoyConnectionChanged(long device, bool connected)
        {
            if (connected) ShowNotice($"Controle conectado: {Input.GetJoyName((int)device)}", 3f);
        }

        private void RefreshHints()
        {
            foreach (var (label, template) in _dynamic)
                if (IsInstanceValid(label)) label.Text = InputHints.Format(template);
            if (_banner != null && _banner.Visible)
            {
                _bannerKey.Text = InputHints.Format(_bannerKeysTemplate);
                _bannerText.Text = InputHints.Format(_bannerTextTemplate);
                LayoutBanner();
            }
        }

        private Label Dynamic(Label label, string template)
        {
            _dynamic.Add((label, template));
            label.Text = InputHints.Format(template);
            return label;
        }

        private void CheckPendingIntro()
        {
            if (SaveManager.Instance == null || !SaveManager.Instance.PendingIntro) return;
            SaveManager.Instance.PendingIntro = false;
            ShowIntro();
        }

        // ---------- Faixa de dica ----------

        private void BuildBanner()
        {
            var style = new StyleBoxFlat
            {
                BgColor = new Color(.025f, .065f, .048f, .9f),
                BorderColor = new Color(.74f, .68f, .40f, .85f),
                BorderWidthBottom = 2,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                ContentMarginLeft = 20, ContentMarginRight = 20, ContentMarginTop = 10, ContentMarginBottom = 10
            };
            _banner = new PanelContainer { Name = "Banner", Visible = false, MouseFilter = MouseFilterEnum.Ignore };
            _banner.AddThemeStyleboxOverride("panel", style);
            AddChild(_banner);

            var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            row.AddThemeConstantOverride("separation", 14);
            _banner.AddChild(row);

            _bannerKey = new Label { VerticalAlignment = VerticalAlignment.Center };
            _bannerKey.AddThemeFontSizeOverride("font_size", 20);
            _bannerKey.AddThemeColorOverride("font_color", new Color("ffe9a8"));
            _bannerKey.AddThemeStyleboxOverride("normal", new StyleBoxFlat
            {
                BgColor = new Color("334331"), BorderColor = new Color("88966b"),
                BorderWidthBottom = 2, ContentMarginLeft = 12, ContentMarginRight = 12,
                ContentMarginTop = 6, ContentMarginBottom = 6
            });
            row.AddChild(_bannerKey);

            _bannerText = new Label { VerticalAlignment = VerticalAlignment.Center };
            _bannerText.AddThemeFontSizeOverride("font_size", 17);
            _bannerText.AddThemeColorOverride("font_color", new Color("e8ecd8"));
            _bannerText.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _bannerText.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            row.AddChild(_bannerText);
        }

        private void LayoutBanner()
        {
            if (_banner == null) return;
            // Below the health/sword HUD, with a bounded width even for controller names.
            float width = Mathf.Min(680, Mathf.Max(280, Size.X - 48));
            float keyWidth = _bannerKey.Visible ? _bannerKey.GetCombinedMinimumSize().X + 14 : 0;
            _bannerText.CustomMinimumSize = new Vector2(width - 40 - keyWidth, 0);
            _banner.Size = new Vector2(width, 0);
            _banner.Position = new Vector2((Size.X - width) / 2, 110);
            Callable.From(() => { if (IsInstanceValid(_banner)) _banner.ResetSize(); }).CallDeferred();
        }

        public void ShowBanner(string owner, string keys, string text)
        {
            _bannerOwner = owner;
            _bannerKeysTemplate = keys ?? "";
            _bannerTextTemplate = text ?? "";
            _bannerKey.Text = InputHints.Format(_bannerKeysTemplate);
            _bannerKey.Visible = !string.IsNullOrEmpty(keys);
            _bannerText.Text = InputHints.Format(_bannerTextTemplate);
            _banner.Visible = true;
            LayoutBanner();
            _bannerTween?.Kill();
            _bannerTween = CreateTween();
            _bannerTween.TweenProperty(_banner, "modulate:a", 1f, .25f);
        }

        // Só esconde se a faixa ainda for da zona que pediu (zonas vizinhas não se apagam)
        public void HideBanner(string owner)
        {
            if (owner != _bannerOwner) return;
            _bannerTween?.Kill();
            _bannerTween = CreateTween();
            _bannerTween.TweenInterval(1.2f);
            _bannerTween.TweenProperty(_banner, "modulate:a", 0f, .4f);
            _bannerTween.TweenCallback(Callable.From(() => _banner.Visible = false));
        }

        // Aviso curto que some sozinho (ex.: chefe derrotado)
        public void ShowNotice(string text, float seconds = 5f)
        {
            ShowBanner("notice", "", text);
            GetTree().CreateTimer(seconds, true, false, true).Timeout += () =>
            {
                if (IsInstanceValid(this)) HideBanner("notice");
            };
        }

        // ---------- Tela de abertura ----------

        public void ShowIntro()
        {
            if (_intro == null) BuildIntro();
            _intro.Visible = true;
            if (GameManager.Instance != null) GameManager.Instance.IsGameOver = true; // Esc e M não abrem nada por cima
            GetTree().Paused = true;
            _intro.GetNode<Button>("Center/Card/Margin/Box/StartButton").GrabFocus();
        }

        public void CloseIntro()
        {
            if (_intro == null || !_intro.Visible) return;
            _intro.Visible = false;
            if (GameManager.Instance != null) GameManager.Instance.IsGameOver = false;
            GetTree().Paused = false;
        }

        private void BuildIntro()
        {
            _intro = new Control { Name = "Intro", Visible = false, ProcessMode = ProcessModeEnum.Always };
            AddChild(_intro);
            _intro.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var dim = new ColorRect { Color = new Color(.01f, .03f, .02f, .86f), MouseFilter = MouseFilterEnum.Stop };
            _intro.AddChild(dim);
            dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var center = new CenterContainer { Name = "Center", MouseFilter = MouseFilterEnum.Ignore };
            _intro.AddChild(center);
            center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

            var card = new PanelContainer { Name = "Card", CustomMinimumSize = new Vector2(880, 0) };
            card.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(.025f, .065f, .048f, .97f),
                BorderColor = new Color(.74f, .68f, .40f, .88f),
                BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
                CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3
            });
            center.AddChild(card);
            card.AddChild(new TutorialFrameArt { Name = "BotanicalFrame", MouseFilter = MouseFilterEnum.Ignore });

            var margin = new MarginContainer { Name = "Margin" };
            foreach (var side in new[] { "left", "right" }) margin.AddThemeConstantOverride($"margin_{side}", 30);
            foreach (var side in new[] { "top", "bottom" }) margin.AddThemeConstantOverride($"margin_{side}", 18);
            card.AddChild(margin);

            var box = new VBoxContainer { Name = "Box" };
            box.AddThemeConstantOverride("separation", 8);
            margin.AddChild(box);

            box.AddChild(MakeLabel("A ÚLTIMA SEMENTE", 13, "b6bf94", HorizontalAlignment.Center));
            var title = MakeLabel("Reflorescer", 46, "f4edce", HorizontalAlignment.Center);
            title.AddThemeFontOverride("font", GD.Load<Font>("res://assets/fonts/cormorant/CormorantGaramond.ttf"));
            box.AddChild(title);

            var story = MakeLabel(
                "A floresta está morrendo. Kairo carrega a última semente viva e precisa atravessar terras devastadas " +
                "até onde ela possa voltar a brotar. Faísca, o vaga-lume, ilumina o caminho.",
                16, "e1e7d2", HorizontalAlignment.Center);
            story.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            story.CustomMinimumSize = new Vector2(820, 0);
            box.AddChild(story);

            var goal = MakeLabel(
                "OBJETIVO: restaure as 3 áreas degradadas da Floresta Tropical (cada uma cura o Kairo e aumenta a vida dele) " +
                "e derrote Korrag, o javali que revira a mata.",
                15, "d6bb73", HorizontalAlignment.Center);
            goal.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            goal.CustomMinimumSize = new Vector2(820, 0);
            box.AddChild(goal);

            box.AddChild(new HSeparator());

            var grid = new GridContainer { Name = "Controls", Columns = 4 };
            grid.AddThemeConstantOverride("h_separation", 12);
            grid.AddThemeConstantOverride("v_separation", 6);
            box.AddChild(grid);
            string[,] controls =
            {
                { "{move}", "andar" },
                { "{jump}", "pular (segure pra subir mais)" },
                { "{attack}", "atacar com a Espada de Grama" },
                { "{toggle_sword}", "guardar a espada: corre e pula mais, mas não ataca" },
                { "{interact}", "interagir e restaurar a terra" },
                { "Parede", "no ar, segure a direção pra deslizar e {jump} pra saltar" },
                { "{map}", "mapa" },
                { "{pause}", "pausa" },
            };
            for (int i = 0; i < controls.GetLength(0); i++)
            {
                var key = Dynamic(MakeLabel("", 16, "ffe9a8", HorizontalAlignment.Center), controls[i, 0]);
                key.CustomMinimumSize = new Vector2(110, 36);
                key.AddThemeStyleboxOverride("normal", new StyleBoxFlat
                {
                    BgColor = new Color("263b2d"), BorderColor = new Color("687c52"),
                    BorderWidthBottom = 3, BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1,
                    CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                    CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4,
                    ContentMarginLeft = 8, ContentMarginRight = 8
                });
                grid.AddChild(key);
                var desc = Dynamic(MakeLabel("", 15, "e8ecd8", HorizontalAlignment.Left), controls[i, 1]);
                desc.CustomMinimumSize = new Vector2(278, 0);
                desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;
                desc.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                grid.AddChild(desc);
            }

            var warn = MakeLabel("Cuidado: quedas altas machucam. Ao morrer, o Kairo volta pra última bandeira.",
                14, "e0a27a", HorizontalAlignment.Center);
            box.AddChild(warn);
            box.AddChild(MakeLabel("Dá pra jogar com controle: aperte qualquer botão dele e as teclas desta tela mudam.",
                13, "9fb08c", HorizontalAlignment.Center));

            var start = new Button { Name = "StartButton", Text = "Começar a jornada", CustomMinimumSize = new Vector2(280, 42),
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter };
            start.AddThemeStyleboxOverride("normal", ButtonStyle(false));
            start.AddThemeStyleboxOverride("hover", ButtonStyle(true));
            start.AddThemeStyleboxOverride("focus", ButtonStyle(true));
            start.AddThemeStyleboxOverride("pressed", ButtonStyle(true));
            start.AddThemeColorOverride("font_color", new Color("e8e7ce"));
            start.AddThemeColorOverride("font_focus_color", new Color("fff4ca"));
            start.Pressed += CloseIntro;
            box.AddChild(start);
        }

        private static Label MakeLabel(string text, int size, string color, HorizontalAlignment align)
        {
            var label = new Label { Text = text, HorizontalAlignment = align, VerticalAlignment = VerticalAlignment.Center };
            label.AddThemeFontSizeOverride("font_size", size);
            label.AddThemeColorOverride("font_color", new Color(color));
            return label;
        }

        private static StyleBoxFlat ButtonStyle(bool selected) => new()
        {
            BgColor = selected ? new Color(.20f, .29f, .19f, .92f) : new Color(.08f, .14f, .10f, .9f),
            BorderColor = selected ? new Color("d8c57d") : new Color(.42f, .51f, .36f, .8f),
            BorderWidthLeft = selected ? 2 : 1,
            CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2,
            ContentMarginLeft = 18, ContentMarginRight = 18
        };
    }
}
