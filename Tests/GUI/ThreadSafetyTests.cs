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
    public class ThreadSafetyTests
    {
        [Test]
        public void GUIAssemblyModule_MethodsWithForbidGUICalls_DontCallGUI()
        {
            // Arrange / Act
            var mainModule = ModuleDefinition.ReadModule(Assembly.GetAssembly(typeof(CKAN.GUI.Main))
                                                                 .Location);
            var allMethods = mainModule.Types
                                       .SelectMany(t => GetAllNestedTypes(t))
                                       .SelectMany(t => t.Methods)
                                       .ToArray();
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

        private IEnumerable<TypeDefinition> GetAllNestedTypes(TypeDefinition td)
            => Enumerable.Repeat(td, 1)
                         .Concat(td.NestedTypes.SelectMany(nested => GetAllNestedTypes(nested)));

        private IEnumerable<MethodCall> FindStartedTasks(MethodDefinition md)
            => StartNewCalls(md).Select(FindStartNewArgument)
                                .Select(taskArg => new MethodCall() { md, taskArg });

        private IEnumerable<Instruction> StartNewCalls(MethodDefinition md)
            => md.Body?.Instructions.Where(instr => callOpCodes.Contains(instr.OpCode.Name)
                                                    && instr.Operand is MethodReference mr
                                                    && isStartNew(mr))
                      ?? Enumerable.Empty<Instruction>();

        private bool isStartNew(MethodReference mr)
            => (mr.DeclaringType.Namespace == "System.Threading.Tasks"
                && mr.DeclaringType.Name   == "TaskFactory"
                && mr.Name                 == "StartNew")
               || (mr.DeclaringType.Namespace == "System.Threading.Tasks"
                   && mr.DeclaringType.Name   == "Task"
                   && mr.Name                 == "Run");

        // The sequence to start a task seems to be:
        // 1. ldftn the function to start
        // 2. newobj a System.Action to hold it
        // 3. callvirt StartNew
        // ... so find the operand of the ldftn most immediately preceding the call
        private MethodDefinition FindStartNewArgument(Instruction instr)
            => instr.OpCode.Name == "ldftn" ? instr.Operand as MethodDefinition
                                            : FindStartNewArgument(instr.Previous);

        private IEnumerable<MethodCall> GetAllCallsWithoutForbidGUI(MethodCall initialStack)
            => VisitMethodDefinition(initialStack, initialStack.Last(), new CallsDict(), hasForbidGUIAttribute, unsafeCall);

        private string SimpleName(MethodDefinition md) => $"{md.DeclaringType.Name}.{md.Name}";

        // https://gist.github.com/lnicola/b48db1a6ff3617bdac2a
        private IEnumerable<MethodCall> VisitMethodDefinition(MethodCall                   fullStack,
                                                              MethodDefinition             methDef,
                                                              CallsDict                    calls,
                                                              Func<MethodDefinition, bool> skip,
                                                              Func<MethodDefinition, bool> stopAfter)
        {
            var called = calls[methDef] = methodsCalledBy(methDef).Distinct().ToList();
            foreach (var calledMeth in called)
            {
                if (!calls.ContainsKey(calledMeth) && !skip(calledMeth))
                {
                    var newStack = fullStack.Append(calledMeth).ToList();
                    yield return newStack;
                    if (!stopAfter(calledMeth))
                    {
                        // yield from, please!
                        foreach (var subcall in VisitMethodDefinition(newStack, calledMeth, calls, skip, stopAfter))
                        {
                            yield return subcall;
                        }
                    }
                }
            }
        }

        private IEnumerable<MethodDefinition> methodsCalledBy(MethodDefinition methDef)
            => methDef.Body
                      .Instructions
                      .Where(instr => callOpCodes.Contains(instr.OpCode.Name))
                      .Select(instr => instr.Operand as MethodDefinition
                                       ?? GetSetterDef(instr.Operand as MethodReference))
                      .Where(calledMeth => calledMeth?.Body != null);

        // Property setters are virtual and have references instead of definitions
        private MethodDefinition GetSetterDef(MethodReference mr)
            => (mr?.Name.StartsWith("set_") ?? false) ? mr.Resolve()
                                                      : null;

        private bool hasForbidGUIAttribute(MethodDefinition md)
            => md.CustomAttributes.Any(attr => attr.AttributeType.Namespace == forbidAttrib.Namespace
                                            && attr.AttributeType.Name      == forbidAttrib.Name);

        private bool unsafeCall(MethodDefinition md)
            // If it has [ForbidGUICalls], then treat as safe because it'll be checked on its own
            => !hasForbidGUIAttribute(md)
                // Adding an event handler is OK
                && !md.IsAddOn
                // Getting a property is OK
                && !md.IsGetter
                // Otherwise treat anything on a System.Windows.Forms object as unsafe
                && unsafeType(md.DeclaringType);

        // Any method on a type in WinForms or inheriting from anything in WinForms is presumed unsafe
        private bool unsafeType(TypeDefinition t)
            => t.Namespace == winformsNamespace
                || (t.BaseType != null && unsafeType(t.BaseType.Resolve()));

        private static readonly Type   forbidAttrib      = typeof(ForbidGUICallsAttribute);
        private static readonly string winformsNamespace = typeof(Control).Namespace;

        private static readonly HashSet<string> callOpCodes = new HashSet<string>
        {
            // Constructors
            "newobj",

            // Normal function calls
            "call",

            // Virtual function calls (includes property setters)
            "callvirt",
        };
    }
}
