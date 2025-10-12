#if NETFRAMEWORK || WINDOWS

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Mono.Cecil;
using NUnit.Framework;

namespace Tests.GUI
{
    [TestFixture]
    public class DeprecatedControlTests
    {
        [Test]
        public void GUIAssemblyModule_BadControls_NotUsed()
        {
            // Arrange
            var mainModule = ModuleDefinition.ReadModule(Assembly.GetAssembly(typeof(CKAN.GUI.Main))!
                                                                 .Location);

            // Act
            var badMembers = mainModule.Types
                                       .Where(NotAbsolved)
                                       .SelectMany(t => t.Fields
                                                         .Where(BadMember)
                                                         .Select(f => (t, f)))
                                       .ToArray();

            // Assert
            Assert.Multiple(() =>
            {
                foreach (var (type, field) in badMembers)
                {
                    Assert.Fail($"Deprecated control {field.FieldType.FullName} in {type.Name}.{field.Name}, use {DeprecatedControls[field.FieldType.FullName]} instead.");
                }
            });
        }

        private static bool NotAbsolved(TypeDefinition type)
            => !AbsolvedSinners.Contains(type.FullName);

        // These classes are allowed to reference deprecated controls as a way of
        // using our inheriting controls through standard interfaces
        private static readonly HashSet<string> AbsolvedSinners = new HashSet<string>
        {
            "CKAN.GUI.TabController",
        };

        private static bool BadMember(FieldDefinition field)
            => DeprecatedControls.Keys.Contains(field.FieldType.FullName);

        // We have made replacements for these controls
        private static readonly Dictionary<string, string> DeprecatedControls = new Dictionary<string, string>
        {
            { "System.Windows.Forms.SplitContainer", "CKAN.GUI.UsableSplitContainer" },
            { "System.Windows.Forms.TabControl",     "CKAN.GUI.ThemedTabControl"     },
            { "System.Windows.Forms.ListView",       "CKAN.GUI.ThemedListView"       },
            { "System.Windows.Forms.ProgressBar",    "CKAN.GUI.LabeledProgressBar"   },
        };
    }
}

#endif
