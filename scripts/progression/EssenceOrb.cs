using Godot;
using Joguim.Save;

namespace Joguim.Progression
{
    /// <summary>Luz solta pelo mob morto: voa até o Kairo e credita a essência ao chegar (C20-T5).</summary>
    public partial class EssenceOrb : Node2D
    {
        private const float FloatTime = 0.15f;
        private const float MaxLifetime = 1.5f;
        private const float RiseSpeed = 40f;
        private const float HomingSpeed = 420f;
        private const float CollectDistance = 18f;

        private int _amount;
        private float _age;
        private bool _credited;

        public static void Spawn(Node parent, Vector2 position, int amount)
        {
            if (parent == null || amount <= 0) return;
            var orb = new EssenceOrb { _amount = amount, ZIndex = 6 };
            parent.AddChild(orb);
            orb.GlobalPosition = position;
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;
            _age += dt;
            QueueRedraw();

            if (_age < FloatTime)
            {
                Position += Vector2.Up * RiseSpeed * dt;
                return;
            }

            var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            if (player == null || _age >= MaxLifetime)
            {
                Collect();
                return;
            }

            GlobalPosition = GlobalPosition.MoveToward(player.GlobalPosition, HomingSpeed * dt);
            if (GlobalPosition.DistanceTo(player.GlobalPosition) <= CollectDistance) Collect();
        }

        // Trocar de área ou fechar a cena com a luz no ar não perde a essência.
        public override void _ExitTree() => Credit();

        private void Collect()
        {
            Credit();
            QueueFree();
        }

        private void Credit()
        {
            if (_credited) return;
            _credited = true;
            SaveManager.Instance?.AddEssence(_amount);
        }

        public override void _Draw()
        {
            float pulse = 1f + Mathf.Sin(_age * 18f) * 0.15f;
            DrawCircle(Vector2.Zero, 9f * pulse, new Color(0.95f, 0.82f, 0.32f, 0.25f));
            DrawCircle(Vector2.Zero, 5f * pulse, new Color(0.98f, 0.9f, 0.5f, 0.6f));
            DrawCircle(Vector2.Zero, 2.5f, new Color(1f, 0.98f, 0.82f));
        }
    }
}
