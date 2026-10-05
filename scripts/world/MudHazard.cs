using Godot;
using Joguim.Player;

namespace Joguim.World
{
    // Terreno de lama: enquanto o Kairo está dentro da área, a velocidade de corrida dele é multiplicada.
    // Só afeta a corrida (CurrentMoveSpeed); pulo, dash e pulo de parede ficam iguais.
    public partial class MudHazard : Area2D
    {
        [Export(PropertyHint.Range, "0.1,1,0.05")] public float SpeedMultiplier = 0.55f;

        private PlayerController _player;

        public override void _Ready()
        {
            BodyEntered += OnBodyEntered;
            BodyExited += OnBodyExited;
        }

        public override void _ExitTree()
        {
            if (_player != null && IsInstanceValid(_player)) _player.ClearTerrainSpeedModifier(this);
            _player = null;
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body is not PlayerController player) return;
            _player = player;
            player.SetTerrainSpeedModifier(this, SpeedMultiplier);
        }

        private void OnBodyExited(Node2D body)
        {
            if (body is not PlayerController player) return;
            player.ClearTerrainSpeedModifier(this);
            _player = null;
        }
    }
}
