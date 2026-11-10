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
        // Cena da última bandeira ativada: morrer em outra área recarrega esta cena (decisão de 11/10/2026)
        public string LastCheckpointScene { get; set; } = "";
        public bool HasSword { get; set; } = true;

        // Progression
        public List<string> UnlockedAbilities { get; set; } = new();
        public List<string> CollectedItems { get; set; } = new();
        public List<string> DefeatedEnemies { get; set; } = new();
        public List<string> DefeatedBosses { get; set; } = new();
        public List<string> ActivatedCheckpoints { get; set; } = new();
        public List<string> RestoredPoints { get; set; } = new();
        public List<string> UnlockedShortcuts { get; set; } = new();
        // Células do mapa vistas por Kairo; a lista vazia mantém saves antigos compatíveis.
        public List<string> ExploredMapCells { get; set; } = new();

        // Essência e níveis de upgrade (C20-T5); saves antigos carregam com os valores padrão.
        public int Essence { get; set; }
        public Dictionary<string, int> UpgradeLevels { get; set; } = new();

        // Stats
        public int TotalDeaths { get; set; }
        public float TotalPlayTime { get; set; }

        public SaveData()
        {
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            CurrentScene = SaveManager.NewGameScene;
            CurrentArea = "SopeDaMata";
            PlayerHealth = 100;
            PlayerMaxHealth = 100;
            LastCheckpointId = "";
        }

        // O totem saiu do Dossel Vivo (09/11/2026): a bandeira antiga não existe mais na cena.
        public void MigrateLegacyCheckpoint()
        {
            const string legacyId = "cp_dossel_01";
            ActivatedCheckpoints?.Remove(legacyId);
            if (LastCheckpointId != legacyId) return;
            LastCheckpointId = "";
            LastCheckpointScene = "";
        }

        // IncludeFields: o Vector2 do Godot guarda X/Y como campos; sem isso a posição salvava como {}
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true
        };

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonOptions);
        }

        public static SaveData FromJson(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
            }
            catch (Exception e)
            {
                GD.PrintErr($"SaveData: Failed to parse JSON: {e.Message}");
                return null;
            }
        }
    }
}
