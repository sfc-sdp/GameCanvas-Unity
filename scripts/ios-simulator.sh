#!/bin/bash
# Apple SiliconのiOSシミュレータ用。実機の署名・ビルドとは出力先を分ける。
set -euo pipefail
project_dir="$(cd "$(dirname "$0")/.." && pwd)"
export DEVELOPER_DIR="${DEVELOPER_DIR:-/Applications/Xcode.app/Contents/Developer}"
if [ "$#" -ne 1 ]; then echo 'Usage: scripts/ios-simulator.sh <simctl device UUID>' >&2; exit 2; fi
if [ "$(uname -m)" != arm64 ]; then echo 'この手順はApple Silicon用です。' >&2; exit 2; fi
"$project_dir/scripts/unity-validate.sh" ios-simulator
xcodebuild -project "$project_dir/Build/Validation/iOSSimulator/Unity-iPhone.xcodeproj" \
  -scheme Unity-iPhone -configuration Release -sdk iphonesimulator \
  -destination 'generic/platform=iOS Simulator' ARCHS=arm64 CODE_SIGNING_ALLOWED=NO \
  -derivedDataPath "$project_dir/Build/Validation/iOSSimulatorDerived" \
  > "$project_dir/Build/Validation/xcode-simulator.log" 2>&1
xcrun simctl bootstatus "$1" -b
xcrun simctl install "$1" "$project_dir/Build/Validation/iOSSimulatorDerived/Build/Products/Release-iphonesimulator/GameCanvasProbe.app"
xcrun simctl launch --terminate-running-process "$1" jp.ac.keio.sfc.gamecanvas.probe
