namespace Joguim.Progression
{
    public enum UpgradeId { Damage, AttackSpeed, Health, Defense, MoveSpeed }

    /// <summary>Preços e efeitos dos upgrades comprados com essência (C20-T5).</summary>
    public static class UpgradeCatalog
    {
        public const int MaxLevel = 3;
        private static readonly int[] Prices = { 20, 40, 70 };

        /// <summary>Preço para sair de <paramref name="level"/>; -1 quando já está no máximo.</summary>
        public static int Price(int level) => level >= 0 && level < MaxLevel ? Prices[level] : -1;

        public static int DamageBonus(int level) => 5 * level;
        public static float AttackCooldownReduction(int level) => 0.03f * level;
        public static int HealthBonus(int level) => 15 * level;
        public static int Defense(int level) => 2 * level;
        public static float MoveSpeedMultiplier(int level) => 1f + 0.05f * level;
    }
}
