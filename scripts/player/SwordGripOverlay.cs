using Godot;
using System.Collections.Generic;

namespace Joguim.Player
{
    // Reuses the fist already drawn in each Kairo frame, placing it above the sword handle.
    public partial class SwordGripOverlay : Sprite2D
    {
        private PlayerController _player;
        private GrassSwordVisual _sword;
        private string _lastTexturePath;
        private readonly Dictionary<string, Texture2D> _textures = new();

        public override void _Ready()
        {
            _player = GetNode<PlayerController>("../..");
            _sword = GetNode<GrassSwordVisual>("../RightArm/RightHand/Sword");
        }

        public override void _Process(double delta)
        {
            if (_player?.Sprite == null || _sword == null) return;
            var stateName = _player.Sprite.Animation.ToString();
            Visible = _sword.Extension > .001f && stateName != "dead" && stateName != "hurt";
            if (!Visible) return;
            Position = _sword.GripPosition;
            string state = stateName;
            string path = $"res://assets/sprites/kairo_faisca/kairo_grip_{state}_{_player.Sprite.Frame:00}.png";
            if (path == _lastTexturePath) return;
            if (!_textures.TryGetValue(path, out var texture))
            {
                texture = GD.Load<Texture2D>(path);
                _textures[path] = texture;
            }
            Texture = texture;
            _lastTexturePath = path;
        }
    }
}
