#!/usr/bin/env python3
"""Compile the complete tutorial examples and UPM samples with the project's Unity version."""
from pathlib import Path
import re
import json
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parent.parent
PACKAGE = ROOT / 'Packages/jp.ac.keio.sfc.sdp'
TEMP = ROOT / 'Assets/__GcExampleValidation'


def main():
    if TEMP.exists() or TEMP.with_suffix('.meta').exists():
        raise SystemExit(f'Refusing to overwrite existing files: {TEMP}')
    examples = []
    for path in sorted((PACKAGE / 'Documentation~').glob('*.md')):
        for index, code in enumerate(re.findall(r'^```(?:csharp|cs)\s*\n(.*?)^```', path.read_text(), re.M | re.S)):
            if re.search(r'\bclass\s+\w+\s*:\s*(?:GameCanvas\.)?GameBase\b', code):
                examples.append((f'{path.relative_to(ROOT)} block {index + 1}', code))
    for path in sorted((PACKAGE / 'Samples~').rglob('*.cs')):
        examples.append((str(path.relative_to(ROOT)), path.read_text()))
    if not examples:
        raise SystemExit('No complete examples found')
    TEMP.mkdir()
    try:
        for index, (source, code) in enumerate(examples):
            # Namespace isolation permits independent lessons to each name their class Game.
            (TEMP / f'Example{index}.cs').write_text(
                f'// Source: {source}\nnamespace GcExampleValidation{index}\n{{\n{code}\n}}\n')
        tests = TEMP / 'Tests'
        tests.mkdir()
        (tests / 'GameCanvas.ExampleValidation.asmdef').write_text(json.dumps({
            'name': 'GameCanvas.ExampleValidation',
            'references': ['Game', 'GameCanvas', 'UnityEngine.TestRunner', 'UnityEditor.TestRunner'],
            'overrideReferences': True, 'precompiledReferences': ['nunit.framework.dll'],
            'autoReferenced': False, 'defineConstraints': ['UNITY_INCLUDE_TESTS']
        }))
        (tests / 'ExampleSmokeTest.cs').write_text(r"""
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
public class ExampleSmokeTest
{
    [UnityTest] public IEnumerator CompleteExamplesInitializeDrawAndDispose()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "Game");
        var examples = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(GameCanvas.GameBase).IsAssignableFrom(t)
            && t.Namespace != null && t.Namespace.StartsWith("GcExampleValidation", StringComparison.Ordinal)).ToArray();
        Assert.That(examples.Length, Is.GreaterThan(0));
        foreach (var type in examples)
        {
            var go = new GameObject(type.FullName, typeof(Camera), typeof(AudioListener));
            try
            {
                go.AddComponent(type);
                yield return null;
                yield return null;
                LogAssert.NoUnexpectedReceived();
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
""")
        print(f'Compiling {len(examples)} independent examples', flush=True)
        subprocess.run(['bash', str(ROOT / 'scripts/unity-validate.sh'), 'editmode'], cwd=ROOT, check=True)
        subprocess.run(['bash', str(ROOT / 'scripts/unity-validate.sh'), 'playmode'], cwd=ROOT, check=True)
        print('Example compilation, startup smoke tests and Unity test suites passed', flush=True)
    finally:
        shutil.rmtree(TEMP)
        TEMP.with_suffix('.meta').unlink(missing_ok=True)


if __name__ == '__main__':
    main()
