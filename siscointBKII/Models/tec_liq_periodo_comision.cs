﻿using System;

namespace siscointBKII.Models
{
    public class tec_liq_periodo_comision
    {
        public Int64 id { get; set; }
        public string periodo { get; set; }
        public Int32 espublicado { get; set; }
        public Int32 escerrado { get; set; }
        public Int32 escerradometas { get; set; }
        public string usuario { get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
    }
}
