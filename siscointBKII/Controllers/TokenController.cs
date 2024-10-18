using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public TokenController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;   
        }
        [HttpGet("listarUsuariosToken")]
        [Authorize]
        public IActionResult listarUsuariosToken()
        {
            List<usuario> _usuario = new List<usuario>();
            variable _variable_e = _context.variable.Where(x => x.codigo_variable == "usuarios_token").FirstOrDefault();
            if (_variable_e != null) 
            {
                string[] _usuarios_ = _variable_e.valor_variable.Split(';');
                foreach(string s in _usuarios_)
                {
                    usuario _usuario_e = _context.usuario.Where(x => x.username == s).FirstOrDefault(); 
                    if(_usuario_e != null)
                    {
                        _usuario.Add(_usuario_e);
                    }
                }
            }
            return Ok(_usuario);
        }

        [HttpPost("EnviarTokenAutorizacion")]
        [Authorize]
        public ActionResult EnviarTokenAutorizacion(dynamic data_recibe)
        {
            string mensaje = "";
            string cuerpoMensaje = "";
            string asunto = "";
            string usuario_ = "";
            string proceso_ = "";
            string correoEnviar = "";
            Int32 myRandomNo = 0;
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                usuario_ = Convert.ToString(datObject["usuario"]);
                proceso_ = Convert.ToString(datObject["proceso"]);
                correoEnviar = Convert.ToString(datObject["correoEnviar"]);
                Random rnd = new Random();
                myRandomNo = Convert.ToInt32(rnd.Next(10000000, 99999999));
                token_autoriza _token_autoriza_e = _context.token_autoriza.Where(x => x.token == myRandomNo
                                                                                 && x.estado == 1).FirstOrDefault();
                if (_token_autoriza_e == null)
                {
                    System.Boolean EnviaCorreo = true;
                    cuerpoMensaje = "ENVIO TOKEN DE AUTORIOZACION : \n" + myRandomNo;
                    asunto = "TOKEN DE AUTORIOZACION";
                    EnviaCorreo = General.enviarCorreo(asunto, cuerpoMensaje, correoEnviar, _config.GetConnectionString("conexionDbPruebas"));
                    if (EnviaCorreo && myRandomNo > 0)
                    {
                        token_autoriza token_Autoriza_e = new token_autoriza();
                        token_Autoriza_e.token = myRandomNo;
                        token_Autoriza_e.proceso = proceso_;
                        token_Autoriza_e.usuario = usuario_;
                        token_Autoriza_e.correo_envia = correoEnviar;
                        token_Autoriza_e.fecha_creacion = DateTime.Now;
                        token_Autoriza_e.fecha_expira = DateTime.Now;
                        token_Autoriza_e.estado = 1;
                        _context.token_autoriza.Add(token_Autoriza_e);
                        _context.SaveChanges();
                        //Reviso el token activo por el tipo Proceoso
                        List<token_autoriza> _token_autoriza = _context.token_autoriza.Where(x => x.proceso == proceso_
                                                                                             && x.estado == 1).ToList();

                        if (_token_autoriza.Count > 1)
                        {
                            Int64 max = _token_autoriza.Max(y => y.Id);
                            foreach (var item in _token_autoriza.Select((value, i) => (value, i)).OrderBy(x => x.value.Id))
                            {
                                //escojo el maximo 

                                if (max != item.value.Id)
                                {
                                    item.value.estado = 0;
                                    item.value.fecha_expira = DateTime.Now;
                                    _context.token_autoriza.Update(item.value);
                                    _context.SaveChanges();
                                }

                            }

                        }
                        mensaje = "TOKEN ENVIADO AL CORREO";
                    }
                    else
                    {
                        mensaje = "ERROR AL ENVIAR EL CORREO ";
                    }
                }
                else
                {
                    mensaje = "BEDE GENERAR NUEVAMENTE";
                }
            }
            catch (Exception e) { }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        [HttpPost("ValidarTokenAutorizacion")]
        [Authorize]

        public IActionResult ValidarTokenAutorizacion(dynamic data_recibe)
        {
            string mensaje = "";
            string usuario_ = "";
            string proceso_ = "";
            string prop2 = "";
            string prop3 = "";
            Int32 token = 0;

            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                usuario_ = Convert.ToString(datObject["usuario"]);
                token = Convert.ToInt32(datObject["token"]);
                proceso_ = Convert.ToString(datObject["prop1"]);
                prop2 = Convert.ToString(datObject["prop2"]);
                prop3 = Convert.ToString(datObject["prop3"]);
                //validamos el token para inactivarlo
                token_autoriza _token_autoriza_e = _context.token_autoriza.Where(x => x.token == token
                                                                                 && x.proceso == proceso_
                                                                                 && x.usuario == usuario_
                                                                                 && x.estado == 1).FirstOrDefault();
                if (_token_autoriza_e != null)
                {
                    _token_autoriza_e.estado = 0;
                    _token_autoriza_e.fecha_expira = DateTime.Now;
                    _context.token_autoriza.Update(_token_autoriza_e);
                    int rs = _context.SaveChanges();
                    if (rs > 0)
                    {
                        switch (proceso_)
                        {
                            case "CerrarPeriodo":
                                liq_periodo_comision_v2 _liq_periodo_com_v2_e = new liq_periodo_comision_v2();
                                _liq_periodo_com_v2_e = _context.liq_periodo_comision_v2.Where(x => x.periodo == prop2).FirstOrDefault();
                                if (_liq_periodo_com_v2_e != null)
                                {
                                    int valueInt = Convert.ToInt32(prop3);
                                    System.Boolean estado = Convert.ToBoolean(valueInt);
                                    //Int32 setEstado = 0;
                                    if (estado)
                                    {
                                        //setEstado = 1;
                                        _liq_periodo_com_v2_e.estado = 0;
                                        _liq_periodo_com_v2_e.EsCerrado = 0;
                                    }
                                    else
                                    {
                                        //setEstado = 0;
                                        _liq_periodo_com_v2_e.estado = 1;
                                        _liq_periodo_com_v2_e.EsCerrado = 1;
                                    }
                                    _context.liq_periodo_comision_v2.Update(_liq_periodo_com_v2_e);
                                    int rs2 = _context.SaveChanges();
                                    if (rs2 > 0)
                                    {
                                        mensaje = "PERIODO CERRADO DE FORMA CORRECTA";
                                    }
                                }

                                //liq_periodo_comision_v2 _liq_periodo_comision_v2_e = _
                                break;
                            case "PublicarPeriodo":
                                liq_periodo_comision_v2 _liq_periodo_com_v2_e_P = new liq_periodo_comision_v2();
                                _liq_periodo_com_v2_e_P = _context.liq_periodo_comision_v2.Where(x => x.periodo == prop2).FirstOrDefault();
                                if(_liq_periodo_com_v2_e_P != null)
                                {
                                    int valueIntP = Convert.ToInt32(prop3);
                                    System.Boolean estadoP = Convert.ToBoolean(valueIntP);
                                    if (estadoP)
                                    {
                                        _liq_periodo_com_v2_e_P.EsPublicado = 1;
                                    }
                                    else
                                    {
                                        _liq_periodo_com_v2_e_P.EsPublicado = 0;
                                    }
                                    _context.liq_periodo_comision_v2.Update(_liq_periodo_com_v2_e_P);
                                    int rs3 = _context.SaveChanges();
                                    if(rs3 > 0)
                                    {
                                        mensaje = "PERIODO PUBLICADO DE FORMA CORRECTA";
                                    }
                                }
                                break;
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
                General.CrearLogError(sf.GetMethod().Name, "Inactivar Token", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(mensaje);
            return Ok(json);
        }

        [HttpPost("EnviarTokenAcceso")]
        public ActionResult EnviarTokenAcceso(dynamic data_recibe)
        {
            string mensaje = "";
            string cuerpoMensaje = "";
            string asunto = "";
            string cedula = "";
            string correo = "";
            int EnvioExitoso = 0;
            Int32 myRandomNo = 0;
            string mensajeOut = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                cedula = Convert.ToString(datObject["cedula"]);
                correo = Convert.ToString(datObject["correo"]);
                Random rnd = new Random();
                myRandomNo = Convert.ToInt32(rnd.Next(10000000, 99999999));

                token_envia _token_envia_e = _context.token_envia.Where(x => x.token == myRandomNo
                                                                        && x.estado == 1).FirstOrDefault();
                if(_token_envia_e == null) {
                    System.Boolean EnviaCorreo = true;
                    cuerpoMensaje = "ENVIO TOKEN DE AUTORIOZACION : \n" + myRandomNo;
                    asunto = "TOKEN DE ACCESO";
                    EnviaCorreo = General.EnviarCorreoAuth(asunto, 
                                                           cuerpoMensaje, 
                                                           correo, 
                                                           _config.GetConnectionString("conexionDbPruebas"), 
                                                           out mensajeOut);
                    if (string.IsNullOrEmpty(mensajeOut))
                    {
                        if (EnviaCorreo && myRandomNo > 0)
                        {
                            token_envia token_envia_e = new token_envia();
                            token_envia_e.token = myRandomNo;
                            token_envia_e.cedula = cedula;
                            token_envia_e.email = correo;
                            token_envia_e.fecha_creacion = DateTime.Now;
                            token_envia_e.fecha_expiracion = DateTime.Now;
                            token_envia_e.estado = 1;
                            _context.token_envia.Add(token_envia_e);
                            _context.SaveChanges();
                            List<token_envia> _token_envia = _context.token_envia.Where(x => x.cedula == cedula
                                                                                        && x.estado == 1).ToList(); 
                            if(_token_envia.Count > 1)
                            {
                                Int64 max = _token_envia.Max(y => y.id);
                                foreach(var item in _token_envia.Select((value,i) =>(value,i)).OrderBy(x => x.value.id))
                                {
                                    if(max != item.value.id)
                                    {
                                        item.value.estado = 0;
                                        item.value.fecha_expiracion = DateTime.Now;
                                        _context.token_envia.Update(item.value);
                                        _context.SaveChanges();
                                        EnvioExitoso = 1;
                                    }
                                }
                            }
                            mensaje = "TOKEN ENVIADO AL CORREO";
                        }
                        else
                        {
                            mensaje = "ERROR AL ENVIAR EL CORREO ";
                        }
                    }
                    else
                    {
                        mensaje = mensajeOut;
                    }
                }
            }
            catch (Exception e)
            {
                mensaje = e.Message;
            }
            string json = JsonConvert.SerializeObject(new {Resultado = EnvioExitoso, Mensaje = mensaje});
            return Ok(json);
        }

        // GET: TokenController
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: TokenController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: TokenController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: TokenController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: TokenController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: TokenController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: TokenController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: TokenController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
