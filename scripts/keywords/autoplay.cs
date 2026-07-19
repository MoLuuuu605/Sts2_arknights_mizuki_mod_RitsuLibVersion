using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Arknights_Mizuki.Scripts.keywords;

[RegisterOwnedCardKeyword("AutoPlay", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class AutoPlay
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "AutoPlay");
    public static CardKeyword Autoplay => KeywordId.GetModKeywordCardKeyword();
}

[RegisterOwnedCardKeyword("Echo1", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Echo1
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Echo1");
    public static CardKeyword Echo => KeywordId.GetModKeywordCardKeyword();

}

[RegisterOwnedCardKeyword("Echo2", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Echo2
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Echo2");
    public static CardKeyword Echo => KeywordId.GetModKeywordCardKeyword();

}

[RegisterOwnedCardKeyword("Echo3", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Echo3
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Echo3");
    public static CardKeyword Echo => KeywordId.GetModKeywordCardKeyword();

}

[RegisterOwnedCardKeyword("Monster1des", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Monster1des
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Monster1des");
    public static CardKeyword monster1des => KeywordId.GetModKeywordCardKeyword();
}

[RegisterOwnedCardKeyword("Monster1", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Monster1
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Monster1");
    public static CardKeyword monster1 => KeywordId.GetModKeywordCardKeyword();
}

[RegisterOwnedCardKeyword("Monster2", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Monster2
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Monster2");
    public static CardKeyword monster2 => KeywordId.GetModKeywordCardKeyword();
}

[RegisterOwnedCardKeyword("Monster2des", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Monster2des
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Monster2des");
    public static CardKeyword monster2des => KeywordId.GetModKeywordCardKeyword();
}

[RegisterOwnedCardKeyword("Tandi", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class Tandi
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "Tandi");
    public static CardKeyword tandi => KeywordId.GetModKeywordCardKeyword();
}

[RegisterOwnedCardKeyword("ALLME", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public class ALLME
{
    public static readonly string KeywordId = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, "ALLME");
    public static CardKeyword Allme => KeywordId.GetModKeywordCardKeyword();
}