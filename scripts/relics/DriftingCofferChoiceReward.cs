using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace Arknights_Mizuki.Scripts.Relics;

internal sealed class DriftingCofferChoiceReward : RelicReward
{
    private readonly string _descriptionKey;

    public DriftingCofferChoiceReward(RelicModel relic, Player player, string descriptionKey, bool requiresKeyCharge)
        : base(relic, player)
    {
        _descriptionKey = descriptionKey;
        RequiresKeyCharge = requiresKeyCharge;
    }

    public bool RequiresKeyCharge { get; }

    public bool IsSelectable => !RequiresKeyCharge || Player.GetRelic<Key>()?.ChargesRemaining > 0;

    public override LocString Description => new("relics", _descriptionKey);

    protected override Task<bool> OnSelect()
    {
        return IsSelectable ? base.OnSelect() : Task.FromResult(false);
    }
}
