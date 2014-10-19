using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CKAN
{

    public class Util
    {

        public static void Invoke<T>(T obj, Action action) where T : System.Windows.Forms.Control
        {
            if (obj.InvokeRequired)
            {
                obj.Invoke(new MethodInvoker(action));
            }
            else
            {
             action();
            }
        }

    }

}
