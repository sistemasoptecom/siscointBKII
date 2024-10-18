using System;

namespace siscointBKII.ModelosQ
{
    public class listar_pendientes_nunca_pagos
    {
        public string cedula_asesor { get; set; }
        public string nombre_asesor { get; set; }
        public string zona_asesor { get; set; }
        public string periodo_np { get; set; }
        public double valor_pendiente { get; set; }
        public Int32 pendiente { get; set; }
        public Int32 estado { get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public string tipo_operacion { get; set; }
    }
}
