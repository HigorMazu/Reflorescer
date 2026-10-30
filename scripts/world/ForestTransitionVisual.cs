using Godot;

namespace Joguim.World
{
    /// <summary>Passagem viva que substitui os marcadores provisórios entre as áreas da floresta.</summary>
    public partial class ForestTransitionVisual : Node2D
    {
        public enum PassageTheme
        {
            Sope,
            Dossel,
            Igarape,
            Cave,
            DosselDescent,
            IgarapeCanopyBase
        }

        private static readonly Texture2D SopePassage = GD.Load<Texture2D>(
            "res://assets/environment/tropical/forest_transition_sope_dossel_tree_v3.png");
        private static readonly Texture2D DosselPassage = GD.Load<Texture2D>(
            "res://assets/environment/tropical/forest_transition_sope_dossel_tree_v3.png");
        private static readonly Texture2D DosselUpperPassage = GD.Load<Texture2D>(
            "res://assets/environment/tropical/forest_transition_dossel_upper_v1.png");
        private static readonly Texture2D IgarapePassage = GD.Load<Texture2D>(
            "res://assets/environment/tropical/forest_transition_igarape_v3.png");
        private static readonly Texture2D CavePassage = GD.Load<Texture2D>(
            "res://assets/environment/tropical/forest_transition_cave_v3.png");
        private static readonly Texture2D DosselDescentPassage = GD.Load<Texture2D>(
            "res://assets/environment/tropical/forest_transition_dossel_to_igarape_v8.png");
        private static readonly Texture2D IgarapeCanopyBasePassage = GD.Load<Texture2D>(
            "res://assets/environment/tropical/forest_transition_igarape_from_dossel_v6.png");

        [Export] public bool FlipHorizontally = false;
        [Export] public PassageTheme Theme = PassageTheme.Sope;
        [Export] public float GroundOffsetY = 0f;
        private float _time;

        public override void _Ready()
        {
            var material = new ShaderMaterial
            {
                Shader = GD.Load<Shader>("res://assets/shaders/transition_edge.gdshader")
            };
            if (Theme == PassageTheme.DosselDescent)
            {
                material.SetShaderParameter("top_edge_fade", 0.0f);
                material.SetShaderParameter("bottom_edge_fade", 1.0f);
            }
            else if (Theme is PassageTheme.Sope or PassageTheme.Dossel)
            {
                material.SetShaderParameter("top_edge_fade", Theme == PassageTheme.Dossel ? 1.0f : 0.0f);
                if (Theme == PassageTheme.Dossel)
                    material.SetShaderParameter("top_edge_width", 0.14f);
            }
            Material = material;

            if (Theme == PassageTheme.Dossel && DosselUpperPassage != null)
            {
                // The Dossel camera climbs well above the return passage. A second
                // trunk section continues past its upper limit and overlaps the
                // lower tree, with both alpha edges feathered at their join.
                var upperMaterial = new ShaderMaterial { Shader = material.Shader };
                upperMaterial.SetShaderParameter("top_edge_fade", 0.0f);
                upperMaterial.SetShaderParameter("bottom_edge_fade", 1.0f);
                var upper = new Sprite2D
                {
                    Name = "UpperCanopy",
                    Texture = DosselUpperPassage,
                    Centered = false,
                    Position = new Vector2(-280, -1430 + GroundOffsetY),
                    Scale = new Vector2(1100f / 1536f, 950f / 1024f),
                    TextureFilter = TextureFilterEnum.Nearest,
                    ZIndex = -1,
                    Material = upperMaterial
                };
                AddChild(upper);
            }

        }

        public override void _Process(double delta)
        {
            _time += (float)delta;
            QueueRedraw();
        }

        public override void _Draw()
        {
            Texture2D sidePassage = Theme switch
            {
                PassageTheme.Dossel => DosselPassage,
                PassageTheme.Igarape => IgarapePassage,
                PassageTheme.Cave => CavePassage,
                PassageTheme.DosselDescent => DosselDescentPassage,
                PassageTheme.IgarapeCanopyBase => IgarapeCanopyBasePassage,
                _ => SopePassage
            };
            if (sidePassage == null) return;

            // The root wall is deliberately side-on. Its free side faces the player,
            // rather than reading as a front-facing portal presented to the screen.
            // The 50px baseline is shared by the transition volumes.  It puts the roots
            // directly on the walkable surface in Sopé, Dossel and Igarapé.
            // Wider roots overlap the supporting branch/ground instead of ending
            // as a small isolated patch at the edge of the camera.
            // A conexão Dossel -> Igarapé é um marco de cenário, não um portal
            // compacto: o tronco atravessa a câmera e a abertura fica na altura
            // da plataforma real. As demais passagens mantêm o recorte lateral.
            Rect2 rect = Theme switch
            {
                PassageTheme.DosselDescent => new Rect2(-656, -415 + GroundOffsetY, 1275, 850),
                PassageTheme.Sope or PassageTheme.Dossel => new Rect2(-642, -700 + GroundOffsetY, 1000, 755),
                _ => new Rect2(-112, -95 + GroundOffsetY, 224, 150)
            };
            if (FlipHorizontally)
            {
                DrawSetTransform(Vector2.Zero, 0f, new Vector2(-1f, 1f));
                DrawTextureRect(sidePassage, rect, false);
                DrawSetTransform(Vector2.Zero, 0f, Vector2.One);
            }
            else
            {
                DrawTextureRect(sidePassage, rect, false);
            }

            // Fireflies keep the threshold alive without hiding the player.
            for (int i = 0; i < 6; i++)
            {
                float phase = _time * (0.7f + i * 0.08f) + i * 1.37f;
                float x = Mathf.Sin(phase * 1.6f) * (20f + (i % 2) * 12f);
                float y = -22f - Mathf.PosMod(phase * 13f, 64f);
                float pulse = 0.45f + 0.35f * Mathf.Sin(phase * 3f);
                DrawCircle(new Vector2(x, y), 1.0f + (i % 2) * 0.35f,
                    new Color(0.95f, 0.82f, 0.32f, pulse));
            }
        }
    }
}
