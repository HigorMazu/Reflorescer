using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace Joguim.UI
{
    /// <summary>
    /// Nome do botão de cada ação pro dispositivo que o jogador está usando agora (26/10/2026).
    /// O GameManager repassa todo input pra <see cref="Observe"/>; quando o jogador troca entre teclado e
    /// controle, <see cref="Changed"/> avisa as telas (abertura, faixas de dica, prompt, menu) pra trocar os textos.
    /// Textos com {acao} (ex.: "{jump} pra saltar") são traduzidos por <see cref="Format"/>.
    /// O mapeamento dos botões está no project.godot (layout padrão: A pula, X ataca, Y interage...).
    /// </summary>
    public static class InputHints
    {
        public enum PadStyle { Xbox, PlayStation, Nintendo }

        public static bool UsingGamepad { get; private set; }
        public static PadStyle Style { get; private set; } = PadStyle.Xbox;
        public static event Action Changed;

        private static readonly Dictionary<string, string> Keyboard = new()
        {
            ["move"] = "A / D", ["jump"] = "Espaço", ["attack"] = "K", ["dash"] = "Shift", ["interact"] = "E",
            ["toggle_sword"] = "Q", ["special"] = "J", ["pause"] = "Esc", ["map"] = "M",
            ["navigate"] = "↑ ↓", ["accept"] = "Enter",
        };
        private static readonly Dictionary<string, string> Xbox = new()
        {
            ["move"] = "Analógico", ["jump"] = "A", ["attack"] = "X", ["dash"] = "RB", ["interact"] = "Y",
            ["toggle_sword"] = "LB", ["special"] = "B", ["pause"] = "Start", ["map"] = "View",
            ["navigate"] = "D-pad", ["accept"] = "A",
        };
        // Mesma posição física dos botões do Xbox, com os nomes do PlayStation
        private static readonly Dictionary<string, string> PlayStation = new()
        {
            ["move"] = "Analógico", ["jump"] = "Cruz", ["attack"] = "Quadrado", ["dash"] = "R1", ["interact"] = "Triângulo",
            ["toggle_sword"] = "L1", ["special"] = "Círculo", ["pause"] = "Options", ["map"] = "Share",
            ["navigate"] = "D-pad", ["accept"] = "Cruz",
        };
        // No Switch o botão de baixo se chama B (posição física, igual aos outros)
        private static readonly Dictionary<string, string> Nintendo = new()
        {
            ["move"] = "Analógico", ["jump"] = "B", ["attack"] = "Y", ["dash"] = "R", ["interact"] = "X",
            ["toggle_sword"] = "L", ["special"] = "A", ["pause"] = "+", ["map"] = "−",
            ["navigate"] = "D-pad", ["accept"] = "B",
        };

        public static string Label(string action)
        {
            var table = !UsingGamepad ? Keyboard : Style switch
            {
                PadStyle.PlayStation => PlayStation,
                PadStyle.Nintendo => Nintendo,
                _ => Xbox,
            };
            return table.TryGetValue(action, out var label) ? label : action;
        }

        public static string Format(string text) =>
            string.IsNullOrEmpty(text) ? text : Regex.Replace(text, @"\{(\w+)\}", m => Label(m.Groups[1].Value));

        public static PadStyle StyleFor(string joyName)
        {
            string name = (joyName ?? "").ToLowerInvariant();
            if (name.Contains("playstation") || name.Contains("dualsense") || name.Contains("dualshock")
                || name.Contains("ps4") || name.Contains("ps5") || name.Contains("sony") || name == "wireless controller")
                return PadStyle.PlayStation;
            if (name.Contains("nintendo") || name.Contains("switch") || name.Contains("pro controller") || name.Contains("joy-con"))
                return PadStyle.Nintendo;
            return PadStyle.Xbox;
        }

        public static void Observe(InputEvent @event)
        {
            bool? gamepad = @event switch
            {
                InputEventJoypadButton b when b.Pressed => true,
                InputEventJoypadMotion m when Mathf.Abs(m.AxisValue) > 0.5f => true,
                InputEventKey k when k.Pressed => false,
                InputEventMouseButton mb when mb.Pressed => false,
                _ => null,
            };
            if (gamepad == null) return;
            var style = gamepad.Value ? StyleFor(Input.GetJoyName(@event.Device)) : Style;
            if (gamepad.Value == UsingGamepad && style == Style) return;
            UsingGamepad = gamepad.Value;
            Style = style;
            Changed?.Invoke();
        }

        // Pra testes e pra forçar um estado sem input real
        public static void Set(bool gamepad, PadStyle style = PadStyle.Xbox)
        {
            UsingGamepad = gamepad;
            Style = style;
            Changed?.Invoke();
        }
    }
}
