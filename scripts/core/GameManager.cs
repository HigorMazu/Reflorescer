using Godot;
using System;
using Joguim.Save;

namespace Joguim.Core
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public bool IsGamePaused { get; private set; }
        public string CurrentArea { get; set; } = "";
        // Estado da Espada de Grama na sessão: o Player é recriado a cada troca de cena
        public bool HasSword { get; set; } = true;
        // Tela de Game Over aberta: o jogo fica pausado e o Esc não despausa
        public bool IsGameOver { get; set; }
        // Vida do Kairo na sessão: o Player é recriado a cada troca de cena e nascia com a vida cheia.
        // Desde 11/10/2026 trocar de área não cura; só morrer (voltar à bandeira) ou um jogo novo enchem a vida.
        // -1 (ou 0, depois de morrer) = nascer com a vida cheia.
        public int CarriedPlayerHealth { get; set; } = -1;

        public override void _Ready()
        {
            Instance = this;
            ProcessMode = ProcessModeEnum.Always;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("pause"))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (IsGameOver) return;
            IsGamePaused = !IsGamePaused;
            GetTree().Paused = IsGamePaused;
            EventBus.Instance.EmitSignal("PauseToggled", IsGamePaused);
        }

        public void SetCurrentArea(string areaName)
        {
            CurrentArea = areaName;
            EventBus.Instance.EmitSignal("AreaChanged", areaName);
        }

        public void RestartGame()
        {
            IsGamePaused = false;
            GetTree().Paused = false;
            CarriedPlayerHealth = -1;
            SaveManager.Instance.DeleteSave(0);
            SceneManager.Instance.LoadScene(SaveManager.NewGameScene);
        }
    }
}
