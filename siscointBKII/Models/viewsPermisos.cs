using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class viewsPermisos
    {
        public Int32 id { get; set; }
        public string name_module { get; set; }
        public string module { get; set; }
        public Boolean autorizacion { get; set; }
        public Boolean pe1 { get; set; }
        public Boolean pe2 { get; set; }
    }
}
