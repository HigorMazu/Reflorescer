using Godot;

namespace Joguim.Combat
{
    /// <summary>
    /// Retângulos de colisão em coordenadas do mundo, pra checar dano por contato pelo corpo visível
    /// (sobreposição de hurtboxes) em vez de distância entre centros (11/10/2026).
    /// </summary>
    public static class CombatGeometry
    {
        // Retângulo global de um CollisionShape2D com RectangleShape2D (sem rotação; escala negativa = espelhado)
        public static Rect2 GlobalRect(CollisionShape2D cs)
        {
            if (cs == null || cs.Shape is not RectangleShape2D rect) return new Rect2();
            var xf = cs.GlobalTransform;
            Vector2 size = new(Mathf.Abs(rect.Size.X * xf.Scale.X), Mathf.Abs(rect.Size.Y * xf.Scale.Y));
            return new Rect2(xf.Origin - size / 2, size);
        }

        // Hurtbox de qualquer personagem com o nó "Hurtbox/CollisionShape2D"
        public static Rect2 HurtRect(Node2D character) =>
            GlobalRect(character?.GetNodeOrNull<CollisionShape2D>("Hurtbox/CollisionShape2D"));

        // Troca a forma retangular de um CollisionShape2D por uma nova (as formas da cena base são compartilhadas).
        // O wrapper C# da forma nova é liberado na hora, na thread principal (ver ROADMAP_DEV C12-T4).
        public static void SetRect(CollisionShape2D cs, Vector2 size, Vector2 offset)
        {
            if (cs == null || size == Vector2.Zero) return;
            using var shape = new RectangleShape2D { Size = size };
            cs.Shape = shape;
            cs.Position = offset;
        }
    }
}
