using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace WidowsOfWar
{
    public static class RecruitModel
    {
        public static bool CanBuyTroops(CharacterObject troopType, int available, int cost_multiplier, string textVariableSuffix = "")
        {
            if (0 < available)
            {
                int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troopType, Hero.MainHero, false) * cost_multiplier;
                if (Hero.MainHero.Gold >= troopRecruitmentCost)
                {
                    int count = Math.Min(available, Hero.MainHero.Gold / troopRecruitmentCost);

                    MBTextManager.SetTextVariable("WIDOW_COUNT" + textVariableSuffix, count);
                    MBTextManager.SetTextVariable("WIDOW_NAME" + textVariableSuffix, troopType.Name + (count > 1 ? "s" : ""), false);
                    MBTextManager.SetTextVariable("WIDOW_TOTAL_COST" + textVariableSuffix, count * troopRecruitmentCost);
                    return true;
                }
            }
            return false;
        }

        public static int BuyTroops(CharacterObject troopType, int available, int costMultiplier = 1)
        {
            int troopRecruitmentCost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troopType, Hero.MainHero, false) * costMultiplier;
            if (Hero.MainHero.Gold < troopRecruitmentCost)
                return 0;
            int count = Math.Min(available, Hero.MainHero.Gold / troopRecruitmentCost);
            MobileParty.MainParty.MemberRoster.AddToCounts(troopType, count, false, 0, 0, true, -1);
            GiveGoldAction.ApplyBetweenCharacters((Hero)null, Hero.MainHero, -(count * troopRecruitmentCost), false);
            return count;
        }

        public static CharacterObject GetTroopType(CultureCode cultureCode, bool bandit = false)
        {
            switch (cultureCode)
            {
                case CultureCode.Aserai: return bandit ? CharacterObject.Find("aserai_widow_b_t2") : CharacterObject.Find("aserai_widow_a_t1");
                case CultureCode.Battania: return bandit ? CharacterObject.Find("battania_widow_b_t2") : CharacterObject.Find("battania_widow_a_t1");
                default:
                case CultureCode.Empire: return bandit ? CharacterObject.Find("empire_widow_b_t2") : CharacterObject.Find("empire_widow_a_t1");
                case CultureCode.Khuzait: return bandit ? CharacterObject.Find("khuzait_widow_b_t2") : CharacterObject.Find("khuzait_widow_a_t1");
                case CultureCode.Sturgia: return bandit ? CharacterObject.Find("sturgia_widow_b_t2") : CharacterObject.Find("sturgia_widow_a_t1");
                case CultureCode.Vlandia: return bandit ? CharacterObject.Find("vlandia_widow_b_t2") : CharacterObject.Find("vlandia_widow_a_t1");
            }
        }

        public static CharacterObject GetAlleyTroopTypeReplacement(CharacterObject character, CultureObject settlementCulture)
        {
            switch (character.StringId)
            {
                default:
                case "gangster_1": return RecruitModel.GetTroopType(settlementCulture.GetCultureCode(), false);
                case "gangster_2": return RecruitModel.GetTroopType(settlementCulture.GetCultureCode(), false);
                case "gangster_3": return RecruitModel.GetTroopType(settlementCulture.GetCultureCode(), false);
                case "sea_raiders_bandit":
                case "forest_bandits_bandit":
                case "desert_bandits_bandit":
                case "steppe_bandits_bandit":
                case "mountain_bandits_bandit": return RecruitModel.GetTroopType(settlementCulture.GetCultureCode(), true);
                case "sea_raiders_raider":
                case "forest_bandits_raider":
                case "desert_bandits_raider":
                case "steppe_bandits_raider":
                case "mountain_bandits_raider": return RecruitModel.GetTroopType(settlementCulture.GetCultureCode(), true).UpgradeTargets[0];
            }
        }

    }
}
