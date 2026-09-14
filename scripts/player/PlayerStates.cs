using Godot;
using Joguim.Abilities;
using Joguim.Core;

namespace Joguim.Player
{
    public class IdleState : PlayerState
    {
        public IdleState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            Player.PlayAnimation("idle");
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);
            Player.ApplyHorizontalMovement(delta);
            Player.MoveAndSlide();

            if (!Player.IsOnFloor())
            {
                StateMachine.ChangeState(PlayerStateType.Fall);
                return;
            }

            if (Input.IsActionPressed("move_left") || Input.IsActionPressed("move_right"))
            {
                StateMachine.ChangeState(PlayerStateType.Run);
                return;
            }

            if (Input.IsActionJustPressed("jump"))
            {
                Player.Jump();
                StateMachine.ChangeState(PlayerStateType.Jump);
                return;
            }

            if (Input.IsActionJustPressed("attack") && Player.CanAttack())
            {
                StateMachine.ChangeState(PlayerStateType.Attack);
                return;
            }
        }
    }

    public class RunState : PlayerState
    {
        public RunState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            Player.PlayAnimation("run");
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);
            Player.ApplyHorizontalMovement(delta);
            Player.MoveAndSlide();

            if (!Player.IsOnFloor())
            {
                StateMachine.ChangeState(PlayerStateType.Fall);
                return;
            }

            if (!Input.IsActionPressed("move_left") && !Input.IsActionPressed("move_right"))
            {
                StateMachine.ChangeState(PlayerStateType.Idle);
                return;
            }

            if (Input.IsActionJustPressed("jump"))
            {
                Player.Jump();
                StateMachine.ChangeState(PlayerStateType.Jump);
                return;
            }

            if (Input.IsActionJustPressed("attack") && Player.CanAttack())
            {
                StateMachine.ChangeState(PlayerStateType.Attack);
                return;
            }
        }
    }

    public class JumpState : PlayerState
    {
        public JumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            Player.PlayAnimation("jump");
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);
            Player.ApplyHorizontalMovement(delta);
            Player.MoveAndSlide();

            if (Player.IsOnFloor())
            {
                StateMachine.ChangeState(PlayerStateType.Idle);
                return;
            }

            if (Player.Velocity.Y > 0)
            {
                StateMachine.ChangeState(PlayerStateType.Fall);
                return;
            }

            if (Input.IsActionJustPressed("attack") && Player.CanAttack())
            {
                StateMachine.ChangeState(PlayerStateType.Attack);
                return;
            }

            if (AbilityManager.Instance != null && AbilityManager.Instance.HasAbility(AbilityId.DoubleJump) && Input.IsActionJustPressed("jump"))
            {
                Player.DoubleJump();
            }
        }
    }

    public class FallState : PlayerState
    {
        public FallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            Player.PlayAnimation("fall");
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);
            Player.ApplyHorizontalMovement(delta);
            Player.MoveAndSlide();

            if (Player.IsOnFloor())
            {
                StateMachine.ChangeState(PlayerStateType.Idle);
                return;
            }

            if (Input.IsActionJustPressed("attack") && Player.CanAttack())
            {
                StateMachine.ChangeState(PlayerStateType.Attack);
                return;
            }

            if (AbilityManager.Instance != null && AbilityManager.Instance.HasAbility(AbilityId.DoubleJump) && Input.IsActionJustPressed("jump"))
            {
                Player.DoubleJump();
                StateMachine.ChangeState(PlayerStateType.Jump);
            }
        }
    }

    public class AttackState : PlayerState
    {
        private float _attackTimer;
        private bool _attackFinished;

        public AttackState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            _attackTimer = Player.StatsResource.AttackCooldown;
            _attackFinished = false;
            Player.PlayAnimation("attack");
            Player.PerformAttack();
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);
            Player.MoveAndSlide();

            _attackTimer -= (float)delta;

            if (_attackTimer <= 0 && !_attackFinished)
            {
                _attackFinished = true;
                Player.FinishAttack();
                StateMachine.ChangeState(Player.IsOnFloor() ? PlayerStateType.Idle : PlayerStateType.Fall);
            }
        }
    }

    public class HurtState : PlayerState
    {
        private float _hurtTimer;

        public HurtState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            _hurtTimer = 0.5f;
            Player.PlayAnimation("hurt");
        }

        public override void PhysicsUpdate(double delta)
        {
            Player.ApplyGravity(delta);
            Player.MoveAndSlide();

            _hurtTimer -= (float)delta;
            if (_hurtTimer <= 0)
            {
                if (Player.IsOnFloor())
                {
                    StateMachine.ChangeState(PlayerStateType.Idle);
                }
                else
                {
                    StateMachine.ChangeState(PlayerStateType.Fall);
                }
            }
        }
    }

    public class DeadState : PlayerState
    {
        public DeadState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

        public override void Enter()
        {
            Player.PlayAnimation("dead");
            Player.SetProcess(false);
            Player.SetPhysicsProcess(false);
        }
    }
}
