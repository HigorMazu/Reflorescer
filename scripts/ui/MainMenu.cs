using Godot;
using Joguim.Save;

namespace Joguim.UI
{
    // Tela inicial (decisão C09-T2): "Novo jogo", "Continuar" (só com save existente) e "Sair"
    public partial class MainMenu : Control
    {
        [Export] public NodePath NewGameButtonPath;
        [Export] public NodePath ContinueButtonPath;
        [Export] public NodePath QuitButtonPath;

        private const int SaveSlot = 0;

        public override void _Ready()
        {
            GetTree().Paused = false;

            var newGame = GetNodeOrNull<Button>(NewGameButtonPath);
            var continueButton = GetNodeOrNull<Button>(ContinueButtonPath);
            var quit = GetNodeOrNull<Button>(QuitButtonPath);

            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSave(SaveSlot);

            if (newGame != null) newGame.Pressed += () => SaveManager.Instance?.StartNewGame(SaveSlot);
            if (continueButton != null)
            {
                continueButton.Visible = hasSave;
                continueButton.Pressed += () => SaveManager.Instance?.ContinueGame(SaveSlot);
            }
            if (quit != null) quit.Pressed += () => GetTree().Quit();

            // foco no teclado: Continuar quando existe save, senão Novo jogo
            if (hasSave && continueButton != null) continueButton.GrabFocus();
            else newGame?.GrabFocus();
        }
    }
}
