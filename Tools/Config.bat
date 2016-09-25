@echo off
:: -------------------------------------------------
:: GameCanvas for Unity [Batches Config]
:: 
:: Copyright (c) 2015-2016 Seibe TAKAHASHI
:: This software is released under the MIT License.
:: http://opensource.org/licenses/mit-license.php
:: -------------------------------------------------

:: Unity.exe
set UNITY_EXE="C:\Program Files\Unity\Editor\Unity.exe"

:: Unityプロジェクトのルートディレクトリ
set PROJECT_DIR=%~dp0..\

:: Andoird SDKのルートディレクトリ
set ANDROID_SDK_DIR=%AppData%\..\Local\Android\android-sdk\

:: adb.exe
set ADB_EXE="%ANDROID_SDK_DIR%platform-tools\adb.exe"

mkdir %PROJECT_DIR%Build > NUL 2>&1

exit /b 0
