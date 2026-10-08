using Godot;
using System;
using System.Threading.Tasks;
using Joguim.Abilities;
using Joguim.Player;

/// <summary>Exercises the production input/state path for dash and wall grip.</summary>
public partial class MovementAnimationRuntimeCheck : Node2D
{
    private int _failures;

    private void Check(bool valid, string message)
    {
        if (!valid) _failures++;
        GD.Print($"MOVEMENT_RUNTIME_{(valid ? "PASS" : "FAIL")}: {message}");
    }

    private async Task PhysicsFrames(int count)
    {
        for (int i = 0; i < count; i++)
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    }

    private StaticBody2D AddSurface(Vector2 position, Vector2 size)
    {
        var body = new StaticBody2D { Position = position };
        var collider = new CollisionShape2D { Shape = new RectangleShape2D { Size = size } };
        body.AddChild(collider);
        AddChild(body);
        return body;
    }

    public override async void _Ready()
    {
        try
        {
            AddSurface(new Vector2(0, 200), new Vector2(500, 20));
            AddSurface(new Vector2(100, 20), new Vector2(20, 340));

            var player = GD.Load<PackedScene>("res://scenes/player/Player.tscn").Instantiate<PlayerController>();
            player.Position = new Vector2(0, 170);
            AddChild(player);
            player.GetNode<Camera2D>("Camera2D").Enabled = false;
            var effects = player.GetNode<MovementStateEffects>("MovementStateEffects");

            AbilityManager.Instance.UnlockAbility(AbilityId.Dash);
            await PhysicsFrames(4);
            Check(player.IsOnFloor(), "player settled on the production floor collision");

            Input.ActionPress("dash");
            await ToSignal(GetTree().CreateTimer(.08), SceneTreeTimer.SignalName.Timeout);
            Check(player.IsDashing, "dash input enters the real dash state");
            Check(player.Sprite.Animation == "dash", "real dash state selects the dash animation");
            Check(player.Visual.Modulate == Colors.White, "dash preserves Kairo's normal colors");
            Check(effects.DisplayedState == "dash", "dash movement accents are active in gameplay");
            Input.ActionRelease("dash");

            await ToSignal(GetTree().CreateTimer(.3), SceneTreeTimer.SignalName.Timeout);
            player.GlobalPosition = new Vector2(60, 40);
            player.Velocity = new Vector2(0, 80);
            player.ResetPhysicsInterpolation();
            Input.ActionPress("move_right");
            await ToSignal(GetTree().CreateTimer(.2), SceneTreeTimer.SignalName.Timeout);
            Check(player.IsOnWall(), "player reaches the production wall collision");
            Check(player.IsWallSliding, "holding toward the wall enters the wall-grip state");
            Check(player.Sprite.Animation == "wall_slide", "wall-grip state selects the wall animation");
            Check(effects.DisplayedState == "wall_slide", "wall scrape accents are active in gameplay");
            Input.ActionRelease("move_right");
        }
        catch (Exception error)
        {
            _failures++;
            GD.PushError(error.ToString());
        }
        finally
        {
            Input.ActionRelease("dash");
            Input.ActionRelease("move_right");
        }

        GD.Print($"MOVEMENT_RUNTIME_DONE failures={_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
