using Godot;
using System;
using System.Threading.Tasks;
using Joguim.Player;

// Runs the production input route; no saves, checkpoints or combat targets are created.
public partial class AttackThrustCheck : Node2D
{
    private int _failures;
    private void Check(bool valid, string message)
    {
        if (!valid) _failures++;
        GD.Print($"THRUST_QA_{(valid ? "PASS" : "FAIL")}: {message}");
    }
    private async Task Wait(float seconds) => await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);

    public override async void _Ready()
    {
        try
        {
            var floor = new StaticBody2D { Position = new Vector2(400, 440), CollisionLayer = 1 };
            floor.AddChild(new CollisionShape2D { Shape = new RectangleShape2D { Size = new Vector2(1200, 20) } });
            AddChild(floor);
            var player = GD.Load<PackedScene>("res://scenes/player/Player.tscn").Instantiate<PlayerController>();
            player.Position = new Vector2(260, 430);
            AddChild(player);
            await Wait(.08f);
            player.SetFacingDirection(Vector2.Right);
            player.SetSwordVisible(true);
            await Wait(.03f);

            Input.ParseInputEvent(new InputEventAction { Action = "attack", Pressed = true });
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            Input.ParseInputEvent(new InputEventAction { Action = "attack", Pressed = false });
            var hitbox = player.CombatController.Hitbox;
            await Wait(.04f);
            Check(player.Sprite.Animation == "attack", "attack input starts the attack animation");
            Check(!hitbox.Active, "hitbox stays closed during preparation");
            await Wait(.12f);
            Check(hitbox.Active && hitbox.Monitoring, "hitbox opens on thrust impact");
            Check(Mathf.IsZeroApprox(hitbox.Rotation) && Mathf.IsEqualApprox(hitbox.Position.X, 34), "hitbox is horizontal and aligned ahead of Kairo");
            Check(player.Sprite.Animation == "attack" && player.Sprite.Frame >= 1, "impact occurs after preparation frame");
            await Wait(.12f);
            Check(!hitbox.Active, "hitbox closes for recovery");
            await Wait(.15f);
            Check(player.CanAttack(), "attack returns to ready state after recovery");
        }
        catch (Exception error) { _failures++; GD.PushError(error.ToString()); }
        GD.Print($"THRUST_QA_DONE failures={_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
