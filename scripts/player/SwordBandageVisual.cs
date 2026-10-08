using Godot;

namespace Joguim.Player
{
    // A small cloth wrap on Kairo's right wrist while the Grass Sword is stowed.
    // Drawn in game units so it follows the animated hand without a second fist sprite.
    public partial class SwordBandageVisual : Node2D
    {
        private PlayerController _player;
        private GrassSwordVisual _sword;

        public override void _Ready()
        {
            _player = GetNode<PlayerController>("../../../..");
            _sword = GetNode<GrassSwordVisual>("../Sword");
            UpdateState();
        }

        public override void _Process(double delta) => UpdateState();

        private void UpdateState()
        {
            if (_player?.Sprite == null || _sword == null) return;

            string pose = _player.Sprite.Animation;
            Visible = !_player.HasSword && _sword.Extension <= .001f
                && pose is not ("dead" or "hurt" or "attack");
            if (!Visible) return;

            // RightHand remains attached to the fist; shift the wrap toward the forearm.
            Position = pose switch
            {
                "idle" => new Vector2(4, _player.Sprite.Frame == 0 ? -12 : -13),
                "run" => new Vector2(4, _player.Sprite.Frame == 0 ? -11 : -14),
                "jump" => new Vector2(4, -21),
                "fall" => new Vector2(4, 0),
                _ => new Vector2(4, -12)
            };
        }

        public override void _Draw()
        {
            // Three staggered linen bands, bordered in olive to read at native scale.
            DrawRect(new Rect2(-3.5f, -2, 7, 4), new Color("29331e"));
            DrawRect(new Rect2(-3, -1.5f, 6, 1), new Color("d8d1a5"));
            DrawRect(new Rect2(-3, -.5f, 6, 1), new Color("a4ae72"));
            DrawRect(new Rect2(-3, .5f, 6, 1), new Color("e8ddb2"));
            DrawLine(new Vector2(-1.5f, -1.5f), new Vector2(.5f, 1.5f), new Color("677342"), .5f);
        }
    }
}
