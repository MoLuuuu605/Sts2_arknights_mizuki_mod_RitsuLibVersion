using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.HoverTips;

namespace Arknights_Mizuki.Scripts.Cards;

// 娉ㄥ唽鍗＄墝鍒?MzkCardPool
[RegisterCard(typeof(MzkCardPool))]
public class SpeedUp : ModCardTemplate
{
    // 鍩虹鑰楄兘
    private const int energyCost = 2;
    // 鍗＄墝绫诲瀷锛堥槻寰＄墝鏄妧鑳界被鍨嬶級
    private const CardType type = CardType.Skill;
    // 鍗＄墝绋€鏈夊害
    private const CardRarity rarity = CardRarity.Rare;

    private const TargetType targetType = TargetType.Self;
    // 鐩爣绫诲瀷锛圫elf琛ㄧず鑷繁锛?    private const TargetType targetType = TargetType.Self;
    // 鏄惁鍦ㄥ崱鐗屽浘閴翠腑鏄剧ず
    private const bool shouldShowInCardLibrary = true;

    // 鍗＄墝鐨勫熀纭€灞炴€э紙鏍兼尅鍊硷級
    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[0];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
	{
		HoverTipFactory.FromCard<Learn>()
	};

        public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[]
    {
        CardKeyword.Exhaust
    };
    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/SpeedUp.png";
    public SpeedUp() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 鎵撳嚭鏃剁殑鏁堟灉閫昏緫
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card= Owner.RunState.CreateCard<Learn>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Learn>(Owner), PileType.Draw,Owner,CardPilePosition.Random));
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Learn>(Owner), PileType.Discard,Owner,CardPilePosition.Random));
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        await CardPileCmd.Draw(choiceContext, 1, ((CardModel)this).Owner, false);
    }

    protected override void OnUpgrade()
    {
        this.EnergyCost.UpgradeBy(-1);
    }
}


