using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
#if DEBUG
using TaleWorlds.Library;
#endif

namespace WidowsOfWar
{
    public class VillageRecruitBehavior : CampaignBehaviorBase
    {
        public Dictionary<string, int> RaidedVillageRecruits = new Dictionary<string, int>();
        private static readonly int s_villageRecruitCostMultiplier = 1;
        private static readonly float s_banditPartyTakesPrisionerMaxDistance = 20f;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(AddRecruitMenu));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailyUpdateVillageRecruits));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("WidowRaidedVillageRecruitCount", ref RaidedVillageRecruits);
        }

        public void DailyUpdateVillageRecruits(Settlement settlement)
        {
            if (!settlement.IsVillage)
                return;

            if (settlement.IsUnderRaid && MobileParty.MainParty.CurrentSettlement != null && MobileParty.MainParty.CurrentSettlement.StringId == settlement.StringId)
            {
                // Should avoid the possibility to recruit from villages that have been raided by the player recently (approximately 7 days).
                RaidedVillageRecruits[settlement.StringId] = -GetVillageWidowGrowth(settlement.Village.Hearth) * 7;
            }
            else if (settlement.IsUnderRaid || settlement.IsRaided)
            {
                if (!RaidedVillageRecruits.ContainsKey(settlement.StringId))
                {
                    RaidedVillageRecruits[settlement.StringId] = GetVillageWidowGrowth(settlement.Village.Hearth);
                }
                else
                {
                    RaidedVillageRecruits[settlement.StringId] += GetVillageWidowGrowth(settlement.Village.Hearth);
                }
                RaidedVillageRecruits[settlement.StringId] = DistributeToNearbyBandits(settlement, RaidedVillageRecruits[settlement.StringId]);
            }
            else
            {
                if (RaidedVillageRecruits.ContainsKey(settlement.StringId))
                {
                    RaidedVillageRecruits.Remove(settlement.StringId);
                }
            }
        }

        public void AddRecruitMenu(CampaignGameStarter obj)
        {
            obj.AddGameMenuOption("village_looted", "look_for_survivors", "Look for survivors", new GameMenuOption.OnConditionDelegate(OnConditionLookForSurvivors), new GameMenuOption.OnConsequenceDelegate(OnConsequenceLookForSurvivors), false, 0, false);
            obj.AddGameMenu("village_survivors", "As you look around the rubble and destroyed houses, you find some women gathering all sorts of improvised weapons. There teary eyes look hollow, but you can feel their determination.", args => { }, GameOverlays.MenuOverlayType.None, GameMenu.MenuFlags.None, null);
            obj.AddGameMenuOption("village_survivors", "recruit_widows", "Recruit {WIDOW_COUNT} {WIDOW_NAME} ({WIDOW_TOTAL_COST}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(OnConditionVillageRecruitWidows), new GameMenuOption.OnConsequenceDelegate(OnConsequenceVillageRecruitWidows), false, 0, false);
            obj.AddGameMenuOption("village_survivors", "leave_village", "Leave them be", new GameMenuOption.OnConditionDelegate(OnConditionLeave), args => GameMenu.SwitchToMenu("village_looted"));
        }

        private bool OnConditionLookForSurvivors(MenuCallbackArgs args)
        {
            Settlement settlement = MobileParty.MainParty.CurrentSettlement;
            if (settlement != null &&
                settlement.IsVillage &&
                settlement.IsRaided &&
                RaidedVillageRecruits.ContainsKey(settlement.StringId))
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                return RaidedVillageRecruits[settlement.StringId] > 0;
            }
            return false;
        }

        private void OnConsequenceLookForSurvivors(MenuCallbackArgs args)
        {
            GameMenu.SwitchToMenu("village_survivors");
        }

        private bool OnConditionVillageRecruitWidows(MenuCallbackArgs args)
        {
            Settlement settlement = MobileParty.MainParty.CurrentSettlement;
            if (settlement != null &&
                settlement.IsVillage &&
                settlement.IsRaided &&
                RaidedVillageRecruits.ContainsKey(settlement.StringId) &&
                RaidedVillageRecruits[settlement.StringId] > 0)
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                return RecruitModel.CanBuyTroops(RecruitModel.GetTroopType(settlement.Culture.GetCultureCode()), RaidedVillageRecruits[settlement.StringId], s_villageRecruitCostMultiplier);
            }
            return false;
        }

        private void OnConsequenceVillageRecruitWidows(MenuCallbackArgs args)
        {
            Settlement settlement = MobileParty.MainParty.CurrentSettlement;
            int count = RecruitModel.BuyTroops(RecruitModel.GetTroopType(settlement.Culture.GetCultureCode()), RaidedVillageRecruits[settlement.StringId], s_villageRecruitCostMultiplier);
            RaidedVillageRecruits[settlement.StringId] -= count;
            // Switching to village_looted here, so that the OnCondition gets refreshed, and the recruitment option is removed.
            GameMenu.SwitchToMenu("village_looted");
        }

        private bool OnConditionLeave(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Leave;
            return true;
        }

        private int DistributeToNearbyBandits(Settlement settlement, int currentRecruits)
        {
            if (0 < currentRecruits && settlement != null && settlement.IsVillage)
            {
                List<MobileParty> parties = Campaign.Current.BanditParties.FindAll(x => x.GetPosition2D.Distance(settlement.GetPosition2D) <= s_banditPartyTakesPrisionerMaxDistance);
                if (parties != null && !parties.IsEmpty())
                {
                    foreach (MobileParty mobileParty in parties.Where(x => x != null && x.IsBandit && !x.IsEngaging))
                    {
                        int potential = Math.Min(currentRecruits, mobileParty.Party.PrisonerSizeLimit - mobileParty.Party.NumberOfPrisoners);
                        int available = GetAvailableBanditRecruitCount(currentRecruits);
                        if (0 < available)
                        {
                            CharacterObject troop = RecruitModel.GetTroopType(settlement.Culture.GetCultureCode());
                            int add = Math.Min(available, potential);
                            mobileParty.AddPrisoner(troop, add);
                            currentRecruits -= add;
                            if (currentRecruits == 0)
                                return 0;
#if DEBUG
                        if (0 < add)
                            InformationManager.DisplayMessage(new InformationMessage(add.ToString() + " " + troop.ToString() + " from " + settlement.ToString() + " have been taken prisoner by bandit parties."));
#endif
                        }
                    }
                }
            }
            return currentRecruits;
        }

        private static int GetAvailableBanditRecruitCount(int recruits)
        {
            return Math.Max(1, (int)(recruits * MBRandom.RandomFloatRanged(0.25f, 0.75f)));
        }

        private static int GetVillageWidowGrowth(float hearth)
        {
            return Math.Min(4, Math.Max(1, (int)(hearth / 150)));
        }
    }
}
