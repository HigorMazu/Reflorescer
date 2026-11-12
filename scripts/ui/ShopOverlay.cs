using Godot;
using Joguim.Core;
using Joguim.Player;
using Joguim.Progression;
using Joguim.Save;

namespace Joguim.UI
{
    /// <summary>Loja de upgrades do totem: cartas em arco no centro da tela, com o jogo pausado (C20-T5).</summary>
    public partial class ShopOverlay : Control
    {
        private static readonly Color Background = new(0.06f, 0.12f, 0.1f, 0.95f);
        private static readonly Color Border = new(0.67f, 0.77f, 0.46f, 0.9f);
        private static readonly Color CardSelectedFill = new(0.12f, 0.22f, 0.17f, 0.98f);
        private static readonly Color CardSelected = new(0.85f, 0.91f, 0.66f, 1f);
        private static readonly Color Gold = new(1f, 0.91f, 0.54f);
        private static readonly Color Muted = new(0.73f, 0.78f, 0.63f);
        private static readonly Color CannotAfford = new(0.85f, 0.42f, 0.36f);
        private static readonly Color Maxed = new(0.62f, 0.83f, 0.42f);

        private static readonly (UpgradeId Id, string Name)[] Upgrades =
        {
            (UpgradeId.Damage, "Dano"),
            (UpgradeId.AttackSpeed, "Vel. ataque"),
            (UpgradeId.Health, "Vida"),
            (UpgradeId.Defense, "Defesa"),
            (UpgradeId.MoveSpeed, "Vel. mov."),
        };

        private static readonly Vector2 CardSize = new(200, 250);
        private const float CardSpacing = 216f;
        private const float ArcDrop = 18f;
        private const float CardTilt = 5f;
        private const float SelectedScale = 1.07f;

        private readonly PanelContainer[] _cards = new PanelContainer[Upgrades.Length];

        public bool IsOpen { get; private set; }
        public int SelectedIndex { get; private set; }

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            MouseFilter = MouseFilterEnum.Ignore;
            SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
            Visible = false;
            Build();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            SelectedIndex = 0;
            Visible = true;
            GameManager.Instance?.SetShopOpen(true);
            Refresh();
        }

        public void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            Visible = false;
            GameManager.Instance?.SetShopOpen(false);
        }

        public void MoveSelection(int direction)
        {
            SelectedIndex = Mathf.PosMod(SelectedIndex + direction, Upgrades.Length);
            Refresh();
        }

        public bool BuySelected()
        {
            bool bought = SaveManager.Instance?.TryBuyUpgrade(Upgrades[SelectedIndex].Id) ?? false;
            AudioManager.Instance?.PlaySfx(bought ? "checkpoint_activate.wav" : "sword_off.wav");
            Refresh();
            return bought;
        }

        public override void _Input(InputEvent @event)
        {
            // A repetição da tecla segurada não pode comprar nem navegar sozinha
            if (!IsOpen || @event.IsEcho()) return;

            if (@event.IsActionPressed("pause") || @event.IsActionPressed("ui_cancel")) Close();
            else if (@event.IsActionPressed("ui_left") || @event.IsActionPressed("move_left")) MoveSelection(-1);
            else if (@event.IsActionPressed("ui_right") || @event.IsActionPressed("move_right")) MoveSelection(1);
            else if (@event.IsActionPressed("interact") || @event.IsActionPressed("ui_accept")) BuySelected();
            else return;

            GetViewport().SetInputAsHandled();
        }

        private void Build()
        {
            // Ponto central da tela: as cartas se posicionam em volta dele
            var arc = new Control { Name = "Cards", MouseFilter = MouseFilterEnum.Ignore };
            arc.SetAnchorsAndOffsetsPreset(LayoutPreset.Center);
            AddChild(arc);

            for (int i = 0; i < Upgrades.Length; i++)
            {
                // Parábola: quanto mais longe do meio, mais baixa e mais inclinada
                float fromCenter = i - (Upgrades.Length - 1) / 2f;
                var card = new PanelContainer
                {
                    Name = $"Card{i}",
                    CustomMinimumSize = CardSize,
                    Size = CardSize,
                    PivotOffset = CardSize / 2f,
                    Position = new Vector2(fromCenter * CardSpacing, fromCenter * fromCenter * ArcDrop) - CardSize / 2f,
                    RotationDegrees = fromCenter * CardTilt,
                    MouseFilter = MouseFilterEnum.Ignore,
                };
                arc.AddChild(card);
                _cards[i] = card;

                var content = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
                content.AddThemeConstantOverride("separation", 12);
                card.AddChild(content);

                var name = NewLabel(Upgrades[i].Name, Colors.White, 22);
                name.Name = "Title";
                content.AddChild(name);
                var pips = NewLabel("", Maxed, 22);
                pips.Name = "Pips";
                content.AddChild(pips);
                var effect = NewLabel("", Muted, 18);
                effect.Name = "Effect";
                content.AddChild(effect);
                var price = NewLabel("", Gold, 22);
                price.Name = "Price";
                content.AddChild(price);
            }
        }

        private void Refresh()
        {
            int essence = SaveManager.Instance?.Essence ?? 0;

            for (int i = 0; i < Upgrades.Length; i++)
            {
                var id = Upgrades[i].Id;
                int level = SaveManager.Instance?.GetUpgradeLevel(id) ?? 0;
                int cost = SaveManager.Instance?.GetUpgradePrice(id) ?? -1;
                var content = _cards[i].GetChild(0);

                content.GetNode<Label>("Pips").Text =
                    new string('●', level) + new string('○', UpgradeCatalog.MaxLevel - level);
                content.GetNode<Label>("Effect").Text = EffectText(id, level);

                var price = content.GetNode<Label>("Price");
                price.Text = cost < 0 ? "MÁX" : $"✦ {cost}";
                price.AddThemeColorOverride("font_color", cost < 0 ? Maxed : essence < cost ? CannotAfford : Gold);

                bool selected = i == SelectedIndex;
                _cards[i].AddThemeStyleboxOverride("panel", Box(
                    selected ? CardSelectedFill : Background,
                    selected ? CardSelected : Border, selected ? 3 : 2, 8));
                _cards[i].Scale = Vector2.One * (selected ? SelectedScale : 1f);
                _cards[i].ZIndex = selected ? 1 : 0;
            }
        }

        private string EffectText(UpgradeId id, int level)
        {
            bool maxed = level >= UpgradeCatalog.MaxLevel;
            var player = GetTree().GetFirstNodeInGroup("Player") as PlayerController;

            switch (id)
            {
                case UpgradeId.Damage:
                    int damage = player?.CurrentAttackDamage ?? 0;
                    return maxed ? $"{damage}" : $"{damage} → {damage + UpgradeCatalog.DamageBonus(1)}";
                case UpgradeId.AttackSpeed:
                    float cooldown = player?.CurrentAttackCooldown ?? 0f;
                    return maxed
                        ? $"{Seconds(cooldown)} s"
                        : $"{Seconds(cooldown)} → {Seconds(cooldown - UpgradeCatalog.AttackCooldownReduction(1))} s";
                case UpgradeId.Health:
                    int health = player?.Health?.MaxHealth ?? 0;
                    return maxed ? $"{health}" : $"{health} → {health + UpgradeCatalog.HealthBonus(1)}";
                case UpgradeId.Defense:
                    return maxed
                        ? $"-{UpgradeCatalog.Defense(level)} de dano"
                        : $"-{UpgradeCatalog.Defense(level)} → -{UpgradeCatalog.Defense(level + 1)} de dano";
                default:
                    int percent = Mathf.RoundToInt((UpgradeCatalog.MoveSpeedMultiplier(level) - 1f) * 100f);
                    int next = Mathf.RoundToInt((UpgradeCatalog.MoveSpeedMultiplier(level + 1) - 1f) * 100f);
                    return maxed ? $"+{percent}%" : $"+{percent}% → +{next}%";
            }
        }

        private static string Seconds(float value) =>
            value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',');

        private static Label NewLabel(string text, Color color, int size)
        {
            var label = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center };
            label.AddThemeColorOverride("font_color", color);
            label.AddThemeFontSizeOverride("font_size", size);
            return label;
        }

        private static StyleBoxFlat Box(Color background, Color border, int borderWidth, int padding)
        {
            var box = new StyleBoxFlat { BgColor = background, BorderColor = border };
            box.SetBorderWidthAll(borderWidth);
            box.SetCornerRadiusAll(6);
            box.SetContentMarginAll(padding);
            return box;
        }
    }
}
