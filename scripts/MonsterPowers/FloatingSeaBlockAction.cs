using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.Utils;
using Arknights_Mizuki.RitsuAdapters;
using STS2RitsuLib.Interop.AutoRegistration;
using MinionLib.Targeting;

namespace Arknights_Mizuki.Scripts.Actions;

[RegisterPower]
public sealed class FloatingSeaBlockAction : ModActionTemplate
{
    public override TargetType TargetType => MinionTargetTypes.AnyMinionOrOwner;
    private const int BaseBlock = 3;
    public override bool DecrementAfterAct => true;
    public override bool OnlyRespondIconClick => true;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/FloatingSeaBlockAction.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/FloatingSeaBlockAction.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        (DynamicVar)new BlockVar(BaseBlock, ValueProp.Unpowered)
    ];

    public override LocString Description => AddDynamicDescriptionVars(base.Description);

    protected override async Task OnAct(PlayerChoiceContext choiceContext, Creature? target)
    {
        if (target == null) return;
        var actor = Owner;
        var block = GetBlockAmount();
        Entry.Logger.Info($"[MinionAction] FloatingSeaBlockAction act actor={actor.Name} target={target.Name} block={block}");
        MinionAnimationHelper.Play(actor, "Attack");
        await CreatureCmd.GainBlock(target, new BlockVar(block, ValueProp.Unpowered), null);
        await CreatureCmd.Damage(choiceContext,Owner,1,ValueProp.Unblockable|ValueProp.Unpowered,null,null);
    }

    private LocString AddDynamicDescriptionVars(LocString locString)
    {
        locString.Add("Block", GetBlockAmount());
        return locString;
    }

    private decimal GetBlockAmount() => BaseBlock + GetSeabornizationAmount();

    private decimal GetSeabornizationAmount()
    {
        var owner = IsMutable ? Owner : null;
        return owner?.Powers.OfType<SeabornizationPower>().Sum(power => power.Amount) ?? 0m;
    }
}
