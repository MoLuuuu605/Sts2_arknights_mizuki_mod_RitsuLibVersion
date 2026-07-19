using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Combat.HealthBars;
using Arknights_Mizuki.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Arknights_Mizuki.Scripts.Powers;


[RegisterPower]
/// <summary>
/// 损伤：达到8层时，目标受到最大生命值*25%的伤害（有上限），层数-8并增加损伤倍率
/// </summary>
public sealed class SanityPower : ModPowerTemplate,IHealthBarForecastSource
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override string CustomIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";
    public override string CustomBigIconPath => "res://Arknights_Mizuki/images/powers/Sanity.png";

    private const int BaseDamageAtActOne = 20;
    private const int DamagePerAdditionalAct = 10;
    private const int TriggerThreshold = 8;
    private const int MultiplierIncrement = 1;
    private const double DamageGrowthPerBurst = 1.15d;
    private const int DamagePercentPerUnlimit = 50;
    private const int MultiplayerThresholdPerPlayer = 2;
    private const int InitialFormThresholdReduction = 2;

    public void SetDamage(decimal damage)
	{
		AssertMutable();
		this.DynamicVars.Damage.BaseValue = damage;
	}
    public void SetBaseDamage(decimal damage)
	{
		AssertMutable();
		this.DynamicVars["BaseDamage"].BaseValue = damage;
	}

    public int GetBaseDamage(Creature owner)
    {
        int actNumber = owner.CombatState?.Players.FirstOrDefault()?.RunState.CurrentActIndex ?? 0;
        var damage=BaseDamageAtActOne + actNumber * DamagePerAdditionalAct;
        SetBaseDamage(damage);
        return damage;
    }
	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[3]
    {
		(DynamicVar)new DamageVar(0m, ValueProp.Unpowered|ValueProp.Unblockable),
        new DynamicVar("TriggerThreshold", TriggerThreshold),
        new DynamicVar("BaseDamage", BaseDamageAtActOne)

	};	

    private void SetTriggerThreshold(decimal triggerThreshold)
    {
        AssertMutable();
        DynamicVars["TriggerThreshold"].BaseValue = triggerThreshold;
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (!IsMutable || Owner == null)
            {
                return (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
                {
                    HoverTipFactory.FromPower<SanityBurstDescriptionPower>()
                };
            }

            PowerModel model = ModelDb.Power<SanityBurstDescriptionPower>();
            LocString description = model.SmartDescription;
            description.Add("BaseDamage", GetBaseDamage(Owner));
            description.Add("DamageGrowth", 15);

            return (IEnumerable<IHoverTip>)(object)new IHoverTip[1]
            {
                new HoverTip(model, description.GetFormattedText(), true)
            };
        }
    }

    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        var owner = Owner;
        if (owner == null || context.Creature != owner || !owner.IsAlive)
            yield break;

        int damage = GetForecastDamage();
        if (damage <= 0)
            yield break;

        yield return new HealthBarForecastSegment(
            damage,
            Colors.White,
            HealthBarForecastGrowthDirection.FromRight,
            20,
            null,
            new Color(0.4f, 0.8f, 1.0f, 0.95f)
        );
    }

    public int GetForecastDamage()
    {
        return (int)Math.Ceiling(DynamicVars.Damage.BaseValue);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature applier,
        CardModel cardSource)
    {
        if ((object)power != this)
            return;
        
        var owner = Owner;
        if (owner == null || !owner.IsAlive)
            return;

        int playerCount = GetPlayerCount(owner);
        int triggerThreshold = GetTriggerThreshold(owner, playerCount);
        SetTriggerThreshold(triggerThreshold);
        var multiplier = owner.HasPower<SanityMultiplierPower>()
            ? owner.GetPower<SanityMultiplierPower>().Amount
            : 0;


        // 计算爆发伤害上限
        // 基础上限25 + 已爆发次数*30 + 玩家SanityUnlimitPower层数*20
        var unlimitAmount = GetSharedUnlimitAmount(owner);
        var baseDamage = GetBaseDamage(owner);
        var burstMultiplier = (decimal)Math.Pow(DamageGrowthPerBurst, multiplier);
        var unlimitMultiplier = 1m + unlimitAmount * DamagePercentPerUnlimit / 100m;
        var damage = Math.Ceiling(baseDamage * burstMultiplier * unlimitMultiplier);

        SetDamage(damage);

        if (Amount < triggerThreshold)
            return;
        // 造成 HpLoss 伤害

        
        await PowerCmd.Apply<SanityBurstStrengthDebuffPower>(
            choiceContext,
            owner, 
            1,
            owner, 
            cardSource,
            false
        );
        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            owner, 
            1,
            owner, 
            cardSource,
            false
        );
        
        var unlimit = applier != null && applier.HasPower<SanityBurstPower>()
            ? applier.GetPower<SanityBurstPower>().Amount
            : 0;

        if (unlimit != 0)
        {
            await PowerCmd.Apply<SanityUnlimitPower>(
            choiceContext,
            applier,
            unlimit,
            applier,
            cardSource,
            false);
        }
        await PowerCmd.ModifyAmount(
            choiceContext,
            this,
            -triggerThreshold,
            null,
            null,
            false);

        if (applier != null){
            var players = Owner.CombatState.GetOpponentsOf(Owner);
            foreach (var player in players){
            await PainEchoPower.Trigger(choiceContext, player);
            await BlueSeedPower.Trigger(choiceContext, player);
            }
        }
        PacmanCollectorsEdition? pacmanCollectorsEdition = applier?.Player?.GetRelic<PacmanCollectorsEdition>();
        if (pacmanCollectorsEdition != null)
        {
            pacmanCollectorsEdition.Flash();
            await PowerCmd.Apply<ShrinkPower>(
                choiceContext,
                owner,
                pacmanCollectorsEdition.DynamicVars["ShrinkPower"].BaseValue,
                applier,
                cardSource,
                false);
        }

        // 增加损伤倍率
        await PowerCmd.Apply<SanityMultiplierPower>(
            choiceContext,
            owner,
            MultiplierIncrement,
            owner,
            cardSource,
            false);
            
        await CreatureCmd.Damage(
            choiceContext,
            owner,
            damage,
            ValueProp.Unpowered|ValueProp.Unblockable,
            owner);

        if(!Owner.IsAlive)return;
        Flash();
    }

    private static int GetPlayerCount(Creature owner)
    {
        return Math.Max(1, owner.CombatState?.Players.Count ?? 1);
    }

    private static int GetTriggerThreshold(Creature owner, int playerCount)
    {
        int initialFormCount = owner.CombatState?.Players.Count(player => player.Creature.HasPower<SanityProBurstPower>()) ?? 0;
        int threshold = TriggerThreshold
            + Math.Max(0, playerCount - 1) * MultiplayerThresholdPerPlayer
            - initialFormCount * InitialFormThresholdReduction;

        return Math.Max(1, threshold);
    }


    private static decimal GetSharedUnlimitAmount(Creature owner)
    {
        if (owner.CombatState == null)
            return 0;

        return owner.CombatState.Players
            .Where(player => player.Creature.HasPower<SanityUnlimitPower>())
            .Sum(player => player.Creature.GetPower<SanityUnlimitPower>().Amount);
    }
}
