using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Cards;

[Pool(typeof(MzkCardPool))]
public sealed class AphoticMolt : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
    private int MaxHp = 0;

    private bool losedMaxHpThisCombat=false;
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[4]
    {
        (DynamicVar)new DynamicVar("MaxHpLoss", 6m),
        (DynamicVar)new PowerVar<StealthPower>(3m),
        (DynamicVar)new CardsVar(3),
        (DynamicVar)new EnergyVar(2)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromPower<StealthPower>()
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1] { CardKeyword.Exhaust };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/AphoticMolt.png";

    public AphoticMolt() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override async Task BeforeCombatStart()
    {
        MaxHp=Owner.Creature.MaxHp;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.MaxHp <= MaxHp)losedMaxHpThisCombat=true;
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (Owner.Creature.MaxHp <= MaxHp)losedMaxHpThisCombat=true;
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner.Creature.MaxHp <= MaxHp)losedMaxHpThisCombat=true;
    }



    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if(losedMaxHpThisCombat)await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);

        decimal loss = DynamicVars["MaxHpLoss"].BaseValue;

        await CreatureCmd.SetMaxHp(Owner.Creature, Owner.Creature.MaxHp - loss);
        await PowerCmd.Apply<StealthPower>(choiceContext, Owner.Creature, DynamicVars["StealthPower"].BaseValue, Owner.Creature, (CardModel)(object)this);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

    }

    protected override void OnUpgrade()
    {
        DynamicVars["MaxHpLoss"].UpgradeValueBy(-3m);
    }
}
