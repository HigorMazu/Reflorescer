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
            var bladeSprite = player.GetNode<AnimatedSprite2D>("Visual/RightArm/RightHand/Sword/AnimatedSprite2D");
            var bandage = player.GetNode<SwordBandageVisual>("Visual/RightArm/RightHand/Bandage");
            sword.ProcessMode = ProcessModeEnum.Always;
            await Wait(); await Wait();
            Check(!indicator.HasSword && !player.CanAttack(), "stowed player cannot attack");
            Check(sword.Extension == 0 && !player.Sword.Visible, "stowed spawn has no blade");
            Check(bandage.Visible, "stowed spawn shows wrist bandage");
            hud.SetAbilityDisplay("Dash disponível");
            player.ToggleSword();
            Check(indicator.HasSword && player.CanAttack(), "toggle enables both combat and indicator");
            await ToSignal(GetTree().CreateTimer(.12), SceneTreeTimer.SignalName.Timeout);
            Check(sword.Extension > 0 && sword.Extension < 1, "blade grows through intermediate size");
            await ToSignal(GetTree().CreateTimer(.2), SceneTreeTimer.SignalName.Timeout);
            Check(Mathf.IsEqualApprox(sword.Extension, 1) && player.Sword.Visible, "blade fully appears");
            Check(Mathf.IsEqualApprox(bladeSprite.Scale.Y, 1.4f), "extended blade is longer while the grip remains fixed");
            Check(!bandage.Visible, "active blade hides wrist bandage");
            Check(player.Sprite.SelfModulate == Colors.White, "active character keeps original colors");
            Check(bladeSprite.Offset.IsEqualApprox(new Vector2(-5.5f, -30f)), "only the blade leaves Kairo's closed fist");
            player.Sprite.Play("idle"); player.Sprite.Frame = 0; await Wait(); await Wait();
            Check(player.Sword.GlobalPosition.X < player.GlobalPosition.X, "idle sword grip follows Kairo's right closed fist");
            Check(sword.ZIndex < player.Sprite.ZIndex, "Kairo's hand and arm stay in front of the active blade");
            Check(player.GetNodeOrNull("Visual/SwordGripOverlay") == null, "no duplicate fist overlay remains");
            foreach (string pose in new[] { "idle", "run", "jump", "fall", "dash", "wall_slide", "attack" })
            {
                int frameCount = player.Sprite.SpriteFrames.GetFrameCount(pose);
                for (int frame = 0; frame < frameCount; frame++)
                {
                    player.Sprite.Play(pose);
                    player.Sprite.Frame = frame;
                    await Wait(); await Wait();
                    Check(player.Sword.GlobalPosition.DistanceTo(player.ToGlobal(sword.GripPosition)) < 0.6f,
                        $"{pose}/{frame} sword handle stays on the measured fist anchor");
                }
            }
            player.Sprite.Play("attack"); await Wait(); await Wait();
            Check(bladeSprite.Offset.IsEqualApprox(new Vector2(-5.5f, -30f)), "attack blade begins inside the closed fist");
            Check(sword.ZIndex < player.Sprite.ZIndex, "attack grip stays behind Kairo's closed hand");
            Check(player.Sword.Visible, "straight sword stays visible in attack");
            Check(player.Sprite.SpriteFrames.GetFrameTexture("attack", 0).ResourcePath.Contains("unarmed_v2"), "attack frames no longer embed old curved sword");
            player.Sprite.Play("jump"); await Wait(); await Wait();
            Check(player.Sword.Visible, "separate blade follows unarmed jump pose");
            player.Sprite.Play("idle");
            for (int i = 0; i < 7; i++) player.ToggleSword();
            Check(!indicator.HasSword && !player.HasSword, "rapid toggles leave final state consistent");
            await ToSignal(GetTree().CreateTimer(.35), SceneTreeTimer.SignalName.Timeout);
            Check(sword.Extension == 0 && !player.Sword.Visible, "rapid reversal fully retracts blade");
            Check(bandage.Visible, "wrist bandage returns after blade fully retracts");
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
