using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// 损伤倍率：伤害倍率 * 100 作为层数，计算损伤伤害时用其层数来计算伤害倍率
/// 叠加：依次为 25%(0层) → 40%(15层) → 55%(30层) → 65%(45层) ...
/// </summary>
public sealed class SuperSpeedPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/SuperSpeedPower.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/SuperSpeedPower.png";
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
	{
        HoverTipFactory.FromKeyword(AutoPlay.Autoplay)
	};
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (cardPlay.Card.Owner != Owner.Player)
            return;
        
        var playedCard = cardPlay.Card;
        
        // 检查打出的卡牌是否带有 AutoPlay 关键词
        if (playedCard.Keywords.Contains(AutoPlay.Autoplay))
        {
            // 给玩家自己叠加1层 ShadowPower
            await CardPileCmd.Draw(choiceContext,Owner.Player);
            Flash();
        }
    }
}
