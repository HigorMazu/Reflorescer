using Godot;
using System;
using System.Threading.Tasks;
using Joguim.Bosses;
using Joguim.Combat;
using Joguim.Core;

// Run only as the startup scene of a fresh process. Does not load/create a save.
public partial class BossDefeatCheck : Node
{
    private int _failures;
    private void Check(bool passed, string message)
    {
        if (!passed) _failures++;
        GD.Print($"BOSS_QA_{(passed ? "PASS" : "FAIL")}: {message}");
    }
    private async Task Wait(double seconds) => await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);

    public override async void _Ready()
    {
        try
        {
            int events = 0;
            EventBus.Instance.BossDefeated += id => events++;
            var boss = GD.Load<PackedScene>("res://scenes/bosses/Boss_Javali.tscn").Instantiate<BossBase>();
            AddChild(boss);
            boss.SetPhysicsProcess(false);
            boss.StateMachine.SetPhysicsProcess(false);
            boss.Health.TakeDamage(new DamageInfo(99999, Vector2.Right, 0, boss));
            Check(boss.Visible && boss.Sprite.Animation == "dead", "death starts with visible dead pose");
            Check(events == 1, "victory emitted once at death");
            await Wait(.8);
            Check(GodotObject.IsInstanceValid(boss) && boss.Sprite.Animation == "defeated", "defeated pose shown during fade");
            Check(boss.Modulate.A < 1 && boss.Modulate.A > 0, "fade remains active");
            Check(events == 1, "defeated does not emit another victory");
            await Wait(.65);
            Check(!GodotObject.IsInstanceValid(boss), "boss freed after existing fade interval");
        }
        catch (Exception e) { _failures++; GD.PushError(e.ToString()); }
        Engine.TimeScale = 1;
        GD.Print($"BOSS_QA_DONE failures={_failures}");
        GetTree().Quit(_failures == 0 ? 0 : 1);
    }
}
