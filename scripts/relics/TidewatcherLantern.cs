using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Arknights_Mizuki.Scripts.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Arknights_Mizuki.Scripts.Relics;

[RegisterRelic(typeof(MzkRelicPool))]
public sealed class TidewatcherLantern : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override bool HasUponPickupEffect => true;

    public override string PackedIconPath => "res://Arknights_Mizuki/images/relics/tideng.png";

    protected override string PackedIconOutlinePath => "res://Arknights_Mizuki/images/relics/tideng.png";

    protected override string BigIconPath => "res://Arknights_Mizuki/images/relics/tideng.png";

    public override async Task AfterObtained()
    {
        List<CardModel> curses = PileType.Deck.GetPile(Owner).Cards
            .Where(card => card.Type == CardType.Curse)
            .ToList();

        if (curses.Count > 0)
        {
            await CardPileCmd.RemoveFromDeck(curses);
        }
    }
}
