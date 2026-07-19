using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.keywords;
using Arknights_Mizuki.Scripts.Minions;
using Arknights_Mizuki.Scripts.Pools;
using Arknights_Mizuki.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MinionLib.Commands;
using MinionLib.Minion;

namespace Arknights_Mizuki.Scripts.Cards;



[RegisterCard(typeof(MzkCardPool))]
public sealed class Float : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;  // 
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new CardsVar(3),
        new DynamicVar("Float",3)      // 抽 2 张牌，升级后抽 3 张
    };
    static public bool HasMinion<T>(Player player) where T : MinionModel
{
    return player.PlayerCombatState?.Pets.Any(p =>
        p is { IsAlive: true, IsPet: true, Monster: T }
    ) == true;
}
    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/Float.png";
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[3]
	{
        HoverTipFactory.FromKeyword(Monster1.monster1),
        HoverTipFactory.FromKeyword(Monster1des.monster1des),
        HoverTipFactory.FromPower<SeabornizationPower>()
	};
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Retain  // 
    };

    public Float() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 召唤浮海漂移者
        if(!HasMinion<FloatingSeaMinion>(Owner))
        {
            _ = await MinionCmd.AddMinion<FloatingSeaMinion>(choiceContext,Owner, new MinionSummonOptions(
                MaxHp: 6m,                              // 血量
                PrimaryStatAmount: 2m,                  // 主要参数（具体内容在随从的 OnSummon 里定义），还有次要参数等可以按需传入
                Source: this,                           // 召唤来源（通常是这张牌）
                Position: MinionPosition.Front));
        }
        else 
        {
            Creature? pet = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is FloatingSeaMinion);
            await PowerCmd.Apply<SeabornizationPower>(choiceContext,pet,DynamicVars["Float"].BaseValue,pet,null);
        }
        await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.BaseValue,Owner);
    }

    protected override void OnUpgrade()
    {
        // 升级：抽牌 2 → 3
        base.DynamicVars["Float"].UpgradeValueBy(2m);
    }
}