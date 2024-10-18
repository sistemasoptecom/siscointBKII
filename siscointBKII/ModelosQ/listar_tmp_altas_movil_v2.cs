using System;

namespace siscointBKII.ModelosQ
{
    public class listar_tmp_altas_movil_v2
    {
        public string cedula_asesor {  get; set; }
        public string nombre_asesor { get; set; }
        public string cedula_supervisor { get; set; }
        public string nombre_supervisor { get; set; }   
        public Int32 unidad {  get; set; }  
        public double valor { get; set; }   
        public string periodo { get; set; } 
        public string observacion { get; set; }
        public Int32 estado { get; set; }
        public Int32 EsProcesado { get; set; }
        public Int32 lote_importe { get; set; }
        public string usuario { get; set; }
        public string cedula_cliente {  get; set; }
        public string imei { get; set; } 
        public string celular { get; set; } 
        public Int32 codigo_tipo_escala { get; set; }
        public Int32 EsValido { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public string nombre_tipo_esquema { get; set; } 
    }
}
