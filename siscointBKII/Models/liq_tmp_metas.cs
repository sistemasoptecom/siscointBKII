using System;

namespace siscointBKII.Models
{
    public class liq_tmp_metas
    {
        public Int64 id {  get; set; }
        public Int32 mes_importe_liq { get; set; }
        public string cedula_asesor { get; set; }
        public Int32 cod_tipo_escala { get; set; } 
        public Int32 numero_carta_meta_ftth { get; set; }
        public Int32 numero_carta_meta_movil { get; set; }
        public Int32 numero_carta_meta_tv {  get; set; }
        public string cedula_supervisor { get; set; }
        public DateTime fecha_ingreso { get; set; }
        public DateTime fecha_retiro { get; set; }
        public string activo { get; set; }
        public Int32 estado { get; set; }  
        public string usuario { get; set; } 
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set;  }
        public string periodo_importe { get; set; } 
        public string zona { get; set; } 
        public string empresa_contratante { get; set; }
        public string empresa_supervisor { get; set; }
        public string ciudad {  get; set; }
        public Int32 tipo_liquidador { get; set; }
        public string nombre_liquidador { get; set; }
    }
}
