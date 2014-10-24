using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKAN
{

    public class Util
    {

        public static bool IsLinux
        {
            get
            {
                // Magic numbers ahoy! This arcane incantation was found
                // in a Unity help-page, which was found on a scroll,
                // which was found in an urn that dated back to Mono 2.0.
                // It documents singular numbers of great power.
                //
                // "And lo! 'pon the 4, 6, and 128 the penguin shall
                // come, and it infiltrate dominate from the smallest phone to
                // the largest cloud."
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }


        // utility helper to deal with multi-threading and UI
        public static void Invoke<T>(T obj, Action action) where T : System.Windows.Forms.Control
        {
            if (obj.InvokeRequired) // if we're not in the UI thread
            {
                // enqueue call on UI thread and wait for it to return
                obj.Invoke(new MethodInvoker(action));
            }
            else
            {
                // we're on the UI thread, execute directly
                action();
            }
        }

        // utility helper to deal with multi-threading and UI
        // async version, doesn't wait for UI thread
        // use with caution, when not sure use blocking Invoke()
        public static void AsyncInvoke<T>(T obj, Action action) where T : System.Windows.Forms.Control
        {
            if (obj.InvokeRequired) // if we're not in the UI thread
            {
                // enqueue call on UI thread and continue
                obj.BeginInvoke(new MethodInvoker(action));
            }
            else
            {
                // we're on the UI thread, execute directly
                action();
            }
        }

    }

}
