using System;
using System.Linq;
using System.Collections.Generic;

using CKAN.Configuration;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    public class InstallFiltersScreen : ConsoleScreen {

        public InstallFiltersScreen(IConfiguration globalConfig, GameInstance instance)
        {
            this.globalConfig = globalConfig;
            this.instance     = instance;
            globalFilters   = globalConfig.GlobalInstallFilters.ToList();
            instanceFilters = instance.InstallFilters.ToList();

            AddTip("F2", "Accept");
            AddBinding(Keys.F2, (object sender, ConsoleTheme theme) => {
                Save();
                // Close screen
                return false;
            });
            AddTip("Esc", "Cancel");
            AddBinding(Keys.Escape, (object sender, ConsoleTheme theme) => {
                // Discard changes
                return false;
            });

            mainMenu = new ConsolePopupMenu(new List<ConsoleMenuOption>() {
                new ConsoleMenuOption("Add MiniAVC", "",
                    "Prevent MiniAVC from being installed",
                    true, AddMiniAVC),
            });

            int vMid = Console.WindowHeight / 2;

            globalList = new ConsoleListBox<string>(
                2, 2, -2, vMid - 1,
                globalFilters,
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>() {
                        Header   = "Global Filters",
                        Width    = 40,
                        Renderer = f => f,
                    }
                },
                0
            );
            AddObject(globalList);
            globalList.AddTip("A", "Add");
            globalList.AddBinding(Keys.A, (object sender, ConsoleTheme theme) => {
                AddFilter(theme, globalList, globalFilters);
                return true;
            });
            globalList.AddTip("R", "Remove");
            globalList.AddBinding(Keys.R, (object sender, ConsoleTheme theme) => {
                RemoveFilter(globalList, globalFilters);
                return true;
            });
            instanceList = new ConsoleListBox<string>(
                2, vMid + 1, -2, -2,
                instanceFilters,
                new List<ConsoleListBoxColumn<string>>() {
                    new ConsoleListBoxColumn<string>() {
                        Header   = "Instance Filters",
                        Width    = 40,
                        Renderer = f => f,
                    }
                },
                0
            );
            AddObject(instanceList);
            instanceList.AddTip("A", "Add");
            instanceList.AddBinding(Keys.A, (object sender, ConsoleTheme theme) => {
                AddFilter(theme, instanceList, instanceFilters);
                return true;
            });
            instanceList.AddTip("R", "Remove");
            instanceList.AddBinding(Keys.R, (object sender, ConsoleTheme theme) => {
                RemoveFilter(instanceList, instanceFilters);
                return true;
            });
        }

        /// <summary>
        /// Put CKAN 1.25.5 in top left corner
        /// </summary>
        protected override string LeftHeader()
        {
            return $"CKAN {Meta.GetVersion()}";
        }

        /// <summary>
        /// Put description in top center
        /// </summary>
        protected override string CenterHeader()
        {
            return "Installation Filters";
        }

        private bool AddMiniAVC(ConsoleTheme theme)
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
            string filter = new InstallFilterAddDialog().Run(theme);
            DrawBackground(theme);
            if (!string.IsNullOrEmpty(filter) && !filters.Contains(filter)) {
                filters.Add(filter);
                box.SetData(filters);
            }
        }

        private void RemoveFilter(ConsoleListBox<string> box, List<string> filters)
        {
            filters.Remove(box.Selection);
            box.SetData(filters);
        }

        private void Save()
        {
            globalConfig.GlobalInstallFilters = globalFilters.ToArray();
            instance.InstallFilters           = instanceFilters.ToArray();
        }

        private IConfiguration globalConfig;
        private GameInstance   instance;

        private List<string> globalFilters;
        private List<string> instanceFilters;

        private ConsoleListBox<string> globalList;
        private ConsoleListBox<string> instanceList;

        private static readonly string[] miniAVC = new string[] {
            "MiniAVC.dll",
            "MiniAVC.xml",
            "LICENSE-MiniAVC.txt",
            "MiniAVC-V2.dll",
            "MiniAVC-V2.dll.mdb",
        };
    }

}
