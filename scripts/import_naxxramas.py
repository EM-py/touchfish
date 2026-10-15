"""Import Naxxramas independently; never mutate the frozen Classic package."""
from pathlib import Path
import argparse, copy, hashlib, json
from import_classic import BUILD, URL, plain, spell_damage

ROOT = Path(__file__).resolve().parents[1]

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--offline', action='store_true')
    args = parser.parse_args()
    cached = ROOT / 'work' / f'hsjson-{BUILD}-zhCN.json'
    if not cached.exists():
        if args.offline:
            raise SystemExit('Pinned source snapshot missing')
        import urllib.request
        cached.parent.mkdir(exist_ok=True)
        urllib.request.urlretrieve(URL, cached)
    raw = cached.read_bytes()
    source = json.loads(raw)
    playable = [c for c in source if c.get('set') == 'NAXX' and c.get('collectible')]
    assert len(playable) == 30
    helpers = {'FP1_002t', 'FP1_007t', 'FP1_012t', 'FP1_014t', 'FP1_019t'}
    selected = playable + [c for c in source if c['id'] in helpers]
    source_path=ROOT/'data/sources/naxxramas-source.json'
    source_path.write_text(json.dumps(selected, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    records = []
    for c in selected:
        # Undead was not a minion type in 2014. Preserve original Beast/Demon tags.
        races = c.get('races', [c.get('race', '')])
        race = next((r for r in races if r in ('BEAST', 'DEMON')), '')
        records.append(dict(Id=c['id'], BaseId=c['id'], EffectId=c['id'], DbfId=c['dbfId'],
            LegacyDbfId=0, Name=c['name'], Text=plain(c.get('text')), RawText=c.get('text', ''),
            CardClass=c.get('cardClass', 'NEUTRAL'), Type=c['type'], Rarity=c.get('rarity', 'FREE'),
            SetId='naxxramas-2014', Cost=c.get('cost', 0), Attack=c.get('attack', 0),
            Health=c.get('health', 0), Durability=c.get('durability', c.get('health', 0)) if c['type']=='WEAPON' else 0,
            Collectible=c in playable, Mechanics=c.get('mechanics', []), Race=race,
            Entourage=c.get('entourage', []), Overload=c.get('overload', 0), SpellDamage=spell_damage(c)))
    package = dict(SchemaVersion=1, Id='naxxramas-2014', Name='纳克萨玛斯 2014',
        DisplayName='纳克萨玛斯的诅咒', Version=f'NAXX-2014-reconstructed-{BUILD}', Cards=sorted(records, key=lambda c:c['Id']))
    out = ROOT / 'data/cardsets/naxxramas-2014.json'
    out.write_text(json.dumps(package, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    provenance = dict(SourceUrl=URL, SourceBuild=BUILD, SnapshotSha256=hashlib.sha256(raw).hexdigest(),
        SourceExtractSha256=hashlib.sha256(source_path.read_bytes()).hexdigest(),
        PackageSha256=hashlib.sha256(out.read_bytes()).hexdigest(), DeckableCards=30, HelperCards=len(helpers),
        Reconstruction=['Removed later Undead race tags; retained original Beast/Demon types.',
            'Explicit EffectId retains unsupported-effect blocking until Naxxramas rules are implemented.'],
        Scope='Collectible multiplayer cards and five summon tokens only; no adventure bosses or puzzle cards.')
    (ROOT / 'data/sources/naxxramas-provenance.json').write_text(json.dumps(provenance, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    classic = json.loads((ROOT / 'data/cardsets/classic-2014.json').read_text(encoding='utf-8-sig'))
    bow = copy.deepcopy(next(c for c in classic['Cards'] if c['Id']=='VAN_EX1_536'))
    bow.update(Id='NAX_EX1_536', DbfId=bow['LegacyDbfId'], LegacyDbfId=0,
        SetId='naxxramas-balance-2014', EffectId='EX1_536', FriendlySecretsOnly=True,
        Text='每当一个友方奥秘被揭示时，便获得+1耐久度。', RawText='每当一个友方奥秘被揭示时，便获得+1耐久度。')
    balance = dict(SchemaVersion=1, Id='naxxramas-balance-2014', Name='经典牌·纳克萨玛斯修订',
        DisplayName='经典牌·纳克萨玛斯修订', HiddenInBrowser=True, Version='2014-08-before-1.2.0', Cards=[bow])
    (ROOT / 'data/cardsets/naxxramas-balance-2014.json').write_text(json.dumps(balance, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    pool_path=ROOT/'data/pools.json'
    pools=json.loads(pool_path.read_text(encoding='utf-8-sig'))
    base=next(p for p in pools['Pools'] if p['Id']=='classic-2014')
    base['Archived']=True
    extended=copy.deepcopy(base)
    extended.update(Id='naxxramas-2014', Archived=False, Name='纳克萨玛斯·2014年8月',
        CardSetIds=['classic-2014','naxxramas-2014','naxxramas-balance-2014'],
        CardReplacements={'VAN_EX1_536':'NAX_EX1_536'})
    pools['Pools']=[p for p in pools['Pools'] if p['Id']!='naxxramas-2014']+[extended]
    pool_path.write_text(json.dumps(pools, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    provenance.update(Cutoff='2014-08, all adventure wings open; before September balance changes',
        BalanceSources=['https://hearthstone.blizzard.com/en-gb/news/14907058'],
        BalanceChanges={'EX1_536':'Only friendly Secret reveals grant durability'},
        ExcludedLaterChanges=['2014-09 Leeroy 4 -> 5 mana','2014-09 Buzzard 2 mana 2/1 -> 5 mana 3/2'])
    (ROOT / 'data/sources/naxxramas-provenance.json').write_text(json.dumps(provenance, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(json.dumps(provenance, ensure_ascii=True))

if __name__ == '__main__':
    main()
