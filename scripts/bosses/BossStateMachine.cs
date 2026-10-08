using Godot;
using System.Collections.Generic;

namespace Joguim.Bosses
{
    public enum BossStateType
    {
        Phase1,
        Phase2,
        Enraged,
        Hurt,
        Dead,
        Intro,
        Defeated
    }

    public abstract class BossState
    {
        protected BossBase Boss;
        protected BossStateMachine StateMachine;

        public BossState(BossBase boss, BossStateMachine stateMachine)
        {
            Boss = boss;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }
        public virtual void PhysicsUpdate(double delta) { }
    }

    public partial class BossStateMachine : Node
    {
        private Dictionary<BossStateType, BossState> _states = new();
        private BossState _currentState;
        private BossBase _boss;

        public BossState CurrentState => _currentState;
        public BossStateType CurrentStateType { get; private set; }

        public override void _Ready()
        {
            _boss = GetParent<BossBase>();
        }

        public void Initialize(BossBase boss)
        {
            _boss = boss;

            _states[BossStateType.Intro] = new BossIntroState(boss, this);
            _states[BossStateType.Phase1] = new BossPhase1State(boss, this);
            _states[BossStateType.Phase2] = new BossPhase2State(boss, this);
            _states[BossStateType.Enraged] = new BossEnragedState(boss, this);
            _states[BossStateType.Hurt] = new BossHurtState(boss, this);
            _states[BossStateType.Dead] = new BossDeadState(boss, this);
            _states[BossStateType.Defeated] = new BossDefeatedState(boss, this);
        }

        public void ChangeState(BossStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
            {
                GD.PrintErr($"BossStateMachine: State {newStateType} not found.");
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

    public class BossIntroState : BossState
    {
        private float _introTimer;

        public BossIntroState(BossBase boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

        public override void Enter()
        {
            _introTimer = 2.0f;
            Boss.PlayAnimation("intro");
            Boss.StopMovement();
        }

        public override void PhysicsUpdate(double delta)
        {
            Boss.ApplyGravity(delta);
            Boss.MoveAndSlide();

            _introTimer -= (float)delta;
            if (_introTimer <= 0)
            {
                StateMachine.ChangeState(BossStateType.Phase1);
            }
        }
    }

    public class BossPhase1State : BossState
    {
        private float _attackTimer;
        private int _attackCount;
        private bool _started;

        public BossPhase1State(BossBase boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

        public override void Enter()
        {
            // Voltar do Hurt não zera o timer de ataque, senão o boss nunca ataca sob pressão
            if (!_started)
            {
                _started = true;
                _attackTimer = Boss.GetCurrentPhaseCooldown();
                _attackCount = 0;
            }
            Boss.PlayAnimation("phase1_idle");
        }

        public override void PhysicsUpdate(double delta)
        {
            Boss.ApplyGravity(delta);
            Boss.MoveAndSlide();

            if (!Boss.IsBusy) _attackTimer -= (float)delta;

            if (_attackTimer <= 0)
            {
                Boss.ChooseAttack();
                _attackCount++;
                _attackTimer = Boss.GetCurrentPhaseCooldown();
            }

            if (!Boss.IsBusy) Boss.ChasePlayer(delta);

            if (Boss.HealthPercentage <= 0.5f)
            {
                StateMachine.ChangeState(BossStateType.Phase2);
            }
        }
    }

    public class BossPhase2State : BossState
    {
        private float _attackTimer;
        private bool _phaseTransitionDone;

        public BossPhase2State(BossBase boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

        public override void Enter()
        {
            // A transição de fase só acontece uma vez; voltar do Hurt retoma a luta direto
            if (_phaseTransitionDone) return;
            _attackTimer = 1.5f;
            Boss.PlayAnimation("phase2_transition");
            Boss.StopMovement();
        }

        public override void PhysicsUpdate(double delta)
        {
            Boss.ApplyGravity(delta);
            Boss.MoveAndSlide();

            if (!_phaseTransitionDone)
            {
                _attackTimer -= (float)delta;
                if (_attackTimer <= 0)
                {
                    _phaseTransitionDone = true;
                    Boss.SetPhase(1);
                    Boss.PlayAnimation("walk");
                }
                return;
            }

            if (!Boss.IsBusy) _attackTimer -= (float)delta;

            if (_attackTimer <= 0)
            {
                Boss.ChooseAttack();
                _attackTimer = Boss.GetCurrentPhaseCooldown();
            }

            if (!Boss.IsBusy) Boss.ChasePlayer(delta);

            if (Boss.HealthPercentage <= 0.2f)
            {
                StateMachine.ChangeState(BossStateType.Enraged);
            }
        }
    }

    public class BossEnragedState : BossState
    {
        private float _attackTimer;
        private bool _enragedTransitionDone;

        public BossEnragedState(BossBase boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

        public override void Enter()
        {
            if (_enragedTransitionDone) return;
            _attackTimer = 1.0f;
            Boss.PlayAnimation("enraged_transition");
            Boss.StopMovement();
        }

        public override void PhysicsUpdate(double delta)
        {
            Boss.ApplyGravity(delta);
            Boss.MoveAndSlide();

            if (!_enragedTransitionDone)
            {
                _attackTimer -= (float)delta;
                if (_attackTimer <= 0)
                {
                    _enragedTransitionDone = true;
                    Boss.SetPhase(2);
                    Boss.PlayAnimation("walk");
                }
                return;
            }

            if (!Boss.IsBusy) _attackTimer -= (float)delta;

            if (_attackTimer <= 0)
            {
                Boss.ChooseAttack();
                _attackTimer = Boss.GetCurrentPhaseCooldown() * 0.7f;
            }

            if (!Boss.IsBusy) Boss.ChasePlayer(delta);
        }
    }

    public class BossHurtState : BossState
    {
        private float _hurtTimer;

        public BossHurtState(BossBase boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

        public override void Enter()
        {
            _hurtTimer = 0.5f;
            Boss.PlayAnimation("hurt");
            Boss.StopMovement();
        }

        public override void PhysicsUpdate(double delta)
        {
            Boss.ApplyGravity(delta);
            Boss.MoveAndSlide();

            _hurtTimer -= (float)delta;
            if (_hurtTimer <= 0)
            {
                if (Boss.HealthPercentage <= 0.2f)
                {
                    StateMachine.ChangeState(BossStateType.Enraged);
                }
                else if (Boss.HealthPercentage <= 0.5f)
                {
                    StateMachine.ChangeState(BossStateType.Phase2);
                }
                else
                {
                    StateMachine.ChangeState(BossStateType.Phase1);
                }
            }
        }
    }

    public class BossDeadState : BossState
    {
        public BossDeadState(BossBase boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

        public override void Enter()
        {
            Boss.PlayAnimation("dead");
            Boss.StopMovement();
            Boss.EmitDefeated();
        }
    }

    public class BossDefeatedState : BossState
    {
        public BossDefeatedState(BossBase boss, BossStateMachine stateMachine) : base(boss, stateMachine) { }

        public override void Enter()
        {
            Boss.PlayAnimation("defeated");
        }
    }
}
