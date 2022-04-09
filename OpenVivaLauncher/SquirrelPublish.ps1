#https://github.com/clowd/Clowd.Squirrel/releases/download/2.8.40/SquirrelTools-2.8.40.zip
Invoke-Webrequest -URI "https://github.com/clowd/Clowd.Squirrel/releases/download/2.8.40/SquirrelTools-2.8.40.zip" -OutFile "SquirrelTools.zip"
Expand-Archive -Path "SquirrelTools.zip"
.\SquirrelTools\Squirrel.exe pack --packId "OpenVivaLauncher" --packVersion "1.0.0" --packDirectory ".\OpenVivaLauncher\bin\Release\net6.0-windows" --allowUnaware
