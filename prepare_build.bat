@echo off
echo Préparation de la build Steam...

REM Créer le dossier Build si nécessaire
if not exist "Build" mkdir Build

REM Copier steam_appid.txt vers le dossier Build
copy "steam_appid.txt" "Build\" >nul
echo ✓ steam_appid.txt copié

REM Copier les DLL Steam nécessaires
if exist "Assets\com.rlabrecque.steamworks.net\Plugins\steam_api64.dll" (
    copy "Assets\com.rlabrecque.steamworks.net\Plugins\steam_api64.dll" "Build\" >nul
    echo ✓ steam_api64.dll copié
)

if exist "Assets\com.rlabrecque.steamworks.net\Plugins\steam_api.dll" (
    copy "Assets\com.rlabrecque.steamworks.net\Plugins\steam_api.dll" "Build\" >nul
    echo ✓ steam_api.dll copié
)

echo.
echo 📦 Préparation terminée ! 
echo.
echo ⚠️  IMPORTANT : Après avoir fait la build Unity :
echo    1. Copiez ce dossier Build vers votre ami
echo    2. Vous devez TOUS LES DEUX avoir Steam ouvert
echo    3. Lancez le jeu depuis Steam ou ajoutez-le comme jeu non-Steam
echo.
echo 🎮 Pour tester :
echo    - Un joueur clique "Host Lobby"
echo    - L'autre rejoint via l'invitation Steam ou Steam Overlay
echo.
pause 