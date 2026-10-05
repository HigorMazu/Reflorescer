using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Joguim.Abilities;
using Joguim.Core;
using Joguim.Player;

namespace Joguim.Save
{
    public partial class SaveManager : Node
    {
        public static SaveManager Instance { get; private set; }

        private const string SavePath = "user://save_";
        private const string SaveExtension = ".json";
        private const int MaxSlots = 3;

        // Início da demo (EPIC-C13). O TestLevel (Main.tscn) continua só como cena de teste isolada.
        public const string NewGameScene = "res://scenes/areas/SopeDaMata.tscn";
        public const string MainMenuScene = "res://scenes/ui/MainMenu.tscn";

        private SaveData _currentSave;

        public override void _Ready()
        {
            Instance = this;

            EventBus.Instance.CheckpointActivated += OnCheckpointActivated;
            EventBus.Instance.BossDefeated += OnBossDefeated;
            EventBus.Instance.ItemCollected += OnItemCollected;
            EventBus.Instance.AbilityUnlocked += OnAbilityUnlocked;
            EventBus.Instance.AreaRestored += OnAreaRestored;
        }

        // Consultado pelos RestorationPoint ao entrarem numa cena, pra nascer já restaurados
        public bool IsPointRestored(string pointId)
        {
            return _currentSave != null && _currentSave.RestoredPoints.Contains(pointId);
        }

        public bool IsBossDefeated(string bossId)
        {
            return _currentSave != null && _currentSave.DefeatedBosses.Contains(bossId);
        }

        // Menu inicial → "Novo jogo": apaga o save e zera o estado da sessão que vive em autoloads
        public void StartNewGame(int slot = 0)
        {
            DeleteSave(slot);
            _currentSave = null;
            AbilityManager.Instance?.ResetToInitial();
            if (GameManager.Instance != null) GameManager.Instance.HasSword = true;
            SceneManager.Instance.LoadScene(NewGameScene);
        }

        // Menu inicial → "Continuar": restaura o estado de sessão já, troca pra cena salva
        // e aplica posição/checkpoints/etc. quando a cena nova estiver pronta (SceneTree.SceneChanged)
        public bool ContinueGame(int slot = 0)
        {
            var save = ReadSave(slot);
            if (save == null) return false;

            _currentSave = save;
            if (GameManager.Instance != null) GameManager.Instance.HasSword = save.HasSword;
            AbilityManager.Instance?.SetUnlockedAbilities(ParseAbilities(save.UnlockedAbilities));

            string scene = !string.IsNullOrEmpty(save.CurrentScene) && save.CurrentScene != MainMenuScene && ResourceLoader.Exists(save.CurrentScene)
                ? save.CurrentScene
                : NewGameScene;
            GetTree().Connect(SceneTree.SignalName.SceneChanged, Callable.From(ApplyCurrentSave), (uint)ConnectFlags.OneShot);
            SceneManager.Instance.LoadScene(scene);
            GD.Print($"Continuando do slot {slot} em {scene}.");
            return true;
        }

        private void ApplyCurrentSave() => ApplySaveData(_currentSave);

        public bool HasSave(int slot)
        {
            return FileAccess.FileExists(GetSavePath(slot));
        }

        public SaveData CreateNewSave(int slot)
        {
            _currentSave = new SaveData
            {
                Slot = slot,
                PlayerHealth = 100,
                PlayerMaxHealth = 100,
                CurrentScene = NewGameScene,
                CurrentArea = "SopeDaMata"
            };

            return _currentSave;
        }

        public void SaveGame(int slot)
        {
            var player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;
            if (player != null)
            {
                if (_currentSave == null)
                {
                    _currentSave = CreateNewSave(slot);
                }

                _currentSave.PlayerPosition = player.GlobalPosition;
                _currentSave.PlayerHealth = player.Health?.CurrentHealth ?? 100;
                _currentSave.PlayerMaxHealth = player.Health?.MaxHealth ?? 100;
                _currentSave.CurrentScene = SceneManager.Instance.CurrentScenePath;
                _currentSave.HasSword = GameManager.Instance?.HasSword ?? true;
                _currentSave.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                _currentSave.UnlockedAbilities.Clear();
                foreach (var ability in AbilityManager.Instance.GetAllUnlockedAbilities())
                {
                    _currentSave.UnlockedAbilities.Add(ability.ToString());
                }

                SaveToFile(slot, _currentSave);
                GD.Print($"Game saved to slot {slot}.");
            }
            else
            {
                GD.PrintErr("SaveManager: Player not found. Cannot save.");
            }
        }

        public SaveData LoadGame(int slot)
        {
            _currentSave = ReadSave(slot);
            if (_currentSave == null) return null;

            ApplySaveData(_currentSave);
            GD.Print($"Game loaded from slot {slot}.");

            return _currentSave;
        }

        private SaveData ReadSave(int slot)
        {
            string path = GetSavePath(slot);
            if (!FileAccess.FileExists(path))
            {
                GD.PrintErr($"SaveManager: Save file not found at {path}");
                return null;
            }

            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"SaveManager: Failed to open save file at {path}");
                return null;
            }

            string json = file.GetAsText();
            file.Close();

            var save = SaveData.FromJson(json);
            if (save == null) GD.PrintErr("SaveManager: Failed to parse save data.");
            return save;
        }

        private static List<AbilityId> ParseAbilities(List<string> names)
        {
            var abilities = new List<AbilityId>();
            foreach (var name in names)
            {
                if (Enum.TryParse<AbilityId>(name, out var abilityId)) abilities.Add(abilityId);
            }
            return abilities;
        }

        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            if (FileAccess.FileExists(path))
            {
                DirAccess.RemoveAbsolute(path);
                GD.Print($"Save slot {slot} deleted.");
            }
        }

        public List<SaveData> GetAllSaves()
        {
            var saves = new List<SaveData>();

            for (int i = 0; i < MaxSlots; i++)
            {
                if (HasSave(i))
                {
                    var file = FileAccess.Open(GetSavePath(i), FileAccess.ModeFlags.Read);
                    if (file != null)
                    {
                        string json = file.GetAsText();
                        file.Close();

                        var save = SaveData.FromJson(json);
                        if (save != null)
                        {
                            saves.Add(save);
                        }
                    }
                }
            }

            return saves;
        }

        public void ApplySaveData(SaveData save)
        {
            if (save == null) return;

            var player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;
            if (player != null)
            {
                // Saves antigos gravaram a posição como {} (0,0): nesse caso fica no spawn da cena
                if (save.PlayerPosition != Vector2.Zero) player.GlobalPosition = save.PlayerPosition;
                player.Health?.SetMaxHealth(save.PlayerMaxHealth, true);
                player.Health?.Heal(save.PlayerHealth);
            }

            // Restaura sem emitir AbilityUnlocked: carregar um save não é desbloquear de novo
            AbilityManager.Instance.SetUnlockedAbilities(ParseAbilities(save.UnlockedAbilities));

            foreach (var checkpointId in save.ActivatedCheckpoints)
            {
                var checkpoints = GetTree().GetNodesInGroup("Checkpoints");
                foreach (var checkpoint in checkpoints)
                {
                    if (checkpoint is World.Checkpoint cp && cp.CheckpointId == checkpointId)
                    {
                        cp.Activated = true;
                        // respawn pós-Continuar volta pro último checkpoint ativado
                        if (checkpointId == save.LastCheckpointId) player?.SetRespawnPoint(cp.GlobalPosition);
                    }
                }
            }

            foreach (var node in GetTree().GetNodesInGroup(World.RestorationPoint.GroupName))
            {
                if (node is World.RestorationPoint point && save.RestoredPoints.Contains(point.PointId))
                {
                    point.SetRestoredWithoutEffect();
                }
            }
        }

        public void SaveCurrentGame(int slot = 0)
        {
            SaveGame(slot);
        }

        private string GetSavePath(int slot)
        {
            return $"{SavePath}{slot}{SaveExtension}";
        }

        private void SaveToFile(int slot, SaveData save)
        {
            string path = GetSavePath(slot);
            string json = save.ToJson();

            var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr($"SaveManager: Failed to create save file at {path}");
                return;
            }

            file.StoreString(json);
            file.Close();
        }

        private void OnCheckpointActivated(Vector2 position, string checkpointId)
        {
            if (_currentSave == null)
            {
                _currentSave = CreateNewSave(0);
            }

            _currentSave.LastCheckpointId = checkpointId;
            if (!_currentSave.ActivatedCheckpoints.Contains(checkpointId))
            {
                _currentSave.ActivatedCheckpoints.Add(checkpointId);
            }

            _currentSave.PlayerPosition = position;
            SaveGame(_currentSave.Slot);
        }

        private void OnBossDefeated(string bossId)
        {
            if (_currentSave == null) return;

            if (!_currentSave.DefeatedBosses.Contains(bossId))
            {
                _currentSave.DefeatedBosses.Add(bossId);
            }

            SaveGame(_currentSave.Slot);
        }

        private void OnItemCollected(string itemId, string itemType)
        {
            if (_currentSave == null) return;

            if (!_currentSave.CollectedItems.Contains(itemId))
            {
                _currentSave.CollectedItems.Add(itemId);
            }

            SaveGame(_currentSave.Slot);
        }

        private void OnAreaRestored(string pointId)
        {
            if (_currentSave == null)
            {
                _currentSave = CreateNewSave(0);
            }

            if (!_currentSave.RestoredPoints.Contains(pointId))
            {
                _currentSave.RestoredPoints.Add(pointId);
            }

            SaveGame(_currentSave.Slot);
        }

        private void OnAbilityUnlocked(string abilityId)
        {
            // Persiste imediatamente: fechar o jogo antes do próximo checkpoint não perde o unlock
            if (_currentSave == null)
            {
                _currentSave = CreateNewSave(0);
            }

            SaveGame(_currentSave.Slot);
        }
    }
}
