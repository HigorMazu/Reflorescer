using Godot;
using Joguim.Abilities;

namespace Joguim.Resources
{
    [GlobalClass]
    public partial class AbilityResource : Resource
    {
        [Export] public AbilityId Id;
        [Export] public string DisplayName = "";
        [Export] public string Description = "";
        [Export] public Texture2D Icon;
        [Export] public bool StartsUnlocked = false;
    }
}
