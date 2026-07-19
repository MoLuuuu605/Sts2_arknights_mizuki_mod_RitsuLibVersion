using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
public sealed class ShadowEvolutionPower : ModPowerTemplate
{
    private const int BaseCardsLeft = 6;
    private const string CardsLeftKey = "CardsLeft";

    private sealed class Data
    {
        public bool TriggeredThisTurn { get; set; }
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override bool AllowNegative => false;

    public override int DisplayAmount => DynamicVars[CardsLeftKey].IntValue;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/shadow.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/shadow.png";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new(CardsLeftKey, BaseCardsLeft)
    };

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card is not BabyHs)
            return;

        Data data = GetInternalData<Data>();

        DynamicVars[CardsLeftKey].BaseValue--;
        InvokeDisplayAmountChanged();

        if (DynamicVars[CardsLeftKey].IntValue > 0)
            return;

        DynamicVars[CardsLeftKey].BaseValue = 6m;
        InvokeDisplayAmountChanged();

        Flash();
        await CreatureCmd.GainMaxHp(Owner, Amount);

    }
}
