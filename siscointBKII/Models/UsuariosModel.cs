using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    public class UsuariosModel
    {
        public long id { get; set; }
        public string codigo { get; set; }
        public string nombre_usuario { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public int id_tipo_usuario { get; set; }
        public int estado { get; set; }
        public string cargo { get; set; }
        public string area { get; set; }
        public Nullable<int> modulo { get; set; }
    }
}
