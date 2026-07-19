#!/usr/bin/env python3
"""Export registered card metadata to a Markdown table."""

from __future__ import annotations

import argparse
import json
import re
from dataclasses import dataclass
from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[1]
CARDS_DIR = PROJECT_ROOT / "scripts" / "cards"
DEFAULT_LOCALE = PROJECT_ROOT / "Arknights_Mizuki" / "localization" / "zhs" / "cards.json"
DEFAULT_OUTPUT = PROJECT_ROOT / "card_catalog.md"
LOCALIZATION_PREFIX = "ARKNIGHTS_MIZUKI"
PRIMARY_CARD_POOL = "MzkCardPool"
TOKEN_CARD_POOL = "TokenCardPool"

TYPE_NAMES = {
    "Attack": "攻击",
    "Skill": "技能",
    "Power": "能力",
    "Status": "状态",
    "Curse": "诅咒",
}

RARITY_NAMES = {
    "Basic": "基础",
    "Common": "普通",
    "Uncommon": "罕见",
    "Rare": "稀有",
    "Ancient": "远古",
    "Status": "状态",
    "Curse": "诅咒",
    "0": "普通",
    "1": "基础",
    "2": "普通",
    "3": "罕见",
    "4": "稀有",
}


@dataclass
class DynamicValue:
    base: float
    upgrade_delta: float = 0
    upgrade_set: float | None = None

    def value(self, upgraded: bool) -> float:
        if upgraded and self.upgrade_set is not None:
            return self.upgrade_set
        if upgraded:
            return self.base + self.upgrade_delta
        return self.base


@dataclass
class CardInfo:
    source: Path
    pool: str
    class_name: str
    card_id: str
    title: str
    rarity: str
    cost: str
    upgraded_cost: str
    card_type: str
    description: str
    upgraded_description: str


def main() -> None:
    parser = argparse.ArgumentParser(description="Export registered cards to Markdown.")
    parser.add_argument("-o", "--output", type=Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--locale", type=Path, default=DEFAULT_LOCALE)
    token_group = parser.add_mutually_exclusive_group()
    token_group.add_argument(
        "--include-token-pool",
        dest="include_token_pool",
        action="store_true",
        help="Include cards registered to TokenCardPool (default).",
    )
    token_group.add_argument(
        "--exclude-token-pool",
        dest="include_token_pool",
        action="store_false",
        help="Exclude cards registered to TokenCardPool.",
    )
    parser.set_defaults(include_token_pool=True)
    args = parser.parse_args()

    localization = json.loads(args.locale.read_text(encoding="utf-8-sig"))
    cards = collect_cards(localization, args.include_token_pool)
    if not cards:
        raise RuntimeError(
            f"No registered cards found in {CARDS_DIR}. "
            "Check the card registration attribute parser."
        )
    write_markdown(cards, args.output)
    print(f"Wrote {len(cards)} cards to {args.output}")


def collect_cards(localization: dict[str, str], include_token_pool: bool) -> list[CardInfo]:
    cards: list[CardInfo] = []
    for path in sorted(CARDS_DIR.glob("*.cs")):
        text = path.read_text(encoding="utf-8-sig")
        pool_match = re.search(
            r"\[(?:RegisterCard|Pool)\s*\(\s*typeof\s*\(\s*([\w.]+)\s*\)\s*\)\s*\]",
            text,
        )
        if not pool_match:
            continue

        pool = pool_match.group(1).rsplit(".", maxsplit=1)[-1]
        if pool not in {PRIMARY_CARD_POOL, TOKEN_CARD_POOL}:
            continue
        if pool == TOKEN_CARD_POOL and not include_token_pool:
            continue

        show_match = re.search(r"shouldShowInCardLibrary\s*=\s*(true|false)", text)
        if show_match and show_match.group(1) == "false":
            continue

        class_name = require_match(r"\bclass\s+(\w+)\s*:", text, path)
        card_id = class_to_card_id(class_name)
        localization_base = f"{LOCALIZATION_PREFIX}_CARD_{card_id}"
        title = require_localization(localization, f"{localization_base}.title", path)
        template = require_localization(localization, f"{localization_base}.description", path)

        variables = parse_dynamic_values(text)
        apply_upgrades(text, variables)

        cost = parse_cost(text)
        upgraded_cost = parse_upgraded_cost(text, cost)
        raw_rarity = parse_named_const(text, "CardRarity", "rarity")
        rarity = "衍生" if pool == TOKEN_CARD_POOL else RARITY_NAMES.get(raw_rarity, raw_rarity)
        card_type = parse_named_const(text, "CardType", "type")

        description = render_description(template, variables, upgraded=False)
        upgraded_description = render_description(template, variables, upgraded=True)

        cards.append(
            CardInfo(
                source=path,
                pool=pool,
                class_name=class_name,
                card_id=card_id,
                title=title,
                rarity=rarity,
                cost=cost,
                upgraded_cost=upgraded_cost,
                card_type=TYPE_NAMES.get(card_type, card_type),
                description=description,
                upgraded_description=upgraded_description,
            )
        )

    return sorted(cards, key=lambda card: (rarity_sort_key(card.rarity), card.title))


def require_match(pattern: str, text: str, path: Path) -> str:
    match = re.search(pattern, text)
    if not match:
        raise ValueError(f"Could not parse {path}: {pattern}")
    return match.group(1)


def require_localization(localization: dict[str, str], key: str, path: Path) -> str:
    if key not in localization:
        raise ValueError(f"Missing localization for {path}: {key}")
    return localization[key]


def class_to_card_id(class_name: str) -> str:
    with_boundaries = re.sub(r"(?<=[a-z0-9])(?=[A-Z])", "_", class_name)
    with_boundaries = re.sub(r"(?<=[A-Z])(?=[A-Z][a-z])", "_", with_boundaries)
    return with_boundaries.upper()


def parse_named_const(text: str, enum_name: str, const_name: str) -> str:
    match = re.search(
        rf"const\s+{enum_name}\s+{const_name}\s*=\s*(?:{enum_name}\.)?(\w+)|"
        rf"const\s+{enum_name}\s+{const_name}\s*=\s*\({enum_name}\)(\d+)",
        text,
    )
    if not match:
        return "?"
    return next(group for group in match.groups() if group is not None)


def parse_cost(text: str) -> str:
    if re.search(r"HasEnergyCostX\s*=>\s*true", text):
        return "X"
    match = re.search(r"const\s+int\s+energyCost\s*=\s*(-?\d+)", text)
    if match:
        return normalize_cost(int(match.group(1)))
    base_match = re.search(r":\s*base\(([-]?\d+)\s*,", text)
    if base_match:
        return normalize_cost(int(base_match.group(1)))
    return "?"


def parse_upgraded_cost(text: str, base_cost: str) -> str:
    if base_cost == "X":
        return base_cost
    match = re.search(r"EnergyCost\.UpgradeBy\((-?\d+)m?\)", text)
    if not match:
        return base_cost
    try:
        return normalize_cost(int(base_cost) + int(match.group(1)))
    except ValueError:
        return base_cost


def normalize_cost(cost: int) -> str:
    if cost < 0:
        return "不可打出"
    return str(cost)


def parse_dynamic_values(text: str) -> dict[str, DynamicValue]:
    string_consts = dict(re.findall(r"const\s+string\s+(\w+)\s*=\s*\"([^\"]+)\"", text))
    numeric_consts = {
        name: parse_number(value)
        for name, value in re.findall(r"const\s+(?:int|decimal|float|double)\s+(\w+)\s*=\s*([-+]?\d+(?:\.\d+)?)m?", text)
    }
    variables: dict[str, DynamicValue] = {}

    typed_patterns = [
        (r"new\s+DamageVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "Damage"),
        (r"new\s+BlockVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "Block"),
        (r"new\s+CardsVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "Cards"),
        (r"new\s+RepeatVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "Repeat"),
        (r"new\s+EnergyVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "Energy"),
        (r"new\s+HealVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "Heal"),
        (r"new\s+HpLossVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "HpLoss"),
        (r"new\s+CalculationBaseVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "CalculationBase"),
        (r"new\s+CalculationExtraVar\(\s*([-+]?\d+(?:\.\d+)?)m?", "CalculationExtra"),
    ]
    for pattern, name in typed_patterns:
        for match in re.finditer(pattern, text):
            variables[name] = DynamicValue(parse_number(match.group(1)))

    for match in re.finditer(r"new\s+PowerVar<(\w+)>\(\s*([-+]?\d+(?:\.\d+)?)m?", text):
        variables[match.group(1)] = DynamicValue(parse_number(match.group(2)))

    for match in re.finditer(
        r"new\s+(?:DynamicVar|IntVar)\(\s*(?:\"([^\"]+)\"|(\w+))\s*,\s*([-+]?\d+(?:\.\d+)?|\w+)m?",
        text,
    ):
        literal_name, const_name, value = match.groups()
        name = literal_name or string_consts.get(const_name, const_name)
        variables[name] = DynamicValue(parse_number_or_const(value, numeric_consts))

    for match in re.finditer(r"new\s+CalculatedVar\(\s*\"([^\"]+)\"", text):
        variables.setdefault(match.group(1), DynamicValue(0))

    return variables


def apply_upgrades(text: str, variables: dict[str, DynamicValue]) -> None:
    named_access_patterns = [
        r"DynamicVars\[\s*\"([^\"]+)\"\s*\]\)?\s*\.UpgradeValueBy\(\s*([-+]?\d+(?:\.\d+)?)m?\s*\)",
        r"DynamicVars\[\s*(\w+)\s*\]\)?\s*\.UpgradeValueBy\(\s*([-+]?\d+(?:\.\d+)?)m?\s*\)",
        r"DynamicVars\.([A-Za-z]\w*)\)?\s*\.UpgradeValueBy\(\s*([-+]?\d+(?:\.\d+)?)m?\s*\)",
    ]
    string_consts = dict(re.findall(r"const\s+string\s+(\w+)\s*=\s*\"([^\"]+)\"", text))

    for pattern in named_access_patterns:
        for name, delta in re.findall(pattern, text):
            apply_delta(variables, string_consts.get(name, name), parse_number(delta))

    for match in re.finditer(
        r"DynamicVars\[\s*\"([^\"]+)\"\s*\]\.BaseValue\s*=\s*([-+]?\d+(?:\.\d+)?)m?",
        text,
    ):
        apply_set(variables, match.group(1), parse_number(match.group(2)))

    for match in re.finditer(
        r"DynamicVars\[\s*(\w+)\s*\]\.BaseValue\s*=\s*([-+]?\d+(?:\.\d+)?)m?",
        text,
    ):
        apply_set(variables, string_consts.get(match.group(1), match.group(1)), parse_number(match.group(2)))


def apply_delta(variables: dict[str, DynamicValue], name: str, delta: float) -> None:
    variables.setdefault(name, DynamicValue(0)).upgrade_delta += delta


def apply_set(variables: dict[str, DynamicValue], name: str, value: float) -> None:
    variables.setdefault(name, DynamicValue(0)).upgrade_set = value


def parse_number(raw: str) -> float:
    value = float(raw)
    return int(value) if value.is_integer() else value


def parse_number_or_const(raw: str, numeric_consts: dict[str, float]) -> float:
    if raw in numeric_consts:
        return numeric_consts[raw]
    return parse_number(raw)


def render_description(template: str, variables: dict[str, DynamicValue], upgraded: bool) -> str:
    text = template.replace("\r\n", "\n")
    text = render_if_upgraded(text, upgraded)
    text = re.sub(r"\[(?:/?)(?:blue|gold|red|green|white)\]", "", text)
    text = re.sub(r"\{([^}:]+):energyIcons\(\)\}", lambda match: render_energy(match, variables, upgraded), text)
    text = re.sub(r"\{([^}:]+):[^}]+\}", lambda match: render_var(match, variables, upgraded), text)
    text = re.sub(r"\{([^}:]+)\}", lambda match: render_var(match, variables, upgraded), text)
    text = re.sub(r"\s*\n\s*", "<br>", text.strip())
    return escape_markdown_cell(text)


def render_if_upgraded(text: str, upgraded: bool) -> str:
    pattern = re.compile(r"\{IfUpgraded:show:([^{}|]*)\|([^{}]*)\}")
    while True:
        next_text = pattern.sub(lambda match: match.group(1) if upgraded else match.group(2), text)
        if next_text == text:
            return text
        text = next_text


def render_energy(match: re.Match[str], variables: dict[str, DynamicValue], upgraded: bool) -> str:
    value = value_for(match.group(1), variables, upgraded)
    if value == "?":
        return "?"
    return f"{value}点能量"


def render_var(match: re.Match[str], variables: dict[str, DynamicValue], upgraded: bool) -> str:
    return value_for(match.group(1), variables, upgraded)


def value_for(name: str, variables: dict[str, DynamicValue], upgraded: bool) -> str:
    if name not in variables:
        return "{" + name + "}"
    value = variables[name].value(upgraded)
    if isinstance(value, float) and value.is_integer():
        value = int(value)
    return str(value)


def escape_markdown_cell(text: str) -> str:
    return text.replace("|", "\\|")


def rarity_sort_key(rarity: str) -> tuple[int, str]:
    order = {
        "基础": 0,
        "普通": 1,
        "罕见": 2,
        "稀有": 3,
        "远古": 4,
        "衍生": 5,
        "状态": 6,
        "诅咒": 7,
    }
    return (order.get(rarity, 99), rarity)


def write_markdown(cards: list[CardInfo], output: Path) -> None:
    rows = [
        "# 已注册卡牌整理",
        "",
        f"共 {len(cards)} 张。来源：`scripts/cards/*.cs` 中带 `[RegisterCard(...)]` 或旧版 `[Pool(...)]` 的卡牌，已排除 `.NotForUse` 文件。",
        "",
        "| 名称 | 稀有度 | 能耗 | 类型 | 效果 |",
        "|---|---|---:|---|---|",
    ]

    for card in cards:
        cost = card.cost
        if card.upgraded_cost != card.cost:
            cost = f"{card.cost}（升级：{card.upgraded_cost}）"

        effect = card.description
        if card.upgraded_description and card.upgraded_description != card.description:
            effect = f"{card.description}（升级：{card.upgraded_description}）"

        rows.append(f"| {card.title} | {card.rarity} | {cost} | {card.card_type} | {effect} |")

    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text("\n".join(rows) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
