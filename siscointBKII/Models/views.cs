using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class views
    {
        public Int32 id { get; set; }
        public Int32 id_vista { get; set; }
        public string name_module { get; set; }
        public string module { get; set; }
        public string icon { get; set; }
        public string url { get; set; }
        public string visible { get; set; }
        public string routeurl { get; set; }
        public string menu_padre { get; set; }
        public int estado { get; set; }
        public DateTime fechaCreacion { get; set; }

    } 
}
