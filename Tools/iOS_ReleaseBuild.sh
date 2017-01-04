#!/bin/sh
# -------------------------------------------------
# GameCanvas for Unity [iOS: Release Build]
# 
# Copyright (c) 2015-2017 Seibe TAKAHASHI
# This software is released under the MIT License.
# http://opensource.org/licenses/mit-license.php
# -------------------------------------------------

PROJECT_DIR="$(cd $(dirname $0) && pwd)/../"
source Config.sh
cd ${PROJECT_DIR}

echo "iOSアプリをビルドします"

mkdir -p "$PROJECT_DIR/Build" 2>/dev/null
${UNITY_APP} -batchmode -quit -logFile ${PROJECT_DIR}Build/Build.log -projectPath ${PROJECT_DIR} -buildTarget ios -executeMethod GameCanvas.Editor.Builder.BuildXcodeProj

echo "ビルドが終了しました"
