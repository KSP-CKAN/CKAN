using System;

namespace CKAN.GUI
{
    // https://docs.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
    [Flags]
    public enum WindowExStyles : uint
    {
        WS_EX_TOPMOST       =        0x8,
        WS_EX_TRANSPARENT   =       0x20,
        WS_EX_TOOLWINDOW    =       0x80,
        WS_EX_CONTROLPARENT =    0x10000,
        WS_EX_NOACTIVATE    = 0x08000000,
    }
}
