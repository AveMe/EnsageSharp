;Lagger For Dota2 by DobroMulti http://youtube.com/user/DobroMulti
;Press F11 to toggle
;End key to close the application
;This NEVER will be detected by VAC

F11:: Hotkey, *~$Space, Toggle

End::
ExitApp

*~$Space::
Sleep 5
Loop
{
    GetKeyState, SpaceState, Space, P
    If SpaceState = U
        break 
    Sleep 1
    Send, {Blind}{Space}
}
Return