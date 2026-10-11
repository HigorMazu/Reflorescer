using Godot;

namespace Joguim.QA
{
    /// <summary>Teste de QA: força coleta de lixo do .NET para expor finalizadores de objetos Godot já liberados.</summary>
    public partial class GcProbe : Node
    {
        public void Collect()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }
    }
}
