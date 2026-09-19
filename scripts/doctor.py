#!/usr/bin/env python3
"""学生とAIが共用する読み取り専用診断。端末ID・署名・認証情報は出力しない。"""
import argparse
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys

VERSION = "6000.6.2f1"
ROOT = Path(__file__).resolve().parents[1]


def run(args, env=None):
    try:
        p = subprocess.run(args, capture_output=True, text=True, timeout=30, env=env)
        return p.stdout.strip() if p.returncode == 0 else None
    except (OSError, subprocess.TimeoutExpired):
        return None


def diagnose():
    checks = []
    def check(name, ok, detail, action=""):
        checks.append(dict(name=name, status="ok" if ok else "action_required", detail=detail, action="" if ok else action))
    editor = Path(os.environ.get("UNITY_EDITOR", f"/Applications/Unity/Hub/Editor/{VERSION}/Unity.app/Contents/MacOS/Unity"))
    hub_editor = editor.parents[3] if sys.platform == "darwin" else editor.parent
    engines = hub_editor / "PlaybackEngines" if sys.platform == "darwin" else editor.parent / "Data/PlaybackEngines"
    check("unity", editor.is_file(), str(editor), f"Unity Hubで{VERSION}を導入し、必要ならUNITY_EDITORを指定してください。")
    project_version = (ROOT / "ProjectSettings/ProjectVersion.txt").read_text().splitlines()[0].split(": ")[1]
    check("project_version", project_version == VERSION, project_version, f"プロジェクトは{VERSION}に固定しています。")
    free_gb = round(shutil.disk_usage(ROOT).free / 1024**3, 1)
    check("disk", free_gb >= 30, f"{free_gb} GiB free", "ビルド用に30GiB以上の空きを用意してください（診断上の目安）。")
    for module in ["AndroidPlayer", "iOSSupport", "WebGLSupport"]:
        check(module, (engines / module).is_dir(), str(engines / module), "Unity Hubのモジュール追加から導入してください。")
    android = engines / "AndroidPlayer"
    for name, path in [("android_sdk36", android / "SDK/platforms/android-36/android.jar"), ("android_ndk", android / "NDK/source.properties"), ("android_jdk", android / ("OpenJDK/bin/java.exe" if os.name == "nt" else "OpenJDK/bin/java"))]:
        check(name, path.is_file(), str(path), "Unity HubでAndroid SDK & NDK ToolsとOpenJDKを追加してください。")
    adb = android / ("SDK/platform-tools/adb.exe" if os.name == "nt" else "SDK/platform-tools/adb")
    adb_cmd = str(adb) if adb.exists() else shutil.which("adb")
    devices = []
    if adb_cmd:
        output = run([adb_cmd, "devices"]) or ""
        for line in output.splitlines()[1:]:
            fields = line.split()
            if len(fields) < 2: continue
            serial, state = fields[:2]
            device = {"state": state}
            if state == "device":
                for field, prop in [("model", "ro.product.model"), ("os", "ro.build.version.release"), ("api", "ro.build.version.sdk"), ("build", "ro.build.display.id")]:
                    device[field] = run([adb_cmd, "-s", serial, "shell", "getprop", prop])
            devices.append(device)
    check("android_devices", any(d["state"] == "device" for d in devices), devices, "端末を接続し、開発者向けオプション・USBデバッグを有効にして接続を許可してください。")
    if sys.platform == "darwin":
        developer_dir = os.environ.get("DEVELOPER_DIR", "/Applications/Xcode.app/Contents/Developer")
        env = dict(os.environ, DEVELOPER_DIR=developer_dir)
        xcode = run(["xcodebuild", "-version"], env)
        check("xcode", xcode is not None, xcode, "Xcodeを導入し、一度開いて初期設定を完了してください。")
        check("xcode_selection", run(["xcode-select", "-p"]) == developer_dir, run(["xcode-select", "-p"]), f"この作業ではDEVELOPER_DIR={developer_dir}を使います。")
    return dict(schema_version=1, unity_required=VERSION, host=dict(os=platform.platform(), architecture=platform.machine()), checks=checks)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--json", action="store_true")
    args = parser.parse_args()
    report = diagnose()
    if args.json:
        print(json.dumps(report, ensure_ascii=False, indent=2))
    else:
        for check in report["checks"]:
            print(f'[{check["status"]}] {check["name"]}: {check["detail"]}')
            if check["action"]: print(f'  次の操作: {check["action"]}')
    return 0 if all(c["status"] == "ok" for c in report["checks"]) else 2

if __name__ == "__main__":
    sys.exit(main())
