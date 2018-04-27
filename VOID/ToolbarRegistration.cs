using UnityEngine;
using ToolbarControl_NS;

namespace VOID
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(VOIDCore_Generic<VOIDCore_Editor>.MODID, VOIDCore_Generic<VOIDCore_Editor>.MODNAME);
        }
    }
}