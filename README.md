# Widows of War
Female troop tree and recruitment behavior mod for Mount &amp; Blade II: Bannerlord

## Content

This mod adds six new troop trees to the game; one for each major faction. Each troop tree consists of a regular branch (up to tier 5) and a bandit branch (up to tier 4) which can be upgraded to elite troops (tier 5 and tier 6) similar to the base game bandits.

The modded troops are designed to be as balanced as the base game. Each modded unit is inspired by a unit from the base game and uses its skill levels or a slight variation of it. The equipment is carefully selected to look good and be in line with comparable base game equipment.

There are several ways to recruit the modded troops, catering towards early, mid and late game.
* Female villagers are dynamically added to roaming villager parties in low numbers.
* Whenever a village is raided, this village generates modded troops that can be recruited as long as the village is recovering from the raid (smoke rises from the village on the world map).
* If the modded troops are not recruited by the player from raided villages, they are distributed to roaming bandit parties as prisoners.
* If the player owns an alley in a town, they can talk to a clan member inside this town and invest into a *widows' refuge*. This will convert bandit troops in the alley to modded troops. The refuge will be destroyed if the player no longer owns the alley.
* If the player owns a fief, they can talk to a clan member inside the fief and select if the local notables should replace their basic and/or elite recruits with modded troops. This decision is reversible at any time.

## Compatibility

In terms of XML compatibility, this mod only adds stuff and does not override any base game behavior. 
Adding the mod mid game may lead to issues because of the added cultures.
Removing the mod mid game is not safe because of existing custom troops in parties and fiefs.

The submodule uses Harmony to postfix patch the functions `DefaultAlleyModel.GetTroopsToRecruitFromAlleyDependingOnAlleyRandom` and `RecruitmentCampaignBehavior.UpdateVolunteersOfNotablesInSettlement` in order to replace base game troops with modded troops. Other mods that also patch those functions might lead to the modded troops not being recruitable from alleys or notables.
