using System;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace WidowsOfWar
{
    public class TownRecruitBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(AddRefugeDialogs));
            CampaignEvents.AlleyOwnerChanged.AddNonSerializedListener(this, new Action<Alley, Hero, Hero>(OnAlleyOwnerChanged));
        }

        public override void SyncData(IDataStore dataStore)
        {
            RefugeModel.SyncData(dataStore);
        }

        public void AddRefugeDialogs(CampaignGameStarter obj)
        {
            string askwidow = "{=askwidow}I have noticed a lot of desperate looking women on the roads. Do you know anything about it?";
            string negwidow = "{=negwidow}Terrible, if you ask me. Sadly, we do not have enough influence in this town to change anything about it.";
            string nvermind = "{=nvermind}Never mind.";
            string poswidow = "{=poswidow}Terrible, if you ask me. I've wondered many a night if there is anything that can be done about it. I might be able to establish a widows' refuge in our alley. Are you interested?";
            string im_sorry = "{=im_sorry}I'm sorry, I cannot do that right now.";
            string how_much = "{=how_much}How much does this cost us?";
            string ask_cost = "{=ask_cost}Covering all the basic needs would cost me {REFUGE_COST}{GOLD_ICON}. Those desperate souls will be eternally grateful and we might be able to convince some of them to work for us in return. I leave the big decisions to you and will concentrate my efforts on maintaining the daily business. How does this sound?";
            string noafford = "{=noafford}I cannot afford that right now.";
            string letsdoit = "{=letsdoit}Let's do this!";
            string payaccpt = "{=payaccpt}Great! I will take care of everything. Come see me again in some time.";

            string getID(int x) { return "widows_refuge_conversation_" + new TextObject(x).ToString(); }
            int i = 0;

            // negative
            obj.AddPlayerLine(getID(i++), "hero_main_options", "widows_refuge_talk_denied", askwidow, ConversationOnConditionWidowsRefugeNegative, null);
            obj.AddDialogLine(getID(i++), "widows_refuge_talk_denied", "widows_refuge_talk_denied_answer", negwidow, null, null);
            obj.AddPlayerLine(getID(i++), "widows_refuge_talk_denied_answer", "lord_pretalk", nvermind, null, null);

            // positive
            obj.AddPlayerLine(getID(i++), "hero_main_options", "widows_refuge_talk", askwidow, OnConditionConversationWidowsRefugePositive, null);
            obj.AddDialogLine(getID(i++), "widows_refuge_talk", "widows_refuge_talk_answer", poswidow, null, null);
            obj.AddPlayerLine(getID(i++), "widows_refuge_talk_answer", "widows_refuge_ask_cost", how_much, null, null);
            obj.AddPlayerLine(getID(i++), "widows_refuge_talk_answer", "lord_pretalk", im_sorry, null, null);
            obj.AddDialogLine(getID(i++), "widows_refuge_ask_cost", "widows_refuge_ask_cost_answer", ask_cost, ConversationOnConditionWidowsRefugeAskCost, null);
            obj.AddPlayerLine(getID(i++), "widows_refuge_ask_cost_answer", "widows_refuge_pay", letsdoit, OnConditionConversationWidowsRefugePay, OnConsequenceConversationWidowsRefugePay);
            obj.AddPlayerLine(getID(i++), "widows_refuge_ask_cost_answer", "lord_pretalk", noafford, null, null);
            obj.AddDialogLine(getID(i++), "widows_refuge_pay", "close_window", payaccpt, null, null);
        }

        private void OnAlleyOwnerChanged(Alley alley, Hero newOwner, Hero oldOwner)
        {
            if (alley == null || newOwner == null || oldOwner == null)
                return;

            if (oldOwner.Clan == Clan.PlayerClan && newOwner.Clan != Clan.PlayerClan)
            {
                if (RefugeModel.HasWidowsRefuge(alley.Settlement))
                {
                    RefugeModel.DestroyWidowsRefuge(alley.Settlement);
                }
            }
        }

        private bool OnConditionConversationWidowsRefugePositive()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            if (!CanEstablishWidowsRefuge(Hero.OneToOneConversationHero))
                return false;
            return WantsToEstablishWidowsRefuge(Hero.OneToOneConversationHero);
        }

        private bool ConversationOnConditionWidowsRefugeNegative()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            if (!CanEstablishWidowsRefuge(Hero.OneToOneConversationHero))
                return false;
            return !WantsToEstablishWidowsRefuge(Hero.OneToOneConversationHero);
        }

        private bool ConversationOnConditionWidowsRefugeAskCost()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            MBTextManager.SetTextVariable("REFUGE_COST", GetWidowsRefugeEstablishCost(Hero.OneToOneConversationHero).ToString(), false);
            return true;
        }

        private bool OnConditionConversationWidowsRefugePay()
        {
            if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.CurrentSettlement == null)
                return false;
            return GetWidowsRefugeEstablishCost(Hero.OneToOneConversationHero) <= Hero.MainHero.Gold;
        }

        private void OnConsequenceConversationWidowsRefugePay()
        {
            GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -GetWidowsRefugeEstablishCost(Hero.OneToOneConversationHero), false);
            RefugeModel.EstablishWidowsRefuge(Hero.OneToOneConversationHero);
        }

        private bool CanEstablishWidowsRefuge(Hero hero)
        {
            if (hero.CurrentSettlement == null || hero.CurrentSettlement.IsVillage || hero.CurrentSettlement.IsCastle || hero.CurrentSettlement.IsHideout)
                return false;
            return hero.Clan == Clan.PlayerClan && !RefugeModel.HasWidowsRefuge(hero.CurrentSettlement);
        }

        private bool WantsToEstablishWidowsRefuge(Hero hero)
        {
            return hero.CurrentSettlement.Alleys.Any(x => x.Owner != null && x.Owner.Clan == Clan.PlayerClan);
        }

        private int GetWidowsRefugeEstablishCost(Hero hero)
        {
            int cost = 2500;
            float modifier = 1f;
            if (hero.CurrentSettlement.OwnerClan == Clan.PlayerClan)
                modifier -= 0.2f;
            if (hero.CurrentSettlement.IsStarving)
                modifier += 0.2f;
            if (hero.CurrentSettlement.IsBooming)
                modifier -= 0.1f;
            return (int)(cost * modifier);
        }
    }
}
