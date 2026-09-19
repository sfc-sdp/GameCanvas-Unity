#!/bin/bash
# 検証APKだけを更新する。アプリのデータ・許可は維持する。
set -euo pipefail
project_dir="$(cd "$(dirname "$0")/.." && pwd)"
adb_bin="${ADB:-/Applications/Unity/Hub/Editor/6000.6.2f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb}"
if [ "$#" -ne 1 ]; then echo 'Usage: scripts/android-run.sh <adb device ID>' >&2; exit 2; fi
package=jp.ac.keio.sfc.gamecanvas.probe
apk="$project_dir/Build/Validation/GameCanvasProbe.apk"
[ -f "$apk" ] || { echo '先に scripts/unity-validate.sh android を実行してください。' >&2; exit 2; }
"$adb_bin" -s "$1" get-state
"$adb_bin" -s "$1" shell am force-stop "$package"
"$adb_bin" -s "$1" install -r "$apk"
# 更新直後にOSがActivityを復元する経路でも、Unityを同じプロセスで二重初期化しない。
"$adb_bin" -s "$1" shell am force-stop "$package"
"$adb_bin" -s "$1" shell am start -W -n "$package/com.unity3d.player.UnityPlayerActivity"
