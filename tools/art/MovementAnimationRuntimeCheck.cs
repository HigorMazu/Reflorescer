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

            // Seed test abilities without the progression event that autosaves slot 0.
            var testAbilities = AbilityManager.Instance.GetAllUnlockedAbilities();
            testAbilities.Add(AbilityId.Dash);
            AbilityManager.Instance.SetUnlockedAbilities(testAbilities);
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
            Check(effects.ContactSide > 0 && effects.ZIndex == 2, "right-wall contact is visible in front of the body");
            Check(effects.WallPoseOffset.X > 11f && effects.WallPoseRotation < -.6f,
                "claw and boot pivot toward the right-wall collision edge");
            Check(effects.WallClawContact.X > 8f && effects.WallBootContact.X > 8f,
                "right-wall scrape anchors follow the visible claw and front boot");
            Check(player.Velocity.Y <= player.StatsResource.WallSlideSpeed + 1, "wall slide still limits falling speed");
            Input.ActionPress("jump");
            await PhysicsFrames(2);
            Check(player.Velocity.X < 0 && player.Velocity.Y < 0, "wall jump still pushes away and upward");
            Input.ActionRelease("jump");
            Input.ActionRelease("move_right");
            await ToSignal(GetTree().CreateTimer(.2), SceneTreeTimer.SignalName.Timeout);
            AddSurface(new Vector2(-100, 20), new Vector2(20, 340));
            player.GlobalPosition = new Vector2(-60, 40);
            player.Velocity = new Vector2(0, 80);
            player.ResetPhysicsInterpolation();
            Input.ActionPress("move_left");
            await ToSignal(GetTree().CreateTimer(.2), SceneTreeTimer.SignalName.Timeout);
            Check(player.IsWallSliding && effects.ContactSide < 0, "left-wall contact follows the collision normal");
            Check(effects.WallPoseOffset.X < -11f && effects.WallPoseRotation > .6f,
                "mirrored claw and boot pivot toward the left-wall collision edge");
            Check(effects.WallClawContact.X < -8f && effects.WallBootContact.X < -8f,
                "left-wall scrape anchors follow the mirrored claw and front boot");
            Input.ActionRelease("move_left");
            player.GlobalPosition = new Vector2(0, 20);
            player.Velocity = Vector2.Zero;
            await PhysicsFrames(4);
            Check(effects.DisplayedState == "none" && !effects.Visible, "wall contact ends when the player leaves the wall");
            Check(effects.WallPoseOffset == Vector2.Zero, "wall pose returns to its normal position");
            Check(Mathf.IsZeroApprox(effects.WallPoseRotation), "wall pose returns to its normal angle");
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
            Input.ActionRelease("move_left");
            Input.ActionRelease("jump");
        }

        GD.Print($"MOVEMENT_RUNTIME_DONE failures={_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
