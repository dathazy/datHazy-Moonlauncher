#Requires AutoHotkey v2.0

moonlightPath := "C:\Program Files\Moonlight Game Streaming\Moonlight.exe"

if !FileExist(moonlightPath)
{
    MsgBox("Moonlight.exe not found.`n`nPlease install Moonlight or edit the script path.")
    ExitApp
}

ProcessClose("Moonlight.exe")
Sleep(50)
ProcessClose("Moonlight.exe")

Sleep(500)

Run('"' moonlightPath '" pair station4.dathazy.com')

Sleep(5000)

Run('"' moonlightPath '" stream station4.dathazy.com "UMVC3"')