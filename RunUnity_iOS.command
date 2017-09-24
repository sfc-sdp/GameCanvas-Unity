#!/bin/sh
MY_DIRNAME=$(dirname $0)
cd $MY_DIRNAME

/Applications/Unity/Unity.app/Contents/MacOS/Unity -projectPath $MY_DIRNAME -buildTarget ios
