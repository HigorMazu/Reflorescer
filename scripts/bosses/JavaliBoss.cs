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
        [Export] public float ChargeDuration = 0.9f;
        [Export] public float ChargeMinDistance = 180.0f;
        [Export] public float StompRadius = 100.0f;
        [Export] public float StompWindup = 0.4f;
        [Export] public int StompDamage = 30;
        [Export] public float StompKnockback = 400.0f;

        private bool _isCharging;
        private float _chargeTimer;
        private float _chargeElapsed;
        private Vector2 _chargeDirection;
        private bool _isStomping;
        private float _stompTimer;

        public override bool IsBusy => _isCharging || _isStomping;

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
                        BossName = "Korrag, o Javali",
                        Gravity = 980f,
                        KnockbackResistance = 0.6f
                    };
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            // O MoveAndSlide fica com o estado de fase (BossStateMachine); aqui só define a velocidade
            if (_isCharging)
            {
                if (_chargeTimer > 0)
                {
                    _chargeTimer -= (float)delta;
                    StopMovement();
                }
                else
                {
                    ExecuteCharge(delta);
                }
            }

            if (_isStomping)
            {
                _stompTimer -= (float)delta;
                StopMovement();
                if (_stompTimer <= 0)
                {
                    _isStomping = false;
                    PerformStomp();
                    PlayAnimation("walk");
                }
            }
        }

        // Decisão de ataque do Korrag: pisada de perto, investida de longe, melee genérico no meio-termo.
        // Fases mais avançadas (_currentPhaseIndex 1 = fase 2, 2 = enraged) usam os especiais com mais frequência.
        public override void ChooseAttack()
        {
            if (_player == null || IsBusy || !IsActive) return;

            float distance = Mathf.Abs(_player.GlobalPosition.X - GlobalPosition.X);
            float roll = GD.Randf();

            if (distance <= StompRadius)
            {
                float stompChance = _currentPhaseIndex >= 2 ? 0.5f : 0.35f;
                if (roll < stompChance) StartStomp();
                else PerformAttack();
            }
            else if (distance >= ChargeMinDistance)
            {
                float chargeChance = _currentPhaseIndex == 0 ? 0.6f : 0.8f;
                if (roll < chargeChance) StartCharge();
                else PerformAttack();
            }
            else
            {
                if (roll < 0.4f) StartCharge();
                else PerformAttack();
            }
        }

        public void StartCharge()
        {
            if (_player == null) return;

            // investida só na horizontal, na direção do jogador no momento do windup
            _chargeDirection = new Vector2(_player.GlobalPosition.X < GlobalPosition.X ? -1 : 1, 0);
            _isCharging = true;
            _chargeTimer = ChargeWindup;
            _chargeElapsed = 0f;
            PlayAnimation("charge_windup");
            StopMovement();

            if (Sprite != null)
            {
                Sprite.FlipH = _chargeDirection.X < 0;
            }
        }

        private void ExecuteCharge(double delta)
        {
            Velocity = new Vector2(_chargeDirection.X * ChargeSpeed, Velocity.Y);
            _chargeElapsed += (float)delta;

            if (_player != null && GlobalPosition.DistanceTo(_player.GlobalPosition) < 60f)
            {
                var health = _player.GetNodeOrNull<Joguim.Combat.Health>("Health");
                if (health != null && !health.IsInvulnerable)
                {
                    health.TakeDamage(new Joguim.Combat.DamageInfo((int)ChargeDamage, _chargeDirection, ChargeSpeed, this));
                }
            }

            if (_chargeElapsed >= ChargeDuration || IsOnWall())
            {
                StopCharge();
            }
        }

        public void StopCharge()
        {
            _isCharging = false;
            Velocity = new Vector2(0, Velocity.Y);
            PlayAnimation("walk");
        }

        public void StartStomp()
        {
            _isStomping = true;
            _stompTimer = StompWindup;
            PlayAnimation("attack");
            StopMovement();
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

        protected override void OnDied()
        {
            _isStomping = false;
            if (_isCharging)
            {
                StopCharge();
            }

            base.OnDied();
            AudioManager.Instance?.PlaySfx("boss_defeat.wav");
        }
    }
}
