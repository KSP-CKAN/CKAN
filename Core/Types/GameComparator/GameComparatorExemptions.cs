using System;
using System.Collections.Generic;
using System.Linq;

namespace CKAN
{
    public class GameComparatorExemptions : IGameComparatorExemptions
    {
        private struct CkanModuleProxy
        {
            public string Name { get; set; }
            public string Identifier { get; set; }
        }

        private IEnumerable<CkanModuleProxy> _exemptions;


        public GameComparatorExemptions(IEnumerable<CkanModule> exemptions = null)
        {
            if(exemptions != null)
            {
                _exemptions = exemptions.Select(ex => new CkanModuleProxy
                {
                    Name = ex.name,
                    Identifier = String.IsNullOrEmpty(ex.identifier)
                                    ? ex.name 
                                    : ex.identifier
                });
            }
            else
            {
                initDefaultGameExemptions();
            }
        }

        public GameComparatorExemptions()
        {
            initDefaultGameExemptions();
        }

        private void initDefaultGameExemptions()
        {
            Func<string, CkanModuleProxy> exMod = 
                name => new CkanModuleProxy { Name = name };
            _exemptions = new List<CkanModuleProxy>
            {
                new CkanModuleProxy {Name= "CustomBarnKit" },
                new CkanModuleProxy {Name= "ContractConfigurator" },
                new CkanModuleProxy {Name= "ModuleManager" },
                new CkanModuleProxy {Name= "Strategia" },
            };
        }

        public bool IsExempt(CkanModule module)
        {
            _exemptions = _exemptions ?? new List<CkanModuleProxy>();
            return _exemptions.Any(e => e.Name.ToLower() == module.name.ToLower()
            || e.Name.ToLower() == module.identifier.ToLower());
        }
    }
}