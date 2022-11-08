using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViewsController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        
        public ViewsController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpGet("views")]
        [Authorize]
        public IActionResult views()
        {
            string username = "";
            
            List<viewsPermisos> viewsPermisos = new List<viewsPermisos>();
            try
            {
                viewsPermisos = getListVistasPermisos(username);
            }
            catch(Exception e)
            {
                //Aqui en log de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "listarPermisos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
           
            return Ok(viewsPermisos);
        }
        [HttpPost("viewsUser")]
        [Authorize]
        public IActionResult viewsUsers([FromBody] permisos_usuII permisos_UsuII)
        {
            string username = "";
            List<viewsPermisos> viewsPermisos = new List<viewsPermisos>();
            try
            {
                var usuarioPermisos = _context.permisos_usuIII.FirstOrDefault(x => x.usuario == permisos_UsuII.usuario);
                if (usuarioPermisos != null)
                    username = usuarioPermisos.usuario.ToString();

                viewsPermisos = getListVistasPermisos(username);
            }
            catch(Exception e)
            {
                //log de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "permisos_usuIII", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(viewsPermisos);
        }

        //public IActionResult views()
        //{
        //    var vistas = getListViews();
        //    return Ok(vistas);
        //}

        //[HttpGet("views/{username}")]
        //[Authorize]
        //public IActionResult views(string username)
        //{

        //}

        public Object getListViews()
        {
            var vistas = _context.views.ToList();
            return vistas;
        }
        public List<viewsPermisos> getListVistasPermisos(string username)
        {
            var usuariospermiso = new Object();
            List<viewsPermisos> ViewsPermisos = new List<viewsPermisos>();
            string query = "";
            try
            {
                if(username == "")
                {
                    query = "select id, name_module, module, (select 0 ) as autorizacion,(select 0 ) as pe1,(select 0 )as pe2 from views";
                }
                else
                {
                    query = "select id,name_module,module, " +
                            "ISNULL((select autorizacion from permisos_usuIII where usuario = '" + username + "' and id_view = id),0) as autorizacion,"+
                            "ISNULL((select pe1 from permisos_usuIII where usuario = '" + username + "' and id_view = id),0) as pe1,"+
                            "ISNULL((select pe2 from permisos_usuIII where usuario = '" + username + "' and id_view = id),0)as pe2 from views";
                }
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    
                    using(SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                ViewsPermisos.Add(new viewsPermisos
                                {
                                    id = Convert.ToInt32(sdr["id"]),
                                    name_module = sdr["name_module"]+"",
                                    module = sdr["module"]+"",
                                    autorizacion = Convert.ToBoolean(Convert.ToInt32(sdr["autorizacion"])),
                                    pe1 = Convert.ToBoolean(Convert.ToInt32(sdr["pe1"])),
                                    pe2 = Convert.ToBoolean(Convert.ToInt32(sdr["pe2"]))
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
                General.CrearLogError(sf.GetMethod().Name, "permisos_usuIII", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }

            return ViewsPermisos;
        }
       

    }
}
