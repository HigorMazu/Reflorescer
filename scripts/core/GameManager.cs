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
            SaveManager.Instance.DeleteSave(0);
            SceneManager.Instance.LoadScene("res://scenes/Main.tscn");
        }
    }
}
