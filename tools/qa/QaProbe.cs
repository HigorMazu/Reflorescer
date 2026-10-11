using Godot;
using Joguim.Combat;

namespace Joguim.QA
{
    /// <summary>Teste de QA: ações que os scripts GDScript de teste não conseguem fazer sozinhos.</summary>
    public partial class QaProbe : Node
    {
        // Aplica dano pelo mesmo caminho do combate (Health.TakeDamage), ignorando i-frames.
        public void Damage(Health health, int amount)
        {
            if (health == null) return;
            health.SetInvulnerable(false);
            health.TakeDamage(new DamageInfo(amount, Vector2.Zero, 0f, null));
        }
    }
}
