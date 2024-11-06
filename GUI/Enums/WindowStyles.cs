using System;

namespace CKAN.GUI
{
    // https://docs.microsoft.com/en-us/windows/win32/winmsg/window-styles
    [Flags]
    public enum WindowStyles : uint
    {
        WS_VISIBLE = 0x10000000,
        WS_CHILD   = 0x40000000,
        WS_POPUP   = 0x80000000,
    }
}
