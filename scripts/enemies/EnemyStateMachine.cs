using Godot;
using System.Collections.Generic;

namespace Joguim.Enemies
{
    public enum EnemyStateType
    {
        Idle,
        Patrol,
        DetectPlayer,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    public abstract class EnemyState
    {
        protected EnemyBase Enemy;
        protected EnemyStateMachine StateMachine;

        public EnemyState(EnemyBase enemy, EnemyStateMachine stateMachine)
        {
            Enemy = enemy;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double delta) { }
        public virtual void PhysicsUpdate(double delta) { }
    }

    public partial class EnemyStateMachine : Node
    {
        private Dictionary<EnemyStateType, EnemyState> _states = new();
        private EnemyState _currentState;
        private EnemyBase _enemy;

        public EnemyState CurrentState => _currentState;
        public EnemyStateType CurrentStateType { get; private set; }

        public override void _Ready()
        {
            _enemy = GetParent<EnemyBase>();
        }

        public void Initialize(EnemyBase enemy)
        {
            _enemy = enemy;

            _states[EnemyStateType.Idle] = new EnemyIdleState(enemy, this);
            _states[EnemyStateType.Patrol] = new EnemyPatrolState(enemy, this);
            _states[EnemyStateType.DetectPlayer] = new EnemyDetectPlayerState(enemy, this);
            _states[EnemyStateType.Chase] = new EnemyChaseState(enemy, this);
            _states[EnemyStateType.Attack] = new EnemyAttackState(enemy, this);
            _states[EnemyStateType.Hurt] = new EnemyHurtState(enemy, this);
            _states[EnemyStateType.Dead] = new EnemyDeadState(enemy, this);
        }

        public void ChangeState(EnemyStateType newStateType)
        {
            if (!_states.ContainsKey(newStateType))
            {
                GD.PrintErr($"EnemyStateMachine: State {newStateType} not found.");
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

    public class EnemyIdleState : EnemyState
    {
        private float _idleTimer;

        public EnemyIdleState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _idleTimer = Enemy.GetIdleDuration();
            Enemy.PlayAnimation("idle");
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);
            Enemy.MoveAndSlide();

            _idleTimer -= (float)delta;
            if (_idleTimer <= 0)
            {
                StateMachine.ChangeState(EnemyStateType.Patrol);
                return;
            }

            if (Enemy.CanDetectPlayer())
            {
                StateMachine.ChangeState(EnemyStateType.DetectPlayer);
            }
        }
    }

    public class EnemyPatrolState : EnemyState
    {
        public EnemyPatrolState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            Enemy.PlayAnimation("walk");
            Enemy.StartPatrol();
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);
            Enemy.MovePatrol(delta);
            Enemy.MoveAndSlide();

            if (Enemy.AtPatrolEdge())
            {
                // vira antes de parar, pra retomar a patrulha na direção oposta
                Enemy.Flip();
                StateMachine.ChangeState(EnemyStateType.Idle);
                return;
            }

            if (Enemy.CanDetectPlayer())
            {
                StateMachine.ChangeState(EnemyStateType.DetectPlayer);
            }
        }
    }

    public class EnemyDetectPlayerState : EnemyState
    {
        private float _detectTimer;

        public EnemyDetectPlayerState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _detectTimer = 0.5f;
            Enemy.PlayAnimation("detect");
            Enemy.StopMovement();
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);
            Enemy.MoveAndSlide();

            _detectTimer -= (float)delta;

            if (!Enemy.CanSeePlayer())
            {
                StateMachine.ChangeState(EnemyStateType.Patrol);
                return;
            }

            if (_detectTimer <= 0)
            {
                if (Enemy.IsInAttackRange())
                {
                    StateMachine.ChangeState(EnemyStateType.Attack);
                }
                else
                {
                    StateMachine.ChangeState(EnemyStateType.Chase);
                }
            }
        }
    }

    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            Enemy.PlayAnimation("walk");
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);
            Enemy.ChasePlayer(delta);
            Enemy.MoveAndSlide();

            if (!Enemy.CanSeePlayer())
            {
                StateMachine.ChangeState(EnemyStateType.Patrol);
                return;
            }

            if (Enemy.IsInAttackRange())
            {
                StateMachine.ChangeState(EnemyStateType.Attack);
            }
        }
    }

    public class EnemyAttackState : EnemyState
    {
        private float _attackTimer;
        private bool _hasAttacked;

        public EnemyAttackState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _attackTimer = Enemy.StatsResource.AttackCooldown;
            _hasAttacked = false;
            Enemy.PlayAnimation("attack");
            Enemy.StopMovement();
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);
            Enemy.MoveAndSlide();

            _attackTimer -= (float)delta;

            if (_attackTimer <= Enemy.StatsResource.AttackCooldown * 0.5f && !_hasAttacked)
            {
                _hasAttacked = true;
                Enemy.PerformAttack();
            }

            if (_attackTimer <= 0)
            {
                if (Enemy.IsInAttackRange() && Enemy.CanSeePlayer())
                {
                    StateMachine.ChangeState(EnemyStateType.Attack);
                }
                else if (Enemy.CanSeePlayer())
                {
                    StateMachine.ChangeState(EnemyStateType.Chase);
                }
                else
                {
                    StateMachine.ChangeState(EnemyStateType.Patrol);
                }
            }
        }
    }

    public class EnemyHurtState : EnemyState
    {
        private float _hurtTimer;

        public EnemyHurtState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            _hurtTimer = 0.3f;
            Enemy.PlayAnimation("hurt");
        }

        public override void PhysicsUpdate(double delta)
        {
            Enemy.ApplyGravity(delta);
            Enemy.MoveAndSlide();

            _hurtTimer -= (float)delta;
            if (_hurtTimer <= 0)
            {
                if (Enemy.CanDetectPlayer())
                {
                    StateMachine.ChangeState(EnemyStateType.Chase);
                }
                else
                {
                    StateMachine.ChangeState(EnemyStateType.Idle);
                }
            }
        }
    }

    public class EnemyDeadState : EnemyState
    {
        public EnemyDeadState(EnemyBase enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

        public override void Enter()
        {
            Enemy.PlayAnimation("dead");
            Enemy.StopMovement();
            Enemy.SetCollisionLayer(0);
            Enemy.SetCollisionMask(0);
        }
    }
}
