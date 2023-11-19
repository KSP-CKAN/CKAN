using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CKAN.GUI
{
    public struct XClassHint
    {
        public IntPtr res_name;
        public IntPtr res_class;
    }

    public static class X11
    {
        /// <summary>
        /// Set the window identifying properties in X11
        /// </summary>
        /// <param name="name">The window name</param>
        /// <param name="wmClass">The window class</param>
        /// <param name="handle">Value of Form.Handle for the window</param>
        public static void SetWMClass(string name, string wmClass, IntPtr handle)
        {
            var hint = new XClassHint()
            {
                res_name  = Marshal.StringToCoTaskMemAnsi(name),
                res_class = Marshal.StringToCoTaskMemAnsi(wmClass)
            };
            IntPtr classHints = Marshal.AllocCoTaskMem(Marshal.SizeOf(hint));
            Marshal.StructureToPtr(hint, classHints, true);

            try
            {
                XSetClassHint(DisplayHandle, GetWindow(handle), classHints);
            }
            catch (DllNotFoundException)
            {
                // If the DLL isn't there, don't worry about it
            }

            Marshal.FreeCoTaskMem(hint.res_name);
            Marshal.FreeCoTaskMem(hint.res_class);
            Marshal.FreeCoTaskMem(classHints);
        }

        private static readonly Assembly MonoWinformsAssembly = Assembly.Load("System.Windows.Forms");

        private static readonly Type Hwnd = MonoWinformsAssembly
            .GetType("System.Windows.Forms.Hwnd");

        private static IntPtr DisplayHandle
            => (IntPtr)MonoWinformsAssembly
                .GetType("System.Windows.Forms.XplatUIX11")
                .GetField("DisplayHandle",
                          BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);

        private static IntPtr GetWindow(IntPtr handle)
        {
            return (IntPtr)Hwnd.GetField(
                "whole_window",
                BindingFlags.NonPublic | BindingFlags.Instance
            ).GetValue(GetHwnd(handle));
        }

        private static object GetHwnd(IntPtr handle)
        {
            return Hwnd.GetMethod(
                "ObjectFromHandle",
                BindingFlags.Public | BindingFlags.Static
            ).Invoke(null, new object[] { handle });
        }

        [DllImport("libX11", EntryPoint = "XSetClassHint", CharSet = CharSet.Ansi)]
        private static extern int XSetClassHint(IntPtr display, IntPtr window, IntPtr classHint);
    }
}
