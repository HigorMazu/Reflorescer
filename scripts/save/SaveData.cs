using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Joguim.Save
{
    public class SaveData
    {
        public int Slot { get; set; }
        public string Timestamp { get; set; }
        public string CurrentScene { get; set; }
        public string CurrentArea { get; set; }

        // Player data
        public Vector2 PlayerPosition { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerMaxHealth { get; set; }
        public string LastCheckpointId { get; set; }

        // Progression
        public List<string> UnlockedAbilities { get; set; } = new();
        public List<string> CollectedItems { get; set; } = new();
        public List<string> DefeatedEnemies { get; set; } = new();
        public List<string> DefeatedBosses { get; set; } = new();
        public List<string> ActivatedCheckpoints { get; set; } = new();
        public List<string> UnlockedShortcuts { get; set; } = new();

        // Stats
        public int TotalDeaths { get; set; }
        public float TotalPlayTime { get; set; }

        public SaveData()
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CurrentScene = "res://scenes/Main.tscn";
            CurrentArea = "Area_01";
            PlayerHealth = 100;
            PlayerMaxHealth = 100;
            LastCheckpointId = "";
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public static SaveData FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<SaveData>(json);
            }
            catch (Exception e)
            {
                GD.PrintErr($"SaveData: Failed to parse JSON: {e.Message}");
                return null;
            }
        }
    }
}
