using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class DriftingCoffer : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2),
        new GoldVar(30)
    };

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/drifting_coffer.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/drifting_coffer_outline.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/drifting_coffer.png";

    public override async Task AfterObtained()
    {
        Key? key = Owner.GetRelic<Key>();
        if (key == null || key.ChargesRemaining <= 0)
        {
            await RelicCmd.Remove(this);
            await RelicCmd.Obtain<BrokenDriftingCoffer>(Owner);
            return;
        }
        key.ChargesRemaining--;
        Flash();

        List<Reward> rewards = new();
        CardCreationOptions options = new(
            new CardPoolModel[] { Owner.Character.CardPool },
            CardCreationSource.Other,
            CardRarityOddsType.RegularEncounter);

        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            rewards.Add(new CardReward(options, 3, Owner));
        }
        if (!RewardScreenExtraRewardInjector.TryAppendToCurrentSelection(Owner, rewards))
            await RewardsCmd.OfferCustom(Owner, rewards);

        await RelicCmd.Remove(this);
    }
}
