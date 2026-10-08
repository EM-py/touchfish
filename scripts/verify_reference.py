"""Check C# output against HearthSim's independently downloaded reference codec."""
from pathlib import Path
import json, sys
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "work" / "reference"))
from hearthstone.deckstrings import parse_deckstring, write_deckstring
cases = json.loads((ROOT / "previews" / "deckcode-reference-cases.json").read_text(encoding="utf-8-sig"))
assert len(cases) == 9
for case in cases:
    cards, heroes, fmt, sideboards = parse_deckstring(case["Code"])
    expected = sorted((int(card), count) for card, count in case["Cards"].items())
    assert cards == expected
    assert heroes == [case["Hero"]] and int(fmt) == case["Format"] == 3
    assert not sideboards
    assert write_deckstring(cards, heroes, fmt, sideboards) == case["Code"]
print("PASS: nine classes match HearthSim decoding and canonical encoding byte-for-byte.")
(ROOT / "previews" / "reference-verification.txt").write_text(
    "PASS: All 9 class codes independently decoded and re-encoded byte-for-byte using HearthSim python-hearthstone.\n"
    "No in-game client interoperability test was performed.\n", encoding="utf-8")
