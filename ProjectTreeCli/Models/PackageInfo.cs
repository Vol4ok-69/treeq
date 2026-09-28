using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectTreeCli.Models
{
    public sealed class PackageInfo
    {
        public string Name { get; set; } = string.Empty;

        public string Requested { get; set; } = string.Empty;

        public string Resolved { get; set; } = string.Empty;
    }
}
