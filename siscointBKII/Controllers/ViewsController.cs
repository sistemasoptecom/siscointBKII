using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using siscointBKII.Models;
using siscointBKII.ModelosQ;
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
                General.CrearLogError(sf.GetMethod().Name, "listarPermisos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexionDbPruebas"));
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
                General.CrearLogError(sf.GetMethod().Name, "permisos_usuIII", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(viewsPermisos);
        }

        [HttpPost("listarMenuPadre")]
        [Authorize]
        public IActionResult listarMenuPadre([FromBody] usuario usuario)
        {
            List<listar_menu_padre> _menu_padre = new List<listar_menu_padre>();
            try
            {
                
                string _usuario = usuario.username;
                _menu_padre = getListMenuPadre(_usuario);
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "views", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_menu_padre);
        }

        [HttpPost("listarMenuHijo")]
        [Authorize]
        public IActionResult listarMenuHijo([FromBody] usuario usuario)
        {
            List<views> _menu_hijo = new List<views>();
            try
            {
                string _ussuario = usuario.username;
                _menu_hijo = getListMenuHijo(_ussuario);
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "views", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_menu_hijo);
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
        public List<listar_menu_padre> getListMenuPadre(string username)
        {
            List<listar_menu_padre> menu_padre = new List<listar_menu_padre>();
            string query = "";
            try
            {
                if (!string.IsNullOrEmpty(username))
                {
                    query = "select distinct(v.menu_padre) from views v inner join permisos_usuIII p on p.id_view = v.id_vista  \n"+
                            " where p.usuario = '"+ username + "' and p.autorizacion = 1";
                    using(SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
                    {
                        using(SqlCommand cmd = new SqlCommand(query))
                        {
                            cmd.Connection = con;
                            con.Open();
                            using(SqlDataReader srd = cmd.ExecuteReader())
                            {
                                while (srd.Read())
                                {
                                    menu_padre.Add(new listar_menu_padre
                                    {
                                        menu_padre = srd["menu_padre"] + ""
                                    });
                                }
                            }
                            con.Close();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "views", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return menu_padre;
        }

        //listar los menus
        public List<views> getListMenuHijo(string username)
        {
            List<views> menu_hijo = new List<views>();
            //var data = new object();
            try
            {
               var  data = (from v in _context.views
                        join pe in _context.permisos_usuIII on v.id_vista equals pe.id_view
                        where (pe.usuario == username && v.estado == 1 && pe.autorizacion == "1")
                        select new
                        {
                            id = v.id,
                            id_vista = v.id_vista,
                            name_module = v.name_module,
                            module = v.module,
                            icon = v.icon,
                            url = v.url,
                            visble = v.visible,
                            routeurl = v.routeurl,
                            menu_padre = v.menu_padre,
                            estado = v.estado
                        }).ToList();
               foreach(var result in data)
               {
                    menu_hijo.Add(new views
                    {
                        id = result.id,
                        id_vista = result.id_vista,
                        name_module = result.name_module,
                        module = result.module,
                        icon = result.icon,
                        visible = result.visble,
                        routeurl = result.routeurl,
                        menu_padre = result.menu_padre,
                        estado = result.estado
                    });
               }
                //menu_hijo.Add(data);
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "views", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            
            return menu_hijo;
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
                    query = "select id_vista as id, name_module, module, (select 0 ) as autorizacion,(select 0 ) as pe1,(select 0 )as pe2 from views";
                }
                else
                {
                    query = "select id_vista as id,name_module,module, " +
                            "ISNULL((select autorizacion from permisos_usuIII where usuario = '" + username + "' and id_view = id_vista),0) as autorizacion," +
                            "ISNULL((select pe1 from permisos_usuIII where usuario = '" + username + "' and id_view = id_vista),0) as pe1," +
                            "ISNULL((select pe2 from permisos_usuIII where usuario = '" + username + "' and id_view = id_vista),0)as pe2 from views";
                }
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
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
                General.CrearLogError(sf.GetMethod().Name, "permisos_usuIII", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexionDbPruebas"));
            }

            return ViewsPermisos;
        }
       

    }
}
