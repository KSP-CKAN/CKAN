using System;
using System.Linq;
using System.Collections.Generic;

using CKAN.Configuration;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// A screen for editing the global and instance install filters
    /// </summary>
    public class InstallFiltersScreen : ConsoleScreen {

        /// <summary>
        /// Initialize the screen
        /// </summary>
        /// <param name="theme">The visual theme to use to draw the dialog</param>
        /// <param name="globalConfig">Object holding the global configuration</param>
        /// <param name="instance">The current instance</param>
        public InstallFiltersScreen(ConsoleTheme theme, IConfiguration globalConfig, GameInstance instance)
            : base(theme)
        {
            this.globalConfig = globalConfig;
            this.instance     = instance;
            globalFilters   = globalConfig.GlobalInstallFilters.ToList();
            instanceFilters = instance.InstallFilters.ToList();

            AddTip("F2", Properties.Resources.Accept);
            AddBinding(Keys.F2, (object sender) => {
                Save();
                // Close screen
                return false;
            });
            AddTip(Properties.Resources.Esc, Properties.Resources.Cancel);
            AddBinding(Keys.Escape, (object sender) => {
                // Discard changes
                return false;
            });

            mainMenu = new ConsolePopupMenu(new List<ConsoleMenuOption?>() {
                new ConsoleMenuOption(Properties.Resources.FiltersAddMiniAVCMenu, "",
                    Properties.Resources.FiltersAddMiniAVCMenuTip,
                    true, AddMiniAVC),
            });

            int vMid = Console.WindowHeight / 2;

            globalList = new ConsoleListBox<string>(
                2, 2, -2, vMid - 1,
                globalFilters,
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>(
                        Properties.Resources.FiltersGlobalHeader,
                        f => f,
                        null,
                        null)
                },
                0
            );
            AddObject(globalList);
            globalList.AddTip("A", Properties.Resources.Add);
            globalList.AddBinding(Keys.A, (object sender) => {
                AddFilter(theme, globalList, globalFilters);
                return true;
            });
            globalList.AddTip("R", Properties.Resources.Remove);
            globalList.AddBinding(Keys.R, (object sender) => {
                RemoveFilter(globalList, globalFilters);
                return true;
            });
            instanceList = new ConsoleListBox<string>(
                2, vMid + 1, -2, -2,
                instanceFilters,
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>(
                        Properties.Resources.FiltersInstanceHeader,
                        f => f,
                        null,
                        null)
                },
                0
            );
            AddObject(instanceList);
            instanceList.AddTip("A", Properties.Resources.Add);
            instanceList.AddBinding(Keys.A, (object sender) => {
                AddFilter(theme, instanceList, instanceFilters);
                return true;
            });
            instanceList.AddTip("R", Properties.Resources.Remove);
            instanceList.AddBinding(Keys.R, (object sender) => {
                RemoveFilter(instanceList, instanceFilters);
                return true;
            });
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
            => $"{Meta.GetProductName()} {Meta.GetVersion()}";

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
            => Properties.Resources.FiltersTitle;

        private bool AddMiniAVC()
        {
            globalFilters = globalFilters
                .Concat(miniAVC)
                .Distinct()
                .ToList();
            globalList.SetData(globalFilters);
            return true;
        }

        private void AddFilter(ConsoleTheme theme, ConsoleListBox<string> box, List<string> filters)
        {
            var filter = new InstallFilterAddDialog(theme).Run();
            DrawBackground();
            if (filter != null && !string.IsNullOrEmpty(filter) && !filters.Contains(filter)) {
                filters.Add(filter);
                box.SetData(filters);
            }
        }

        private static void RemoveFilter(ConsoleListBox<string> box, List<string> filters)
        {
            if (box.Selection is not null)
            {
                filters.Remove(box.Selection);
                box.SetData(filters);
            }
        }

        private void Save()
        {
            globalConfig.GlobalInstallFilters = globalFilters.ToArray();
            instance.InstallFilters           = instanceFilters.ToArray();
        }

        private readonly IConfiguration globalConfig;
        private readonly GameInstance   instance;

        private List<string> globalFilters;
        private readonly List<string> instanceFilters;

        private readonly ConsoleListBox<string> globalList;
        private readonly ConsoleListBox<string> instanceList;

        private static readonly string[] miniAVC = new string[] {
            "MiniAVC.dll",
            "MiniAVC.xml",
            "LICENSE-MiniAVC.txt",
            "MiniAVC-V2.dll",
            "MiniAVC-V2.dll.mdb",
        };
    }

}
