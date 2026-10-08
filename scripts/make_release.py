"""Build a runtime ZIP and GitHub update manifest; never include user data."""
from pathlib import Path
import argparse, hashlib, json, re, zipfile

ROOT = Path(__file__).resolve().parents[1]
def digest(data): return hashlib.sha256(data).hexdigest()
def allowed(name):
    return name in {'Touchfish.exe', 'Touchfish.Update.exe', 'README.md', 'CHANGELOG.md', 'VERSION', 'data/README.md'} or name.startswith('data/') and name.endswith('.json')
def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--version', default='v' + (ROOT / 'VERSION').read_text().strip())
    parser.add_argument('--exe', default=str(ROOT / 'Touchfish.exe'))
    parser.add_argument('--legacy', type=Path)
    parser.add_argument('--out', type=Path, default=ROOT / 'work' / 'release-assets')
    args = parser.parse_args()
    assert re.fullmatch(r'v\d+\.\d+(?:\.\d+)?', args.version), 'Invalid version'
    payload = {}
    if args.legacy:
        with zipfile.ZipFile(args.legacy) as archive:
            for entry in archive.infolist():
                name = entry.filename.replace('\\', '/')
                if not entry.is_dir() and allowed(name): payload[name] = archive.read(entry)
    else:
        for name in ['README.md','CHANGELOG.md','VERSION','Touchfish.Update.exe']:
            payload[name] = (ROOT / name).read_bytes()
        payload['Touchfish.exe'] = Path(args.exe).read_bytes()
        for path in (ROOT / 'data').rglob('*'):
            if path.is_file() and allowed(path.relative_to(ROOT).as_posix()):
                payload[path.relative_to(ROOT).as_posix()] = path.read_bytes()
    assert 'Touchfish.exe' in payload and 'data/pools.json' in payload
    payload['VERSION'] = (args.version[1:] + '\n').encode()
    args.out.mkdir(parents=True, exist_ok=True)
    name = f'Touchfish-{args.version}-win.zip'
    target = args.out / name
    with zipfile.ZipFile(target,'w',zipfile.ZIP_DEFLATED) as archive:
        for path, data in sorted(payload.items()): archive.writestr(path,data)
    manifest = {'Schema':1,'Version':args.version,'ZipName':name,'ZipSha256':digest(target.read_bytes()),
                'HealthCheck':not bool(args.legacy),'Files':[{'Path':path,'Sha256':digest(data)} for path,data in sorted(payload.items())]}
    (args.out / 'Touchfish-update.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding='utf-8')
    (args.out / (name + '.sha256')).write_text(manifest['ZipSha256']+'  '+name+'\n', encoding='ascii')
    print(json.dumps({'Version':args.version,'Package':str(target),'Files':len(payload),'Sha256':manifest['ZipSha256']}))
if __name__ == '__main__': main()
