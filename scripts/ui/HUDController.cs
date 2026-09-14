using Godot;
using Joguim.Core;
using Joguim.Player;

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

        private PlayerController _player;

        public override void _Ready()
        {
            _healthBar = GetNodeOrNull<HealthBar>(HealthBarPath);
            _healthLabel = GetNodeOrNull<Label>(HealthLabelPath);
            _interactionPrompt = GetNodeOrNull<Label>(InteractionPromptPath);
            _abilityDisplay = GetNodeOrNull<Label>(AbilityDisplayPath);
            _pauseMenu = GetNodeOrNull<Control>(PauseMenuPath);

            if (_pauseMenu != null)
            {
                _pauseMenu.Visible = false;
            }

            EventBus.Instance.PlayerHealthChanged += OnPlayerHealthChanged;
            EventBus.Instance.PauseToggled += OnPauseToggled;
            EventBus.Instance.AbilityUnlocked += OnAbilityUnlocked;

            CallDeferred(MethodName.InitializeHUD);
        }

        private void InitializeHUD()
        {
            _player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;

            if (_player != null && _player.Health != null)
            {
                OnPlayerHealthChanged(_player.Health.CurrentHealth, _player.Health.MaxHealth);
            }
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
        }

        public void ShowGameOver()
        {
            GD.Print("Game Over");
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
