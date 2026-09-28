using Godot;

namespace Joguim.Enemies
{
    // Arquétipo Voador: ignora gravidade, flutua oscilando em Y e persegue o jogador nos dois eixos.
    // Reaproveita toda a máquina de estados do EnemyBase (idle/patrol/detect/chase/attack/hurt/dead).
    public partial class EnemyVoador : EnemyBase
    {
        [Export] public float HoverAmplitude = 10.0f;
        [Export] public float HoverFrequency = 2.0f;
        [Export] public float HoverStiffness = 6.0f;

        private float _hoverBaseY;
        private float _hoverTime;

        public override void _Ready()
        {
            base._Ready();
            MotionMode = MotionModeEnum.Floating;
            _hoverBaseY = GlobalPosition.Y;
        }

        // No lugar da gravidade: puxa suavemente pra altura de voo, com uma oscilação senoidal
        public override void ApplyGravity(double delta)
        {
            _hoverTime += (float)delta;
            float targetY = _hoverBaseY + Mathf.Sin(_hoverTime * HoverFrequency) * HoverAmplitude;
            Velocity = new Vector2(Velocity.X, (targetY - GlobalPosition.Y) * HoverStiffness);
        }

        public override void ChasePlayer(double delta)
        {
            if (_player == null) { _player = GetTree().GetFirstNodeInGroup("Player") as Node2D; if (_player == null) return; }

            // mira no tronco do jogador, não nos pés
            Vector2 target = _player.GlobalPosition + new Vector2(0, -20f);
            Vector2 direction = (target - GlobalPosition).Normalized();
            Velocity = direction * StatsResource.ChaseSpeed;
            if (direction.X > 0 && !IsFacingRight()) Flip();
            else if (direction.X < 0 && IsFacingRight()) Flip();

            // ao parar de perseguir, continua flutuando na altura em que está
            _hoverBaseY = GlobalPosition.Y;
        }

        // Voando não existe beira de plataforma
        public override bool AtPatrolEdge() => false;
    }
}
