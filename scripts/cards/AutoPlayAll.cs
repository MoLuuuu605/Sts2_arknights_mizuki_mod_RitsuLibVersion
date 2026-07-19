using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.HoverTips;

namespace Arknights_Mizuki.Scripts.Cards;

// 娉ㄥ唽鍗＄墝鍒?MzkCardPool
[RegisterCard(typeof(MzkCardPool))]
public class AutoPlayAll : ModCardTemplate
{
    // 鍩虹鑰楄兘
    private const int energyCost = 1;
    // 鍗＄墝绫诲瀷锛堥槻寰＄墝鏄妧鑳界被鍨嬶級
    private const CardType type = CardType.Skill;
    // 鍗＄墝绋€鏈夊害
    private const CardRarity rarity = CardRarity.Uncommon;

    private const TargetType targetType = TargetType.Self;
    // 鐩爣绫诲瀷锛圫elf琛ㄧず鑷繁锛?    private const TargetType targetType = TargetType.Self;
    // 鏄惁鍦ㄥ崱鐗屽浘閴翠腑鏄剧ず
    private const bool shouldShowInCardLibrary = true;

    // 鍗＄墝鐨勫熀纭€灞炴€э紙鏍兼尅鍊硷級
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[0];
    public override IEnumerable<CardKeyword> CanonicalKeywords => (IEnumerable<CardKeyword>)(object)new CardKeyword[1] {CardKeyword.Exhaust};
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[3]
	{
		HoverTipFactory.FromCard<Learn>(this.IsUpgraded),
        HoverTipFactory.FromCard<Share>(this.IsUpgraded),
        HoverTipFactory.FromCard<Explain>(this.IsUpgraded)
	};
    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/AutoPlayAll.png";
    public AutoPlayAll() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 鎵撳嚭鏃剁殑鏁堟灉閫昏緫
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    if(this.IsUpgraded){
        var learn=CombatState.CreateCard<Learn>(Owner);
        var share=CombatState.CreateCard<Share>(Owner);
        var shock=CombatState.CreateCard<Shock>(Owner);
        CardCmd.Upgrade(learn);
        CardCmd.Upgrade(share);
        CardCmd.Upgrade(shock);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(learn, PileType.Discard,Owner,CardPilePosition.Bottom));
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(share, PileType.Discard,Owner,CardPilePosition.Bottom));
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(shock,PileType.Discard,Owner,CardPilePosition.Bottom));
    }
    else{
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Learn>(Owner), PileType.Discard,Owner,CardPilePosition.Bottom));
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Share>(Owner), PileType.Discard,Owner,CardPilePosition.Bottom));
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Shock>(Owner), PileType.Discard,Owner,CardPilePosition.Bottom));
        
        }
        var card=CombatState.CreateCard<AutoPlayAll>(Owner);
        if(IsUpgraded)CardCmd.Upgrade(card);
        await CardPileCmd.AddGeneratedCardToCombat(card,PileType.Hand,Owner);
    }

    protected override void OnUpgrade()
    {
    }
}


