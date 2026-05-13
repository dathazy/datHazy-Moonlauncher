#Requires AutoHotkey v2.0

moonlightPath := "C:\Program Files\Moonlight Game Streaming\Moonlight.exe"

recommendedArgs := "--720 --fps 60 --bitrate 15000 --video-codec hevc --no-vsync --video-decoder hardware"

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

Run('"' moonlightPath '" stream ' recommendedArgs ' station4.dathazy.com "UMVC3"')