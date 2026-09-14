using Godot;
using Joguim.Core;
using Joguim.Resources;

namespace Joguim.Bosses
{
    public partial class JavaliBoss : BossBase
    {
        [Export] public float ChargeSpeed = 300.0f;
        [Export] public float ChargeDamage = 40.0f;
        [Export] public float ChargeWindup = 1.0f;
        [Export] public float StompRadius = 100.0f;
        [Export] public int StompDamage = 30;
        [Export] public float StompKnockback = 400.0f;

        private bool _isCharging;
        private float _chargeTimer;
        private Vector2 _chargeDirection;

        public override void _Ready()
        {
            base._Ready();
            BossId = "boss_javali";

            if (StatsResource == null)
            {
                StatsResource = GD.Load<BossStatsResource>("res://resources/bosses/JavaliStatsResource.tres");
                if (StatsResource == null)
                {
                    StatsResource = new BossStatsResource
                    {
                        MaxHealth = 500,
                        BossName = "Javali das Ruínas",
                        Gravity = 980f,
                        KnockbackResistance = 0.6f
                    };
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            if (_isCharging)
            {
                _chargeTimer -= (float)delta;
                if (_chargeTimer <= 0)
                {
                    ExecuteCharge(delta);
                }
            }
        }

        public void StartCharge()
        {
            if (_player == null) return;

            _chargeDirection = (_player.GlobalPosition - GlobalPosition).Normalized();
            _isCharging = true;
            _chargeTimer = ChargeWindup;
            PlayAnimation("charge_windup");
            StopMovement();
        }

        private void ExecuteCharge(double delta)
        {
            Velocity = new Vector2(
                _chargeDirection.X * ChargeSpeed,
                Velocity.Y
            );
            MoveAndSlide();

            if (Sprite != null)
            {
                Sprite.FlipH = _chargeDirection.X < 0;
            }
        }

        public void StopCharge()
        {
            _isCharging = false;
            Velocity = new Vector2(0, Velocity.Y);
        }

        public void PerformStomp()
        {
            if (_player == null) return;

            float distance = GlobalPosition.DistanceTo(_player.GlobalPosition);
            if (distance <= StompRadius)
            {
                var health = _player.GetNodeOrNull<Joguim.Combat.Health>("Health");
                if (health != null)
                {
                    Vector2 knockbackDir = (_player.GlobalPosition - GlobalPosition).Normalized();
                    var damageInfo = new Joguim.Combat.DamageInfo(
                        StompDamage,
                        knockbackDir,
                        StompKnockback,
                        this
                    );
                    health.TakeDamage(damageInfo);
                }
            }

            AudioManager.Instance?.PlaySfx("boss_stomp.wav");
        }

        protected override void OnDamageReceived(int damage, Vector2 knockbackDirection)
        {
            base.OnDamageReceived(damage, knockbackDirection);

            if (_isCharging)
            {
                StopCharge();
            }
        }

        protected override void OnDied()
        {
            if (_isCharging)
            {
                StopCharge();
            }

            base.OnDied();
            AudioManager.Instance?.PlaySfx("boss_defeat.wav");
        }
    }
}
