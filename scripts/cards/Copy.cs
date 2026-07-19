using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

using Arknights_Mizuki.Scripts.Pools;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Copy : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private CardModel _currentCopyTarget;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
    {
        new RepeatVar(1)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (_currentCopyTarget != null)
                return (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
                {
                    HoverTipFactory.FromCard(_currentCopyTarget)
                };
            return Array.Empty<IHoverTip>();
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        (IEnumerable<CardKeyword>)(object)new CardKeyword[1] { CardKeyword.Exhaust };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/Copy.png";

    public Copy() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override bool IsPlayable => _currentCopyTarget != null;
    protected override bool ShouldGlowGoldInternal => IsPlayable;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await base.AfterCardPlayed(choiceContext, cardPlay);

        if (cardPlay.Card == this) return;

        var lastCard = cardPlay.Card;
        if (lastCard == null) return;
        if (lastCard.Owner != Owner) return;

        _currentCopyTarget = lastCard;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_currentCopyTarget == null)
            return;

        for (int i = 0; i < DynamicVars.Repeat.BaseValue; i++)
        {
            var clonedCard = CombatState.CloneCard(_currentCopyTarget);

            if (clonedCard == null) return;
            // 保留附魔
            if (_currentCopyTarget.Enchantment != null)
            {
                var enchantClone = (EnchantmentModel)_currentCopyTarget.Enchantment.ClonePreservingMutability();
                clonedCard.EnchantInternal(enchantClone, enchantClone.Amount);
            }
            CardCmd.ApplyKeyword(clonedCard, CardKeyword.Exhaust);
            if (_currentCopyTarget.IsUpgraded && clonedCard.IsUpgradable)
            {
                CardCmd.Upgrade(clonedCard);
            }

            foreach (var key in _currentCopyTarget.DynamicVars.Keys)
            {
                if (clonedCard.DynamicVars.ContainsKey(key))
                {
                    clonedCard.DynamicVars[key].BaseValue = _currentCopyTarget.DynamicVars[key].BaseValue;
                }
            }
            if (_currentCopyTarget.TargetType == TargetType.AnyEnemy)
                await CardCmd.AutoPlay(choiceContext, _currentCopyTarget,
                    CombatState.RunState.Rng.CombatTargets.NextItem(_currentCopyTarget.CombatState.HittableEnemies));
            else await CardCmd.AutoPlay(choiceContext, _currentCopyTarget, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
    }
}
