using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

using CKAN.Extensions;

namespace Tests
{
    [TestFixture]
    public class StringFormatSafetyTests : MonoCecilAnalysisBase
    {
        [TestCase(new object[]
                  {
                      typeof(CKAN.IUser),
                      typeof(CKAN.CmdLine.ConsoleUser),
                      typeof(CKAN.ConsoleUI.Toolkit.ConsoleScreen),
                      #if NETFRAMEWORK || WINDOWS
                      typeof(CKAN.GUI.GUIUser),
                      #endif
                      typeof(CKAN.NetKAN.ConsoleUser),
                  })]
        public void AssemblyModule_StringSyntaxCompositeFormat_SameOrLiteralsOnly(object[] types)
        {
            // Arrange
            var methodsByName = types.OfType<Type>()
                                     .Select(t => Assembly.GetAssembly(t))
                                     .OfType<Assembly>()
                                     .Select(a => ModuleDefinition.ReadModule(a.Location))
                                     .SelectMany(m => m.Types
                                                       .SelectMany(GetAllNestedTypes)
                                                       .SelectMany(t => t.Methods)
                                                       .SelectMany(GetMethodAndTasks))
                                     .GroupBy(FullyQualifiedName)
                                     .Where(grp => grp.Count() == 1)
                                     .ToDictionary(grp => grp.Key,
                                                   grp => grp.Single());
            var specialNames = methodsByName.Values.Where(AnyParamHasStringSyntaxAttribute)
                                                   .Select(FullyQualifiedName)
                                                   .ToHashSet();

            // Act / Assert
            Assert.Multiple(() =>
            {
                foreach ((string name, MethodDefinition meth) in methodsByName)
                {
                    foreach (var call in GetCallsBy(meth))
                    {
                        if (call.Operand is MethodReference calledRef
                            && FullyQualifiedName(calledRef) is string calledRefName
                            && specialNames.Contains(calledRefName))
                        {
                            Assert.IsFalse(IsEmptyArray(call.Previous)
                                           && !IsStringLiteral(call.Previous.Previous)
                                           && !IsI18nResource(call.Previous.Previous),
                                           $"Unsafe format string in {name} --> {InstrDescription(call, meth)}({InstrDescription(call.Previous.Previous, meth)})");
                        }
                    }
                }
            });
        }

        private static string? InstrDescription(Instruction instr, MethodDefinition parent)
            => instr switch
               {
                   {
                       Operand: MethodReference mRef,
                   } => FullyQualifiedName(mRef),
                   {
                       OpCode:  {Name: "ldstr"},
                       Operand: string s,
                   } => $"literal \"{s}\"",
                   {
                       OpCode:  {Name: "ldfld"},
                       Operand: FieldReference fRef,
                   } => $"field \"{fRef.Name}\"",
                   {
                       OpCode: {Name: string a},
                   } => ldargPattern.TryMatch(a, out Match? argM)
                        && int.TryParse(argM.Groups["argNum"].Value, out int argNum)
                        && argNum > 0 && argNum <= parent.Parameters.Count
                            ? $"argument \"{parent.Parameters[argNum - 1].Name}\""
                            : ldlocPattern.TryMatch(a, out Match? locM)
                                ? $"local {locM.Groups["locNum"].Value}"
                                : a,
                   _ => null,
               };

        private static readonly Regex ldargPattern =
            new Regex(@"ldarg\.(?<argNum>\d+$)",
                      RegexOptions.Compiled);

        private static readonly Regex ldlocPattern =
            new Regex(@"ldloc\.(?<locNum>.+$)",
                      RegexOptions.Compiled);

        private static bool IsEmptyArray(Instruction instr)
            => instr.OpCode.Name == "call"
                && instr.Operand is MethodReference mRef
                && mRef.Name == "Empty";

        private static bool IsI18nResource(Instruction instr)
            => instr.Operand is MethodReference mRef
                && FullyQualifiedName(mRef).Contains(".Properties.Resources.");

        private static bool IsStringLiteral(Instruction instr)
            => instr.OpCode.Name == "ldstr" && instr.Operand is string;

        private static IEnumerable<MethodDefinition> GetMethodAndTasks(MethodDefinition meth)
            => Enumerable.Repeat(meth, 1)
                         .Concat(FindStartedTasks(meth).Select(stack => stack.Last()));

        private static readonly Type strSynAttrib = typeof(StringSyntaxAttribute);

        private static bool AnyParamHasStringSyntaxAttribute(MethodDefinition md)
            => AnyParamHasAttribute(md, strSynAttrib);

        private static bool AnyParamHasAttribute(MethodDefinition md, Type attrib)
            => md.Parameters.SelectMany(p => p.CustomAttributes)
                            .Any(attr => attr.AttributeType.Namespace == attrib.Namespace
                                      && attr.AttributeType.Name      == attrib.Name);
    }
}
