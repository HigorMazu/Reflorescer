using Godot;
using System;

namespace Joguim.Core
{
    public partial class EventBus : Node
    {
        public static EventBus Instance { get; private set; }

        // Player signals
        [Signal] public delegate void PlayerDamagedEventHandler(int damage, Vector2 knockbackDirection);
        [Signal] public delegate void PlayerDiedEventHandler();
        [Signal] public delegate void PlayerHealthChangedEventHandler(int currentHealth, int maxHealth);
        [Signal] public delegate void PlayerRespawnedEventHandler(Vector2 position);
        [Signal] public delegate void PlayerAbilityUsedEventHandler(string abilityName);
        // Espada de Grama ativada (true) ou guardada (false) com Q
        [Signal] public delegate void SwordToggledEventHandler(bool hasSword);
        // Prompt do interagível ao alcance do jogador ("" = nenhum, esconde o prompt)
        [Signal] public delegate void InteractionPromptChangedEventHandler(string prompt);

        // Ability signals
        [Signal] public delegate void AbilityUnlockedEventHandler(string abilityId);

        // Boss signals
        [Signal] public delegate void BossDefeatedEventHandler(string bossId);
        [Signal] public delegate void BossPhaseChangedEventHandler(string bossId, int phase);

        // Checkpoint signals
        [Signal] public delegate void CheckpointActivatedEventHandler(Vector2 position, string checkpointId);

        // Item signals
        [Signal] public delegate void ItemCollectedEventHandler(string itemId, string itemType);

        // Area signals
        [Signal] public delegate void AreaChangedEventHandler(string areaName);

        // Combat signals
        [Signal] public delegate void EnemyDefeatedEventHandler(string enemyId);
        [Signal] public delegate void DamageDealtEventHandler(Node2D target, int damage);

        // World signals
        [Signal] public delegate void SecretRevealedEventHandler(string secretId);
        // Restauração ODS 15: um RestorationPoint foi restaurado (não confundir com CheckpointActivated)
        [Signal] public delegate void AreaRestoredEventHandler(string pointId);

        // Pause signals
        [Signal] public delegate void PauseToggledEventHandler(bool isPaused);

        public override void _Ready()
        {
            Instance = this;
        }
    }
}
