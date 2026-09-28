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

        private SaveData _currentSave;

        public override void _Ready()
        {
            Instance = this;

            EventBus.Instance.CheckpointActivated += OnCheckpointActivated;
            EventBus.Instance.BossDefeated += OnBossDefeated;
            EventBus.Instance.ItemCollected += OnItemCollected;
            EventBus.Instance.AbilityUnlocked += OnAbilityUnlocked;
        }

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
                CurrentScene = "res://scenes/Main.tscn",
                CurrentArea = "Area_01"
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

            _currentSave = SaveData.FromJson(json);
            if (_currentSave == null)
            {
                GD.PrintErr("SaveManager: Failed to parse save data.");
                return null;
            }

            ApplySaveData(_currentSave);
            GD.Print($"Game loaded from slot {slot}.");

            return _currentSave;
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
                player.GlobalPosition = save.PlayerPosition;
                player.Health?.SetMaxHealth(save.PlayerMaxHealth, true);
                player.Health?.Heal(save.PlayerHealth);
            }

            // Restaura sem emitir AbilityUnlocked: carregar um save não é desbloquear de novo
            var abilities = new List<AbilityId>();
            foreach (var abilityStr in save.UnlockedAbilities)
            {
                if (Enum.TryParse<AbilityId>(abilityStr, out var abilityId))
                {
                    abilities.Add(abilityId);
                }
            }
            AbilityManager.Instance.SetUnlockedAbilities(abilities);

            foreach (var checkpointId in save.ActivatedCheckpoints)
            {
                var checkpoints = GetTree().GetNodesInGroup("Checkpoints");
                foreach (var checkpoint in checkpoints)
                {
                    if (checkpoint is World.Checkpoint cp && cp.CheckpointId == checkpointId)
                    {
                        cp.Activated = true;
                    }
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
