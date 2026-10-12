using System.Collections.Generic;
using Godot;

namespace Joguim.Core
{
    /// <summary>
    /// Mantém carregados, o jogo inteiro, os Resources com script C# de res://resources
    /// (stats de inimigos, boss, fases e jogador).
    ///
    /// Sem isso, quando o último nó que usa um desses .tres é liberado (inimigo morto, troca de cena),
    /// o Godot descarrega o Resource; se ele é carregado de novo enquanto o finalizador do GC do .NET
    /// ainda está liberando o objeto C# antigo, o runtime do Godot 4.7.2 trava com
    /// FATAL "gchandle.is_released()" (e às vezes o inimigo nasce sem StatsResource).
    /// Investigação e reprodução: ROADMAP_DEV C12-T4, tools/qa/GcStressCheck.gd.
    /// Tem que ser o primeiro autoload, antes de qualquer cena carregar esses arquivos.
    /// </summary>
    public partial class ResourceKeeper : Node
    {
        private const string Root = "res://resources";
        private static readonly List<Resource> _kept = new();

        public static int Count => _kept.Count;

        public override void _Ready()
        {
            if (_kept.Count == 0) KeepDir(Root);
            GD.Print($"ResourceKeeper: {_kept.Count} recursos mantidos carregados.");
        }

        private static void KeepDir(string dir)
        {
            foreach (var file in DirAccess.GetFilesAt(dir))
            {
                // No jogo exportado os .tres aparecem como .tres.remap
                string name = file.EndsWith(".remap") ? file[..^".remap".Length] : file;
                if (!name.EndsWith(".tres")) continue;
                var res = ResourceLoader.Load($"{dir}/{name}");
                if (res != null) _kept.Add(res);
            }
            foreach (var sub in DirAccess.GetDirectoriesAt(dir))
                KeepDir($"{dir}/{sub}");
        }
    }
}
