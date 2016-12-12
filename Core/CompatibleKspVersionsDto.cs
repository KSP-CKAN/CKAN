using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CKAN
{
    class CompatibleKspVersionsDto
    {
        public CompatibleKspVersionsDto()
        {
            this.compatibleKspVersions = new List<String>();
        }

        public String versionOfKspWhenWritten { get; set; }

        public List<String> compatibleKspVersions { get; set; }
    }
}
