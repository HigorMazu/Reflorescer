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

        private HealthBar _healthBar;
        private Label _healthLabel;
        private Label _interactionPrompt;
        private Label _abilityDisplay;
        private Control _pauseMenu;
        private Label _unlockNotification;
        private Tween _unlockTween;

        private Control _gameOverPanel;

        private const float UnlockNotificationDuration = 2.5f;
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

            AddToGroup(GroupName);

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
        }

        private void InitializeHUD()
        {
            _player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;

            if (_player != null && _player.Health != null)
            {
                OnPlayerHealthChanged(_player.Health.CurrentHealth, _player.Health.MaxHealth);
            }
            if (_player != null) OnSwordToggled(_player.HasSword);
        }

        private void OnSwordToggled(bool hasSword)
        {
            SetAbilityDisplay(hasSword ? "Espada: ativada [Q]" : "Espada: guardada [Q]");
        }

        public override void _Process(double delta)
        {
            if (_player == null)
            {
                _player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;
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

            _unlockNotification.Text = $"Nova habilidade: {GetAbilityDisplayName(abilityId)}";
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
        }

        public void ShowAll()
        {
            if (_healthBar != null) _healthBar.Visible = true;
            if (_healthLabel != null) _healthLabel.Visible = true;
        }
    }
}
