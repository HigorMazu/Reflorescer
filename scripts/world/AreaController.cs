using Godot;
using Joguim.Core;

namespace Joguim.World
{
    public partial class AreaController : Node2D
    {
        [Export] public string AreaName = "Area_01";
        // Música de fundo do bioma (arquivo em res://assets/audio/music/). Vazio = sem música.
        [Export] public string MusicTrack = "";
        [Export] public Node2D[] SpawnPoints;
        [Export] public Node2D[] EnemySpawns;
        [Export] public Node2D[] Checkpoints;

        public string CurrentAreaName => AreaName;

        public override void _Ready()
        {
            GameManager.Instance?.SetCurrentArea(AreaName);
            if (!string.IsNullOrEmpty(MusicTrack)) AudioManager.Instance?.PlayMusic(MusicTrack);
        }

        public Vector2 GetRespawnPosition()
        {
            if (SpawnPoints != null && SpawnPoints.Length > 0)
            {
                return SpawnPoints[0].GlobalPosition;
            }
            return GlobalPosition;
        }

        public Vector2 GetCheckpointPosition(string checkpointId)
        {
            if (Checkpoints == null) return GetRespawnPosition();

            foreach (var checkpoint in Checkpoints)
            {
                if (checkpoint is World.Checkpoint cp && cp.CheckpointId == checkpointId && cp.Activated)
                {
                    return cp.GlobalPosition;
                }
            }

            return GetRespawnPosition();
        }
    }
}
