using Godot;
using System;
using Joguim.Save;

namespace Joguim.Core
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }

        public bool IsGamePaused { get; private set; }
        public bool IsMapOpen { get; private set; }
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
            // Teclado ou controle: as dicas da tela seguem o último usado (26/10/2026)
            Joguim.UI.InputHints.Observe(@event);
            if (@event.IsActionPressed("map") && !@event.IsEcho())
            {
                ToggleMap();
                GetViewport().SetInputAsHandled();
                return;
            }
            if (@event.IsActionPressed("pause"))
            {
                if (IsMapOpen)
                {
                    ToggleMap();
                    GetViewport().SetInputAsHandled();
                    return;
                }
                TogglePause();
            }
        }

        // Pra testes em GDScript (a classe InputHints é estática)
        public string GetInputHint(string action) => Joguim.UI.InputHints.Label(action);
        public void SetInputHintsForTest(bool gamepad, int style) =>
            Joguim.UI.InputHints.Set(gamepad, (Joguim.UI.InputHints.PadStyle)style);

        public void ToggleMap()
        {
            if (IsGameOver || IsGamePaused) return;
            if (GetTree().CurrentScene?.SceneFilePath == SaveManager.MainMenuScene) return;
            if (!IsMapOpen)
            {
                // Só abre onde existe mapa; senão o jogo pausaria sem nada na tela (C16-T3)
                var overlay = GetTree().CurrentScene?.GetNodeOrNull("HUD/WorldMapOverlay");
                if (overlay == null || !overlay.Call("has_map").AsBool()) return;
            }
            IsMapOpen = !IsMapOpen;
            GetTree().Paused = IsMapOpen;
        }

        public bool IsWorldMapOpen() => IsMapOpen;

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
            IsMapOpen = false;
            IsGamePaused = false;
            GetTree().Paused = false;
            CarriedPlayerHealth = -1;
            SaveManager.Instance?.StartNewGame(0);
        }
    }
}
