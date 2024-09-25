using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

using CKAN.GUI.Attributes;

namespace Tests.GUI
{
    using MethodCall = List<MethodDefinition>;
    using CallsDict = Dictionary<MethodDefinition, List<MethodDefinition>>;

    [TestFixture]
    public class ThreadSafetyTests : MonoCecilAnalysisBase
    {
        [Test]
        public void GUIAssemblyModule_MethodsWithForbidGUICalls_DontCallGUI()
        {
            // Arrange
            var mainModule = ModuleDefinition.ReadModule(Assembly.GetAssembly(typeof(CKAN.GUI.Main))!
                                                                 .Location);
            var allMethods = mainModule.Types
                                       .SelectMany(GetAllNestedTypes)
                                       .SelectMany(t => t.Methods)
                                       .ToArray();

            // Act
            var calls = allMethods.Where(hasForbidGUIAttribute)
                                  .Select(meth => new MethodCall() { meth })
                                  .Concat(allMethods.SelectMany(FindStartedTasks))
                                  .SelectMany(GetAllCallsWithoutForbidGUI);

            // Assert
            Assert.Multiple(() =>
            {
                foreach (var callStack in calls)
                {
                    Assert.IsFalse(unsafeCall(callStack.Last()),
                                   $"Forbidden GUI call: {string.Join(" -> ", callStack.Select(SimpleName))}");
                }
            });
        }

        // The sequence to start a task seems to be:
        // 1. ldftn the function to start
        // 2. newobj a System.Action to hold it
        // 3. callvirt StartNew
        // ... so find the operand of the ldftn most immediately preceding the call
        private static MethodDefinition? FindStartNewArgument(Instruction instr)
            => instr.OpCode.Name == "ldftn" ? instr.Operand as MethodDefinition
                                            : FindStartNewArgument(instr.Previous);

        private static IEnumerable<MethodCall> GetAllCallsWithoutForbidGUI(MethodCall initialStack)
            => VisitMethodDefinition(initialStack,
                                     initialStack.Last(),
                                     new CallsDict(),
                                     hasForbidGUIAttribute,
                                     unsafeCall);

        private static bool hasForbidGUIAttribute(MethodDefinition md)
            => md.CustomAttributes.Any(attr => attr.AttributeType.Namespace == forbidAttrib.Namespace
                                            && attr.AttributeType.Name      == forbidAttrib.Name);

        private static bool unsafeCall(MethodDefinition md)
            // If it has [ForbidGUICalls], then treat as safe because it'll be checked on its own
            => !hasForbidGUIAttribute(md)
                // Adding an event handler is OK
                && !md.IsAddOn
                // Getting a property is OK
                && !md.IsGetter
                // Otherwise treat anything on a System.Windows.Forms object as unsafe
                && unsafeType(md.DeclaringType);

        // Any method on a type in WinForms or inheriting from anything in WinForms is presumed unsafe
        private static bool unsafeType(TypeDefinition t)
            => t.Namespace == winformsNamespace
                || (t.BaseType != null && unsafeType(t.BaseType.Resolve()));

        private static readonly Type   forbidAttrib      = typeof(ForbidGUICallsAttribute);
        private static readonly string winformsNamespace = typeof(Control).Namespace!;
    }
}
