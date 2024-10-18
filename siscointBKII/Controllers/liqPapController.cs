using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using siscointBKII.ModelosQ;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Graph;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class liqPapController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public liqPapController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("listarTblLiqPap")]
        [Authorize]
        public IActionResult ListarTblLiqPap(dynamic data_recibe)
        {

            List<listar_liq_tbl_pap> _lista_liq_tbl_pap = new List<listar_liq_tbl_pap>();
            string query = "";
            string concatCondicionQueryPap = "";
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                string param = datObejt["parametro"] + "";

                if (param.Equals("1"))
                {
                    concatCondicionQueryPap = "  where pap.codigo_liq_esq = 1 ";
                }else if (param.Equals("2"))
                {
                    concatCondicionQueryPap = "  where pap.codigo_liq_esq = 2 ";
                }
                else if (param.Equals("5"))
                {
                    concatCondicionQueryPap = "  where pap.codigo_liq_esq = 5 ";
                }
                query = " select pap.id, es.nivel_escala, es.rango_altas, me.valor_mega, pap.valor, pap.cumplimiento, pap.descripcion \n" +
                        " from liq_pap pap inner join liq_escala_altas es on pap.valor_nivel = es.rango_altas \n" +
                        " inner join liq_valores_megabytes me on pap.valor_mega = me.codigo_valor "+ concatCondicionQueryPap + " \n" +
                        " group by pap.id, es.nivel_escala, es.rango_altas, me.valor_mega, pap.valor, pap.cumplimiento, pap.descripcion \n" +
                        " order by  me.valor_mega";

                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _lista_liq_tbl_pap.Add(new listar_liq_tbl_pap
                                {
                                    id = Convert.ToInt64(sdr["id"]),
                                    nivel_escala = sdr["nivel_escala"] + "",
                                    rango_altas = Convert.ToInt32(sdr["rango_altas"]),
                                    valor_mega = Convert.ToInt32(sdr["valor_mega"]),
                                    valor = Convert.ToInt64(sdr["valor"]),
                                    cumplimiento = sdr["cumplimiento"] + "",
                                    descripcion = sdr["descripcion"]+""
                                });

                            }
                        }
                        con.Close();
                    }
                }

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar liq tbl pap", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }


            return Ok(_lista_liq_tbl_pap);
        }

        [HttpGet("listarTblCall")]
        [Authorize]
        public IActionResult listarTblCall()
        {
            List<listar_liq_tbl_pap> _lista_liq_tbl_pap = new List<listar_liq_tbl_pap>();
            try
            {
                string query = "select esc.id, esc.codigo_esquema_call, lqm.valor_mega, esc.nivel, esc.cumplimiento, esc.valor " +
                               " from liq_esquema_call esc inner join liq_valores_megabytes lqm on lqm.codigo_valor = esc.codigo_tipo_internet";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _lista_liq_tbl_pap.Add(new listar_liq_tbl_pap
                                {
                                    id = Convert.ToInt64(sdr["id"]),
                                    nivel_escala = "Nivel "+sdr["nivel"] + "",
                                    valor_mega = Convert.ToInt32(sdr["valor_mega"]),
                                    valor = Convert.ToInt64(sdr["valor"]),
                                    cumplimiento = sdr["cumplimiento"] + ""
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar liq tbl Call", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_lista_liq_tbl_pap);
        }

        [HttpGet("listarTblMovil")]
        [Authorize]
        public IActionResult listarTblMovil()
        {
            List<listar_liq_tbl_pap> _lista_liq_tbl_pap = new List<listar_liq_tbl_pap>();
            try
            {
                string query = " select * from liq_esquema_movil";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _lista_liq_tbl_pap.Add(new listar_liq_tbl_pap
                                {
                                    id = Convert.ToInt64(sdr["id"]),
                                    nivel_escala = "Nivel " + sdr["nivel"] + "",
                                    valor = Convert.ToInt64(sdr["valor"]),
                                    cumplimiento = sdr["cumplimiento"] + "",
                                    tipo_renta = sdr["tipo_renta"] + "",
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listar liq tbl Movil", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_lista_liq_tbl_pap);
        }

        [HttpPost("ActualizarTblPap")]
        [Authorize]
        public IActionResult ActualizarTblPap(dynamic tipo)
        {
            int rs = 0;
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(tipo);
                var datObejt = JObject.Parse(data);
                Int64 id = Convert.ToInt64(datObejt["id"]);
                Int64 valor = Convert.ToInt64(datObejt["valor"]);
                string TipoEsquema = Convert.ToString(datObejt["esquema"]);

                if((TipoEsquema.Equals("PAP")) || (TipoEsquema.Equals("PYMES")))
                {
                    liq_pap _liq_Pap = _context.liq_pap.Where(x => x.id == id).FirstOrDefault();
                    if (_liq_Pap != null)
                    {
                        if (valor >= 0)
                        {
                            _liq_Pap.valor = valor;
                            rs = _context.SaveChanges();
                        }
                    }
                }else if(TipoEsquema.Equals("CALL OUT"))
                {
                    liq_esquema_call _liq_esquema_call = _context.liq_esquema_call.Where(x => x.id == id).FirstOrDefault();
                    if (_liq_esquema_call != null)
                    {
                        if (valor > 0)
                        {
                            _liq_esquema_call.valor = valor;
                            rs = _context.SaveChanges();
                        }
                    }
                }else if(TipoEsquema.Equals("MOVIL"))
                {
                    liq_esquema_movil _liq_esquema_movil = _context.liq_esquema_movil.Where(x => x.id == id).FirstOrDefault();
                    if(_liq_esquema_movil != null)
                    {
                        if(valor > 0)
                        {
                            _liq_esquema_movil.valor = valor;
                            rs = _context.SaveChanges();
                        }
                    }
                }

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Actualizar Tabla Pap", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(rs);
        }

        [HttpPost("listarValoresMega")]
        [Authorize]
        public IActionResult listarValoresMega(dynamic recibe)
        {
            List<liq_valores_megabytes> _liq_valores_megabytes = new List<liq_valores_megabytes>();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(recibe);
                var datObejt = JObject.Parse(data);
                Int32 esquemaComision = Convert.ToInt32(datObejt["esquema"]);
                _liq_valores_megabytes = _context.liq_valores_megabytes.Where(x => x.codigo_tipo_escala == esquemaComision
                                                                              && x.estado == 1).ToList();
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Listar Megas", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_liq_valores_megabytes);
        }

        [HttpPost("actualizarLiqValoresMega")]
        [Authorize]
        public IActionResult actualizarLiqValoresMega(dynamic recibe)
        {
            string mensaje = "";
            int rs = 0;
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(recibe);
                var datObejt = JObject.Parse(data);
                Int64 id = Convert.ToInt64(datObejt["id"]);
                Int64 valor = Convert.ToInt64(datObejt["valor"]);
                if(valor > 0)
                {
                    
                    liq_valores_megabytes _liq_valores_megabytes_e = _context.liq_valores_megabytes.Where(x => x.id == id).FirstOrDefault();
                    if(_liq_valores_megabytes_e != null)
                    {
                        _liq_valores_megabytes_e.valor_mega = (int)valor;
                        rs = _context.SaveChanges();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Actualizar Megas", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            if(rs >  0)
            {
                mensaje = "ACTUALIZADO DE FORMA CORRECTA;";
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }
        [HttpPost("listarEscalaAltas")]
        [Authorize]
        public IActionResult listarEscalaAltas(dynamic data_recibe)
        {
            string mensaje = "";
            var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
            var datObejt = JObject.Parse(data);
            Int32 codigo_tipo_esquema = Convert.ToInt32(datObejt["codigo_tipo_esquema"]);
            List<liq_escala_altas> _liq_escala_altas = _context.liq_escala_altas.Where(x => x.codigo_tipo_escala == codigo_tipo_esquema
                                                                                       && x.estado == 1).ToList();
            return Ok(_liq_escala_altas);
            
        }
        [HttpPost("actualizarEscalaAltas")]
        [Authorize]
        public IActionResult actualizarEscalaAltas(dynamic recibe)
        {
            string mensaje = "";
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(recibe);
                var datObejt = JObject.Parse(data);
                Int64 id = Convert.ToInt64(datObejt["id"]);
                Int32 valor = Convert.ToInt64(datObejt["valor"]);

                liq_escala_altas _liq_escala_altas_e = _context.liq_escala_altas.Where(x => x.id == id).FirstOrDefault();
                if(_liq_escala_altas_e != null)
                {
                    if(valor == 0)
                    {
                        _liq_escala_altas_e.rango_altas = valor;
                        _liq_escala_altas_e.nivel_escala = "n/a";
                        
                    }else if(valor > 0)
                    {
                        _liq_escala_altas_e.rango_altas = valor;
                        _liq_escala_altas_e.nivel_escala = "NIVEL "+valor;
                    }
                    int rs = _context.SaveChanges();
                   if(rs > 0)
                    {
                        mensaje = "ACTUALIZADO DE FORMA CORRECTA";
                    }   
                }

            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Actualizar Altas", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }
        [HttpPost("listarEmpHome")]
        [Authorize]
        public IActionResult listarEmpHome(dynamic data_recibe)
        {
            var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
            var datObejt = JObject.Parse(data);
            Int32 codigo_tipo_esquema = Convert.ToInt32(datObejt["codigo_tipo_esquema"]);
            
            var liq_emp_home = (from liq_emp in _context.liq_empaqhome
                                join liq_tp_emp in _context.liq_tipo_empahome on liq_emp.tipo_empaquehome equals liq_tp_emp.cod_tipo_emphome
                                where liq_emp.cod_tipo_esquema == codigo_tipo_esquema
                                select new
                                {
                                    id = liq_emp.id,
                                    cod_liq_dtn = liq_emp.cod_liq_dtn,
                                    tipo_empaquehome = liq_emp.tipo_empaquehome,
                                    descripcion = liq_tp_emp.descripcion,
                                    valor = liq_emp.valor,
                                    cumplimiento = liq_emp.cumplimiento,
                                    homologa_cumplimieno = liq_emp.homologa_cumplimieno,
                                    cod_tipo_esquema = liq_emp.cod_tipo_esquema,
                                    estado = liq_emp.estado,
                                    fecha_creacion = liq_emp.fecha_creacion,
                                    fecha_modificacion = liq_emp.fecha_modificacion,
                                    codigo_nivel = liq_emp.codigo_nivel
                                }).ToList();
                               
            return Ok(liq_emp_home);
        }
        [HttpPost("actualizarEmpHome")]
        [Authorize]
        public ActionResult actualizarEmpHome(dynamic recibe)
        {
            string mensaje = "";
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(recibe);
                var datObejt = JObject.Parse(data);
                Int64 id = Convert.ToInt64(datObejt["id"]);
                Int64 valor = Convert.ToInt64(datObejt["valor"]);
                liq_empaqhome _liq_empHome_e = _context.liq_empaqhome.Where(x => x.id == id).FirstOrDefault();
                if(_liq_empHome_e != null)
                {
                    _liq_empHome_e.valor = valor;
                    int rs = _context.SaveChanges();
                    if(rs > 0)
                    {
                        mensaje = "ACTUALIZADO DE FORMA CORRECTA";
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Actualizar EmpHome", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }

        [HttpPost("importarEsquena")]
        [Authorize]
        public IActionResult importarEsquena(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario = "";
            string mensaje = "";
            try
            {
                List<listar_importe_esquema_comision> _listar_esquema_periodo = new List<listar_importe_esquema_comision>();
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["data"]);
                usuario = Convert.ToString(datObject["usuario"]);
                _listar_esquema_periodo = JsonConvert.DeserializeObject<List<listar_importe_esquema_comision>>(base_);
                if(_listar_esquema_periodo.Count > 0)
                {
                    //string tipo_parametrizacion = _context.variable.Where(x => x.codigo_variable == "tipo_importe"
                    //                                                      && x.estado == 1)
                    //                                               .FirstOrDefault().valor_variable;
                    string tipo_parametrizacion = "";
                    variable variable_1 = _context.variable.Where(x => x.codigo_variable == "tipo_importe"
                                                                  && x.estado == 1)
                                                            .FirstOrDefault();

                    if(variable_1 != null)
                    {
                        tipo_parametrizacion = variable_1.valor_variable;
                    }

                    string tipo_nivel_cumplimiento = "";
                    variable variable_2 = _context.variable.Where(x => x.codigo_variable == "nivel_cumplimiento"
                                                                  && x.estado == 1)
                                                           .FirstOrDefault();
                    if(variable_2 != null)
                    {
                        tipo_nivel_cumplimiento = variable_2.valor_variable;
                    }
                    //string tipo_nivel_cumplimiento = _context.variable.Where(x => x.codigo_variable == "nivel_cumplimiento"
                    //                                                         && x.estado == 1)
                    //                                                  .FirstOrDefault().valor_variable;       
                    string[] rango_params = tipo_parametrizacion.Split(";");
                    string[] niveles_cumplimiento = tipo_nivel_cumplimiento.Split(";");
                    Int32 cod_liq_pap = _context.liq_pap.Max(x => x.cod_liq_pap);
                    foreach (listar_importe_esquema_comision item in _listar_esquema_periodo)
                    {
                        
                        cod_liq_pap = cod_liq_pap + 1;
                        string valor_mega = item.VALOR_MEGA;
                        string esquema = item.ESQUEMA;
                        string nivel = item.NIVEL;
                        string cumplimiento = item.CUMPLIMIENTO;
                        string tipo = item.TIPO;
                        Int32 tipo_liquidador = 0;
                        Int32.TryParse(tipo, out tipo_liquidador);
                        //obtenmos el codigo esquema
                        Int32 codigo_valor = 0;
                        liq_tipo_esquema liq_tipo_esquema_e = _context.liq_tipo_esquema.Where(x => x.esquema == esquema
                                                                                              && x.estado == 1)
                                                                                       .FirstOrDefault();
                        if(liq_tipo_esquema_e != null)
                        {
                            codigo_valor = liq_tipo_esquema_e.codigo_valor;
                        }
                        //obtenemos el numero escala altas
                        Int32 rango_altas = 0;
                        liq_escala_altas liq_escala_altas_e = _context.liq_escala_altas.Where(x => x.nivel_escala == nivel
                                                                                              && x.codigo_tipo_escala == codigo_valor
                                                                                              && x.estado == 1)
                                                                                       .FirstOrDefault();
                        if(liq_escala_altas_e != null)
                        {
                            rango_altas = liq_escala_altas_e.rango_altas;
                        }

                        //escogemos el valor de la mega
                        Int32 codigo_valor_mega = 0;
                        Int32 codigo_valor_mega_aux = 0;
                        Int32.TryParse(valor_mega, out codigo_valor_mega_aux);
                        liq_valores_megabytes liq_valores_megabytes_e = _context.liq_valores_megabytes.Where(x => x.valor_mega == codigo_valor_mega_aux
                                                                                 && x.codigo_tipo_escala == codigo_valor
                                                                                 && x.estado == 1)
                                                                          .FirstOrDefault();
                        if(liq_valores_megabytes_e != null)
                        {
                            codigo_valor_mega = liq_valores_megabytes_e.codigo_valor;
                        }

                        string[] subStrCumpl = cumplimiento.Split('%');
                        string[] subStrCumplB = subStrCumpl[0].Split('-');
                        string homologa_cumplimiento = subStrCumplB[0] + "," + subStrCumplB[1];


                        //escogemos el tipo parametrizacion para el importe
                        
                        string tipo_rango = "";
                        string descripcion_liq = "";
                        string tipo_nivel_cumpl = "";
                        Int32 nivel_cumplimiento = 0;
                        //escogemos en el forecah
                        foreach(string s in rango_params)
                        {
                            if(s.Contains(tipo))
                            {
                                tipo_rango = s;
                            }
                        }
                        if(!string.IsNullOrEmpty(tipo_rango))
                        {
                            string[] getTipoRango = tipo_rango.Split("-");
                            descripcion_liq = getTipoRango[1];
                        }
                        foreach(string s in niveles_cumplimiento)
                        {
                            if(s.Contains(homologa_cumplimiento))
                            {
                                tipo_nivel_cumpl = s;
                            }
                        }

                        string[] sub_niveles_cumplimiento = tipo_nivel_cumpl.Split("-");
                        Int32.TryParse(sub_niveles_cumplimiento[1], out nivel_cumplimiento);
                        //nivel_cumplimiento = Convert.ToInt32(sub_niveles_cumplimiento[1]);
                        liq_pap _liq_pap_e = new liq_pap();
                        _liq_pap_e.cod_liq_pap = cod_liq_pap;
                        _liq_pap_e.valor_mega = codigo_valor_mega;
                        _liq_pap_e.valor_nivel = rango_altas;
                        Int64 valorMb = 0;
                        Int64.TryParse(item.VALOR, out valorMb);
                        _liq_pap_e.valor = valorMb;
                        _liq_pap_e.cumplimiento = cumplimiento;
                        _liq_pap_e.nivel_cumplimiento = nivel_cumplimiento;
                        _liq_pap_e.codigo_liq_esq = codigo_valor;
                        _liq_pap_e.homologa_cumplimiento = homologa_cumplimiento;
                        _liq_pap_e.tipo_liquidador = tipo_liquidador;
                        _liq_pap_e.descripcion = descripcion_liq;
                        _liq_pap_e.estado = 1;
                        _liq_pap_e.fecha_creacion = DateTime.Now;
                        _liq_pap_e.fecha_modificacion = DateTime.Now;
                        _context.liq_pap.AddRange(_liq_pap_e);
                        
                    }
                }

                _context.SaveChanges();
                mensaje = "INSERTADO DE FORMA CORRECTA";
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                

                General.CrearLogError(sf.GetMethod().Name, "importarEsquena", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }
        public IActionResult Index()
        {
            
            return Ok();
        }
    }
}
