using System;

namespace siscointBKII.Models
{
    public class liq_tmp_solicitud_np
    { 
        public Int64 id {  get; set; }  
        public string cedula_asesor {  get; set; }
        public string nombre_asesor { get; set; } 
        public string id_peticion {  get; set; }
        public string imei {  get; set; }   
        public string tipo_operacion { get; set; }
        public string tipo_operacion_peticion { get; set; } 
        public string periodo_np { get; set; }
        public string periodo_cm {  get; set; } 
        public string calculo {  get; set; }    
        public double valor_comision { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public int EsIngresado { get; set; }
        public int Esprocesado { get; set; }
    }
}
