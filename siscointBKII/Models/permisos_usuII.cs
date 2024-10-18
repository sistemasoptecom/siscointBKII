using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class permisos_usuII
    {
        public Int64 id_permiso { get; set; }
        public int id_view { get; set; }
        public string cod_usuario { get; set; }
        public int id_usuario { get; set; }
        public string usuario { get; set; }
        public string autorizacion { get; set; }
        public string pe1 { get; set; }
        public string pe2 { get; set; }
        public string usuario_crea { get; set; }
        public DateTime Fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public int Estado { get; set; }

    }
}
