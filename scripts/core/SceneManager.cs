using Godot;
using System;

namespace Joguim.Core
{
    public partial class SceneManager : Node
    {
        public static SceneManager Instance { get; private set; }

        private string _currentScenePath = "";

        public string CurrentScenePath => _currentScenePath;

        public override void _Ready()
        {
            Instance = this;
            _currentScenePath = GetTree().CurrentScene?.SceneFilePath ?? "";
        }

        public void LoadScene(string scenePath)
        {
            if (!ResourceLoader.Exists(scenePath))
            {
                GD.PrintErr($"SceneManager: Scene not found at {scenePath}");
                return;
            }

            _currentScenePath = scenePath;
            GetTree().ChangeSceneToFile(scenePath);
        }

        public void ReloadCurrentScene()
        {
            if (string.IsNullOrEmpty(_currentScenePath))
            {
                GD.PrintErr("SceneManager: No scene to reload.");
                return;
            }
            LoadScene(_currentScenePath);
        }

        public void LoadSceneWithTransition(string scenePath)
        {
            LoadScene(scenePath);
        }
    }
}
