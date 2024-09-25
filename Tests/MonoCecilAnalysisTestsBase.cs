using System;
using System.Linq;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using CKAN.Extensions;

namespace Tests
{
    using MethodCall = List<MethodDefinition>;
    using CallsDict  = Dictionary<MethodDefinition, List<MethodDefinition>>;

    public abstract class MonoCecilAnalysisBase
    {
        protected static string FullyQualifiedName(MethodReference md)
            => $"{FullyQualifiedName(md.DeclaringType)}.{md.Name}";

        protected static string FullyQualifiedName(TypeReference td)
            => string.Join(".", td.TraverseNodes(td => td.DeclaringType)
                                  .Reverse()
                                  .Select(td => td.DeclaringType == null
                                                && td.Namespace != null
                                                    ? $"{td.Namespace}.{td.Name}"
                                                    : td.Name));

        protected static string SimpleName(MethodDefinition md)
            => $"{md.DeclaringType.Name}.{md.Name}";

        // https://gist.github.com/lnicola/b48db1a6ff3617bdac2a
        protected static IEnumerable<MethodCall> VisitMethodDefinition(MethodCall                   fullStack,
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

        private static IEnumerable<MethodDefinition> methodsCalledBy(MethodDefinition methDef)
            => GetCallsBy(methDef).Select(instr => instr.Operand as MethodDefinition
                                                   ?? GetSetterDef(instr.Operand as MethodReference))
                                  .OfType<MethodDefinition>();

        protected static IEnumerable<Instruction> GetCallsBy(MethodDefinition methDef)
            => methDef.Body
                      ?.Instructions
                       .Where(instr => callOpCodes.Contains(instr.OpCode.Name))
                      ?? Enumerable.Empty<Instruction>();

        // Property setters are virtual and have references instead of definitions
        private static MethodDefinition? GetSetterDef(MethodReference? mr)
            => (mr?.Name.StartsWith("set_") ?? false) ? mr.Resolve()
                                                      : null;

        protected static IEnumerable<TypeDefinition> GetAllNestedTypes(TypeDefinition td)
            => Enumerable.Repeat(td, 1)
                         .Concat(td.NestedTypes.SelectMany(GetAllNestedTypes));

        protected static IEnumerable<MethodCall> FindStartedTasks(MethodDefinition md)
            => StartNewCalls(md).Select(FindStartNewArgument)
                                .OfType<MethodDefinition>()
                                .Select(taskArg => new MethodCall() { md, taskArg });

        private static IEnumerable<Instruction> StartNewCalls(MethodDefinition md)
            => md.Body?.Instructions.Where(instr => callOpCodes.Contains(instr.OpCode.Name)
                                                    && instr.Operand is MethodReference mr
                                                    && isStartNew(mr))
                      ?? Enumerable.Empty<Instruction>();

        private static bool isStartNew(MethodReference mr)
            => (mr.DeclaringType.Namespace == "System.Threading.Tasks"
                && mr.DeclaringType.Name   == "TaskFactory"
                && mr.Name                 == "StartNew")
               || (mr.DeclaringType.Namespace == "System.Threading.Tasks"
                   && mr.DeclaringType.Name   == "Task"
                   && mr.Name                 == "Run");

        private static MethodDefinition? FindStartNewArgument(Instruction instr)
            => instr.OpCode.Name == "ldftn" ? instr.Operand as MethodDefinition
                                            : FindStartNewArgument(instr.Previous);

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
