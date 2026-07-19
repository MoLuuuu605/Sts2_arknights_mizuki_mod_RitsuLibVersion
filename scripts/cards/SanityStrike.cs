using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.ValueProps;
using Arknights_Mizuki.Scripts.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class SanityStrike: ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        new DamageVar(6,ValueProp.Move),
        new DynamicVar("DamagePerSanity",2),
        new DynamicVar("TrueDamage",6)
    };

    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/todo.png";


    public SanityStrike() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        int Amounts=0;
        if(target.HasPower<SanityPower>()){
        Amounts = cardPlay.Target.GetPowerAmount<SanityPower>();
        }
        var damage = DynamicVars.Damage.IntValue + Amounts * DynamicVars["DamagePerSanity"].BaseValue;
        await DamageCmd.Attack(damage) // 造成伤害，数值来源于卡牌的基础伤害属性
            .FromCard(this,cardPlay) // 伤害来源于这张卡牌
            .Targeting(cardPlay.Target) // 伤害目标是玩家选择的目标
            .Execute(choiceContext);
        

    }
    protected override void OnUpgrade()
    {
        this.DynamicVars["DamagePerSanity"].UpgradeValueBy(1);
    }
}