using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public sealed class TideReflection : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;
    
    private const int DamagePerCard = 2;
    private int GetBaseDamage()
    {
        return this.IsUpgraded ? 8 : 12;
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        new DamageVar(8,MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        (DynamicVar)new PowerVar<SanityPower>(1m),
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
    {
        HoverTipFactory.FromPower<SanityPower>(),
        HoverTipFactory.FromPower<SanityBurstDescriptionPower>(),
        HoverTipFactory.FromKeyword(AutoPlay.Autoplay)
    };

    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/TideReflection.png";

    public TideReflection() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this,cardPlay)
        .Targeting(cardPlay.Target)
        .Execute(choiceContext);
        if(!cardPlay.Target.IsAlive)return;
        await PowerCmd.Apply<SanityPower>(choiceContext,cardPlay.Target,DynamicVars["SanityPower"].BaseValue,Owner.Creature,this,false);
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return;
        if(!cardPlay.Card.Keywords.Contains(AutoPlay.Autoplay))return;
        if (this.Pile.Type == PileType.Hand)
        {
            this.DynamicVars.Damage.BaseValue+=DamagePerCard;
        }
    }
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        this.DynamicVars.Damage.BaseValue =GetBaseDamage();
    }



    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["SanityPower"].UpgradeValueBy(1);
    }
}
