using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using siscointBKII.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class permisos_usuIIController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public permisos_usuIIController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpGet("listarPermisos")]
        [Authorize]
        public IActionResult permisos()
        {
            var permisos = new Object();
            try
            {
                permisos = listarPermisos();
              
            }
            catch(Exception e)
            {
                //log de enventos errores del sistemas
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listarPermisos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(permisos);
        }
        
        [HttpPost("listarPermisosUsuarios")]
        [Authorize]
        public IActionResult listarPermisosUsuarios([FromBody] permisos_usuII permisos_UsuII)
        {
            var permisosUsu = new Object();
            try
            {
                permisosUsu = _context.permisos_usuIII.Where(x => x.usuario == permisos_UsuII.usuario || x.cod_usuario == permisos_UsuII.cod_usuario).ToList();
            }
            catch(Exception e)
            {
                //aqui el log de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "permisos_usuIII", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            
            return Ok(permisosUsu);
        }
        [HttpPost("actulizarPermisosUsu")]
        [Authorize]
        public IActionResult actulizarPermisosUsu(dynamic data)
        {
            try
            {
                //DataTable dtt = (DataTable)JsonConvert.DeserializeObject(data);
                string dat = System.Text.Json.JsonSerializer.Serialize(data);
                List<permisos_usuIII> permisos = JsonConvert.DeserializeObject<List<permisos_usuIII>>(dat);
                foreach(permisos_usuIII permiso in permisos)
                {
                    var dato = _context.permisos_usuIII.FirstOrDefault(x => x.id_view == permiso.id_view && x.usuario == permiso.usuario);
                    
                    if (dato != null)
                    {
                        if (permiso.autorizacion == "true")
                        {
                            dato.autorizacion = "1";
                        }
                        else
                        {
                            dato.autorizacion = "0";
                        }
                        if (permiso.pe1 == "true")
                        {
                            dato.pe1 = "1";
                        }
                        else
                        {
                            dato.pe1 = "0";
                        }
                        if (permiso.pe2 == "true")
                        {
                            dato.pe2 = "1";
                        }
                        else
                        {
                            dato.pe2 = "0";
                        }
                        dato.fecha_modificacion = permiso.fecha_modificacion;
                        _context.Update(dato);
                        _context.SaveChanges();
                    }
                    else
                    {
                        //verificamos si hay un modulo nuevo en y el usuario no lo tiene asiociado en la tabla permisos se crean
                        permisos_usuIII permisosAdd = new permisos_usuIII();
                        permisosAdd.id_view = permiso.id_view;
                        permisosAdd.cod_usuario = permiso.cod_usuario;
                        permisosAdd.id_usuario = permiso.id_usuario;
                        permisosAdd.usuario = permiso.usuario;
                        if (permiso.autorizacion == "true")
                        {
                            permisosAdd.autorizacion = "1";
                        }
                        else
                        {
                            permisosAdd.autorizacion = "0";
                        }
                        if (permiso.pe1 == "true")
                        {
                            permisosAdd.pe1 = "1";
                        }
                        else
                        {
                            permisosAdd.pe1 = "0";
                        }
                        if (permiso.pe2 == "true")
                        {
                            permisosAdd.pe2 = "1";
                        }
                        else
                        {
                            permisosAdd.pe2 = "0";
                        }
                        permisosAdd.usuario_crea = permiso.usuario_crea;
                        permisosAdd.Fecha_creacion = permiso.Fecha_creacion;
                        permisosAdd.fecha_modificacion = permiso.fecha_modificacion;
                        permisosAdd.Estado = 1;
                        _context.Add(permisosAdd);
                        _context.SaveChanges();
                    }
                    
                }

            }
            catch(Exception e)
            {
                //aqui log de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "permisos_usuIII", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
           
            return Ok();
        }
        public Object listarPermisos()
        {
            var permisos = _context.permisos_usuIII.ToList();
            return permisos;
        }

        
    }
}
