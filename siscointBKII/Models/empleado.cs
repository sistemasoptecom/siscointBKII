using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class empleado
    {
        public int id { get; set; }
        public string cedula_emp { get; set; }
        public string nombre { get; set; }
        public string snombre { get; set; }
        public string ppellido { get; set; }
        public string spellido { get; set; }
        public string area { get; set; }
        public string cargo { get; set; }
        public int estado { get; set; }
        public int permiso { get; set; }
        public int ccosto { get; set; }
        public int empresa { get; set; }
    }
}
