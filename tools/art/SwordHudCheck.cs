using Godot;
using System;
using System.Threading.Tasks;
using Joguim.Core;
using Joguim.Player;
using Joguim.UI;

// Isolated production player/HUD integration. No checkpoints or save operations.
public partial class SwordHudCheck : Node
{
    private int _failures;
    private const string IndicatorPath = "MarginContainer/VBoxContainer/TopRow/HealthSection/SwordStatus";
    private void Check(bool valid, string message)
    {
        if (!valid) _failures++;
        GD.Print($"SWORD_HUD_{(valid ? "PASS" : "FAIL")}: {message}");
    }
    private async Task Wait() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    public override async void _Ready()
    {
        try
        {
            GameManager.Instance.HasSword = false;
            var hud = GD.Load<PackedScene>("res://scenes/ui/HUD.tscn").Instantiate<HUDController>();
            AddChild(hud);
            await Wait(); await Wait();
            var indicator = hud.GetNode<SwordStatusIndicator>(IndicatorPath);
            Check(!indicator.HasSword, "HUD before player uses session state");
            var player = GD.Load<PackedScene>("res://scenes/player/Player.tscn").Instantiate<PlayerController>();
            player.ProcessMode = ProcessModeEnum.Disabled;
            AddChild(player);
            var sword = player.GetNode<GrassSwordVisual>("Visual/RightArm/RightHand/Sword");
            sword.ProcessMode = ProcessModeEnum.Always;
            await Wait(); await Wait();
            Check(!indicator.HasSword && !player.CanAttack(), "stowed player cannot attack");
            Check(sword.Extension == 0 && !player.Sword.Visible, "stowed spawn has no blade");
            hud.SetAbilityDisplay("Dash disponível");
            player.ToggleSword();
            Check(indicator.HasSword && player.CanAttack(), "toggle enables both combat and indicator");
            await ToSignal(GetTree().CreateTimer(.12), SceneTreeTimer.SignalName.Timeout);
            Check(sword.Extension > 0 && sword.Extension < 1, "blade grows through intermediate size");
            await ToSignal(GetTree().CreateTimer(.2), SceneTreeTimer.SignalName.Timeout);
            Check(Mathf.IsEqualApprox(sword.Extension, 1) && player.Sword.Visible, "blade fully appears");
            Check(player.Sprite.SelfModulate == Colors.White, "active character keeps original colors");
            player.Sprite.Play("idle"); player.Sprite.Frame = 0; await Wait();
            Check(player.Sword.GlobalPosition.X < player.GlobalPosition.X, "idle sword grip follows Kairo's right closed fist");
            var gripOverlay = player.GetNode<Sprite2D>("Visual/SwordGripOverlay");
            Check(gripOverlay.ZIndex > player.Sword.GetParent<CanvasItem>().ZIndex, "closed fist layer draws over sword grip");
            Check(gripOverlay.Position.IsEqualApprox(new Vector2(-25.5f, 0)), "grip overlay matches the original idle fist center");
            player.Sprite.Play("attack"); await Wait(); await Wait();
            Check(player.Sword.Visible, "straight sword stays visible in attack");
            Check(player.Sprite.SpriteFrames.GetFrameTexture("attack", 0).ResourcePath.Contains("unarmed_v2"), "attack frames no longer embed old curved sword");
            player.Sprite.Play("jump"); await Wait(); await Wait();
            Check(player.Sword.Visible, "separate blade follows unarmed jump pose");
            player.Sprite.Play("idle");
            for (int i = 0; i < 7; i++) player.ToggleSword();
            Check(!indicator.HasSword && !player.HasSword, "rapid toggles leave final state consistent");
            await ToSignal(GetTree().CreateTimer(.35), SceneTreeTimer.SignalName.Timeout);
            Check(sword.Extension == 0 && !player.Sword.Visible, "rapid reversal fully retracts blade");
            Check(player.Sprite.SelfModulate == Colors.White, "stowed character keeps original colors");
            Check(hud.GetNode<Label>("MarginContainer/VBoxContainer/TopRow/AbilitySection/AbilityDisplay").Text == "Dash disponível", "ability text preserved");
            hud.HideAll(); Check(!indicator.Visible, "HideAll includes sword");
            hud.ShowAll(); Check(indicator.Visible, "ShowAll restores sword");
            await Wait(); await Wait();
            Check(indicator.Size == new Vector2(156, 32), "fixed 156 x 32 footprint");
            hud.QueueFree(); await Wait(); await Wait();
            hud = GD.Load<PackedScene>("res://scenes/ui/HUD.tscn").Instantiate<HUDController>();
            AddChild(hud); await Wait(); await Wait();
            indicator = hud.GetNode<SwordStatusIndicator>(IndicatorPath);
            Check(!indicator.HasSword, "replacement HUD restores stowed state");
            player.ToggleSword(); Check(indicator.HasSword, "new HUD subscribed after old HUD freed");
            GetTree().Paused = true;
            Check(indicator.HasSword, "pause preserves state");
            GetTree().Paused = false;
        }
        catch (Exception error) { _failures++; GD.PushError(error.ToString()); }
        GD.Print($"SWORD_HUD_DONE failures={_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
