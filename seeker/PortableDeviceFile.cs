using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace seeker
{
    public class PortableDeviceFile : PortableDeviceObject
    {
        public PortableDeviceFile(string id, string name) : base(id, name)
        {
        }

        public string Path { get; set; }
        public string FullName { get; set; }
    }

}
