using Godot;
using System;
using Joguim.Combat;
using Joguim.Core;
using Joguim.Resources;

namespace Joguim.Enemies
{
    public partial class EnemyBase : CharacterBody2D
    {
        [Export] public EnemyStatsResource StatsResource;
        [Export] public NodePath SpritePath;
        [Export] public NodePath CombatControllerPath;
        [Export] public NodePath PatrolStartPath;
        [Export] public NodePath PatrolEndPath;
        [Export] public string EnemyId = "enemy_basic";

        public AnimatedSprite2D Sprite { get; private set; }
        public CombatController CombatController { get; private set; }
        public EnemyStateMachine StateMachine { get; private set; }
        public Health Health { get; private set; }

        private ProgressBar _hpBar;
        private bool _isDead = false;

        protected Node2D _player;
        protected Vector2 _patrolStart;
        protected Vector2 _patrolEnd;
        protected int _patrolDirection = 1;
        protected bool _isFacingRight = true;
        private float _contactTimer = 0f;
        private float _contactCooldown = 1.0f;
        private int _contactDamage = 12;

        public override void _Ready()
        {
            Sprite = GetNodeOrNull<AnimatedSprite2D>(SpritePath);
            CombatController = GetNodeOrNull<CombatController>(CombatControllerPath);
            StateMachine = GetNodeOrNull<EnemyStateMachine>("EnemyStateMachine");
            Health = GetNodeOrNull<Health>("Health");

            if (StatsResource == null)
            {
                StatsResource = new EnemyStatsResource();
                GD.PrintErr("EnemyBase: StatsResource not found. Using defaults.");
            }

            if (Health != null)
            {
                Health.MaxHealth = StatsResource.MaxHealth;
                Health.Reset();
                Health.DamageReceived += OnDamageReceived;
                Health.Died += OnDied;
                Health.HealthChanged += OnHealthChanged;
            }

            if (StateMachine != null)
            {
                StateMachine.Initialize(this);
                StateMachine.ChangeState(EnemyStateType.Idle);
            }

            InitializePatrolPoints();
            AddToGroup("Enemies");
            _contactDamage = Mathf.Max(8, StatsResource.AttackDamage);
            CreateHealthBar();
        }

        private void CreateHealthBar()
        {
            _hpBar = new ProgressBar();
            _hpBar.Name = "FloatingHP";
            _hpBar.Position = new Vector2(-18, -46);
            _hpBar.Size = new Vector2(36, 5);
            _hpBar.MinValue = 0;
            _hpBar.MaxValue = Health != null ? Health.MaxHealth : 75;
            _hpBar.Value = _hpBar.MaxValue;
            _hpBar.ShowPercentage = false;
            _hpBar.Visible = false;

            var bg = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.1f, 0.1f),
                CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2,
                CornerRadiusBottomLeft = 2, CornerRadiusBottomRight = 2
            };
            var fg = new StyleBoxFlat
            {
                BgColor = new Color(0.85f, 0.15f, 0.15f),
                CornerRadiusTopLeft = 2, CornerRadiusTopRight = 2,
                CornerRadiusBottomLeft = 2, CornerRadiusBottomRight = 2
            };
            _hpBar.AddThemeStyleboxOverride("background", bg);
            _hpBar.AddThemeStyleboxOverride("fill", fg);
            AddChild(_hpBar);
        }

        private void OnHealthChanged(int curr, int max)
        {
            if (_hpBar == null || _isDead) return;
            _hpBar.MaxValue = max;
            _hpBar.Value = curr;
            _hpBar.Visible = curr < max && curr > 0;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isDead) return;

            if (_contactTimer > 0) _contactTimer -= (float)delta;
            if (_player == null) _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            if (_player != null && Health != null && !Health.IsDead && _contactTimer <= 0)
            {
                float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
                if (dist < 32f)
                {
                    var pHealth = _player.GetNodeOrNull<Health>("Health");
                    if (pHealth != null && !pHealth.IsInvulnerable)
                    {
                        Vector2 dir = (_player.GlobalPosition - GlobalPosition).Normalized();
                        var dmg = new DamageInfo(_contactDamage, dir, 180f, this);
                        pHealth.TakeDamage(dmg);
                        _contactTimer = _contactCooldown;
                        GD.Print($"Player tomou {_contactDamage} de contato com {EnemyId}");
                    }
                }
            }
        }

        protected virtual void InitializePatrolPoints()
        {
            var patrolStart = GetNodeOrNull<Node2D>(PatrolStartPath);
            var patrolEnd = GetNodeOrNull<Node2D>(PatrolEndPath);
            _patrolStart = patrolStart != null ? patrolStart.GlobalPosition : GlobalPosition - Vector2.Right * 80f;
            _patrolEnd = patrolEnd != null ? patrolEnd.GlobalPosition : GlobalPosition + Vector2.Right * 80f;
        }

        public void ApplyGravity(double delta)
        {
            if (!IsOnFloor())
                Velocity += new Vector2(0, StatsResource.Gravity * (float)delta);
        }

        public float GetIdleDuration() => (float)GD.RandRange(1.0, 3.0);

        public void StartPatrol() => _patrolDirection = IsFacingRight() ? 1 : -1;

        public void MovePatrol(double delta)
        {
            float speed = StatsResource.PatrolSpeed;
            Velocity = new Vector2(_patrolDirection * speed, Velocity.Y);
            if (GlobalPosition.X >= _patrolEnd.X && _patrolDirection > 0) { _patrolDirection = -1; Flip(); }
            else if (GlobalPosition.X <= _patrolStart.X && _patrolDirection < 0) { _patrolDirection = 1; Flip(); }
        }

        public bool AtPatrolEdge() => false;

        public void ChasePlayer(double delta)
        {
            if (_player == null) { _player = GetTree().GetFirstNodeInGroup("Player") as Node2D; if (_player == null) return; }
            Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
            Velocity = new Vector2(direction.X * StatsResource.ChaseSpeed, Velocity.Y);
            if (direction.X > 0 && !_isFacingRight) Flip();
            else if (direction.X < 0 && _isFacingRight) Flip();
        }

        public bool CanDetectPlayer()
        {
            if (_player == null) _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            if (_player == null) return false;
            return GlobalPosition.DistanceTo(_player.GlobalPosition) <= StatsResource.DetectionRange;
        }

        public bool CanSeePlayer() => CanDetectPlayer();

        public bool IsInAttackRange()
        {
            if (_player == null) return false;
            return GlobalPosition.DistanceTo(_player.GlobalPosition) <= StatsResource.AttackRange;
        }

        public void PerformAttack()
        {
            if (CombatController?.Hitbox != null)
            {
                CombatController.Hitbox.SetDamage(StatsResource.AttackDamage);
                CombatController.EnableHitbox();
            }
        }

        public void StopMovement() => Velocity = new Vector2(0, Velocity.Y);

        public void PlayAnimation(string animName)
        {
            if (Sprite != null && Sprite.SpriteFrames != null && Sprite.SpriteFrames.HasAnimation(animName))
                Sprite.Play(animName);
        }

        public bool IsFacingRight() => _isFacingRight;

        public void Flip()
        {
            _isFacingRight = !_isFacingRight;
            if (Sprite != null) Sprite.FlipH = !_isFacingRight;
        }

        protected virtual void OnDamageReceived(int damage, Vector2 knockbackDirection)
        {
            if (_isDead) return;
            GD.Print($"Enemy {EnemyId} levou {damage} dano. HP: {Health.CurrentHealth}/{Health.MaxHealth}");

            Modulate = new Color(1.5f, 0.5f, 0.5f);
            GetTree().CreateTimer(0.12).Timeout += () => { if (GodotObject.IsInstanceValid(this)) Modulate = Colors.White; };

            if (StateMachine != null && StateMachine.CurrentStateType != EnemyStateType.Dead)
                StateMachine.ChangeState(EnemyStateType.Hurt);

            Vector2 knockback = knockbackDirection * 220f * (1f - StatsResource.KnockbackResistance);
            Velocity = knockback;

            Engine.TimeScale = 0.15f;
            GetTree().CreateTimer(0.06, true, false, true).Timeout += () => Engine.TimeScale = 1f;
        }

        protected virtual void OnDied()
        {
            if (_isDead) return;
            _isDead = true;
            GD.Print($"Enemy {EnemyId} MORREU! HP=0 -> sumindo do jogo.");

            Visible = false;

            CollisionLayer = 0;
            CollisionMask = 0;
            SetPhysicsProcess(false);

            var hurt = GetNodeOrNull<Area2D>("Hurtbox");
            if (hurt != null) { hurt.Monitoring = false; hurt.Monitorable = false; }

            var hit = GetNodeOrNull<Area2D>("AttackHitbox");
            if (hit != null) { hit.Monitoring = false; hit.Monitorable = false; }

            var col = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            if (col != null) col.Disabled = true;

            if (StateMachine != null) StateMachine.ChangeState(EnemyStateType.Dead);

            EventBus.Instance?.EmitSignal("EnemyDefeated", EnemyId);

            GetTree().CreateTimer(0.3).Timeout += () =>
            {
                if (GodotObject.IsInstanceValid(this)) QueueFree();
            };
        }
    }
}
