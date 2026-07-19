using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Minions;
using MinionLib.Minion;
using MinionLib.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Arknights_Mizuki.Scripts.Powers;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Spray : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new DamageVar(3m, (ValueProp)5),
        new DynamicVar("Harvest", 1m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
	{
        HoverTipFactory.FromKeyword(Monster2.monster2),
        HoverTipFactory.FromKeyword(Monster2des.monster2des),
        HoverTipFactory.FromPower<SeabornizationPower>()	
        };
    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Spray.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [AutoPlay.Autoplay];

    public Spray() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }



    static public bool HasMinion<T>(Player player) where T : MinionModel
{
    return player.PlayerCombatState?.Pets.Any(p =>
        p is { IsAlive: true, IsPet: true, Monster: T }
    ) == true;
}
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(((CardModel)this).CombatState)
            .Execute(choiceContext);
         if(!HasMinion<HarvestMinion>(Owner))
        {
            _ = await MinionCmd.AddMinion<HarvestMinion>(choiceContext,Owner, new MinionSummonOptions(
                MaxHp: 8m,                              // 血量
                PrimaryStatAmount: 2m,                  // 主要参数（具体内容在随从的 OnSummon 里定义），还有次要参数等可以按需传入
                Source: this,                           // 召唤来源（通常是这张牌）
                Position: MinionPosition.Front));
        }
        else 
        {
            Creature? pet = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is HarvestMinion);
            await PowerCmd.Apply<SeabornizationPower>(choiceContext,pet,DynamicVars["Harvest"].BaseValue,pet,null);
        }
    }

    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars["Harvest"].UpgradeValueBy(1m);
    }
}
