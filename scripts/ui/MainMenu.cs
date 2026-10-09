using Godot;
using Joguim.Save;
using System;

namespace Joguim.UI
{
    // Tela inicial (decisão C09-T2): "Novo jogo", "Continuar" (só com save existente) e "Sair"
    public partial class MainMenu : Control
    {
        [Export] public NodePath NewGameButtonPath;
        [Export] public NodePath ContinueButtonPath;
        [Export] public NodePath QuitButtonPath;

        private const int SaveSlot = 0;
        private bool _leaving;

        public override void _Ready()
        {
            GetTree().Paused = false;

            var newGame = GetNodeOrNull<Button>(NewGameButtonPath);
            var continueButton = GetNodeOrNull<Button>(ContinueButtonPath);
            var quit = GetNodeOrNull<Button>(QuitButtonPath);

            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSave(SaveSlot);

            if (newGame != null) newGame.Pressed += () => TransitionTo(() =>
            {
                if (SaveManager.Instance == null) return false;
                SaveManager.Instance.StartNewGame(SaveSlot);
                return true;
            });
            if (continueButton != null)
            {
                continueButton.Visible = hasSave;
                continueButton.Pressed += () => TransitionTo(() => SaveManager.Instance?.ContinueGame(SaveSlot) ?? false);
            }
            if (quit != null) quit.Pressed += () => TransitionTo(() => { GetTree().Quit(); return true; });

            // foco no teclado: Continuar quando existe save, senão Novo jogo
            if (hasSave && continueButton != null) continueButton.GrabFocus();
            else newGame?.GrabFocus();
        }

        private async void TransitionTo(Func<bool> action)
        {
            if (_leaving) return;
            _leaving = true;
            var presentation = GetNode<Node>("Presentation");
            presentation.Call("begin_exit");
            foreach (var child in GetNode("Box").GetChildren())
                if (child is Button button) button.Disabled = true;
            await ToSignal(GetTree().CreateTimer(.44), SceneTreeTimer.SignalName.Timeout);
            if (!IsInsideTree()) return;
            if (action()) return;
            // A missing/unreadable save must not leave the user on a black screen.
            _leaving = false;
            presentation.Call("cancel_exit");
            foreach (var child in GetNode("Box").GetChildren())
                if (child is Button button) button.Disabled = false;
            GetNode<Button>(NewGameButtonPath).GrabFocus();
        }
    }
}
