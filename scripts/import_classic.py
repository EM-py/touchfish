"""Import frozen Classic-format records; never use current LEGACY balance values."""
from pathlib import Path
import argparse, collections, datetime, hashlib, html, json, re, urllib.request

ROOT = Path(__file__).resolve().parents[1]
BUILD = "253932"
URL = f"https://api.hearthstonejson.com/v1/{BUILD}/zhCN/cards.json"

def plain(value):
    value = re.sub(r"<br\s*/?>", "\n", value or "", flags=re.I)
    return html.unescape(re.sub(r"<[^>]+>", "", value)).replace("[x]", "").replace("$", "").replace("#", "").strip()

def spell_damage(card):
    # Frozen VANILLA spellDamage can be a presence marker (Malygos: 1).
    # The printed numeric bonus is the authoritative magnitude.
    if "SPELLPOWER" in card.get("mechanics", []):
        bonus = re.search(r"法术伤害\s*[+＋]\s*(\d+)", plain(card.get("text")))
        if bonus:
            return int(bonus.group(1))
    return card.get("spellDamage", 0)

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--offline", action="store_true")
    args = parser.parse_args()
    cached = ROOT / "work" / f"hsjson-{BUILD}-zhCN.json"
    cached.parent.mkdir(exist_ok=True)
    if not cached.exists():
        if args.offline:
            raise SystemExit("Pinned source snapshot missing")
        urllib.request.urlretrieve(URL, cached)
    payload = cached.read_bytes()
    all_cards = json.loads(payload)
    by_id = {c["id"]: c for c in all_cards}
    source = [c for c in all_cards if c.get("set") == "VANILLA"]
    heroes = [by_id[f"HERO_{n:02d}"] for n in range(1, 10)]
    source_path = ROOT / "data" / "sources" / "hearthstonejson-classic-zhCN.json"
    source_path.write_text(json.dumps(source + heroes, ensure_ascii=False, indent=2), encoding="utf-8")
    normalized = []
    for c in source + heroes:
        base_id = c["id"].removeprefix("VAN_")
        normalized.append({
            "Id": c["id"], "BaseId": base_id, "DbfId": c["dbfId"],
            "LegacyDbfId": by_id.get(base_id, {}).get("dbfId", 0),
            "Name": c["name"], "Text": plain(c.get("text")), "RawText": c.get("text", ""),
            "CardClass": c.get("cardClass", "NEUTRAL"), "Type": c["type"],
            "Rarity": c.get("rarity", "FREE"), "SetId": "classic-2014", "Cost": c.get("cost", 0),
            "Attack": c.get("attack", 0), "Health": c.get("health", 0),
            "Durability": (c.get("durability") or c.get("health", 0)) if c["type"] == "WEAPON" else 0,
            "Collectible": bool(c.get("collectible")) and c["type"] in ("MINION", "SPELL", "WEAPON"),
            "Mechanics": c.get("mechanics", []), "Race": c.get("race", ""),
            "Entourage": c.get("entourage", []), "Overload": c.get("overload", 0),
            "SpellDamage": spell_damage(c),
        })
    package = {"SchemaVersion": 1, "Id": "classic-2014", "Name": "经典 2014", "Version": f"VANILLA-{BUILD}",
               "Cards": sorted(normalized, key=lambda c: c["Id"])}
    out = ROOT / "data" / "cardsets" / "classic-2014.json"
    out.write_text(json.dumps(package, ensure_ascii=False, indent=2), encoding="utf-8")
    collectible = [c for c in normalized if c["Collectible"]]
    classes = dict(collections.Counter(c["CardClass"] for c in collectible))
    assert len(collectible) == 382, len(collectible)
    assert classes["NEUTRAL"] == 157 and all(v == 25 for k, v in classes.items() if k != "NEUTRAL")
    assert len({c["Id"] for c in normalized}) == len(normalized)
    assert len({c["DbfId"] for c in collectible}) == len(collectible)
    n = {c["Id"]: c for c in normalized}
    assert n["VAN_EX1_116"]["Cost"] == 4
    assert n["VAN_EX1_308"]["Cost"] == 0
    assert n["VAN_CS2_237"]["Cost"] == 2 and n["VAN_CS2_237"]["Health"] == 1
    assert n["VAN_NEW1_019"]["Attack"] == 3
    assert n["VAN_CS2_106"]["Cost"] == 2
    assert n["VAN_CS2_106"]["Durability"] == 2 and n["VAN_EX1_567"]["Durability"] == 8
    assert all(c["Durability"] > 0 for c in collectible if c["Type"] == "WEAPON")
    assert n["VAN_EX1_563"]["SpellDamage"] == 5
    pool = {"SchemaVersion": 1, "Pools": [{"Id": "classic-2014", "Name": "经典 2014", "CardSetIds": ["classic-2014"],
        "DeckSize": 30, "CopyLimit": 2, "LegendaryLimit": 1, "DeckstringFormat": 3,
        "HeroDbfIds": {h["cardClass"]: h["dbfId"] for h in heroes}}]}
    (ROOT / "data" / "pools.json").write_text(json.dumps(pool, ensure_ascii=False, indent=2), encoding="utf-8")
    report = {"SourceUrl": URL, "ImportedUtc": datetime.datetime.now(datetime.timezone.utc).isoformat(),
        "SnapshotCachedUtc": datetime.datetime.fromtimestamp(cached.stat().st_mtime, datetime.timezone.utc).isoformat(),
        "SourceBuild": BUILD, "RulesBaseline": "2014-06 / 1.0.0.5832 via frozen VANILLA records",
        "FullSnapshotSha256": hashlib.sha256(payload).hexdigest(),
        "SourceExtractSha256": hashlib.sha256(source_path.read_bytes()).hexdigest(),
        "PackageSha256": hashlib.sha256(out.read_bytes()).hexdigest(),
        "TotalRecords": len(normalized), "DeckableCards": len(collectible), "ClassCounts": classes,
        "Note": "Noncollectible VANILLA helper records are retained for provenance, not treated as legal deck cards."
    }
    (ROOT / "data" / "sources" / "provenance.json").write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps(report, ensure_ascii=False))

if __name__ == "__main__":
    main()
