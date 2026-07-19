using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

using Arknights_Mizuki.Scripts.Pools;

namespace Arknights_Mizuki.Scripts.Cards;

[Pool(typeof(MzkCardPool))]
public class MzkEnergyDrain : CustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        (DynamicVar)new HealVar(4m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
    {
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[1]
    {
        CardKeyword.Exhaust
    };


    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/eat.png";

    public MzkEnergyDrain() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel consumedCard;

        if (((CardModel)this).IsUpgraded)
        {
            // 升级后：指定手牌内消耗一张
            consumedCard = (await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1),
                context: choiceContext,
                player: ((CardModel)this).Owner,
                filter: null,
                source: (CardModel)this
            )).FirstOrDefault();

            if (consumedCard == null)
                return;

            await CardCmd.Exhaust(choiceContext, consumedCard);
        }
        else
        {
            // 随机消耗一张手牌
            CardPile pile = PileType.Hand.GetPile(((CardModel)this).Owner);
            consumedCard = ((CardModel)this).Owner.RunState.Rng.CombatCardSelection.NextItem(pile.Cards);
            if (consumedCard == null)
                return;

            await CardCmd.Exhaust(choiceContext, consumedCard);
        }

        // 回复等同于其法力值消耗的血量
        decimal healAmount = ((CardModel)this).DynamicVars.Heal.BaseValue;
        if (healAmount > 0)
        {
            await CreatureCmd.Heal(((CardModel)this).Owner.Creature, healAmount);
        }
    }

    protected override void OnUpgrade()
    {
        ((CardModel)this).DynamicVars.Heal.UpgradeValueBy(2m);
    }
}