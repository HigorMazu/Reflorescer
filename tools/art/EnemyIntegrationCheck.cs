using Godot;
using System;
using System.Threading.Tasks;
using Joguim.Combat;
using Joguim.Enemies;
using Joguim.Resources;

// Standalone fixture: no checkpoints, restore points, boss or save operations.
public partial class EnemyIntegrationCheck : Node2D
{
    private int _failures;
    private Node2D _target;
    private Health _targetHealth;

    private void Check(bool value, string message)
    {
        if (!value) _failures++;
        GD.Print($"ENEMY_QA_{(value ? "PASS" : "FAIL")}: {message}");
    }

    private async Task Wait(double seconds) => await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);

    public override async void _Ready()
    {
        try
        {
            var floor = new StaticBody2D { Position = new Vector2(400, 460), CollisionLayer = 1 };
            floor.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(1600, 20) } });
            AddChild(floor);
            _target = new Node2D { Name = "TrainingTarget", Position = new Vector2(4000, 450) };
            _targetHealth = new Health { Name = "Health", MaxHealth = 10000, InvulnerabilityDuration = 0 };
            _target.AddChild(_targetHealth);
            var hurt = new Hurtbox { Name = "Hurtbox", CollisionLayer = 2, CollisionMask = 16 };
            hurt.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(12, 12) } });
            _target.AddChild(hurt);
            AddChild(_target);
            _target.AddToGroup("Player");

            var fallback = GD.Load<PackedScene>("res://scenes/enemies/Enemy_Base.tscn").Instantiate<EnemyBase>();
            fallback.Position = new Vector2(-300, 450);
            AddChild(fallback);
            Check(fallback.GetNode<CanvasItem>("Body").Visible, "base enemy retains visible fallback");
            fallback.QueueFree();
            await Wait(.05);

            foreach (string archetype in new[] { "Rapido", "Robusto", "Voador" })
                await CheckEnemy(archetype);
        }
        catch (Exception error)
        {
            _failures++;
            GD.PushError(error.ToString());
        }
        Engine.TimeScale = 1;
        GD.Print($"ENEMY_QA_DONE failures={_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }

    private async Task CheckEnemy(string archetype)
    {
        _target.Position = new Vector2(4000, 450);
        var enemy = GD.Load<PackedScene>($"res://scenes/enemies/Enemy_{archetype}.tscn").Instantiate<EnemyBase>();
        enemy.Position = new Vector2(250, archetype == "Voador" ? 330 : 450);
        AddChild(enemy);
        await Wait(.12);
        Check(enemy.Health.MaxHealth == enemy.StatsResource.MaxHealth, $"{archetype}: stats preserved");
        Check(!enemy.GetNode<CanvasItem>("Body").Visible && !enemy.GetNode<CanvasItem>("EyeLeft").Visible &&
            !enemy.GetNode<CanvasItem>("EyeRight").Visible, $"{archetype}: placeholders hidden");
        Check(enemy.Sprite.Centered && Mathf.IsEqualApprox(enemy.Sprite.Position.X, 0), $"{archetype}: centered flip axis");
        Check(Mathf.Abs(enemy.Sprite.Position.Y + 48 * enemy.Sprite.Scale.Y) < .001f, $"{archetype}: foot baseline at origin");
        Check(Mathf.IsEqualApprox(enemy.Sprite.Scale.X, .56f) && 96 * enemy.Sprite.Scale.X <= 54, $"{archetype}: further 12% enlargement, maximum 54 px");
        foreach (string state in new[] { "idle", "walk", "detect", "attack", "hurt", "dead" })
        {
            var frames = enemy.Sprite.SpriteFrames;
            Check(frames.HasAnimation(state), $"{archetype}: {state} exists");
            for (int i = 0; i < frames.GetFrameCount(state); i++)
                Check(frames.GetFrameTexture(state, i)?.GetSize() == new Vector2(128, 96), $"{archetype}: {state}/{i} canvas");
        }
        float startX = enemy.Position.X;
        enemy.StateMachine.ChangeState(EnemyStateType.Patrol);
        await Wait(.18);
        Check(enemy.Position.X > startX && enemy.Sprite.Animation == "walk", $"{archetype}: patrol movement and walk");
        if (archetype == "Voador")
            Check(!enemy.IsOnFloor() && enemy.Position.Y < 360, "Voador: hover ignores gravity");
        else
            Check(enemy.IsOnFloor(), $"{archetype}: grounded patrol");

        _target.Position = enemy.Position + new Vector2(100, 0);
        await Wait(.12);
        Check(enemy.StateMachine.CurrentStateType == EnemyStateType.DetectPlayer && enemy.Sprite.Animation == "detect",
            $"{archetype}: natural detection");
        await Wait(.55);
        Check(enemy.StateMachine.CurrentStateType is EnemyStateType.Chase or EnemyStateType.Attack,
            $"{archetype}: natural chase after detection");
        enemy.SetPhysicsProcess(false); // Disable contact damage for directional-hit tests.
        enemy.StateMachine.SetPhysicsProcess(false);
        enemy.Velocity = Vector2.Zero;

        foreach (string state in new[] { "idle", "walk", "attack" })
        {
            enemy.PlayAnimation(state);
            await Wait(1.4 / (enemy.Sprite.SpriteFrames.GetAnimationSpeed(state) * enemy.Sprite.SpeedScale));
            Check(enemy.Sprite.IsPlaying() && enemy.Sprite.Frame > 0, $"{archetype}: {state} advances frames");
        }
        foreach (string area in new[] { "SopeDaMata", "DosselVivo", "IgarapeSufocado" })
        {
            enemy.StatsResource = GD.Load<EnemyStatsResource>($"res://resources/enemies/areas/{archetype}_{area}.tres");
            enemy.PlayAnimation("attack");
            double duration = 4 / (enemy.Sprite.SpriteFrames.GetAnimationSpeed("attack") * enemy.Sprite.SpeedScale);
            Check(Math.Abs(duration - enemy.StatsResource.AttackCooldown) < .001, $"{archetype}/{area}: attack duration matches cooldown");
            enemy.Sprite.SetFrameAndProgress(3, .8f);
            enemy.PlayAnimation("attack");
            Check(enemy.Sprite.Frame == 0 && enemy.Sprite.FrameProgress == 0, $"{archetype}/{area}: repeated attack restarts");
        }
        enemy.StatsResource = GD.Load<EnemyStatsResource>($"res://resources/enemies/areas/{archetype}_SopeDaMata.tres");
        var positionBeforeFlip = enemy.Sprite.GlobalPosition;
        enemy.Flip();
        Check(enemy.Sprite.FlipH && enemy.Sprite.GlobalPosition == positionBeforeFlip, $"{archetype}: flip without positional jump");
        foreach (int direction in new[] { -1, 1 })
        {
            if (enemy.IsFacingRight() != (direction > 0)) enemy.Flip();
            _target.Position = enemy.Position + new Vector2(direction * 20, 0);
            await Wait(.05);
            int health = _targetHealth.CurrentHealth;
            enemy.StateMachine.ChangeState(EnemyStateType.Attack);
            enemy.StateMachine._PhysicsProcess(enemy.StatsResource.AttackCooldown * .49);
            Check(!enemy.CombatController.Hitbox.Active, $"{archetype}/{direction}: no damage in windup");
            enemy.StateMachine._PhysicsProcess(enemy.StatsResource.AttackCooldown * .02);
            await Wait(.05);
            Check(_targetHealth.CurrentHealth == health - enemy.StatsResource.AttackDamage, $"{archetype}/{direction}: exactly one directional hit");
            enemy.StateMachine.ChangeState(EnemyStateType.Idle);
            Check(!enemy.CombatController.Hitbox.Active, $"{archetype}/{direction}: attack closes on exit");
        }
        _target.Position = new Vector2(4000, 450);
        enemy.StateMachine.ChangeState(EnemyStateType.Attack);
        enemy.Health.TakeDamage(new DamageInfo(1, Vector2.Right, 0, _target));
        Check(enemy.StateMachine.CurrentStateType == (enemy.StatsResource.InterruptOnHit ? EnemyStateType.Hurt : EnemyStateType.Attack),
            $"{archetype}: interruption policy preserved");
        await Wait(.2);

        // Lethal damage through a real Area2D overlap, as during gameplay.
        var striker = new Node2D { Name = "LethalStrike", Position = enemy.Position + new Vector2(0, -16) };
        var hit = new Hitbox { CollisionLayer = 8, CollisionMask = 4, Damage = 9999, Active = true };
        hit.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(30, 30) } });
        striker.AddChild(hit);
        AddChild(striker);
        await Wait(.15);
        Check(enemy.Health.IsDead && enemy.Visible && enemy.Sprite.Animation == "dead", $"{archetype}: lethal physics hit shows death pose");
        Check(!enemy.CombatController.Hitbox.Active && !enemy.CombatController.Hurtbox.Active && enemy.CollisionLayer == 0,
            $"{archetype}: death disables combat");
        Check(!enemy.GetNode<CanvasItem>("FloatingHP").Visible, $"{archetype}: no health bar on corpse");
        striker.QueueFree();
        await Wait(.9);
        Check(!GodotObject.IsInstanceValid(enemy), $"{archetype}: freed after death fade");
    }
}
