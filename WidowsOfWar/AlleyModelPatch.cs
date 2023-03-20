using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;

namespace WidowsOfWar
{
    [HarmonyPatch(typeof(DefaultAlleyModel), "GetTroopsToRecruitFromAlleyDependingOnAlleyRandom")]
    public class AlleyModelPatch
    {
        public static void Postfix(Alley alley, ref TroopRoster __result)
        {
            if (!RefugeModel.HasWidowsRefuge(alley.Settlement))
                return;

            if (0 == __result.Count)
                return;

            TroopRoster replacementTroopRoster = TroopRoster.CreateDummyTroopRoster();
            foreach(var elem in __result.GetTroopRoster())
            {
                CharacterObject replacmentTroop = RecruitModel.GetAlleyTroopTypeReplacement(elem.Character, alley.Settlement.Culture);
                replacementTroopRoster.AddToCounts(replacmentTroop, elem.Number);
            }
            __result = replacementTroopRoster;
        }
    }
}
