using System;

namespace siscointBKII.Models
{
    public class liq_super_esquema_acelerador
    {
        public Int32 id {  get; set; }
        public string escala_factor_mult_asesor {  get; set; }
        public string homologa_escala_factor_asesor { get; set; }
        public string aceleracion_desaceleracion {  get; set; } 
        public string valor {  get; set; } 
        public string factor_multiplicador_asesores { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public Int32 codigo_tipo_esquema { get; set; }  
    }
}
