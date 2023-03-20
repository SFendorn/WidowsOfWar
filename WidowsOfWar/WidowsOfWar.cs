using HarmonyLib;
using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace WidowsOfWar
{
    public class WidowsOfWar : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Harmony harmony = new Harmony("WidowsOfWarHarmony");
            harmony.PatchAll(Assembly.GetAssembly(GetType()));
        }
    }
}
