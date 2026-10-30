using Godot;
using Joguim.Combat;
using Joguim.Core;
using Joguim.UI;

namespace Joguim.Bosses
{
    /// <summary>
    /// Chefe do Deserto (C15-T2, 26/10/2026). Usa a arte da cobra do Higor (SandViperSpriteFrames) e os
    /// três golpes aprovados: bote frontal, varrida de cauda (baixa, se desvia pulando) e mergulho na areia
    /// (some, um redemoinho segue o Kairo e ela sai por baixo dele). Vida, fases, dano de contato, morte e
    /// save vêm do BossBase, igual ao Korrag. Cada golpe tem aviso, janela de dano curta e acerta uma vez.
    /// </summary>
    public partial class SerpentBoss : BossBase
    {
        private enum Move { None, StrikeWindup, Strike, TailWindup, Tail, BurrowEnter, Underground, BurrowExit }

        [Export] public float StrikeWindup = 0.55f;
        [Export] public float StrikeDuration = 0.8f;
        [Export] public float TailWindup = 0.35f;
        [Export] public float TailDuration = 0.65f;
        [Export] public float BurrowEnterTime = 0.75f;
        [Export] public float UndergroundTime = 1.1f;
        [Export] public float UndergroundSpeed = 320f;
        [Export] public float BurrowExitTime = 0.75f;
        // Distância em que ela para de rastejar e ataca
        [Export] public float StrikeRange = 230f;
        // Limites da arena (x global) pro mergulho não sair dela
        [Export] public float ArenaMinX = 2950f;
        [Export] public float ArenaMaxX = 3780f;
        // Parede de areia que fecha a arena quando a luta começa e abre quando ela cai
        [Export] public NodePath ArenaGatePath;

        // Áreas de golpe em coordenadas locais, com a cobra olhando pra direita (FacingX = +1)
        public static readonly Rect2 StrikeRect = new(60, -180, 190, 170);
        public static readonly Rect2 TailRect = new(-330, -48, 660, 48);
        public static readonly Rect2 EruptionRect = new(-120, -200, 240, 200);

        private Move _move = Move.None;
        private float _moveTimer;
        private bool _moveHit;
        private Tween _telegraph;
        private AnimatedSprite2D _sandMarker;
        private StaticBody2D _arenaGate;
        private bool _gateClosed;
        private Hurtbox _hurtbox;

        public override bool IsBusy => _move != Move.None;
        public bool IsUnderground => _move == Move.Underground || _move == Move.BurrowEnter && _moveTimer < BurrowEnterTime * 0.4f;
        public string CurrentMove => _move.ToString();
        public int PhaseIndex => _currentPhaseIndex;
        protected override bool ContactDamageEnabled => _move is not (Move.BurrowEnter or Move.Underground);

        public override void _Ready()
        {
            BossId = "boss_serpente";
            base._Ready();
            if (IsQueuedForDeletion()) { OpenGate(); return; }

            SpriteFacesLeft = true;
            _hurtbox = GetNodeOrNull<Hurtbox>("Hurtbox");
            _arenaGate = GetNodeOrNull<StaticBody2D>(ArenaGatePath);
            SetGate(false);

            _sandMarker = new AnimatedSprite2D
            {
                Name = "SandMarker", TopLevel = true, Visible = false, ZIndex = 1,
                SpriteFrames = GD.Load<SpriteFrames>("res://assets/environment/desert/hazards/DesertQuicksandSpriteFrames.tres"),
                Scale = new Vector2(.55f, .16f)
            };
            AddChild(_sandMarker);

            // Barra de vida acima da cabeça (a padrão do BossBase é pensada pro Korrag, menor)
            var bar = GetNodeOrNull<ProgressBar>("FloatingHealthBar");
            if (bar != null)
            {
                bar.Position = new Vector2(-70, -300); bar.Size = new Vector2(140, 10);
                bar.AddThemeStyleboxOverride("background", new StyleBoxFlat
                {
                    BgColor = new Color("18202a"), BorderColor = new Color("b89b64"),
                    BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1
                });
                bar.AddThemeStyleboxOverride("fill", new StyleBoxFlat
                {
                    BgColor = new Color("d4ae64"), ContentMarginTop = 2, ContentMarginBottom = 2
                });
            }
        }

        private static void OpenGateOf(StaticBody2D gate, bool closed)
        {
            if (gate == null) return;
            var shape = gate.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            shape?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !closed);
            foreach (var child in gate.GetChildren())
                if (child is CanvasItem item) item.Visible = closed;
        }

        private void SetGate(bool closed)
        {
            _gateClosed = closed;
            OpenGateOf(_arenaGate ?? GetNodeOrNull<StaticBody2D>(ArenaGatePath), closed);
        }

        private void OpenGate() => OpenGateOf(GetNodeOrNull<StaticBody2D>(ArenaGatePath), false);

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            if (Health == null || Health.IsDead) return;
            if (IsActive && !_gateClosed)
            {
                SetGate(true);
                var bar = GetNodeOrNull<Control>("FloatingHealthBar");
                if (bar != null) bar.Visible = true;
            }
            if (_move == Move.None) return;

            _moveTimer += (float)delta;
            StopMovement();
            switch (_move)
            {
                case Move.StrikeWindup:
                    if (_moveTimer >= StrikeWindup) Begin(Move.Strike, "attack");
                    break;
                case Move.Strike:
                    if (_moveTimer >= 0.15f && _moveTimer <= 0.45f) TryHit(StrikeRect, GetCurrentPhaseDamage(), 380f);
                    if (_moveTimer >= StrikeDuration) Finish();
                    break;
                case Move.TailWindup:
                    if (_moveTimer >= TailWindup) Begin(Move.Tail, "tail_sweep");
                    break;
                case Move.Tail:
                    if (_moveTimer >= 0.1f && _moveTimer <= 0.45f) TryHit(TailRect, Mathf.RoundToInt(GetCurrentPhaseDamage() * .8f), 300f);
                    if (_moveTimer >= TailDuration) Finish();
                    break;
                case Move.BurrowEnter:
                    if (_moveTimer >= BurrowEnterTime) GoUnderground();
                    break;
                case Move.Underground:
                    TrackUnderground(delta);
                    if (_moveTimer >= UndergroundTime) Emerge();
                    break;
                case Move.BurrowExit:
                    if (_moveTimer <= 0.35f) TryHit(EruptionRect, Mathf.RoundToInt(GetCurrentPhaseDamage() * 1.2f), 420f);
                    if (_moveTimer >= BurrowExitTime) Finish();
                    break;
            }
        }

        // ---------- Escolha de golpe ----------

        public override void ChooseAttack()
        {
            if (_player == null || IsBusy || !IsActive || Health == null || Health.IsDead) return;
            float dx = _player.GlobalPosition.X - GlobalPosition.X;
            float distance = Mathf.Abs(dx);
            float roll = GD.Randf();
            float burrowChance = _currentPhaseIndex switch { 0 => .15f, 1 => .4f, _ => .55f };

            if (roll < burrowChance || distance > StrikeRange * 1.8f) StartBurrow();
            else if (distance < StrikeRange * .75f && GD.Randf() < .5f) StartTail();
            else StartStrike();
        }

        public void StartStrike()
        {
            if (_player != null) FaceDirection(_player.GlobalPosition.X - GlobalPosition.X);
            Begin(Move.StrikeWindup, "windup");
            Telegraph(new Color(1f, .45f, .45f));
            AudioManager.Instance?.PlaySfx("snake_hiss.wav", -7f, .82f);
        }

        public void StartTail()
        {
            Begin(Move.TailWindup, "tail_sweep");
            Sprite?.Pause();
            Telegraph(new Color(1f, .9f, .4f));
        }

        public void StartBurrow()
        {
            Begin(Move.BurrowEnter, "burrow_enter");
            AudioManager.Instance?.PlaySfx("snake_hiss.wav", -10f, .6f);
        }

        // ---------- Golpes ----------

        private void Begin(Move move, string animation)
        {
            _move = move;
            _moveTimer = 0f;
            _moveHit = false;
            StopTelegraph();
            // Every move starts at its first pose, including repeated tail sweeps.
            Sprite?.Stop();
            PlayOwn(animation);
            if (Sprite != null)
            {
                float duration = move switch
                {
                    Move.StrikeWindup => StrikeWindup, Move.Strike => StrikeDuration,
                    Move.Tail => TailDuration, Move.BurrowEnter => BurrowEnterTime,
                    Move.BurrowExit => BurrowExitTime, _ => 0f
                };
                float authoredDuration = Sprite.SpriteFrames.GetFrameCount(animation)
                    / (float)Sprite.SpriteFrames.GetAnimationSpeed(animation);
                Sprite.SpeedScale = duration > 0 ? authoredDuration / duration : 1f;
            }
        }

        private void Finish()
        {
            StopTelegraph();
            _move = Move.None;
            _moveTimer = 0f;
            if (Sprite != null) Sprite.Visible = true;
            SetVulnerable(true);
            PlayOwn("idle");
        }

        private void GoUnderground()
        {
            _move = Move.Underground;
            _moveTimer = 0f;
            if (Sprite != null) Sprite.Visible = false;
            SetVulnerable(false);
            _sandMarker.GlobalPosition = new Vector2(GlobalPosition.X, GlobalPosition.Y - 2);
            _sandMarker.Visible = true;
            _sandMarker.Play("bubble");
        }

        private void TrackUnderground(double delta)
        {
            float targetX = _player?.GlobalPosition.X ?? _sandMarker.GlobalPosition.X;
            float speed = UndergroundSpeed * (_currentPhaseIndex >= 1 ? 1.2f : 1f);
            float x = Mathf.MoveToward(_sandMarker.GlobalPosition.X, targetX, speed * (float)delta);
            _sandMarker.GlobalPosition = new Vector2(Mathf.Clamp(x, ArenaMinX, ArenaMaxX), _sandMarker.GlobalPosition.Y);
        }

        private void Emerge()
        {
            GlobalPosition = new Vector2(_sandMarker.GlobalPosition.X, GlobalPosition.Y);
            _sandMarker.Visible = false;
            _sandMarker.Stop();
            if (_player != null) FaceDirection(_player.GlobalPosition.X - GlobalPosition.X);
            if (Sprite != null) Sprite.Visible = true;
            SetVulnerable(true);
            Begin(Move.BurrowExit, "burrow_exit");
            AudioManager.Instance?.PlaySfx("boss_stomp.wav", -4f, 1.25f);
        }

        // Retângulo do golpe no mundo, espelhado pro lado que ela olha
        public Rect2 WorldRect(Rect2 local)
        {
            float x = FacingX > 0 ? local.Position.X : -local.End.X;
            return new Rect2(GlobalPosition + new Vector2(x, local.Position.Y), local.Size);
        }

        private void TryHit(Rect2 local, int damage, float knockback)
        {
            if (_moveHit || _player == null) return;
            if (!WorldRect(local).Intersects(CombatGeometry.HurtRect(_player))) return;
            var health = _player.GetNodeOrNull<Health>("Health");
            if (health == null || health.IsInvulnerable || health.IsDead) return;
            Vector2 dir = new(Mathf.Sign(_player.GlobalPosition.X - GlobalPosition.X), -.35f);
            health.TakeDamage(new DamageInfo(damage, dir.Normalized(), knockback, this));
            _moveHit = true;
        }

        private void SetVulnerable(bool vulnerable)
        {
            if (_hurtbox != null) _hurtbox.Active = vulnerable;
            var shape = _hurtbox?.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
            shape?.SetDeferred(CollisionShape2D.PropertyName.Disabled, !vulnerable);
        }

        private void Telegraph(Color color)
        {
            if (Sprite == null) return;
            _telegraph?.Kill();
            _telegraph = CreateTween().SetLoops();
            _telegraph.TweenProperty(Sprite, "self_modulate", color, .1);
            _telegraph.TweenProperty(Sprite, "self_modulate", Colors.White, .1);
        }

        private void StopTelegraph()
        {
            _telegraph?.Kill();
            _telegraph = null;
            if (Sprite != null) Sprite.SelfModulate = Colors.White;
        }

        // ---------- Movimento e animação ----------

        public override void ChasePlayer(double delta)
        {
            if (_player == null || !IsActive) return;
            float dx = _player.GlobalPosition.X - GlobalPosition.X;
            FaceDirection(dx);
            if (Mathf.Abs(dx) > StrikeRange * .8f)
            {
                Velocity = new Vector2(Mathf.Sign(dx) * GetCurrentPhaseSpeed(), Velocity.Y);
                PlayOwn("slither");
            }
            else
            {
                StopMovement();
                PlayOwn("idle");
            }
        }

        // A máquina de estados dos chefes pede animações com os nomes do Korrag; aqui viram as da cobra.
        // Durante um golpe ela não troca a animação (o golpe controla a própria).
        public override void PlayAnimation(string animName)
        {
            if (IsBusy) return;
            PlayOwn(animName switch
            {
                "walk" => "slither",
                "hurt" => "hurt",
                "dead" or "defeated" => "dead",
                _ => "idle",
            });
        }

        private void PlayOwn(string animName)
        {
            if (Sprite?.SpriteFrames == null || !Sprite.SpriteFrames.HasAnimation(animName)) return;
            if (Sprite.Animation == animName && Sprite.IsPlaying()) return;
            Sprite.SpeedScale = 1f;
            Sprite.Play(animName);
        }

        protected override void OnDied()
        {
            StopTelegraph();
            _move = Move.None;
            if (Sprite != null) Sprite.Visible = true;
            if (_sandMarker != null) _sandMarker.Visible = false;
            SetGate(false);
            base.OnDied();
            AudioManager.Instance?.PlaySfx("boss_defeat.wav");
            (GetTree().GetFirstNodeInGroup(TutorialOverlay.GroupName) as TutorialOverlay)?
                .ShowNotice("A serpente caiu. O deserto respira de novo — o caminho adiante ainda está sendo aberto.", 6f);
        }
    }
}
