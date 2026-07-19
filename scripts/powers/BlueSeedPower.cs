using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public class BlueSeedPower : ModPowerTemplate
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

	public override PowerInstanceType InstanceType => (PowerInstanceType)1;

	public override string? CustomIconPath => "res://Arknights_Mizuki/images/powers/BlueSeedPower.png";

	public override string? CustomBigIconPath => "res://Arknights_Mizuki/images/powers/BlueSeedPower.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
		(DynamicVar)new EnergyVar(2),
	};
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		HoverTipFactory.FromPower<SanityPower>(),
		HoverTipFactory.FromPower<SanityBurstDescriptionPower>(),
	];

    public static async Task Trigger(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Creatures.Creature owner)
    {
        if(!owner.HasPower<BlueSeedPower>())
            return;

        await PlayerCmd.GainEnergy(2,owner.Player);
    }
}
