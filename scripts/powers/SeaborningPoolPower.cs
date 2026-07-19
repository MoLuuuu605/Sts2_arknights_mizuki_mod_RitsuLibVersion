using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public class SeaborningPoolPower : ModPowerTemplate
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => PowerStackType.Counter;


	public override string? CustomIconPath => "res://Arknights_Mizuki/images/powers/Human.png";

	public override string? CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Human.png";

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
		(DynamicVar)new CardsVar(2),
	};	
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
	{
        if (Owner != player.Creature)
            return;

        if(Amount == 0 )return;
        DynamicVars.Cards.BaseValue=2*Amount;
        for (int i = 0 ; i<DynamicVars.Cards.BaseValue;i++)
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<BabyHs>(Owner.Player),MegaCrit.Sts2.Core.Entities.Cards.PileType.Draw,Owner.Player,CardPilePosition.Bottom));
    }
}
