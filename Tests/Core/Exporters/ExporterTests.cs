using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using CKAN;
using CKAN.Types;
using CKAN.Exporters;
using Tests.Data;

namespace Tests.Core.Exporters
{
    [TestFixture]
    public sealed class ExporterTests
    {
        [OneTimeSetUp]
        public void MakeInstance()
        {
            var repo = new Repository("test", "https://github.com/");
            inst     = new DisposableKSP(TestData.TestRegistry());
            repoData = new TemporaryRepositoryData(new NullUser(), new Dictionary<Repository, RepositoryData>
                       {
                           {
                               repo,
                               RepositoryData.FromJson(TestData.TestRepository(), null)!
                           },
                       });
            regMgr   = RegistryManager.Instance(inst.KSP, repoData.Manager,
                                                new Repository[] { repo });
            // The saved test data doesn't have sorted_repositories, use our fake repo
            regMgr.registry.RepositoriesAdd(repo);
        }

        [OneTimeTearDown]
        public void DisposeAll()
        {
            regMgr?.Dispose();
            repoData?.Dispose();
            inst?.Dispose();
        }

        [TestCaseSource(nameof(GetExportTypes))]
        public void Export_WithType_Works(ExportFileType exportType)
        {
            // Arrange
            var sut = new Exporter(exportType);
            using (var stream = new MemoryStream())
            {
                // Act
                sut.Export(regMgr!, regMgr!.registry, stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var exported = reader.ReadToEnd();

                    // Assert
                    foreach (var m in regMgr.registry.InstalledModules
                                                     .Select(im => im.Module))
                    {
                        Assert.IsTrue(exported.Contains(exportType switch
                                      {
                                          ExportFileType.Ckan      => repoData!.Manager
                                                                               .GetAvailableModules(regMgr.registry.Repositories.Values,
                                                                                                    m.identifier)
                                                                               .Any()
                                                                          ? $@"""name"": ""{m.identifier}"""
                                                                          : "",
                                          ExportFileType.PlainText => $"{m.name} ({m})",
                                          ExportFileType.Markdown  => $"- **{m.name}** `{m}`",
                                          ExportFileType.BbCode    => $"[*][B]{m.name}[/B] ({m})",
                                          ExportFileType.Csv       => $"{m.identifier},{m.version}",
                                          ExportFileType.Tsv       => $"{m.identifier}\t{m.version}\t{m.name}\t{m.@abstract}",
                                          _ => throw new ArgumentOutOfRangeException(nameof(exportType),
                                                                                     exportType.ToString()),
                                      }),
                                      $"{m} missing from: {exported}");
                    }
                }
            }
        }

        private static Array GetExportTypes() => Enum.GetValues(typeof(ExportFileType));

        private DisposableKSP?           inst;
        private TemporaryRepositoryData? repoData;
        private RegistryManager?         regMgr;
    }
}
