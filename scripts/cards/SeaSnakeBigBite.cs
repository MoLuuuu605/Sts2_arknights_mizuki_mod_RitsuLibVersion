using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.Minions;
using MinionLib.Minion;
using MinionLib.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using Arknights_Mizuki.Scripts.keywords;

namespace Arknights_Mizuki.Scripts.Cards;

// 注册卡牌。如果要写自定义池看添加人物的开头
[RegisterCard(typeof(MzkCardPool))]
public class SeaSnakeBigBite : ModCardTemplate
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Skill;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Uncommon;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[2]
	{
		(DynamicVar)new PowerVar<SanityPower>(2m),
        new DynamicVar("Harvest",4)
	};
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
	{
		HoverTipFactory.FromPower<SanityPower>(),
        HoverTipFactory.FromPower<SanityBurstDescriptionPower>(),
        HoverTipFactory.FromKeyword(Monster2.monster2),
        HoverTipFactory.FromKeyword(Monster2des.monster2des),
        HoverTipFactory.FromPower<SeabornizationPower>()
	};

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/qiutukunjing.png";

    public SeaSnakeBigBite() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    static public bool HasMinion<T>(Player player) where T : MinionModel
{
    return player.PlayerCombatState?.Pets.Any(p =>
        p is { IsAlive: true, IsPet: true, Monster: T }
    ) == true;
}
    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
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
        await PowerCmd.Apply<SanityPower>(choiceContext, cardPlay.Target, ((DynamicVar)((CardModel)this).DynamicVars["SanityPower"]).BaseValue, ((CardModel)this).Owner.Creature, (CardModel)(object)this, false);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
		((DynamicVar)((CardModel)this).DynamicVars["SanityPower"]).UpgradeValueBy(1m);
        ((DynamicVar)((CardModel)this).DynamicVars["Harvest"]).UpgradeValueBy(1m);
    }
}