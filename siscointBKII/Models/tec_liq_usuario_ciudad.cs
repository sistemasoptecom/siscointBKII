using System;

namespace siscointBKII.Models
{
    public class tec_liq_usuario_ciudad
    {
        public Int64 id {  get; set; }
        public string usuario { get; set; }
        public string cod_ciudad { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
