using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace WidowsOfWar
{
    public static class RecruitModel
    {
        private static Dictionary<string, bool> settlementBasicRecruitReplacer = new Dictionary<string, bool>();
        private static Dictionary<string, bool> settlementEliteRecruitReplacer = new Dictionary<string, bool>();

        public static void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("WidowSettlementBasicRecruitReplacer", ref settlementBasicRecruitReplacer);
            dataStore.SyncData("WidowSettlementEliteRecruitReplacer", ref settlementEliteRecruitReplacer);
        }

        public static void SetBasicRecruitmentReplacer(Settlement settlement, bool enable)
        {
            settlementBasicRecruitReplacer[settlement.StringId] = enable;
        }

        public static void SetEliteRecruitmentReplacer(Settlement settlement, bool enable)
        {
            settlementEliteRecruitReplacer[settlement.StringId] = enable;
        }

        public static bool IsBasicRecruitReplacementEnabled(Settlement settlement)
        {
            return settlementBasicRecruitReplacer.TryGetValue(settlement.StringId, out bool enable) && enable;
        }

        public static bool IsEliteRecruitReplacementEnabled(Settlement settlement)
        {
            return settlementEliteRecruitReplacer.TryGetValue(settlement.StringId, out bool enable) && enable;
        }

        public static CharacterObject GetBasicTroop(Settlement settlement, bool replacement)
        {
            return replacement ? GetTroopType(settlement.Culture.GetCultureCode(), false) : settlement.Culture.BasicTroop;
        }

        public static CharacterObject GetEliteTroop(Settlement settlement, bool replacement)
        {
            return replacement ? GetTroopType(settlement.Culture.GetCultureCode(), true) : settlement.Culture.EliteBasicTroop;
        }

        public static CharacterObject GetActiveBasicRecruit(Settlement settlement)
        {
            return GetBasicTroop(settlement, IsBasicRecruitReplacementEnabled(settlement));
        }

        public static CharacterObject GetActiveEliteRecruit(Settlement settlement)
        {
            return GetEliteTroop(settlement, IsEliteRecruitReplacementEnabled(settlement));
        }

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

        public static bool IsInTroopTree(CharacterObject target, CharacterObject current)
        {
            if (target == current)
                return true;

            foreach (var elem in current.UpgradeTargets)
            {
                if (IsInTroopTree(target, elem))
                    return true;
            }
            return false;
        }

        public static CharacterObject UpgradeToTier(CharacterObject troop, int tier)
        {
            while (troop.Tier < tier)
            {
                if (troop.UpgradeTargets.IsEmpty())
                    return troop;

                troop = troop.UpgradeTargets[MBRandom.RandomInt(troop.UpgradeTargets.Length)];
            }
            return troop;
        }
    }
}
