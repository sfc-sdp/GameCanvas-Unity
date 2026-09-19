#!/bin/bash
# Unityは同じプロジェクトで同時起動しない。結果はBuild/Validationへ出す。
set -euo pipefail
project_dir="$(cd "$(dirname "$0")/.." && pwd)"
unity_bin="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.2f1/Unity.app/Contents/MacOS/Unity}"
export DEVELOPER_DIR="${DEVELOPER_DIR:-/Applications/Xcode.app/Contents/Developer}"
mkdir -p "$project_dir/Build/Validation"
mode="${1:-prepare}"
case "$mode" in
  prepare) target=Android; method=Prepare ;;
  android) target=Android; method=Android ;;
  ios) target=iOS; method=IOS ;;
  ios-simulator) target=iOS; method=IOSSimulator ;;
  web) target=WebGL; method=Web ;;
  editmode|playmode)
    if [ "$mode" = editmode ]; then suite=EditMode; else suite=PlayMode; fi
    result_file="$project_dir/Build/Validation/$mode.xml"
    if [ -e "$result_file" ]; then mv "$result_file" "$result_file.$(date +%s).previous"; fi
    "$unity_bin" -batchmode -projectPath "$project_dir" -runTests -testPlatform "$suite" -testResults "$result_file" -logFile "$project_dir/Build/Validation/$mode.log"
    python3 - "$result_file" <<'PY'
import sys, xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
if root.get('result') != 'Passed' or int(root.get('testcasecount', '0')) == 0:
    raise SystemExit('テストが失敗したか、実行件数が0です。XMLを確認してください。')
print('Tests:', root.attrib)
PY
    exit 0 ;;
  *) echo 'Usage: scripts/unity-validate.sh prepare|android|ios|ios-simulator|web|editmode|playmode' >&2; exit 2 ;;
esac
report_file=""
if [ "$mode" != prepare ]; then
  if [ "$mode" = ios-simulator ]; then report_target=iOSSimulator; else report_target="$target"; fi
  report_file="$project_dir/Build/Validation/build-$report_target.json"
  if [ -e "$report_file" ]; then mv "$report_file" "$report_file.$(date +%s).previous"; fi
fi
"$unity_bin" -batchmode -projectPath "$project_dir" -buildTarget "$target" -executeMethod "GameCanvas.Editor.GcValidationBuild.$method" -quit -logFile "$project_dir/Build/Validation/$mode.log"

if [ -n "$report_file" ]; then
  python3 - "$report_file" <<'PYRESULT'
import json, sys
with open(sys.argv[1]) as stream:
    result = json.load(stream)
if result.get('result') != 'Succeeded' or result.get('errors') != 0:
    raise SystemExit('ビルド結果を確認してください。')
print('Build:', result)
PYRESULT
fi
