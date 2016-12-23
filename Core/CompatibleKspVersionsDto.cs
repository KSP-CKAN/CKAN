using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CKAN
{
    //
    //This is DTO object to be serialized/deserialized as JSON
    //
    class CompatibleKspVersionsDto
    {
        public CompatibleKspVersionsDto()
        {
            this.CompatibleKspVersions = new List<String>();
        }

        public String VersionOfKspWhenWritten { get; set; }

        public List<String> CompatibleKspVersions { get; set; }
    }
}
