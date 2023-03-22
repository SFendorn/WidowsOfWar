using HarmonyLib;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

namespace WidowsOfWar
{
    [HarmonyPatch(typeof(RecruitmentCampaignBehavior), "UpdateVolunteersOfNotablesInSettlement")]
    public class NotableRecruitBehaviorPatch
    {
        public static void Postfix(Settlement settlement)
        {
            bool replaceBasic = RecruitModel.IsBasicRecruitReplacementEnabled(settlement);
            bool replaceElite = RecruitModel.IsEliteRecruitReplacementEnabled(settlement);
            if (!replaceBasic && !replaceElite)
                return;

            foreach (Hero notable in settlement.Notables.Where(x => x.CanHaveRecruits))
            {
                for (int index = 0; index < 6; ++index)
                {
                    CharacterObject volunteerType = notable.VolunteerTypes[index];
                    if (volunteerType != null && !volunteerType.IsFemale)
                    {
                        bool isElite = RecruitModel.IsInTroopTree(volunteerType, volunteerType.Culture.EliteBasicTroop);
                        if (replaceElite && isElite)
                        {
                            CharacterObject newRecruit = RecruitModel.GetActiveEliteRecruit(settlement);
                            notable.VolunteerTypes[index] = RecruitModel.UpgradeToTier(newRecruit, volunteerType.Tier);
                        }
                        if (replaceBasic && !isElite)
                        {
                            CharacterObject newRecruit = RecruitModel.GetActiveBasicRecruit(settlement);
                            notable.VolunteerTypes[index] = RecruitModel.UpgradeToTier(newRecruit, volunteerType.Tier);
                        }
                    }
                }
            }
        }
    }
}
