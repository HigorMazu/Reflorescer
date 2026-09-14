using Godot;
using System;
using System.Collections.Generic;

namespace Joguim.Player
{
    public enum PlayerStateType
    {
        Idle,
        Run,
        Jump,
        Fall,
        Attack,
        Hurt,
        Dodge,
        Dead,
        SpecialAbility
    }

    public abstract class PlayerState
    {
        protected PlayerController Player;
        protected PlayerStateMachine StateMachine;

        public PlayerState(PlayerController player, PlayerStateMachine stateMachine)
        {
            Player = player;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }
        public virtual void PhysicsUpdate(double delta) { }
        public virtual void HandleInput(InputEvent @event) { }
    }

    public partial class PlayerStateMachine : Node
    {
        private Dictionary<PlayerStateType, PlayerState> _states = new();
        private PlayerState _currentState;
        private PlayerState _previousState;
        private PlayerController _player;

        public PlayerState CurrentState => _currentState;
        public PlayerStateType CurrentStateType { get; private set; }

        public override void _Ready()
        {
            _player = GetParent<PlayerController>();
        }

        public void Initialize(PlayerController player)
        {
            _player = player;

            _states[PlayerStateType.Idle] = new IdleState(player, this);
            _states[PlayerStateType.Run] = new RunState(player, this);
            _states[PlayerStateType.Jump] = new JumpState(player, this);
            _states[PlayerStateType.Fall] = new FallState(player, this);
            _states[PlayerStateType.Attack] = new AttackState(player, this);
            _states[PlayerStateType.Hurt] = new HurtState(player, this);
            _states[PlayerStateType.Dead] = new DeadState(player, this);
        }

        public void ChangeState(PlayerStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
            {
                GD.PrintErr($"PlayerStateMachine: State {newStateType} not found.");
                return;
            }

            _currentState?.Exit();
            _previousState = _currentState;
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

        public override void _UnhandledInput(InputEvent @event)
        {
            _currentState?.HandleInput(@event);
        }

        public PlayerState GetPreviousState() => _previousState;
    }
}
