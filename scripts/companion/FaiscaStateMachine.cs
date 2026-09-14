using Godot;
using System.Collections.Generic;

namespace Joguim.Companion
{
    public enum FaiscaStateType
    {
        Follow,
        Idle,
        Investigate,
        Interact,
        ReturnToPlayer
    }

    public abstract class FaiscaState
    {
        protected FaiscaController Faisca;
        protected FaiscaStateMachine StateMachine;

        public FaiscaState(FaiscaController faisca, FaiscaStateMachine stateMachine)
        {
            Faisca = faisca;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }
        public virtual void PhysicsUpdate(double delta) { }
    }

    public partial class FaiscaStateMachine : Node
    {
        private Dictionary<FaiscaStateType, FaiscaState> _states = new();
        private FaiscaState _currentState;
        private FaiscaController _faisca;

        public FaiscaState CurrentState => _currentState;
        public FaiscaStateType CurrentStateType { get; private set; }

        public override void _Ready()
        {
            _faisca = GetParent<FaiscaController>();
        }

        public void Initialize(FaiscaController faisca)
        {
            _faisca = faisca;

            _states[FaiscaStateType.Follow] = new FollowState(faisca, this);
            _states[FaiscaStateType.Idle] = new IdleFaiscaState(faisca, this);
            _states[FaiscaStateType.Investigate] = new InvestigateState(faisca, this);
            _states[FaiscaStateType.ReturnToPlayer] = new ReturnToPlayerState(faisca, this);
        }

        public void ChangeState(FaiscaStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
            {
                GD.PrintErr($"FaiscaStateMachine: State {newStateType} not found.");
                return;
            }

            _currentState?.Exit();
            _currentState = _states[newStateType];
            CurrentStateType = newStateType;
            _currentState.Enter();
        }

        public override void _Process(double delta)
        {
            _currentState?.Update(delta);
        }

        public override void _PhysicsProcess(double delta)
        {
            _currentState?.PhysicsUpdate(delta);
        }
    }

    public class FollowState : FaiscaState
    {
        public FollowState(FaiscaController faisca, FaiscaStateMachine stateMachine) : base(faisca, stateMachine) { }

        public override void Enter()
        {
            Faisca.PlayAnimation("fly");
        }

        public override void PhysicsUpdate(double delta)
        {
            Faisca.FollowPlayer(delta);
        }
    }

    public class IdleFaiscaState : FaiscaState
    {
        private float _idleTimer;

        public IdleFaiscaState(FaiscaController faisca, FaiscaStateMachine stateMachine) : base(faisca, stateMachine) { }

        public override void Enter()
        {
            _idleTimer = 2.0f;
            Faisca.PlayAnimation("idle");
        }

        public override void PhysicsUpdate(double delta)
        {
            Faisca.Hover(delta);
            _idleTimer -= (float)delta;

            if (_idleTimer <= 0)
            {
                StateMachine.ChangeState(FaiscaStateType.Follow);
            }
        }
    }

    public class InvestigateState : FaiscaState
    {
        public InvestigateState(FaiscaController faisca, FaiscaStateMachine stateMachine) : base(faisca, stateMachine) { }

        public override void Enter()
        {
            Faisca.PlayAnimation("investigate");
        }

        public override void PhysicsUpdate(double delta)
        {
            Faisca.Hover(delta);

            if (Faisca.InvestigationComplete)
            {
                StateMachine.ChangeState(FaiscaStateType.ReturnToPlayer);
            }
        }
    }

    public class ReturnToPlayerState : FaiscaState
    {
        public ReturnToPlayerState(FaiscaController faisca, FaiscaStateMachine stateMachine) : base(faisca, stateMachine) { }

        public override void Enter()
        {
            Faisca.PlayAnimation("fly");
        }

        public override void PhysicsUpdate(double delta)
        {
            Faisca.FollowPlayer(delta);

            float distance = Faisca.GlobalPosition.DistanceTo(Faisca.GetTargetPosition());
            if (distance < 30f)
            {
                StateMachine.ChangeState(FaiscaStateType.Follow);
            }
        }
    }
}
