using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using IoFile = System.IO.File;


namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelBcoAgrearioController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        
        public ExcelBcoAgrearioController(AplicationDbContext context , IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("procesarExcelBcoAgrario")]
        [Authorize]
        public IActionResult procesarExcelBcoAgrario(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario_ = "";
            string nombreUsuario_ = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["base"]);
                usuario_ = Convert.ToString(datObject["usuario"]);
                nombreUsuario_ = Convert.ToString(datObject["nombreUser"]);
                Int64 ultimoConsecutivo = 0;

                List<tmp_excel_bco_agrario> _bco_agrario = new List<tmp_excel_bco_agrario>();
                _bco_agrario = JsonConvert.DeserializeObject<List<tmp_excel_bco_agrario>>(base_);
                if(_bco_agrario.Count() > 0)
                {
                    ultimoConsecutivo = consecutivoExcelBcoAgrario();
                    excel_bco_agrario _excel_bco_agrario = new excel_bco_agrario();
                    _excel_bco_agrario.fecha = DateTime.Now;
                    _excel_bco_agrario.ruta = "N/A";
                    _excel_bco_agrario.estado = 1;
                    _excel_bco_agrario.usuario = usuario_;
                    
                    _excel_bco_agrario.consecutivo = ultimoConsecutivo;
                    _context.Add(_excel_bco_agrario);
                    if(_excel_bco_agrario != null)
                    {
                        foreach(tmp_excel_bco_agrario item in _bco_agrario)
                        {
                            //item.excel_bco_agrario = ultimoConsecutivo;
                            tmp_excel_bco_agrario _tmp_excel_bco_agrario = new tmp_excel_bco_agrario();
                            _tmp_excel_bco_agrario = item;
                            _tmp_excel_bco_agrario.excel_bco_agrario = ultimoConsecutivo;
                            
                            _context.Add(_tmp_excel_bco_agrario);
                        }
                    }
                }
                //foreach(tmp_excel_bco_agrario item in _bco_agrario)
                //{

                //}
                int rs = _context.SaveChanges();
                return Ok(rs);
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "ExcekBancoAgrario", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(resultado);
        }

        [HttpGet("listarExcelBcoAgrario")]
        [Authorize]
        public IActionResult listarExcelBcoAgrario()
        {
            List<excel_bco_agrario> _excel_bco_agrario = _context.excel_bco_agrario.OrderByDescending(x => x.id).ToList();
            return Ok(_excel_bco_agrario);
        }

        [HttpGet("ObtenerBcoAgrario/{consecutivo}")]
        [Authorize]

        public IActionResult getExcelBcoAgrario(int consecutivo)
        {
            
            var data = new Object();
            string cuerpo_mensaje_txt = "";
            try
            {
                Int64 _conseutivo = Convert.ToInt64(consecutivo);
                List<tmp_excel_bco_agrario> _tpm_excel_bco_agrario = _context.tmp_excel_bco_agrario.
                                                                     Where(x => x.excel_bco_agrario == _conseutivo).ToList();
                if(_tpm_excel_bco_agrario.Count > 0)
                {
                    cuerpo_mensaje_txt = "                      0000000009                                          3 9006687221             OPTECOM SAS \n";
                    foreach(tmp_excel_bco_agrario item in _tpm_excel_bco_agrario)
                    {
                        string campo11 = item.campo_11 + "";
                        string campo13 = item.campo_13 + "";
                        string campo14 = item.campo_14 + "";
                        string campo15 = item.campo_15 + "";
                        string campo16 = item.campo_16 + "";
                        string campo17 = item.campo_17 + "";
                        if (campo11.Length > 11)
                        {
                            campo11 = item.campo_11.Substring(0, 10);
                        }
                        if (campo13.Length > 11)
                        {
                            campo13 = item.campo_13.Substring(0, 10);
                        }
                        if (campo14.Length > 20)
                        {
                            campo14 = item.campo_14.Substring(0, 19);
                        }
                        if (campo15.Length > 20)
                        {
                            campo15 = item.campo_15.Substring(0, 19);
                        }
                        if (campo16.Length > 20)
                        {
                            campo16 = item.campo_16.Substring(0, 19);
                        }
                        if (campo17.Length > 20)
                        {
                            campo17 = item.campo_17.Substring(0, 19);
                        }
                        cuerpo_mensaje_txt += item.campo_1 + "" + item.campo_2 + "" + item.campo_3 + "" + item.campo_4 + "" + item.campo_5 + "" + item.campo_6 + ""
                                            + item.campo_7 + "" + item.campo_8 + "" + item.campo_9.PadRight(16) + "" + item.campo_10 + "" + campo11.PadRight(11) + "" + item.campo_12 + ""
                                            + campo13.PadRight(11) + "" + campo14.PadRight(20) + "" + campo15.PadRight(20) + "" + campo16.PadRight(20) + "" + campo17.PadRight(20) + ""
                                            + item.campo_18 + "\n";
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Excel bco agrario", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(cuerpo_mensaje_txt);
        }

        public Int64 consecutivoExcelBcoAgrario()
        {
            Int64 consecutvo = 0;
            if(_context.excel_bco_agrario.Count() > 0)
            {
                consecutvo = _context.excel_bco_agrario.Where(x => x.estado == 1).Select(x => x.consecutivo).Max();
                consecutvo = consecutvo + 1;
            }
            else
            {
                consecutvo = 1;
            }
            return consecutvo;
        }
    }
}
