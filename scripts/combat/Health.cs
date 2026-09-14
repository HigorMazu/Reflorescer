using Godot;
using System;

namespace Joguim.Combat
{
    [GlobalClass]
    public partial class Health : Node
    {
        [Export] public int MaxHealth = 100;
        [Export] public bool Invulnerable = false;
        [Export] public float InvulnerabilityDuration = 1.0f;

        [Signal] public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
        [Signal] public delegate void DamageReceivedEventHandler(int damage, Vector2 knockbackDirection);
        [Signal] public delegate void DiedEventHandler();
        [Signal] public delegate void InvulnerabilityStartedEventHandler();
        [Signal] public delegate void InvulnerabilityEndedEventHandler();

        private int _currentHealth;
        private bool _isInvulnerable;
        private Timer _invulnerabilityTimer;

        public int CurrentHealth => _currentHealth;
        public bool IsDead => _currentHealth <= 0;
        public bool IsInvulnerable => _isInvulnerable;

        public override void _Ready()
        {
            _currentHealth = MaxHealth;

            _invulnerabilityTimer = new Timer();
            _invulnerabilityTimer.OneShot = true;
            _invulnerabilityTimer.Timeout += OnInvulnerabilityTimeout;
            AddChild(_invulnerabilityTimer);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_isInvulnerable || IsDead) return;
            if (damageInfo == null) return;

            int actualDamage = Mathf.Max(0, damageInfo.Damage);
            _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);

            Vector2 knockbackDir = damageInfo.KnockbackDirection.Normalized();
            EmitSignal("DamageReceived", actualDamage, knockbackDir);
            EmitSignal("HealthChanged", _currentHealth, MaxHealth);

            if (InvulnerabilityDuration > 0 && !Invulnerable)
            {
                StartInvulnerability(InvulnerabilityDuration);
            }

            if (_currentHealth <= 0)
            {
                EmitSignal("Died");
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;

            int actualHeal = Mathf.Max(0, amount);
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + actualHeal);
            EmitSignal("HealthChanged", _currentHealth, MaxHealth);
        }

        public void SetMaxHealth(int newMax, bool healToFull = false)
        {
            MaxHealth = Mathf.Max(1, newMax);
            if (healToFull)
            {
                _currentHealth = MaxHealth;
            }
            else
            {
                _currentHealth = Mathf.Min(_currentHealth, MaxHealth);
            }
            EmitSignal("HealthChanged", _currentHealth, MaxHealth);
        }

        public void StartInvulnerability(float duration)
        {
            _isInvulnerable = true;
            _invulnerabilityTimer.Start(duration);
            EmitSignal("InvulnerabilityStarted");
        }

        public void SetInvulnerable(bool value)
        {
            _isInvulnerable = value;
            if (value)
            {
                EmitSignal("InvulnerabilityStarted");
            }
            else
            {
                _invulnerabilityTimer.Stop();
                EmitSignal("InvulnerabilityEnded");
            }
        }

        private void OnInvulnerabilityTimeout()
        {
            _isInvulnerable = false;
            EmitSignal("InvulnerabilityEnded");
        }

        public void Reset()
        {
            _currentHealth = MaxHealth;
            _isInvulnerable = false;
            _invulnerabilityTimer.Stop();
            EmitSignal("HealthChanged", _currentHealth, MaxHealth);
        }
    }
}
