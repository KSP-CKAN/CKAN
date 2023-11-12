using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests
{
    [TestFixture] public class Util
    {
        [Test]
        public void AssembliesHaveNoAsyncVoids()
        {
            UtilStatic.AssertNoAsyncVoidMethods(GetType().Assembly);
        }

        [Test]
        public void CompareFiles_EqualFiles_ReturnsTrue()
        {
            string f1 = Path.GetTempFileName();
            string f2 = Path.GetTempFileName();

            File.WriteAllText(f1, "TestLorem IpsumThats SomeCrap");
            File.WriteAllText(f2, "TestLorem IpsumThats SomeCrap");

            Assert.IsTrue(UtilStatic.CompareFiles(f1, f2));

            File.Delete(f2);
            File.Delete(f1);
        }

        [Test]
        public void CompareFiles_DifferentFiles_ReturnsFalse()
        {
            string f1 = Path.GetTempFileName();
            string f2 = Path.GetTempFileName();
            File.WriteAllText(f1, "Heyo");
            File.WriteAllText(f2, "-");

            Assert.IsFalse(UtilStatic.CompareFiles(f1, f2));

            File.Delete(f2);
            File.Delete(f1);
        }
    }

    public static class UtilStatic
    {
        private static bool HasAttribute<TAttribute>(this MethodInfo method) where TAttribute : Attribute
        {
            return method.GetCustomAttributes(typeof (TAttribute), false).Any();
        }

        public static void AssertNoAsyncVoidMethods(Assembly assembly)
        {
            var messages = assembly
                .GetAsyncVoidMethods()
                .Select(method =>
                    string.Format("'{0}.{1}' is an async void method.",
                        method.DeclaringType.Name,
                        method.Name))
                .ToList();
            Assert.False(messages.Any(),
                "Async void methods found!" + Environment.NewLine + string.Join(Environment.NewLine, messages));
        }

        private static IEnumerable<MethodInfo> GetAsyncVoidMethods(this Assembly assembly)
        {
            return assembly.GetLoadableTypes()
                .SelectMany(type => type.GetMethods(
                    BindingFlags.NonPublic
                    | BindingFlags.Public
                    | BindingFlags.Instance
                    | BindingFlags.Static
                    | BindingFlags.DeclaredOnly))
                .Where(method => method.HasAttribute<AsyncStateMachineAttribute>())
                .Where(method => method.ReturnType == typeof (void));
        }

        private static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        public static async Task Throws<T>(Func<Task> async) where T : Exception
        {
            try
            {
                await async();
                Assert.Fail("Expected exception of type: {0}", typeof (T));
            }
            catch (T)
            {
                return;
            }
        }

        /// <summary>
        /// Compares two files regarding size and content.
        /// File names are neglected.
        /// For the content it compares byte by byte.
        /// Found here: https://stackoverflow.com/questions/7931304/comparing-two-files-in-c-sharp
        /// </summary>
        /// <returns><c>true</c>, if files are content-wise equal, <c>false</c> otherwise.</returns>
        public static bool CompareFiles(string file1, string file2)
        {
            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                return true;
            }

            if (!File.Exists(file1) || !File.Exists(file2))
            {
                return false;
            }

            using (FileStream fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read),
                              fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read))
            {
                //// Check the file sizes. If they are not the same, the files
                //// are not the same.
                if (fs1.Length != fs2.Length)
                {
                    return false;
                }

                // Read and compare a byte from each file until either a
                // non-matching set of bytes is found or until the end of
                // file1 is reached.
                int file1byte;
                int file2byte;
                do
                {
                    // Read one byte from each file.
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));

                // Make sure that file2 has not more bytes than file1.
                // If not, the files are the same, after all these checks.
                return (file1byte - file2byte) == 0;
            }
        }
    }
}
