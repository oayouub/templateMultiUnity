@echo off
echo 🧹 Nettoyage et préparation d'une nouvelle build...

REM Sauvegarder les fichiers Steam importants
if exist "Build\steam_appid.txt" copy "Build\steam_appid.txt" "steam_appid_backup.txt" >nul
if exist "Build\steam_api64.dll" copy "Build\steam_api64.dll" "steam_api64_backup.dll" >nul
if exist "Build\steam_api.dll" copy "Build\steam_api.dll" "steam_api_backup.dll" >nul

REM Nettoyer le dossier Build
if exist "Build" rmdir /s /q "Build"
mkdir "Build"

REM Restaurer les fichiers Steam
if exist "steam_appid_backup.txt" (
    copy "steam_appid_backup.txt" "Build\steam_appid.txt" >nul
    del "steam_appid_backup.txt" >nul
    echo ✅ steam_appid.txt restauré
)

if exist "steam_api64_backup.dll" (
    copy "steam_api64_backup.dll" "Build\steam_api64.dll" >nul
    del "steam_api64_backup.dll" >nul
    echo ✅ steam_api64.dll restauré
)

if exist "steam_api_backup.dll" (
    copy "steam_api_backup.dll" "Build\steam_api.dll" >nul
    del "steam_api_backup.dll" >nul
    echo ✅ steam_api.dll restauré
)

echo.
echo 🎯 Dossier Build nettoyé et préparé !
echo 📝 Maintenant dans Unity : File > Build Settings > Build
echo 📁 Sélectionnez le dossier Build\ 
echo 💾 Nommez : SteamLobbyTutorial.exe
echo.
pause 