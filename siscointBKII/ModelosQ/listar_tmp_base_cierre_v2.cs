using System;

namespace siscointBKII.ModelosQ
{
    public class listar_tmp_base_cierre_v2
    {
        
        public string producto { get; set; }
        public string cedula_asesor { get; set; }
        public string nombre_asesor { get; set; }
        public Int32 mes_seg { get; set; }
        public Int32 unidad { get; set; }
        public string cod_peticion { get; set; }
        public double velocidad { get; set; }
        public Int32 velocidad_ftth_rango { get; set; }
        public Int32 velocidad_pymes_rango { get; set; }
        public string empaqhomo { get; set; }
        public string num_doc_cliente { get; set; }
        public string cedula_supervisor { get; set; }
        public string nombre_supervisor { get; set; }
        public string observacion { get; set; }
        public Int32 cod_tipo_esquema { get; set; }
        public string migracion_otro { get; set; }
        public string periodo { get; set; }
        public Int32 lote_importe { get; set; }
        public Int32 estado { get; set; }
        public Int32 EsProcesado { get; set; }
        public Int32 EsIngresado { get; set; }
        public string usuario { get; set; }
       
        public string tipo_campana { get; set; }
        public Int32 EsValido { get; set; }
        public string nombre_tipo_esquema { get; set; }
    }
}
