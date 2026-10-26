using Godot;
using Joguim.Core;
using Joguim.Player;
using Joguim.Save;

namespace Joguim.UI
{
    public partial class HUDController : CanvasLayer
    {
        [Export] public NodePath HealthBarPath;
        [Export] public NodePath HealthLabelPath;
        [Export] public NodePath InteractionPromptPath;
        [Export] public NodePath AbilityDisplayPath;
        [Export] public NodePath PauseMenuPath;
        [Export] public NodePath SwordStatusPath;
        private SwordStatusIndicator _swordStatus;

        private HealthBar _healthBar;
        private Label _healthLabel;
        private Label _interactionPrompt;
        private Label _abilityDisplay;
        private Control _pauseMenu;
        private Label _unlockNotification;
        private Tween _unlockTween;

        private Control _gameOverPanel;
        private Control _demoEndPanel;
        private VBoxContainer _demoEndBox;
        private ShaderMaterial _demoEndForestMaterial;
        private PanelContainer _demoEndCard;

        private const float UnlockNotificationDuration = 2.5f;
        // Tela de fim da demo (C14-T3): espera o fade do Korrag e o aviso do Dash antes de aparecer
        private const float DemoEndDelay = 4.0f;
        // Pontos de restauração da demo: Sopé, Dossel e Igarapé (o Covil não tem)
        public const int DemoRestorationTotal = 3;
        private const float GameOverDelay = 0.8f;

        public const string GroupName = "HUD";

        private PlayerController _player;

        public override void _Ready()
        {
            _healthBar = GetNodeOrNull<HealthBar>(HealthBarPath);
            _healthLabel = GetNodeOrNull<Label>(HealthLabelPath);
            _interactionPrompt = GetNodeOrNull<Label>(InteractionPromptPath);
            _abilityDisplay = GetNodeOrNull<Label>(AbilityDisplayPath);
            _pauseMenu = GetNodeOrNull<Control>(PauseMenuPath);
            _swordStatus = GetNodeOrNull<SwordStatusIndicator>(SwordStatusPath);

            AddToGroup(GroupName);
            // Tela de abertura e faixas de dica (26/10/2026)
            AddChild(new TutorialOverlay { Name = "Tutorial" });

            if (_pauseMenu != null)
            {
                _pauseMenu.Visible = false;
                // o menu de pausa precisa processar com a árvore pausada, senão os botões não respondem
                _pauseMenu.ProcessMode = ProcessModeEnum.Always;
                WirePauseButton("VBoxContainer/ResumeButton", () => GameManager.Instance?.TogglePause());
                WirePauseButton("VBoxContainer/SaveButton", () => SaveManager.Instance?.SaveCurrentGame());
                WirePauseButton("VBoxContainer/MainMenuButton", GoToMainMenu);
            }

            EventBus.Instance.PlayerDied += OnPlayerDied;
            EventBus.Instance.PlayerHealthChanged += OnPlayerHealthChanged;
            EventBus.Instance.PauseToggled += OnPauseToggled;
            EventBus.Instance.AbilityUnlocked += OnAbilityUnlocked;
            EventBus.Instance.SwordToggled += OnSwordToggled;
            EventBus.Instance.InteractionPromptChanged += OnInteractionPromptChanged;
            EventBus.Instance.AreaRestored += OnAreaRestored;
            EventBus.Instance.BossDefeated += OnBossDefeated;

            CallDeferred(MethodName.InitializeHUD);
        }

        // O EventBus é autoload e sobrevive à troca de cena: sem desinscrever, o handler de um HUD
        // já destruído lança ObjectDisposedException e interrompe os demais inscritos (ex.: o HUD da cena nova).
        public override void _ExitTree()
        {
            if (EventBus.Instance == null) return;
            EventBus.Instance.PlayerDied -= OnPlayerDied;
            EventBus.Instance.PlayerHealthChanged -= OnPlayerHealthChanged;
            EventBus.Instance.PauseToggled -= OnPauseToggled;
            EventBus.Instance.AbilityUnlocked -= OnAbilityUnlocked;
            EventBus.Instance.SwordToggled -= OnSwordToggled;
            EventBus.Instance.InteractionPromptChanged -= OnInteractionPromptChanged;
            EventBus.Instance.AreaRestored -= OnAreaRestored;
            EventBus.Instance.BossDefeated -= OnBossDefeated;
        }

        private void InitializeHUD()
        {
            // Chamada adiada: a cena pode ter sido trocada antes (ex.: Novo jogo seguido de outra troca)
            if (!IsInsideTree()) return;
            _player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;

            if (_player != null && _player.Health != null)
            {
                OnPlayerHealthChanged(_player.Health.CurrentHealth, _player.Health.MaxHealth);
            }
            _swordStatus?.SetState(_player?.HasSword ?? GameManager.Instance?.HasSword ?? true, false);
        }

        private void OnSwordToggled(bool hasSword)
        {
            _swordStatus?.SetState(hasSword);
        }

        public override void _Process(double delta)
        {
            if (!IsInstanceValid(_player))
            {
                InitializeHUD();
            }
        }

        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            _healthBar?.SetValues(currentHealth, maxHealth);

            if (_healthLabel != null)
            {
                _healthLabel.Text = $"{currentHealth} / {maxHealth}";
            }
        }

        private void OnInteractionPromptChanged(string prompt)
        {
            if (string.IsNullOrEmpty(prompt)) HideInteractionPrompt();
            else ShowInteractionPrompt($"[E] {prompt}");
        }

        public void ShowInteractionPrompt(string prompt)
        {
            if (_interactionPrompt != null)
            {
                _interactionPrompt.Text = prompt;
                _interactionPrompt.Visible = true;
            }
        }

        public void HideInteractionPrompt()
        {
            if (_interactionPrompt != null)
            {
                _interactionPrompt.Visible = false;
            }
        }

        public void SetAbilityDisplay(string abilityName)
        {
            if (_abilityDisplay != null)
            {
                _abilityDisplay.Text = abilityName;
                _abilityDisplay.Visible = !string.IsNullOrEmpty(abilityName);
            }
        }

        private void OnPauseToggled(bool isPaused)
        {
            if (_pauseMenu != null)
            {
                _pauseMenu.Visible = isPaused;
            }
        }

        private void OnAbilityUnlocked(string abilityId)
        {
            ShowAbilityUnlockNotification(abilityId);
        }

        private void ShowAbilityUnlockNotification(string abilityId)
        {
            GD.Print($"Ability unlocked notification: {abilityId}");
            ShowNotice($"Nova habilidade: {GetAbilityDisplayName(abilityId)}");
        }

        private void EnsureNoticeLabel()
        {
            if (_unlockNotification == null)
            {
                _unlockNotification = new Label
                {
                    Name = "AbilityUnlockNotification",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    GrowHorizontal = Control.GrowDirection.Both,
                    Visible = false
                };
                _unlockNotification.AddThemeFontSizeOverride("font_size", 28);
                _unlockNotification.AddThemeColorOverride("font_outline_color", Colors.Black);
                _unlockNotification.AddThemeConstantOverride("outline_size", 6);
                AddChild(_unlockNotification);
                _unlockNotification.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterTop);
                _unlockNotification.OffsetTop = 120;
            }
        }

        private void OnAreaRestored(string pointId) => ShowNotice($"Área restaurada · Vida máxima +{Player.PlayerController.MaxHealthPerRestoration}");

        // Aviso central temporário (habilidade nova, área restaurada, vida restaurada na bandeira)
        public void ShowNotice(string text)
        {
            EnsureNoticeLabel();
            _unlockNotification.Text = text;
            _unlockNotification.Visible = true;
            _unlockNotification.Modulate = Colors.White;

            _unlockTween?.Kill();
            _unlockTween = CreateTween();
            _unlockTween.TweenInterval(UnlockNotificationDuration);
            _unlockTween.TweenProperty(_unlockNotification, "modulate:a", 0.0f, 0.4);
            _unlockTween.TweenCallback(Callable.From(() => _unlockNotification.Visible = false));
        }

        private static string GetAbilityDisplayName(string abilityId) => abilityId switch
        {
            "Dash" => "Dash",
            "DoubleJump" => "Pulo Duplo",
            "WallJump" => "Pulo de Parede",
            _ => abilityId
        };

        private void WirePauseButton(string path, System.Action action)
        {
            var button = _pauseMenu.GetNodeOrNull<Button>(path);
            if (button != null) button.Pressed += action;
        }

        private void OnPlayerDied()
        {
            GetTree().CreateTimer(GameOverDelay).Timeout += () =>
            {
                if (IsInstanceValid(this)) ShowGameOver();
            };
        }

        // Tela de Game Over (decisão C10-T2): pausa o jogo e oferece voltar ao último checkpoint ou ao menu inicial
        public void ShowGameOver()
        {
            GD.Print("Game Over");
            if (_gameOverPanel == null) BuildGameOverPanel();

            _gameOverPanel.Visible = true;
            if (GameManager.Instance != null) GameManager.Instance.IsGameOver = true;
            GetTree().Paused = true;
            _gameOverPanel.GetNode<Button>("Box/RetryButton").GrabFocus();
        }

        private void HideGameOver()
        {
            if (_gameOverPanel != null) _gameOverPanel.Visible = false;
            if (GameManager.Instance != null) GameManager.Instance.IsGameOver = false;
            GetTree().Paused = false;
        }

        private void OnRetryPressed()
        {
            HideGameOver();
            _player ??= GetTree().GetFirstNodeInGroup("Player") as PlayerController;
            _player?.RespawnAtLastCheckpoint();
        }

        private void GoToMainMenu()
        {
            HideGameOver();
            if (GameManager.Instance != null && GameManager.Instance.IsGamePaused) GameManager.Instance.TogglePause();
            SceneManager.Instance?.LoadScene(SaveManager.MainMenuScene);
        }

        // Fim da demo (C14-T3, 11/10/2026): derrotar o Korrag mostra quantas áreas foram restauradas
        private void OnBossDefeated(string bossId)
        {
            // Só o Korrag encerra a demo; chefes do jogo completo (ex.: a cobra do Deserto) não abrem esta tela
            if (bossId != "boss_javali") return;
            GetTree().CreateTimer(DemoEndDelay, true, false, true).Timeout += () =>
            {
                if (!IsInstanceValid(this)) return;
                // Morreu no meio do caminho: o Game Over tem prioridade
                if (_gameOverPanel != null && _gameOverPanel.Visible) return;
                ShowDemoEnd(SaveManager.Instance?.RestoredCount ?? 0);
            };
        }

        public void ShowDemoEnd(int restored)
        {
            if (_demoEndPanel == null) BuildDemoEndPanel();
            restored = Mathf.Clamp(restored, 0, DemoRestorationTotal);
            _demoEndBox.GetNode<Label>("Count").Text = $"Áreas restauradas: {restored}/{DemoRestorationTotal}";
            _demoEndBox.GetNode<Label>("Message").Text = restored switch
            {
                DemoRestorationTotal => "Você restaurou toda a mata pelo caminho. A floresta volta a respirar.",
                0 => "Korrag caiu, mas a mata segue ferida. Restaurar as áreas fortalece o Kairo.",
                _ => "A floresta começou a se recuperar, mas ainda há áreas degradadas pelo caminho.",
            };
            _demoEndForestMaterial.SetShaderParameter("recovery", restored / (float)DemoRestorationTotal);
            for (int i = 0; i < DemoRestorationTotal; i++)
                _demoEndBox.GetNode<ColorRect>($"Progress/Area{i}").Color = i < restored
                    ? new Color("a9d878") : new Color("665746");
            _demoEndPanel.Visible = true;
            _demoEndPanel.Modulate = new Color(1, 1, 1, 0);
            // Mesmo bloqueio da tela de Game Over: o jogo pausa e o Esc não despausa
            if (GameManager.Instance != null) GameManager.Instance.IsGameOver = true;
            GetTree().Paused = true;
            _demoEndPanel.CreateTween().SetPauseMode(Tween.TweenPauseMode.Process)
                .TweenProperty(_demoEndPanel, "modulate:a", 1f, .65f);
            _demoEndBox.GetNode<Button>("ExploreButton").GrabFocus();
        }

        private void HideDemoEnd()
        {
            if (_demoEndPanel != null) _demoEndPanel.Visible = false;
            if (GameManager.Instance != null) GameManager.Instance.IsGameOver = false;
            GetTree().Paused = false;
        }

        private void BuildDemoEndPanel()
        {
            _demoEndPanel = new Control { Name = "DemoEnd", ProcessMode = ProcessModeEnum.Always, Visible = false };
            AddChild(_demoEndPanel);
            _demoEndPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var background = new TextureRect
            {
                Name = "Forest",
                Texture = GD.Load<Texture2D>("res://assets/ui/title/forest_dawn_v1.png"),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _demoEndForestMaterial = new ShaderMaterial
            {
                Shader = GD.Load<Shader>("res://assets/shaders/demo_end_forest.gdshader")
            };
            background.Material = _demoEndForestMaterial;
            _demoEndPanel.AddChild(background);
            background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var dim = new ColorRect { Color = new Color(0.012f, 0.03f, 0.025f, 0.50f), MouseFilter = Control.MouseFilterEnum.Stop };
            _demoEndPanel.AddChild(dim);
            dim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var center = new CenterContainer { Name = "Center", MouseFilter = Control.MouseFilterEnum.Ignore };
            _demoEndPanel.AddChild(center);
            center.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var frame = new StyleBoxFlat
            {
                BgColor = new Color(.025f, .065f, .048f, .93f),
                BorderColor = new Color(.74f, .68f, .40f, .88f),
                BorderWidthLeft = 1, BorderWidthTop = 1, BorderWidthRight = 1, BorderWidthBottom = 1,
                CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3,
                CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3
            };
            _demoEndCard = new PanelContainer { Name = "Card", CustomMinimumSize = new Vector2(620, 0) };
            _demoEndCard.AddThemeStyleboxOverride("panel", frame);
            center.AddChild(_demoEndCard);

            var margin = new MarginContainer { Name = "Margin" };
            margin.AddThemeConstantOverride("margin_left", 42);
            margin.AddThemeConstantOverride("margin_right", 42);
            margin.AddThemeConstantOverride("margin_top", 30);
            margin.AddThemeConstantOverride("margin_bottom", 30);
            _demoEndCard.AddChild(margin);

            _demoEndBox = new VBoxContainer { Name = "Box", Alignment = BoxContainer.AlignmentMode.Center };
            _demoEndBox.AddThemeConstantOverride("separation", 13);
            margin.AddChild(_demoEndBox);

            var eyebrow = new Label { Text = "A FLORESTA AINDA TEM CAMINHO", HorizontalAlignment = HorizontalAlignment.Center };
            eyebrow.AddThemeFontSizeOverride("font_size", 12);
            eyebrow.AddThemeColorOverride("font_color", new Color("b6bf94"));
            _demoEndBox.AddChild(eyebrow);

            var title = new Label { Name = "Title", Text = "Reflorescer", HorizontalAlignment = HorizontalAlignment.Center };
            title.AddThemeFontOverride("font", GD.Load<Font>("res://assets/fonts/cormorant/CormorantGaramond.ttf"));
            title.AddThemeFontSizeOverride("font_size", 52);
            title.AddThemeColorOverride("font_color", new Color("f4edce"));
            _demoEndBox.AddChild(title);

            var subtitle = new Label { Text = "FIM DA DEMO", HorizontalAlignment = HorizontalAlignment.Center };
            subtitle.AddThemeFontSizeOverride("font_size", 14);
            subtitle.AddThemeColorOverride("font_color", new Color("d6bb73"));
            _demoEndBox.AddChild(subtitle);

            var count = new Label { Name = "Count", HorizontalAlignment = HorizontalAlignment.Center };
            count.AddThemeFontSizeOverride("font_size", 25);
            count.AddThemeColorOverride("font_color", new Color("c4e69c"));
            _demoEndBox.AddChild(count);

            var progress = new HBoxContainer { Name = "Progress", Alignment = BoxContainer.AlignmentMode.Center };
            progress.AddThemeConstantOverride("separation", 10);
            _demoEndBox.AddChild(progress);
            for (int i = 0; i < DemoRestorationTotal; i++)
                progress.AddChild(new ColorRect
                {
                    Name = $"Area{i}", CustomMinimumSize = new Vector2(70, 4),
                    Color = new Color("665746"), MouseFilter = Control.MouseFilterEnum.Ignore
                });

            var message = new Label { Name = "Message", HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(520, 0),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
            message.AddThemeFontSizeOverride("font_size", 18);
            message.AddThemeColorOverride("font_color", new Color("e1e7d2"));
            _demoEndBox.AddChild(message);

            // Pós-demo (19/10): "Continuar explorando" deixa voltar pela mata ou seguir pro Deserto pelo leste do Covil
            var desertHint = new Label { Name = "DesertHint", Text = "Um caminho se abriu a leste do Covil: o Deserto espera por Kairo.",
                HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart,
                CustomMinimumSize = new Vector2(520, 0), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
            desertHint.AddThemeFontSizeOverride("font_size", 15);
            desertHint.AddThemeColorOverride("font_color", new Color("d6bb73"));
            _demoEndBox.AddChild(desertHint);

            _demoEndBox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 6) });

            StyleBoxFlat ButtonStyle(bool selected) => new()
            {
                BgColor = selected ? new Color(.20f, .29f, .19f, .92f) : new Color(.08f, .14f, .10f, .9f),
                BorderColor = selected ? new Color("d8c57d") : new Color(.42f, .51f, .36f, .8f),
                BorderWidthLeft = selected ? 2 : 1,
                CornerRadiusTopRight = 2, CornerRadiusBottomRight = 2,
                ContentMarginLeft = 18, ContentMarginRight = 18
            };
            Button StyledButton(string name, string text)
            {
                var button = new Button { Name = name, Text = text,
                    CustomMinimumSize = new Vector2(280, 41), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
                button.AddThemeStyleboxOverride("normal", ButtonStyle(false));
                button.AddThemeStyleboxOverride("hover", ButtonStyle(true));
                button.AddThemeStyleboxOverride("focus", ButtonStyle(true));
                button.AddThemeStyleboxOverride("pressed", ButtonStyle(true));
                button.AddThemeColorOverride("font_color", new Color("e8e7ce"));
                button.AddThemeColorOverride("font_focus_color", new Color("fff4ca"));
                return button;
            }

            var explore = StyledButton("ExploreButton", "Continuar explorando");
            explore.Pressed += HideDemoEnd;
            _demoEndBox.AddChild(explore);

            var menu = StyledButton("MenuButton", "Menu inicial");
            menu.Pressed += () => { HideDemoEnd(); GoToMainMenu(); };
            _demoEndBox.AddChild(menu);

            var quit = StyledButton("QuitButton", "Sair");
            quit.Pressed += () => GetTree().Quit();
            _demoEndBox.AddChild(quit);
        }

        private void BuildGameOverPanel()
        {
            _gameOverPanel = new Control { Name = "GameOver", ProcessMode = ProcessModeEnum.Always, Visible = false };
            AddChild(_gameOverPanel);
            _gameOverPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var dim = new ColorRect { Color = new Color(0, 0, 0, 0.7f), MouseFilter = Control.MouseFilterEnum.Stop };
            _gameOverPanel.AddChild(dim);
            dim.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var box = new VBoxContainer { Name = "Box", Alignment = BoxContainer.AlignmentMode.Center };
            box.AddThemeConstantOverride("separation", 16);
            _gameOverPanel.AddChild(box);
            box.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            var title = new Label { Text = "Você caiu", HorizontalAlignment = HorizontalAlignment.Center };
            title.AddThemeFontSizeOverride("font_size", 48);
            box.AddChild(title);

            var retry = new Button { Name = "RetryButton", Text = "Tentar de novo", CustomMinimumSize = new Vector2(240, 44), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
            retry.Pressed += OnRetryPressed;
            box.AddChild(retry);

            var menu = new Button { Name = "MenuButton", Text = "Menu inicial", CustomMinimumSize = new Vector2(240, 44), SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter };
            menu.Pressed += GoToMainMenu;
            box.AddChild(menu);
        }

        public void HideAll()
        {
            if (_healthBar != null) _healthBar.Visible = false;
            if (_healthLabel != null) _healthLabel.Visible = false;
            if (_interactionPrompt != null) _interactionPrompt.Visible = false;
            if (_abilityDisplay != null) _abilityDisplay.Visible = false;
            if (_swordStatus != null) _swordStatus.Visible = false;
        }

        public void ShowAll()
        {
            if (_healthBar != null) _healthBar.Visible = true;
            if (_healthLabel != null) _healthLabel.Visible = true;
            if (_swordStatus != null) _swordStatus.Visible = true;
        }
    }
}
