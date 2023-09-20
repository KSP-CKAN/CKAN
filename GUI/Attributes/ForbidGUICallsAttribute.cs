using System;

namespace CKAN.GUI.Attributes
{
    /// <summary>
    /// Tag background functions with this, and a test will check
    /// that they're not calling GUI code without Util.Invoke.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property
                    | AttributeTargets.Method    | AttributeTargets.Delegate,
                    Inherited = true, AllowMultiple = false)]
    public class ForbidGUICallsAttribute : Attribute { }
}
