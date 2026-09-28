using Godot;
using System;
using Joguim.Combat;
using Joguim.Core;
using Joguim.Resources;
using Joguim.UI;

namespace Joguim.Bosses
{
    public partial class BossBase : CharacterBody2D
    {
        [Export] public BossStatsResource StatsResource;
        [Export] public NodePath SpritePath;
        [Export] public NodePath CombatControllerPath;
        [Export] public NodePath HealthBarPath;
        [Export] public string BossId = "boss_base";

        public AnimatedSprite2D Sprite { get; private set; }
        public CombatController CombatController { get; private set; }
        public HealthBar HealthBar { get; private set; }

        public BossStateMachine StateMachine { get; private set; }
        public Health Health { get; private set; }

        protected Node2D _player;
        protected int _currentPhaseIndex = 0;
        protected bool _isActive = false;
        private bool _isBossDead = false;
        private float _bossContactTimer = 0f;
        private float _bossContactCooldown = 0.9f;
        private int _bossContactDamage = 22;
        private ProgressBar _bossFloatingBar;
        private Label _bossHpLabel;

        public float HealthPercentage => Health != null ? (float)Health.CurrentHealth / Health.MaxHealth : 1f;
        public bool IsActive => _isActive;
        // Ocupado com um ataque especial (windup/execução): não persegue e não é interrompido por dano (super armor)
        public virtual bool IsBusy => false;

        public override void _Ready()
        {
            Sprite = GetNodeOrNull<AnimatedSprite2D>(SpritePath);
            CombatController = GetNodeOrNull<CombatController>(CombatControllerPath);
            HealthBar = GetNodeOrNull<HealthBar>(HealthBarPath);

            StateMachine = GetNodeOrNull<BossStateMachine>("BossStateMachine");
            Health = GetNodeOrNull<Health>("Health");

            if (StatsResource == null)
            {
                StatsResource = new BossStatsResource();
                GD.PrintErr("BossBase: StatsResource not found. Using defaults.");
            }

            if (Health != null)
            {
                Health.MaxHealth = StatsResource.MaxHealth;
                Health.Reset();
                Health.DamageReceived += OnDamageReceived;
                Health.Died += OnDied;
            }

            if (HealthBar != null)
            {
                HealthBar.SetMaxValue(StatsResource.MaxHealth);
                HealthBar.SetValue(StatsResource.MaxHealth);
            }

            if (StateMachine != null)
            {
                StateMachine.Initialize(this);
                StateMachine.ChangeState(BossStateType.Intro);
            }

            CreateBossFloatingBar();
            if (Health != null)
            {
                Health.HealthChanged += OnBossHealthChanged;
                UpdateBossBar(Health.CurrentHealth, Health.MaxHealth);
            }
        }

        private void CreateBossFloatingBar()
        {
            _bossFloatingBar = GetNodeOrNull<ProgressBar>("HealthBar");
            if (_bossFloatingBar == null)
            {
                _bossFloatingBar = new ProgressBar();
                _bossFloatingBar.Name = "FloatingHealthBar";
                _bossFloatingBar.Position = new Vector2(-40, -132);
                _bossFloatingBar.Size = new Vector2(80, 8);
                _bossFloatingBar.MinValue = 0;
                _bossFloatingBar.MaxValue = Health != null ? Health.MaxHealth : 200;
                _bossFloatingBar.Value = _bossFloatingBar.MaxValue;
                _bossFloatingBar.ShowPercentage = false;
                var bg = new StyleBoxFlat { BgColor = new Color(0.12f, 0.12f, 0.12f), CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3 };
                var fg = new StyleBoxFlat { BgColor = new Color(0.85f, 0.12f, 0.85f), CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3, CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3 };
                _bossFloatingBar.AddThemeStyleboxOverride("background", bg);
                _bossFloatingBar.AddThemeStyleboxOverride("fill", fg);
                AddChild(_bossFloatingBar);
                _bossHpLabel = new Label();
                _bossHpLabel.HorizontalAlignment = HorizontalAlignment.Center;
                _bossHpLabel.Position = new Vector2(-40, -147);
                _bossHpLabel.Size = new Vector2(80, 12);
                _bossHpLabel.AddThemeFontSizeOverride("font_size", 8);
                _bossHpLabel.AddThemeColorOverride("font_color", Colors.White);
                AddChild(_bossHpLabel);
            }
        }

        private void OnBossHealthChanged(int curr, int max) => UpdateBossBar(curr, max);
        private void UpdateBossBar(int curr, int max)
        {
            if (_bossFloatingBar == null) return;
            _bossFloatingBar.MaxValue = max;
            var tw = CreateTween();
            tw.TweenProperty(_bossFloatingBar, "value", (double)curr, 0.12);
            if (_bossHpLabel != null) _bossHpLabel.Text = $"{curr}/{max}";
            if (_bossFloatingBar != null) _bossFloatingBar.Visible = curr < max || _isActive;
            if (_bossHpLabel != null) _bossHpLabel.Visible = _bossFloatingBar.Visible;
        }

        public void ActivateBoss()
        {
            _isActive = true;
            _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;

            if (HealthBar != null)
            {
                HealthBar.Visible = true;
            }
        }

        public void ApplyGravity(double delta)
        {
            if (!IsOnFloor())
            {
                Velocity += new Vector2(0, StatsResource.Gravity * (float)delta);
            }
        }

        public void ChasePlayer(double delta)
        {
            if (_player == null || !_isActive) return;

            Vector2 direction = (_player.GlobalPosition - GlobalPosition).Normalized();
            float speed = GetCurrentPhaseSpeed();
            Velocity = new Vector2(direction.X * speed, Velocity.Y);

            if (Sprite != null)
            {
                Sprite.FlipH = direction.X < 0;
            }
        }

        // Chamado pelos estados de fase quando o timer de ataque zera. Subclasses escolhem entre os ataques.
        public virtual void ChooseAttack() => PerformAttack();

        public void PerformAttack()
        {
            if (CombatController?.Hitbox != null)
            {
                var hitbox = CombatController.Hitbox;
                // vira a hitbox pro lado do jogador (o shape fica à direita por padrão)
                bool playerOnLeft = _player != null && _player.GlobalPosition.X < GlobalPosition.X;
                hitbox.Scale = new Vector2(playerOnLeft ? -1 : 1, 1);
                hitbox.SetDamage(GetCurrentPhaseDamage());
                CombatController.EnableHitbox();
                PlayAnimation("attack");
                GetTree().CreateTimer(0.25).Timeout += () =>
                {
                    if (!GodotObject.IsInstanceValid(this)) return;
                    CombatController?.DisableHitbox();
                    if (!_isBossDead) PlayAnimation("walk");
                };
            }
        }

        public void StopMovement()
        {
            Velocity = new Vector2(0, Velocity.Y);
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isBossDead) return;
            // contato do boss = dano contínuo (8 hits para morrer boss, mas boss encostar também machuca)
            if (_bossContactTimer > 0) _bossContactTimer -= (float)delta;
            // boss sempre ativo no prototype para aula
            if (_player == null) _player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
            if (_isActive == false && _player != null && GlobalPosition.DistanceTo(_player.GlobalPosition) < 500f) ActivateBoss();

            if (_player != null && Health != null && !Health.IsDead && _bossContactTimer <= 0)
            {
                float dist = GlobalPosition.DistanceTo(_player.GlobalPosition);
                // boss maior = raio 55
                if (dist < 55f)
                {
                    var pHealth = _player.GetNodeOrNull<Joguim.Combat.Health>("Health");
                    if (pHealth != null && !pHealth.IsInvulnerable)
                    {
                        Vector2 dir = (_player.GlobalPosition - GlobalPosition).Normalized();
                        var dmg = new Joguim.Combat.DamageInfo(_bossContactDamage, dir, 260f, this);
                        pHealth.TakeDamage(dmg);
                        _bossContactTimer = _bossContactCooldown;
                        GD.Print($"Player tomou {_bossContactDamage} de contato com Boss {BossId}");
                    }
                }
            }
        }

        public float GetCurrentPhaseCooldown()
        {
            if (StatsResource.Phases != null && _currentPhaseIndex < StatsResource.Phases.Length)
            {
                return StatsResource.Phases[_currentPhaseIndex].AttackCooldown;
            }
            return 1.5f;
        }

        public float GetCurrentPhaseSpeed()
        {
            if (StatsResource.Phases != null && _currentPhaseIndex < StatsResource.Phases.Length)
            {
                return StatsResource.Phases[_currentPhaseIndex].MoveSpeed;
            }
            return 100f;
        }

        public int GetCurrentPhaseDamage()
        {
            if (StatsResource.Phases != null && _currentPhaseIndex < StatsResource.Phases.Length)
            {
                return StatsResource.Phases[_currentPhaseIndex].Damage;
            }
            return 20;
        }

        public void SetPhase(int phaseIndex)
        {
            _currentPhaseIndex = phaseIndex;
            EventBus.Instance.EmitSignal("BossPhaseChanged", BossId, phaseIndex);
        }

        public void PlayAnimation(string animName)
        {
            if (Sprite != null && Sprite.SpriteFrames != null && Sprite.SpriteFrames.HasAnimation(animName))
            {
                Sprite.Play(animName);
            }
        }

        protected virtual void OnDamageReceived(int damage, Vector2 knockbackDirection)
        {
            GD.Print($"Boss {BossId} levou {damage} dano HP:{Health.CurrentHealth}/{Health.MaxHealth}");
            if (HealthBar != null && Health != null) HealthBar.SetValue(Health.CurrentHealth);
            UpdateBossBar(Health.CurrentHealth, Health.MaxHealth);
            // flash
            Modulate = new Color(1.4f, 0.6f, 0.6f);
            GetTree().CreateTimer(0.12).Timeout += () => { if (GodotObject.IsInstanceValid(this)) Modulate = Colors.White; };
            // super armor: durante Charge/Stomp o dano entra, mas não interrompe nem empurra
            if (!IsBusy)
            {
                if (StateMachine != null && StateMachine.CurrentStateType != BossStateType.Dead)
                    StateMachine.ChangeState(BossStateType.Hurt);
                Vector2 knockback = knockbackDirection * 150f * (1f - StatsResource.KnockbackResistance);
                Velocity = knockback;
            }
            Engine.TimeScale = 0.2f;
            GetTree().CreateTimer(0.05, true, false, true).Timeout += () => Engine.TimeScale = 1f;
        }

        protected virtual void OnDied()
        {
            if (_isBossDead) return;
            _isBossDead = true;
            GD.Print($"Boss {BossId} MORREU! HP=0 -> sumindo do jogo.");

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

            // BossDeadState.Enter() emite BossDefeated — fonte única do evento
            if (StateMachine != null) StateMachine.ChangeState(BossStateType.Dead);

            GetTree().CreateTimer(0.3).Timeout += () =>
            {
                if (GodotObject.IsInstanceValid(this)) QueueFree();
            };
        }

        public void EmitDefeated()
        {
            EventBus.Instance.EmitSignal("BossDefeated", BossId);
        }
    }
}
