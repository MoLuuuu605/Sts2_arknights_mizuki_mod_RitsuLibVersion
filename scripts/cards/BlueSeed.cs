using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class BlueSeed : ModCardTemplate
{
	private const int energyCost = 2;

	private const CardType type = CardType.Power;

	private const CardRarity rarity = (CardRarity)4;

	private const TargetType targetType = (TargetType)0;

	private const bool shouldShowInCardLibrary = true;

	public override string PortraitPath => "res://Arknights_Mizuki/images/cards/blue_seed.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
    {
		(DynamicVar)new EnergyVar(2),
		(DynamicVar)new PowerVar<BlueSeedPower>(1m)
	};

	public BlueSeed()
		: base(2, (CardType)3, (CardRarity)4, (TargetType)0, true)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay _)
	{
		await PowerCmd.Apply<BlueSeedPower>(choiceContext, ((CardModel)this).Owner.Creature, ((CardModel)this).DynamicVars["BlueSeedPower"].BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
	}

	protected override void OnUpgrade()
	{
		this.EnergyCost.UpgradeBy(-1);
	}
}
