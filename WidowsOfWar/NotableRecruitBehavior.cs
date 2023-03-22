using System;
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace WidowsOfWar
{
    public class NotableRecruitBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(AddRecruitReplacerDialogs));
        }

        public override void SyncData(IDataStore dataStore)
        {
            RecruitModel.SyncData(dataStore);
        }

        public void AddRecruitReplacerDialogs(CampaignGameStarter obj)
        {
            string askrplc = "{=askrplc}Let us talk about the recruitment strategy for {SETTLEMENT}.";
            string ansrplc = "{=ansrplc}Of course. Currently, {SETTLEMENT} trains {BASICTROOP}s and {ELITETROOP}s. Do you want to change anything?";
            string rpbasic = "{=rpbasic}I want to replace {BASICTROOPFROM}s with {BASICTROOPTO}s.";
            string rpelite = "{=rpelite}I want to replace {ELITETROOPFROM}s with {ELITETROOPTO}s.";
            string leaveit = "{=leaveit}Leave it at that.";
            string rpldone = "{=rpldone}I will see to it at once. Do you want to change anything else?";
            string rplmore = "{=rplmore}Yes, I do.";

            string getID(int x) { return "widows_replacer_conversation_" + new TextObject(x).ToString(); }
            int i = 0;
            obj.AddPlayerLine(getID(i++), "hero_main_options", "widows_recruit_talk", askrplc, ConversationOnConditionAskForRecruitReplacer, null);
            obj.AddDialogLine(getID(i++), "widows_recruit_talk", "widows_recruit_answer", ansrplc, ConversationOnConditionAnswerRecruitReplacer, null);
            obj.AddPlayerLine(getID(i++), "widows_recruit_answer", "widows_recruit_selected", rpbasic, ConversationOnConditionReplaceBasic, OnConsequenceConversationReplaceBasic);
            obj.AddPlayerLine(getID(i++), "widows_recruit_answer", "widows_recruit_selected", rpelite, ConversationOnConditionReplaceElite, OnConsequenceConversationReplaceElite);
            obj.AddPlayerLine(getID(i++), "widows_recruit_answer", "lord_pretalk", leaveit, null, null);
            obj.AddDialogLine(getID(i++), "widows_recruit_selected", "widows_recruit_select_again", rpldone, null, null);
            obj.AddPlayerLine(getID(i++), "widows_recruit_select_again", "widows_recruit_talk", rplmore, null, null);
            obj.AddPlayerLine(getID(i++), "widows_recruit_select_again", "lord_pretalk", leaveit, null, null);
        }

        private bool ConversationOnConditionAskForRecruitReplacer()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            MBTextManager.SetTextVariable("SETTLEMENT", Hero.OneToOneConversationHero.CurrentSettlement.ToString(), false);
            return CanReplaceRecruits(Hero.OneToOneConversationHero);
        }

        private bool ConversationOnConditionAnswerRecruitReplacer()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            Settlement settlement = Hero.OneToOneConversationHero.CurrentSettlement;
            MBTextManager.SetTextVariable("SETTLEMENT", settlement.ToString(), false);
            MBTextManager.SetTextVariable("BASICTROOP", RecruitModel.GetActiveBasicRecruit(settlement).ToString(), false);
            MBTextManager.SetTextVariable("ELITETROOP", RecruitModel.GetActiveEliteRecruit(settlement).ToString(), false);
            return true;
        }

        private bool ConversationOnConditionReplaceBasic()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            Settlement settlement = Hero.OneToOneConversationHero.CurrentSettlement;
            bool currentState = RecruitModel.IsBasicRecruitReplacementEnabled(settlement);
            MBTextManager.SetTextVariable("BASICTROOPFROM", RecruitModel.GetBasicTroop(settlement, currentState).ToString(), false);
            MBTextManager.SetTextVariable("BASICTROOPTO", RecruitModel.GetBasicTroop(settlement, !currentState).ToString(), false);
            return true;
        }

        private bool ConversationOnConditionReplaceElite()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            Settlement settlement = Hero.OneToOneConversationHero.CurrentSettlement;
            bool currentState = RecruitModel.IsEliteRecruitReplacementEnabled(settlement);
            MBTextManager.SetTextVariable("ELITETROOPFROM", RecruitModel.GetEliteTroop(settlement, currentState).ToString(), false);
            MBTextManager.SetTextVariable("ELITETROOPTO", RecruitModel.GetEliteTroop(settlement, !currentState).ToString(), false);
            return true;
        }

        private void OnConsequenceConversationReplaceBasic()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return;
            Settlement settlement = Hero.OneToOneConversationHero.CurrentSettlement;
            RecruitModel.SetBasicRecruitmentReplacer(settlement, !RecruitModel.IsBasicRecruitReplacementEnabled(settlement));
        }

        private void OnConsequenceConversationReplaceElite()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return;
            Settlement settlement = Hero.OneToOneConversationHero.CurrentSettlement;
            RecruitModel.SetEliteRecruitmentReplacer(settlement, !RecruitModel.IsEliteRecruitReplacementEnabled(settlement));
        }

        private bool CanReplaceRecruits(Hero hero)
        {
            if (hero.CurrentSettlement.Notables.Any(x => x.CanHaveRecruits))
            {
#if DEBUG
                return true;
#else
                return hero.Clan == Clan.PlayerClan && hero.CurrentSettlement.OwnerClan == Clan.PlayerClan;
#endif
            }
            return false;
        }
    }
}
