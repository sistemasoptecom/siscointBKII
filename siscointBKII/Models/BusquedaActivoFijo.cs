using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class BusquedaActivoFijo
    {
        public int id { get; set; }
        public int id_ped { get; set; }
        public string Codigo { get; set; }
        public string Usuario { get; set; }
        public string Descripcion { get; set; }
        public string Grupo { get; set; }
        public Double Valor { get; set; }
        public string CECO { get; set; }
        public int V_util { get; set; }
    }
}
