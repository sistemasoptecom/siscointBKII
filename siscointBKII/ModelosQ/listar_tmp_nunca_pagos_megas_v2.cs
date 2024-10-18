using System;

namespace siscointBKII.ModelosQ
{
    public class listar_tmp_nunca_pagos_megas_v2
    {
        public string cedula_asesor {  get; set; }
        public string nombre_asesor { get; set; }
        public string cedula_supervisor { get; set; }   
        public string nombre_supervisor { get; set; }
        public string zona {  get; set; }
        public string periodo { get; set; }
        public Int32 cod_tipo_esquema { get; set; } 
        public string observacion { get; set; } 
        public double total { get; set; }
        public Int32 estado { get; set; }
        public Int32 EsProcesado { get; set; }
        public Int32 lote_importe { get; set; }
        public string usuario { get; set; }
        public string periodo_ant {  get; set; }
        public string tipo_operacion { get; set; }
        public Int32 EsValido { get; set; }
        public DateTime fecha_creacion { get; set; }    
        public DateTime fecha_modificacion { get; set; }
    }
}
