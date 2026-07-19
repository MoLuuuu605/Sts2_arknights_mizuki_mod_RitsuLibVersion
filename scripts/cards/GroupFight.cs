using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
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
using MinionLib.Minion;

namespace Arknights_Mizuki.Scripts.Cards;



[RegisterCard(typeof(MzkCardPool))]
public sealed class GroupFight : ModCardTemplate
{
    private const int energyCost = 8;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;  // 
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[]
    {
        (DynamicVar)new DamageVar(26,MegaCrit.Sts2.Core.ValueProps.ValueProp.Move),
        new EnergyVar(1)
    };
    static public bool HasMinion<T>(Player player) where T : MinionModel
{
    return player.PlayerCombatState?.Pets.Any(p =>
        p is { IsAlive: true, IsPet: true, Monster: T }
    ) == true;
}

    static public int GetEnergy(Player Owner)
    {
        int energy1=0 , energy2=0;
        if(HasMinion<HarvestMinion>(Owner))
        {
            Creature? pet = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is HarvestMinion);
            energy1 = pet.HasPower<SeabornizationPower>() ? pet.GetPowerAmount<SeabornizationPower>() : 0;
        }
        if(HasMinion<FloatingSeaMinion>(Owner))
        {
            Creature? pet = Owner.PlayerCombatState?.Pets.FirstOrDefault(p => p.Monster is FloatingSeaMinion);
            energy2 = pet.HasPower<SeabornizationPower>() ? pet.GetPowerAmount<SeabornizationPower>() : 0;
        }
        
        return energy1+energy2;
    }
    public override string PortraitPath => "res://Arknights_Mizuki/images/cards/GroupFight.png";
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[]
	{
        HoverTipFactory.FromPower<SeabornizationPower>()	
        };


    public GroupFight() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 召唤浮海漂移者
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    // public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    // {
    //     if(!(power is SeabornizationPower))return;
    //     this.EnergyCost.AddThisCombat(-(int)amount);
    // }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int DecreaseEnergy = GetEnergy(Owner);
        int TargetEnergy =energyCost-DecreaseEnergy;
        this.EnergyCost.SetThisCombat(TargetEnergy);
    }


    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(8m);
    }
}