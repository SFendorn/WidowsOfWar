using HarmonyLib;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign && gameStarter is CampaignGameStarter campaignGameStarter)
            {
                campaignGameStarter.AddBehavior(new VillageRecruitBehavior());
                campaignGameStarter.AddBehavior(new TownRecruitBehavior());
                campaignGameStarter.AddBehavior(new NotableRecruitBehavior());
            }
        }
    }
}
