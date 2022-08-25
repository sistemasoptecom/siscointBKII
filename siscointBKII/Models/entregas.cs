using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class entregas
    {
        public int id_ent { get; set; }
        public string ced_empl { get; set; }
        public System.DateTime fecha { get; set; }
        public string hora { get; set; }
        public int id_empresa { get; set; }
        public int cod_user { get; set; }
        public string observacion { get; set; }
        public int autoriza { get; set; }
        public int estado { get; set; }
        public int tipo_acta { get; set; }
    }
}
