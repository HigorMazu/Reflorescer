using Godot;
using System;
using Joguim.Abilities;
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

        public Health Health { get; private set; }

        public Vector2 LastFacingDirection { get; private set; } = Vector2.Right;
        public bool HasSword { get; set; } = true;
        public int CurrentAttackDamage => StatsResource?.AttackDamage ?? 25;
        // Stats duplos: sem a espada o Kairo é mais rápido e pula mais alto
        public float CurrentMoveSpeed => HasSword ? StatsResource.MoveSpeed : StatsResource.MoveSpeedNoSword;
        public float CurrentJumpVelocity => HasSword ? StatsResource.JumpVelocity : StatsResource.JumpVelocityNoSword;
        // Fonte única de verdade do double jump: AbilityManager
        public bool HasDoubleJump => AbilityManager.Instance?.HasAbility(AbilityId.DoubleJump) ?? false;
        public bool HasDash => AbilityManager.Instance?.HasAbility(AbilityId.Dash) ?? false;
        public bool IsDashing => _dashTimer > 0;
        public bool HasWallJump => AbilityManager.Instance?.HasAbility(AbilityId.WallJump) ?? false;
        public bool IsWallSliding => _isWallSliding;

        private bool _isDead = false;
        private float _attackTimer = 0f;
        private float _hurtTimer = 0f;
        private float _dashTimer = 0f;
        private float _dashCooldownTimer = 0f;
        private float _dashDirection = 1f;
        private float _coyoteTime = 0.15f;
        private float _coyoteCounter = 0f;
        private float _jumpBufferTime = 0.12f;
        private float _jumpBufferCounter = 0f;
        private bool _doubleJumpUsed = false;
        private bool _isWallSliding = false;
        private float _lastWallNormalX = 0f;
        private float _wallCoyoteTime = 0.1f;
        private float _wallCoyoteCounter = 0f;
        private float _wallJumpLockTimer = 0f;
        private int _jumpCount = 0;
        private Vector2? _lastCheckpointPosition;
        private Vector2 _deathPosition;

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

            if (GameManager.Instance != null) HasSword = GameManager.Instance.HasSword;
            SetSwordVisible(HasSword);
            ApplySwordTint();

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

            if (EventBus.Instance != null) EventBus.Instance.CheckpointActivated += OnCheckpointActivated;
            else GD.PrintErr("PlayerController: EventBus not ready yet.");

            // Spawn direcional: se viemos de um TransitionTrigger, reposiciona no ponto de entrada
            CallDeferred(MethodName.ApplyPendingSpawn);
        }

        public override void _ExitTree()
        {
            if (EventBus.Instance != null) EventBus.Instance.CheckpointActivated -= OnCheckpointActivated;
        }

        private void ApplyPendingSpawn()
        {
            if (SceneManager.Instance == null || !SceneManager.Instance.TryConsumePendingSpawn(out Vector2 spawnPosition)) return;
            GlobalPosition = spawnPosition;
            Velocity = Vector2.Zero;
            GetNodeOrNull<Camera2D>("Camera2D")?.ResetSmoothing();
        }

        public override void _PhysicsProcess(double delta)
        {
            if (_isDead) return;

            // Timers
            if (_attackTimer > 0) _attackTimer -= (float)delta;
            if (_hurtTimer > 0) _hurtTimer -= (float)delta;
            if (_dashCooldownTimer > 0) _dashCooldownTimer -= (float)delta;
            if (_coyoteCounter > 0) _coyoteCounter -= (float)delta;
            if (_jumpBufferCounter > 0) _jumpBufferCounter -= (float)delta;
            if (_wallCoyoteCounter > 0) _wallCoyoteCounter -= (float)delta;
            if (_wallJumpLockTimer > 0) _wallJumpLockTimer -= (float)delta;

            HandlePrototypeMovement(delta);
            UpdatePrototypeAnimation();
        }

        private void HandlePrototypeMovement(double delta)
        {
            // Dash: durante o impulso ignora gravidade, input horizontal e pulo
            if (Input.IsActionJustPressed("dash") && CanDash()) StartDash();
            if (IsDashing)
            {
                _dashTimer -= (float)delta;
                Velocity = new Vector2(_dashDirection * StatsResource.DashSpeed, 0);
                MoveAndSlide();
                if (_dashTimer <= 0 || IsOnWall()) EndDash();
                return;
            }

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
            else if (wantsJump && CanWallJump())
            {
                WallJump();
                _jumpBufferCounter = 0;
            }
            else if (wantsJump && HasDoubleJump && !_doubleJumpUsed && !IsOnFloor())
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
                AudioManager.Instance?.PlaySfx("player_attack.wav");

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

            // Logo após um wall jump o input horizontal fica travado por um instante,
            // senão segurar a direção da parede anula o empurrão pro lado oposto
            if (_wallJumpLockTimer <= 0)
            {
                float accel = IsOnFloor() ? StatsResource.Acceleration : StatsResource.AirAcceleration;
                float targetSpeed = inputDir * CurrentMoveSpeed;
                if (Mathf.Abs(inputDir) > 0.01f)
                    Velocity = new Vector2(Mathf.MoveToward(Velocity.X, targetSpeed, accel * (float)delta), Velocity.Y);
                else
                    Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0, StatsResource.Deceleration * (float)delta), Velocity.Y);

                UpdateFacingDirection(inputDir);
            }

            UpdateWallSlide(inputDir);

            // Gravity
            ApplyGravity(delta);
            // Agarrado na parede: a queda fica limitada a uma velocidade baixa (desliza devagar)
            if (_isWallSliding && Velocity.Y > StatsResource.WallSlideSpeed)
                Velocity = new Vector2(Velocity.X, StatsResource.WallSlideSpeed);

            MoveAndSlide();

            // Interact
            if (Input.IsActionJustPressed("interact")) TryInteract();
            if (Input.IsActionJustPressed("toggle_sword")) ToggleSword();
        }

        // Agarrado = no ar, encostado numa parede (IsOnWall do último MoveAndSlide) e segurando a direção dela
        private void UpdateWallSlide(float inputDir)
        {
            _isWallSliding = false;
            if (!HasWallJump || IsOnFloor() || !IsOnWall() || _wallJumpLockTimer > 0) return;

            float normalX = GetWallNormal().X;
            if (Mathf.Abs(normalX) < 0.5f) return;
            if (inputDir * normalX >= -0.01f) return; // não está segurando contra a parede

            _isWallSliding = true;
            _lastWallNormalX = Mathf.Sign(normalX);
            _wallCoyoteCounter = _wallCoyoteTime;
            _doubleJumpUsed = false; // agarrar a parede devolve o pulo duplo
        }

        // Pequena tolerância (wall coyote): dá pra pular logo depois de soltar a parede
        private bool CanWallJump() => HasWallJump && !IsOnFloor() && _wallCoyoteCounter > 0;

        private void WallJump()
        {
            Jump();
            Velocity = new Vector2(_lastWallNormalX * StatsResource.WallJumpHorizontalSpeed, Velocity.Y);
            _wallJumpLockTimer = StatsResource.WallJumpInputLock;
            _wallCoyoteCounter = 0;
            _isWallSliding = false;
            LastFacingDirection = _lastWallNormalX > 0 ? Vector2.Right : Vector2.Left;
            UpdateFacingDirection(_lastWallNormalX);
        }

        private void UpdatePrototypeAnimation()
        {
            if (Visual == null) return;
            // Simple squash/stretch based on state for placeholder prototype
            string anim = "idle";
            if (IsDashing) anim = Sprite?.SpriteFrames != null && Sprite.SpriteFrames.HasAnimation("dash") ? "dash" : "run";
            else if (_hurtTimer > 0) anim = "hurt";
            else if (_attackTimer > 0) anim = "attack";
            else if (_isWallSliding && Velocity.Y >= 0)
                anim = Sprite?.SpriteFrames != null && Sprite.SpriteFrames.HasAnimation("wall_slide") ? "wall_slide" : "fall";
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
        }

        private void TryInteract()
        {
            if (InteractionDetector == null) return;
            foreach (var area in InteractionDetector.GetOverlappingAreas())
            {
                // O interagível pode ser a própria Area2D (Checkpoint) ou o pai dela
                Node candidate = area is Joguim.Interaction.IInteractable ? area : area.GetParent();
                if (candidate is Joguim.Interaction.IInteractable interactable && interactable.CanInteract())
                {
                    interactable.Interact(this);
                    GD.Print($"Interagiu com {candidate.Name}");
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

        public void UpdateFacingDirection(float inputDirection)
        {
            if (Mathf.Abs(inputDirection) > 0.1f && Visual != null)
                Visual.Scale = new Vector2(inputDirection > 0 ? 1 : -1, 1);
        }

        public void Jump()
        {
            Velocity = new Vector2(Velocity.X, CurrentJumpVelocity);
            AudioManager.Instance?.PlaySfx("player_jump.wav");
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
            Velocity = new Vector2(Velocity.X, CurrentJumpVelocity * 0.90f);
            // Faísca pulse quando faz double jump
            var faisca = GetTree().GetFirstNodeInGroup("Companion");
            if (faisca != null && faisca.HasMethod("Flash")) { /* fallback */ }
        }

        // Dash é só no chão: o dash aéreo é outra habilidade (AbilityId.AirDash)
        public bool CanDash() => HasDash && !IsDashing && _dashCooldownTimer <= 0 && _hurtTimer <= 0 && IsOnFloor();

        private void StartDash()
        {
            _dashDirection = LastFacingDirection.X >= 0 ? 1f : -1f;
            _dashTimer = StatsResource.DashDuration;
            _dashCooldownTimer = StatsResource.DashCooldown;
            // i-frames só durante o impulso (sem encurtar uma invulnerabilidade pós-dano que já esteja rodando)
            if (Health != null && !Health.IsInvulnerable) Health.StartInvulnerability(StatsResource.DashDuration);
            AudioManager.Instance?.PlaySfx("player_dash.wav");
            EventBus.Instance?.EmitSignal(EventBus.SignalName.PlayerAbilityUsed, AbilityId.Dash.ToString());

            if (Visual != null)
            {
                Visual.Modulate = new Color(0.75f, 1.0f, 0.85f, 0.75f);
                Visual.Scale = new Vector2(_dashDirection * 1.2f, 0.85f);
            }
        }

        private void EndDash()
        {
            _dashTimer = 0f;
            Velocity = new Vector2(_dashDirection * StatsResource.MoveSpeed, 0);
            if (Visual != null)
            {
                Visual.Modulate = Colors.White;
                Visual.Scale = new Vector2(_dashDirection, 1);
            }
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
            AudioManager.Instance?.PlaySfx("player_hurt.wav");
            _hurtTimer = 0.25f;
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
        }

        private void OnDied()
        {
            _isDead = true;
            _hurtTimer = 0f;
            _dashTimer = 0f;
            PlayAnimation("dead");
            _deathPosition = GlobalPosition;
            EventBus.Instance?.EmitSignal("PlayerDied");
            // Quem decide o respawn é a tela de Game Over do HUD ("Tentar de novo").
            // Cena sem HUD (ex.: cena de teste): volta ao comportamento antigo, respawn automático em 1s.
            if (GetTree().GetFirstNodeInGroup(Joguim.UI.HUDController.GroupName) == null)
            {
                GetTree().CreateTimer(1.0).Timeout += () =>
                {
                    if (IsInstanceValid(this)) RespawnAtLastCheckpoint();
                };
            }
        }

        // Último checkpoint ativado, ou perto de onde morreu
        public void RespawnAtLastCheckpoint() => Respawn(_lastCheckpointPosition ?? _deathPosition + Vector2.Up * 40);

        public void SetRespawnPoint(Vector2 position) => _lastCheckpointPosition = position;

        public void Respawn(Vector2 position)
        {
            _isDead = false; GlobalPosition = position; Health?.Reset(); Velocity = Vector2.Zero;
            SetProcess(true); SetPhysicsProcess(true);
            EventBus.Instance?.EmitSignal("PlayerRespawned", position);
        }

        private void OnCheckpointActivated(Vector2 position, string checkpointId) => _lastCheckpointPosition = position;

        public void Heal(int amount) => Health?.Heal(amount);

        // Q: ativa/guarda a Espada de Grama. Guardada = mais velocidade e pulo, sem ataque (CanAttack exige HasSword).
        // Um golpe já em andamento termina normalmente; a troca não cancela animação nem trava input.
        public void ToggleSword()
        {
            HasSword = !HasSword;
            if (GameManager.Instance != null) GameManager.Instance.HasSword = HasSword;
            SetSwordVisible(HasSword);
            ApplySwordTint();
            AudioManager.Instance?.PlaySfx(HasSword ? "sword_on.wav" : "sword_off.wav");
            EventBus.Instance?.EmitSignal(EventBus.SignalName.SwordToggled, HasSword);
            GD.Print($"Espada {(HasSword ? "ativada" : "guardada")}");
        }

        // Indicador provisório até existir arte com/sem espada: tom esverdeado claro com a espada guardada.
        // Usa SelfModulate do sprite pra não brigar com os flashes de ataque/dano (Modulate do Visual).
        private void ApplySwordTint()
        {
            if (Sprite != null) Sprite.SelfModulate = HasSword ? Colors.White : new Color(0.8f, 1.0f, 0.8f);
        }

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
