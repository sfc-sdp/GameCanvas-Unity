#!/usr/bin/env python3
"""Create a reproducible project ZIP from committed distribution files."""
import argparse
import hashlib
import io
import json
from pathlib import Path
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def git(*args):
    return subprocess.check_output(['git', '-C', str(ROOT), *args])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=Path, default=ROOT / 'Build/Distribution')
    args = parser.parse_args()
    if git('status', '--porcelain', '--untracked-files=normal').strip():
        parser.error('Commit distribution changes first; ignored validation records may remain.')
    paths = git('ls-files', '-z').decode().split('\0')
    if any(p.startswith(('design/', 'scripts/rendering/')) for p in paths):
        parser.error('Working notes or rendering fixtures are tracked. Refusing to distribute them.')
    revision = git('rev-parse', 'HEAD').decode().strip()
    package = json.loads(git('show', 'HEAD:Packages/jp.ac.keio.sfc.sdp/package.json'))
    unity = git('show', 'HEAD:ProjectSettings/ProjectVersion.txt').decode().splitlines()[0].split(': ')[1]
    manifest = dict(schemaVersion=1, framework=package['version'], unity=unity, revision=revision,
                    entryPoint='Assets/Game.cs', assets='Assets/Res', tests='scripts/unity-validate.sh')
    archive = io.BytesIO(git('archive', '--format=zip', '--prefix=GameCanvas/', 'HEAD'))
    with zipfile.ZipFile(archive, 'a', compression=zipfile.ZIP_DEFLATED) as output:
        info = zipfile.ZipInfo('GameCanvas/release-info.json', output.infolist()[0].date_time)
        info.compress_type = zipfile.ZIP_DEFLATED
        info.external_attr = 0o100644 << 16
        output.writestr(info, json.dumps(manifest, ensure_ascii=False, indent=2) + '\n')
    args.output.mkdir(parents=True, exist_ok=True)
    name = f"GameCanvas-{package['version']}-{revision[:8]}.zip"
    destination = args.output / name
    destination.write_bytes(archive.getvalue())
    digest = hashlib.sha256(archive.getvalue()).hexdigest()
    destination.with_suffix('.zip.sha256').write_text(f'{digest}  {name}\n')
    print(json.dumps(dict(path=str(destination), sha256=digest, **manifest), indent=2))


if __name__ == '__main__':
    main()
