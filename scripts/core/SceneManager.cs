using Godot;
using System;

namespace Joguim.Core
{
    public partial class SceneManager : Node
    {
        public static SceneManager Instance { get; private set; }

        public const string SpawnPointsGroup = "SpawnPoints";

        private string _currentScenePath = "";
        private string _pendingSpawnPoint = "";
        private Vector2 _pendingSpawnOffset = Vector2.Zero;

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

        // Carrega a cena e, quando o jogador nascer nela, posiciona no spawn point de nome spawnPoint (+ offset)
        public void LoadSceneAtSpawn(string scenePath, string spawnPoint, Vector2 spawnOffset)
        {
            _pendingSpawnPoint = spawnPoint ?? "";
            _pendingSpawnOffset = spawnOffset;
            LoadScene(scenePath);
        }

        // Chamado pelo jogador ao entrar na cena nova. Procura um nó do grupo "SpawnPoints" com o nome pendente.
        public bool TryConsumePendingSpawn(out Vector2 position)
        {
            position = Vector2.Zero;
            if (string.IsNullOrEmpty(_pendingSpawnPoint)) return false;

            string spawnName = _pendingSpawnPoint;
            Vector2 offset = _pendingSpawnOffset;
            _pendingSpawnPoint = "";
            _pendingSpawnOffset = Vector2.Zero;

            foreach (var node in GetTree().GetNodesInGroup(SpawnPointsGroup))
            {
                if (node is Node2D spawn && spawn.Name == spawnName)
                {
                    position = spawn.GlobalPosition + offset;
                    return true;
                }
            }

            GD.PrintErr($"SceneManager: spawn point '{spawnName}' não encontrado na cena {_currentScenePath}. Usando o spawn padrão.");
            return false;
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
