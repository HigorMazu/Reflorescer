using Godot;
using System;
using Joguim.Combat;
using Joguim.Core;
using Joguim.Resources;

namespace Joguim.Player
{
    public partial class PlayerController : CharacterBody2D
    {
        [Export] public PlayerStatsResource StatsResource;
        [Export] public NodePath SpritePath;
        [Export] public NodePath SwordPath;
        [Export] public NodePath AnimationPlayerPath;
        [Export] public NodePath CombatControllerPath;
        [Export] public NodePath InteractionDetectorPath;
        [Export] public NodePath VisualNodePath;
        [Export] public NodePath RightArmPath;
        [Export] public NodePath RightHandPath;

        public AnimatedSprite2D Sprite { get; private set; }
        public AnimatedSprite2D Sword { get; private set; }
        public AnimationPlayer AnimationPlayer { get; private set; }
        public CombatController CombatController { get; private set; }
        public Area2D InteractionDetector { get; private set; }
        public Node2D Visual { get; private set; }
        public Node2D RightArm { get; private set; }
        public Node2D RightHand { get; private set; }

        public PlayerStateMachine StateMachine { get; private set; }
        public Health Health { get; private set; }

        public Vector2 LastFacingDirection { get; private set; } = Vector2.Right;
        public bool HasSword { get; set; } = true;
        public int CurrentAttackDamage => StatsResource?.AttackDamage ?? 25;

        private bool _isDead = false;
        private float _attackTimer = 0f;
        private float _coyoteTime = 0.15f;
        private float _coyoteCounter = 0f;
        private float _jumpBufferTime = 0.12f;
        private float _jumpBufferCounter = 0f;
        private bool _useDirectMovement = true; // prototype: direct control guarantees playability
        private bool _hasDoubleJump = false;
        private bool _doubleJumpUsed = false;
        private int _jumpCount = 0;

        public override void _Ready()
        {
            Sprite = GetNodeOrNull<AnimatedSprite2D>(SpritePath);
            Sword = GetNodeOrNull<AnimatedSprite2D>(SwordPath);
            AnimationPlayer = GetNodeOrNull<AnimationPlayer>(AnimationPlayerPath);
            CombatController = GetNodeOrNull<CombatController>(CombatControllerPath);
            InteractionDetector = GetNodeOrNull<Area2D>(InteractionDetectorPath);
            Visual = GetNodeOrNull<Node2D>(VisualNodePath);
            RightArm = GetNodeOrNull<Node2D>(RightArmPath);
            RightHand = GetNodeOrNull<Node2D>(RightHandPath);

            StateMachine = GetNodeOrNull<PlayerStateMachine>("PlayerStateMachine");
            Health = GetNodeOrNull<Health>("Health");

            var camera = GetNodeOrNull<Camera2D>("Camera2D");
            if (camera != null) camera.MakeCurrent();

            if (StatsResource == null)
            {
                StatsResource = GD.Load<PlayerStatsResource>("res://resources/player/PlayerStatsResource.tres");
                if (StatsResource == null)
                {
                    GD.PrintErr("PlayerController: StatsResource not found. Usando defaults.");
                    StatsResource = new PlayerStatsResource();
                }
            }

            if (Health == null) GD.PrintErr("PlayerController: Health não encontrado.");
            else
            {
                Health.MaxHealth = StatsResource.MaxHealth;
                Health.Reset();
                Health.HealthChanged += OnHealthChanged;
                Health.DamageReceived += OnDamageReceived;
                Health.Died += OnDied;
                // sincroniza HUD imediatamente
                CallDeferred(nameof(EmitInitialHealth));
            }

            if (StateMachine != null)
            {
                StateMachine.Initialize(this);
                StateMachine.ChangeState(PlayerStateType.Idle);
            }

            if (EventBus.Instance != null) EventBus.Instance.CheckpointActivated += OnCheckpointActivated;
            else GD.PrintErr("PlayerController: EventBus not ready yet.");

            // DoubleJump fica disponível automaticamente no prototype
            CallDeferred(MethodName.InitDoubleJump);
        }

        private void InitDoubleJump()
        {
            // Tenta pegar do AbilityManager; se ainda não existir, habilita direto
            var am = GetNodeOrNull<Joguim.Abilities.AbilityManager>("/root/AbilityManager");
            if (am != null && am.HasAbility(Joguim.Abilities.AbilityId.DoubleJump))
            {
                _hasDoubleJump = true;
            }
            else
            {
                _hasDoubleJump = true; // prototype fallback - sempre permite
            }
            if (EventBus.Instance != null) EventBus.Instance.AbilityUnlocked += OnAbilityUnlocked;
        }

        private void OnAbilityUnlocked(string abilityId)
        {
            if (abilityId == Joguim.Abilities.AbilityId.DoubleJump.ToString()) _hasDoubleJump = true;
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isDead) return;

            // Timers
            if (_attackTimer > 0) _attackTimer -= (float)delta;
            if (_coyoteCounter > 0) _coyoteCounter -= (float)delta;
            if (_jumpBufferCounter > 0) _jumpBufferCounter -= (float)delta;

            if (_useDirectMovement)
            {
                HandlePrototypeMovement(delta);
            }
            // StateMachine still ticks via its own _PhysicsProcess; keep for animation
            UpdatePrototypeAnimation();
        }

        private void HandlePrototypeMovement(double delta)
        {
            // Coyote + jump buffer + double jump
            if (IsOnFloor())
            {
                _coyoteCounter = _coyoteTime;
                _doubleJumpUsed = false;
                _jumpCount = 0;
            }
            if (Input.IsActionJustPressed("jump")) _jumpBufferCounter = _jumpBufferTime;

            bool canJump = _jumpBufferCounter > 0 && _coyoteCounter > 0;
            bool wantsJump = Input.IsActionJustPressed("jump");

            if (canJump)
            {
                Jump();
                _jumpBufferCounter = 0;
                _coyoteCounter = 0;
                _jumpCount = 1;
            }
            else if (wantsJump && _hasDoubleJump && !_doubleJumpUsed && !IsOnFloor())
            {
                DoubleJump();
                _doubleJumpUsed = true;
                _jumpBufferCounter = 0;
                // efeito Faísca
                if (Visual != null)
                {
                    var t2 = CreateTween();
                    t2.TweenProperty(Visual, "scale", new Vector2(Visual.Scale.X * 1.1f, 0.85f), 0.06);
                    t2.TweenProperty(Visual, "scale", new Vector2(Visual.Scale.X > 0 ? 1 : -1, 1), 0.1);
                }
            }
            // Variable jump height: cut velocity when releasing jump early
            if (Input.IsActionJustReleased("jump") && Velocity.Y < -80f)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y * 0.5f);
            }

            // Attack - animação melhorada para apresentação
            if (Input.IsActionJustPressed("attack") && CanAttack())
            {
                PerformAttack();
                _attackTimer = StatsResource.AttackCooldown;

                // flash + lean forward
                if (Visual != null)
                {
                    Visual.Modulate = new Color(1.4f, 1.4f, 1.1f);
                    GetTree().CreateTimer(0.10).Timeout += () => { if (IsInstanceValid(Visual)) Visual.Modulate = Colors.White; };
                    // lean
                    Vector2 leanScale = new Vector2(LastFacingDirection.X > 0 ? 1.15f : -1.15f, 0.88f);
                    var tLean = CreateTween();
                    tLean.TweenProperty(Visual, "scale", leanScale, 0.06);
                    tLean.TweenProperty(Visual, "scale", new Vector2(LastFacingDirection.X > 0 ? 1 : -1, 1), 0.12);
                }
                // sword swing arco: prepara no ombro e corta
                if (RightArm != null)
                {
                    RightArm.Rotation = 0.55f;
                    var blade = RightArm.GetNodeOrNull<ColorRect>("RightHand/Sword/Blade");
                    if (blade != null) blade.Color = new Color(1, 1, 0.6f);
                    var tween = CreateTween();
                    tween.TweenProperty(RightArm, "rotation", -1.35f, 0.09).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
                    tween.TweenProperty(RightArm, "rotation", 0f, 0.16).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
                    tween.TweenCallback(Callable.From(() => {
                        if (IsInstanceValid(blade) && blade != null) blade.Color = new Color(0.7f, 0.9f, 0.3f);
                    }));
                }
            }
            if (_attackTimer <= 0) FinishAttack();

            // Horizontal movement (direct input for prototype responsiveness)
            float inputDir = Input.GetAxis("move_left", "move_right");
            if (Mathf.Abs(inputDir) > 0.01f) LastFacingDirection = inputDir > 0 ? Vector2.Right : Vector2.Left;

            float accel = IsOnFloor() ? StatsResource.Acceleration : StatsResource.AirAcceleration;
            float targetSpeed = inputDir * StatsResource.MoveSpeed;
            if (Mathf.Abs(inputDir) > 0.01f)
                Velocity = new Vector2(Mathf.MoveToward(Velocity.X, targetSpeed, accel * (float)delta), Velocity.Y);
            else
                Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0, StatsResource.Deceleration * (float)delta), Velocity.Y);

            UpdateFacingDirection(inputDir);

            // Gravity
            ApplyGravity(delta);

            MoveAndSlide();

            // Interact
            if (Input.IsActionJustPressed("interact")) TryInteract();
        }

        private void UpdatePrototypeAnimation()
        {
            if (Visual == null) return;
            // Simple squash/stretch based on state for placeholder prototype
            string anim = "idle";
            if (_attackTimer > 0) anim = "attack";
            else if (!IsOnFloor()) anim = Velocity.Y < 0 ? "jump" : "fall";
            else if (Mathf.Abs(Velocity.X) > 10f) anim = "run";

            // Use AnimatedSprite if available, otherwise modulate placeholder
            if (Sprite != null && Sprite.SpriteFrames != null && Sprite.SpriteFrames.HasAnimation(anim))
                Sprite.Play(anim);
            else
            {
                // placeholder visual feedback: scale and color hint
                if (anim == "attack") Visual.Scale = new Vector2(LastFacingDirection.X > 0 ? 1.1f : -1.1f, 0.9f);
                else if (anim == "run") Visual.Scale = new Vector2(LastFacingDirection.X > 0 ? 1 : -1, 1);
                else Visual.Scale = new Vector2(LastFacingDirection.X > 0 ? 1 : -1, 1);
            }

            // StateMachine animation fallback
            if (StateMachine != null && StateMachine.CurrentStateType.ToString().ToLower() != anim)
            {
                // keep StateMachine in sync for compatibility
            }
        }

        private void TryInteract()
        {
            if (InteractionDetector == null) return;
            foreach (var area in InteractionDetector.GetOverlappingAreas())
            {
                var parent = area.GetParent();
                if (parent is Joguim.Interaction.IInteractable interactable && interactable.CanInteract())
                {
                    interactable.Interact(this);
                    GD.Print($"Interagiu com {parent.Name}");
                    break;
                }
            }
        }

        public void ApplyGravity(double delta)
        {
            if (!IsOnFloor())
            {
                float gravity = StatsResource.Gravity;
                if (Velocity.Y > 0) gravity *= StatsResource.FallGravityMultiplier;
                Velocity = new Vector2(Velocity.X, Velocity.Y + gravity * (float)delta);
                Velocity = new Vector2(Velocity.X, Mathf.Min(Velocity.Y, StatsResource.MaxFallSpeed));
            }
        }

        public void ApplyHorizontalMovement(double delta)
        {
            float inputDirection = Input.GetAxis("move_left", "move_right");
            if (Mathf.Abs(inputDirection) > 0.01f) LastFacingDirection = inputDirection > 0 ? Vector2.Right : Vector2.Left;
            float accel = IsOnFloor() ? StatsResource.Acceleration : StatsResource.AirAcceleration;
            float targetSpeed = inputDirection * StatsResource.MoveSpeed;
            if (Mathf.Abs(inputDirection) > 0.1f)
                Velocity = new Vector2(Mathf.MoveToward(Velocity.X, targetSpeed, accel * (float)delta), Velocity.Y);
            else
                Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0, StatsResource.Deceleration * (float)delta), Velocity.Y);
            UpdateFacingDirection(inputDirection);
        }

        public void UpdateFacingDirection(float inputDirection)
        {
            if (Mathf.Abs(inputDirection) > 0.1f && Visual != null)
                Visual.Scale = new Vector2(inputDirection > 0 ? 1 : -1, 1);
        }

        public void Jump()
        {
            Velocity = new Vector2(Velocity.X, StatsResource.JumpVelocity);
            // squash visual
            if (Visual != null)
            {
                var t = CreateTween();
                t.TweenProperty(Visual, "scale", new Vector2(Visual.Scale.X * 0.85f, 1.15f), 0.08);
                t.TweenProperty(Visual, "scale", new Vector2(Visual.Scale.X > 0 ? 1 : -1, 1), 0.12);
            }
        }

        public void DoubleJump()
        {
            Velocity = new Vector2(Velocity.X, StatsResource.JumpVelocity * 0.90f);
            // Faísca pulse quando faz double jump
            var faisca = GetTree().GetFirstNodeInGroup("Companion");
            if (faisca != null && faisca.HasMethod("Flash")) { /* fallback */ }
        }

        public bool CanAttack() => HasSword && CombatController?.Hitbox != null && _attackTimer <= 0.001f;

        public void PerformAttack()
        {
            if (CombatController?.Hitbox != null)
            {
                // posiciona hitbox conforme direção (mão direita)
                var hb = CombatController.Hitbox;
                float dir = LastFacingDirection.X;
                hb.Position = new Vector2(dir > 0 ? 32f : -32f, -12f);
                // gira levemente a área para o arco
                hb.Rotation = dir > 0 ? -0.25f : 0.25f;

                hb.SetDamage(CurrentAttackDamage);
                hb.Active = true;
                hb.Monitoring = true;
                hb.Monitorable = true;
                CombatController.EnableHitbox();
                // janela de dano curta e precisa
                GetTree().CreateTimer(0.14).Timeout += () => { if (IsInstanceValid(hb)) { hb.Active = false; CombatController?.DisableHitbox(); } };
            }
        }

        public void FinishAttack() { if (CombatController?.Hitbox != null) { CombatController.Hitbox.Active = false; CombatController?.DisableHitbox(); } }

        public void PlayAnimation(string animationName)
        {
            if (AnimationPlayer != null && AnimationPlayer.HasAnimation(animationName)) AnimationPlayer.Play(animationName);
            else if (Sprite != null && Sprite.SpriteFrames != null && Sprite.SpriteFrames.HasAnimation(animationName)) Sprite.Play(animationName);
        }

        public void SetFacingDirection(Vector2 direction)
        {
            if (direction.X > 0) { LastFacingDirection = Vector2.Right; if (Visual != null) Visual.Scale = Vector2.One; }
            else if (direction.X < 0) { LastFacingDirection = Vector2.Left; if (Visual != null) Visual.Scale = new Vector2(-1, 1); }
        }

        private void EmitInitialHealth()
        {
            EventBus.Instance?.EmitSignal("PlayerHealthChanged", Health.CurrentHealth, Health.MaxHealth);
        }

        private void OnHealthChanged(int curr, int max)
        {
            EventBus.Instance?.EmitSignal("PlayerHealthChanged", curr, max);
            GD.Print($"Player HP: {curr}/{max}");
        }

        private void OnDamageReceived(int damage, Vector2 knockbackDirection)
        {
            if (_isDead) return;
            GD.Print($"Player tomou {damage} dano. HP {Health.CurrentHealth}");
            Velocity = knockbackDirection * 320f * (1f - StatsResource.KnockbackResistance);
            if (knockbackDirection.Y < -0.2f) Velocity = new Vector2(Velocity.X, -180f);
            if (Visual != null)
            {
                Visual.Modulate = new Color(1, 0.45f, 0.45f);
                GetTree().CreateTimer(0.18).Timeout += () => { if (IsInstanceValid(Visual)) Visual.Modulate = Colors.White; };
                // shake
                var tw = CreateTween();
                tw.TweenProperty(Visual, "position", new Vector2(2, 0), 0.04);
                tw.TweenProperty(Visual, "position", new Vector2(-2, 0), 0.04);
                tw.TweenProperty(Visual, "position", Vector2.Zero, 0.04);
            }
            // hitstop curto
            Engine.TimeScale = 0.2f;
            GetTree().CreateTimer(0.07, true, false, true).Timeout += () => Engine.TimeScale = 1f;
            StateMachine?.ChangeState(PlayerStateType.Hurt);
        }

        private void OnDied()
        {
            _isDead = true;
            StateMachine?.ChangeState(PlayerStateType.Dead);
            EventBus.Instance?.EmitSignal("PlayerDied");
            // respawn after 1s for prototype
            GetTree().CreateTimer(1.0).Timeout += () =>
            {
                var spawn = GetTree().GetFirstNodeInGroup("Checkpoints");
                Vector2 pos = GlobalPosition + Vector2.Up * 40;
                if (spawn is Node2D n) pos = n.GlobalPosition;
                Respawn(pos);
            };
        }

        public void Respawn(Vector2 position)
        {
            _isDead = false; GlobalPosition = position; Health?.Reset(); Velocity = Vector2.Zero;
            SetProcess(true); SetPhysicsProcess(true);
            StateMachine?.ChangeState(PlayerStateType.Idle);
            EventBus.Instance?.EmitSignal("PlayerRespawned", position);
        }

        private void OnCheckpointActivated(Vector2 position, string checkpointId) { }

        public void Heal(int amount) => Health?.Heal(amount);

        public void SetSwordVisible(bool visible)
        {
            if (Sword != null) Sword.Visible = visible;
            if (RightHand != null)
            {
                var bandage = RightHand.GetNodeOrNull<AnimatedSprite2D>("Bandage");
                if (bandage != null) bandage.Visible = !visible;
            }
        }
    }
}
