using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace WidowsOfWar
{
    public static class RefugeModel
    {
        private static Dictionary<string, int> townRefuge = new Dictionary<string, int>();

        public static void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("WidowTownRefuge", ref townRefuge);
        }

        public static bool HasWidowsRefuge(Settlement settlement)
        {
            return townRefuge.ContainsKey(settlement.StringId);
        }

        public static void EstablishWidowsRefuge(Hero hero)
        {
            townRefuge.Add(hero.CurrentSettlement.StringId, 1);
        }

        public static void DestroyWidowsRefuge(Settlement settlement)
        {
            townRefuge.Remove(settlement.StringId);
            InformationManager.DisplayMessage(new InformationMessage("Your widow's refuge has been burned to the ground in " + settlement.ToString(), new Color(0.8f, 0f, 0f)));
        }
    }
}
