using STS2RitsuLib.Cards.DynamicVars;
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
using Arknights_Mizuki.Scripts.Powers;
using Arknights_Mizuki.Scripts.keywords;

namespace Arknights_Mizuki.Scripts.Cards;

[RegisterCard(typeof(MzkCardPool))]
public class Shock : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new DamageVar(3m, ValueProp.Move),
        (DynamicVar)new PowerVar<SanityPower>(1m),
        (DynamicVar)new DynamicVar("Times", 2m)
    };

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
    {
        HoverTipFactory.FromPower<SanityPower>(),
HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
    };

    public override string PortraitPath => $"res://Arknights_Mizuki/images/cards/shock.png";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [AutoPlay.Autoplay];

    public Shock() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for(int i=0;i<DynamicVars["Times"].BaseValue;i++){
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this,cardPlay)
                .TargetingAllOpponents(((CardModel)this).CombatState)
                .Execute(choiceContext);
            var opponents = ((CardModel)this).CombatState.GetOpponentsOf(Owner.Creature)

                .Where(opponent => opponent.IsAlive)

                .ToList();
            foreach (var opponent in opponents)
            {
                if(!opponent.IsAlive) continue;
                await PowerCmd.Apply<SanityPower>(
                    choiceContext,
                    opponent,
                    ((DynamicVar)((CardModel)this).DynamicVars["SanityPower"]).BaseValue,
                    ((CardModel)this).Owner.Creature,
                    (CardModel)(object)this,
                    false
                );
            }
        }
    }

    protected override void OnUpgrade()
    {
        this.DynamicVars.Damage.UpgradeValueBy(2);
    }
}