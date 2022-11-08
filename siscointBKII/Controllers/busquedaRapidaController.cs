using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class busquedaRapidaController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public busquedaRapidaController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("index")]
        [Authorize]
        public IActionResult Index(dynamic dato)
        {
            List<dataBusqueda> buscados = new List<dataBusqueda>();
            try
            {
                string dat = System.Text.Json.JsonSerializer.Serialize(dato);
                List<busquedaRapidaModel> abuscar = JsonConvert.DeserializeObject<List<busquedaRapidaModel>>(dat);
                buscados = searchBusquedaRapida(abuscar);
            }
            catch (Exception e)
            {
                //log Errrores
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "dataBusqueda", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(buscados);
        }


        private  List<dataBusqueda> searchBusquedaRapida(List<busquedaRapidaModel> abusqueda)
        {
            List<dataBusqueda> buscadosS = new List<dataBusqueda>();
            string valor = "";
            string parametro = "";
            foreach(busquedaRapidaModel item in abusqueda)
            {
                var busqueda = new Object();
                try
                {
                    switch (item.entidad)
                    {
                        case "area_ccosto":
                            List<area_ccosto> area_ccostos = new List<area_ccosto>();
                            valor = item.valor;
                            if (valor == "%")
                            {
                               // busqueda = _context.area_ccosto.ToList();
                                area_ccostos = _context.area_ccosto.ToList(); ;
                            }
                            else
                            {
                                area_ccostos = _context.area_ccosto.Where(x => x.ccosto == Convert.ToInt32(valor) || x.area == valor).ToList();
                            }

                            if(area_ccostos.Count() > 0)
                            {
                                foreach(area_ccosto i in area_ccostos)
                                {
                                    dataBusqueda data = new dataBusqueda();
                                    data.id = i.id;
                                    data.valor1 = i.ccosto + "";
                                    data.valor2 = i.area;
                                    data.valor3 = "";
                                    data.valor4 = "";
                                    data.valor5 = "";
                                    data.valor6 = "";
                                    data.valor7 = "";
                                    data.valor8 = "";
                                    buscadosS.Add(data);
                                }
                            }
                            break;
                        case "buscarCentroCosto":
                            List<area_ccosto> area_ccostos_ = new List<area_ccosto>();
                            valor = item.valor;
                            if (valor == "%")
                            {
                                // busqueda = _context.area_ccosto.ToList();
                                area_ccostos_ = _context.area_ccosto.ToList(); ;
                            }
                            else
                            {
                                area_ccostos_ = _context.area_ccosto.Where(x => x.ccosto == Convert.ToInt32(valor) || x.area == valor).ToList();
                            }

                            if (area_ccostos_.Count() > 0)
                            {
                                foreach (area_ccosto i in area_ccostos_)
                                {
                                    dataBusqueda data = new dataBusqueda();
                                    data.id = i.id;
                                    data.valor1 = i.ccosto + "";
                                    data.valor2 = i.area;
                                    data.valor3 = "";
                                    data.valor4 = "";
                                    data.valor5 = "";
                                    data.valor6 = "";
                                    data.valor7 = "";
                                    data.valor8 = "";
                                    buscadosS.Add(data);
                                }
                            }
                            break;

                        case "ArticuloDevolutivo":
                            valor = item.valor;
                            if(valor != "")
                            {
                                List<articulosBusqueda> articulosDevolucion = busquedaArticulosDevolutivos(valor);
                                if(articulosDevolucion.Count() > 0)
                                {
                                   foreach(articulosBusqueda i in articulosDevolucion)
                                    {
                                        dataBusqueda data = new dataBusqueda();
                                        data.id = i.Id;
                                        data.valor1 = i.Codigo;
                                        data.valor2 = i.Descripcion;
                                        data.valor3 = i.Tipo+"";
                                        data.valor4 = "";
                                        data.valor5 = "";
                                        data.valor6 = "";
                                        data.valor7 = "";
                                        data.valor8 = "";
                                        buscadosS.Add(data);
                                    }
                                }
                            }
                            break;
                        case "ArticuloActivoFijo":
                            valor = item.valor;
                            if(valor != "")
                            {
                                List<articulosBusquedaFijo> articulosActivoFijo = BusquedaArticuloFijo(valor);
                                if(articulosActivoFijo.Count() > 0)
                                {
                                    foreach(articulosBusquedaFijo i in articulosActivoFijo)
                                    {
                                        dataBusqueda data = new dataBusqueda();
                                        data.id = i.Id;
                                        data.valor1 = i.Id_ped + "";
                                        data.valor2 = i.Codigo;
                                        data.valor3 = i.Usuario;
                                        data.valor4 = i.Descripcion;
                                        data.valor5 = i.Grupo;
                                        data.valor6 = i.Valor + "";
                                        data.valor7 = i.CECO;
                                        data.valor8 = i.V_util + "";
                                        buscadosS.Add(data);
                                    }
                                }
                            }
                            
                            break;
                        case "ObjetoFijo":
                            valor = item.valor;
                            parametro = item.parametro;
                            if (valor != "")
                            {
                                List<objeto> Objetos = busquedaActivoDisponible(valor, parametro);
                                if(Objetos.Count() > 0)
                                {
                                    foreach(objeto i in Objetos)
                                    {
                                        dataBusqueda data = new dataBusqueda();
                                        data.id = i.id;
                                        data.valor1 = i.af;
                                        data.valor2 = i.imei;
                                        data.valor3 = i.descripcion;
                                        data.valor4 = "";
                                        data.valor5 = "";
                                        data.valor6 = "";
                                        data.valor7 = "";
                                        data.valor8 = "";
                                        buscadosS.Add(data);
                                    }
                                }
                            }
                            break;

                        case "ObjetoDevolutivo":
                            valor = item.valor;
                            parametro = item.parametro;
                            if(valor != "")
                            {
                                //busquedaDevolitvoDisponible
                                List<objeto> Objetos = busquedaDevolitvoDisponible(valor, parametro);
                                if (Objetos.Count() > 0)
                                {
                                    foreach (objeto i in Objetos)
                                    {
                                        dataBusqueda data = new dataBusqueda();
                                        data.id = i.id;
                                        data.valor1 = i.af;
                                        data.valor2 = i.imei;
                                        data.valor3 = i.descripcion;
                                        data.valor4 = "";
                                        data.valor5 = "";
                                        data.valor6 = "";
                                        data.valor7 = "";
                                        data.valor8 = "";
                                        buscadosS.Add(data);
                                    }
                                }
                            }
                            break;

                        case "BuscarEmpleado":
                            valor = item.valor;
                            if(valor != "")
                            {
                                List<empleado> Empleados = busquedaEmpleados(valor);
                                if(Empleados.Count() > 0)
                                {
                                    foreach(empleado i in Empleados)
                                    {
                                        dataBusqueda data = new dataBusqueda();
                                        data.id = i.id;
                                        data.valor1 = i.cedula_emp;
                                        data.valor2 = i.nombre;
                                        data.valor3 = i.snombre;
                                        data.valor4 = i.ppellido;
                                        data.valor5 = i.spellido;
                                        data.valor6 = "";
                                        data.valor7 = "";
                                        data.valor8 = "";
                                        buscadosS.Add(data);
                                    }
                                }
                            }
                            break;
                        case "BusquedaProvedor":
                            valor = item.valor;
                            if(valor != "")
                            {
                                List <busquedaProveedor> proveedors = busquedaProvedores(valor);
                                if(proveedors.Count() > 0)
                                {
                                    foreach(busquedaProveedor i in proveedors)
                                    {
                                        dataBusqueda data = new dataBusqueda();
                                        data.id = i.id;
                                        data.valor1 = i.nit;
                                        data.valor2 = i.razon_social;
                                        data.valor3 = i.contrato;
                                        data.valor4 = "";
                                        data.valor5 = "";
                                        data.valor6 = "";
                                        data.valor7 = "";
                                        data.valor8 = "";
                                        buscadosS.Add(data);
                                    }
                                }
                            }
                            break;
                        case "busquedaArticulo":
                            valor = item.valor;
                            if (valor != "")
                            {
                                List<articulosPedidos> articulos = busquedaArticulosComprasPedidos(valor);
                                if(articulos.Count() > 0)
                                {
                                    foreach(articulosPedidos i in articulos)
                                    {
                                        dataBusqueda data = new dataBusqueda();
                                        data.id = i.Id;
                                        data.valor1 = i.Codigo+"";
                                        data.valor2 = i.Descripcion;
                                        data.valor3 = i.und;
                                        data.valor4 = i.cuenta;
                                        data.valor5 = "";
                                        data.valor6 = "";
                                        data.valor7 = "";
                                        data.valor8 = "";
                                        buscadosS.Add(data);
                                    }
                                }
                            }
                            break;

                        case "busquedaArticuloArticuloFijo":
                            valor = item.valor;
                            
                            List<articulosPedidos> articulosAF = busquedaArticulosFijoPedido(valor);
                            if (articulosAF.Count() > 0)
                            {
                                foreach (articulosPedidos i in articulosAF)
                                {
                                    dataBusqueda data = new dataBusqueda();
                                    data.id = i.Id;
                                    data.valor1 = i.Codigo + "";
                                    data.valor2 = i.Descripcion;
                                    data.valor3 = i.und;
                                    data.valor4 = i.cuenta;
                                    data.valor5 = "";
                                    data.valor6 = "";
                                    data.valor7 = "";
                                    data.valor8 = "";
                                    buscadosS.Add(data);
                                }
                            }
                            break;

                    }
                }
                catch (Exception e)
                {
                    var st = new StackTrace();
                    var sf = st.GetFrame(1);
                    MethodBase site = e.TargetSite;
                    string methodName = site == null ? null : site.Name;
                    General.CrearLogError(sf.GetMethod().Name, "dataBusqueda", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
                }
                
            }
            return buscadosS;
        }

        //

        public List<articulosBusqueda> busquedaArticulosDevolutivos(string valor)
        {
            List<articulosBusqueda> articulosDevolucion = new List<articulosBusqueda>();
            string qwery = "";
            try
            {
                qwery = "SELECT id as Id, \n" +
                        "codigo as Codigo, \n" +
                        "descripcion as Descripcion, \n" +
                        "tipo as Tipo FROM articulos \n" +
                        "WHERE (codigo LIKE '%' + @valor + '%') \n" +
                        "or (descripcion LIKE '%' + @valor + '%') \n" +
                        "or (tipo LIKE '%' + @valor + '%') \n" +
                        "GROUP BY id,codigo,descripcion,tipo";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    using(SqlCommand cmd = new SqlCommand(qwery))
                    {
                        cmd.Connection = con;
                        SqlParameter vlr = new SqlParameter("@valor", SqlDbType.VarChar);
                        vlr.Value = valor;
                        cmd.Parameters.Add(vlr);
                        con.Open();
                        using(SqlDataReader srd = cmd.ExecuteReader())
                        {
                            while (srd.Read())
                            {
                                articulosDevolucion.Add(new articulosBusqueda
                                {
                                    Id          = Convert.ToInt32(srd["Id"]),
                                    Codigo      = srd["Codigo"]+"",
                                    Descripcion = srd["Descripcion"]+"",
                                    Tipo        = Convert.ToInt32(srd["Tipo"])
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
                General.CrearLogError(sf.GetMethod().Name, "articulos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return articulosDevolucion;
        }

        public List<articulosBusquedaFijo> BusquedaArticuloFijo(string valor)
        {
            List<articulosBusquedaFijo> articulosFijo = new List<articulosBusquedaFijo>();
            string qwery = "";
            try
            {
                qwery = " SELECT depreciacion.id as Id, \n" +
                        " depreciacion.id_pedido as Id_ped, \n"+
                        " depreciacion.cod_art as Codigo, \n"+
                        " pedidos.usuario as Usuario, \n"+
                        " depreciacion.descripcion as Descripcion, \n"+ 
                        " articulos_af.grupo as Grupo, \n" +
                        " depreciacion.valor as Valor, \n"+
                        " depreciacion.ccosto as CECO, \n"+
                        " depreciacion.v_util as V_util \n"+
                        " FROM depreciacion \n"+
                        " INNER JOIN  articulos_af on depreciacion.cod_art = articulos_af.codigo \n"+
                        " INNER JOIN  pedidos on depreciacion.id_pedido = pedidos.nro_pedido \n"+
                        " WHERE depreciacion.inventario = 0   and \n"+
                        " pedidoS.estado != 'CANCELADO' \n"+
                        " AND (depreciacion.id like '%' + @valor + '%' \n" +
                        " or depreciacion.id_pedido like'%' + @valor + '%' \n" +
                        " or depreciacion.cod_art like '%' + @valor + '%' \n" +
                        " or pedidos.usuario like '%' + @valor + '%' \n" +
                        " or depreciacion.descripcion like '%' + @valor + '%' \n" +
                        " or articulos_af.grupo like '%' + @valor + '%' \n" +
                        " or depreciacion.valor like '%' + @valor + '%' \n" +
                        " or depreciacion.ccosto like '%' + @valor + '%' \n" +
                        " or depreciacion.v_util like '%' + @valor + '%') ";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    using (SqlCommand cmd = new SqlCommand(qwery))
                    {
                        cmd.Connection = con;
                        SqlParameter vlr = new SqlParameter("@valor", SqlDbType.VarChar);
                        vlr.Value = valor;
                        cmd.Parameters.Add(vlr);
                        con.Open();
                        using (SqlDataReader srd = cmd.ExecuteReader())
                        {
                            while (srd.Read())
                            {
                                articulosFijo.Add(new articulosBusquedaFijo
                                {
                                    Id = Convert.ToInt32(srd["Id"]),
                                    Id_ped = Convert.ToInt32(srd["Id_ped"]),
                                    Codigo = srd["Codigo"]+"",
                                    Usuario = srd["Usuario"]+"",
                                    Descripcion = srd["Descripcion"]+"",
                                    Grupo = srd["Grupo"]+"",
                                    Valor = Convert.ToDouble(srd["Valor"]),
                                    CECO = srd["CECO"]+"",
                                    V_util = Convert.ToInt32(srd["V_util"])
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
                General.CrearLogError(sf.GetMethod().Name, "depreciacion", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }

            return articulosFijo;
        }
        public List<objeto> busquedaActivoDisponible(string valor, string parametro)
        {
            string param = parametro;
            int indexOf = parametro.Length;
            string Estado = parametro.Substring(indexOf-1);
            List<objeto> Objetos = new List<objeto>();
            Objetos = _context.objeto.Where(x => (x.af == valor 
                                            || x.imei == valor 
                                            || x.descripcion.Contains(valor))
                                            && x.estado == Convert.ToInt32(Estado) 
                                            && x.tipo_articulo == 2).ToList();
            return Objetos;
        }

        public List<objeto> busquedaDevolitvoDisponible(string valor, string parametro)
        {
            string param = parametro;
            int indexOf = parametro.Length;
            string Estado = parametro.Substring(indexOf - 1);
            List<objeto> Objetos = new List<objeto>();
            Objetos = _context.objeto.Where(x => (x.af == valor
                                            || x.imei == valor
                                            || x.descripcion.Contains(valor))
                                            && x.estado == Convert.ToInt32(Estado)
                                            && x.tipo_articulo == 1).ToList();
            return Objetos;
        }

        public List<empleado> busquedaEmpleados(string valor)
        {
            List<empleado> Empleados = new List<empleado>();
            try
            {
                Empleados = _context.empleado.Where(x => x.cedula_emp == valor
                                                    || x.nombre.Contains(valor)
                                                    || x.snombre.Contains(valor)
                                                    || x.ppellido.Contains(valor)
                                                    || x.spellido.Contains(valor)
                                                    && x.estado == 1).ToList();
                if(Empleados == null)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Empleados;
        }

        public List<busquedaProveedor> busquedaProvedores(string valor)
        {
            List<busquedaProveedor> proveedores = new List<busquedaProveedor>();
            string qwery = "";
            try
            {
                qwery = "select dp.id, p.nit, p.razon_social, dp.contrato from proveedorII p \n"+
                        " inner join detalle_proveedor dp on p.nit = dp.nit \n"+
                        "where p.razon_social like '%'+@valor+'%' or p.nit like '%'+@valor+'%' or dp.contrato like '%'+@valor+'%' \n" +
                        "and dp.estado = 1 and p.estado = 1";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    using (SqlCommand cmd = new SqlCommand(qwery))
                    {
                        cmd.Parameters.Add(new SqlParameter("@valor", System.Data.SqlDbType.VarChar));
                        cmd.Parameters["@valor"].Value = valor;
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader srd = cmd.ExecuteReader())
                        {
                            while (srd.Read())
                            {
                                proveedores.Add(new busquedaProveedor
                                {
                                    id = Convert.ToInt32(srd["id"]),
                                    nit = srd["nit"]+"",
                                    razon_social = srd["razon_social"]+"",
                                    contrato = srd["contrato"]+""
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
                General.CrearLogError(sf.GetMethod().Name, "proveedorII", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }

            return proveedores;
        }

        public List<articulosPedidos> busquedaArticulosComprasPedidos(string valor)
        {
            List<articulosPedidos> articulos = new List<articulosPedidos>();
            string query = "";
            try
            {
                query = "select id, codigo, descripcion, und, cuenta from compras_articulos \n"+
                        "where codigo like '%'+@valor+'%' or descripcion like '%'+@valor+'%' or und like '%'+@valor+'%' or cuenta like '%'+@valor+'%' and Estado = 1";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Parameters.Add(new SqlParameter("@valor", System.Data.SqlDbType.VarChar));
                        cmd.Parameters["@valor"].Value = valor;
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader srd = cmd.ExecuteReader())
                        {
                            while (srd.Read())
                            {
                                articulos.Add(new articulosPedidos
                                {
                                    Id = Convert.ToInt32(srd["id"]),
                                    Codigo = Convert.ToInt32(srd["codigo"]),
                                    Descripcion = srd["descripcion"]+"",
                                    und = srd["und"]+"",
                                    cuenta = srd["cuenta"]+""
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
                General.CrearLogError(sf.GetMethod().Name, "compras_articulos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return articulos;
        }

        public List<articulosPedidos> busquedaArticulosFijoPedido(string valor)
        {
            List<articulosPedidos> articulos = new List<articulosPedidos>();
            string query = "";
            try
            {
                query = "select id, codigo, descripcion, und, cuenta from articulos_af \n"+
                        "where codigo like '%'+@valor+'%' or descripcion like '%'+@valor+'%' or und like '%'+@valor+'%' or cuenta like '%'+@valor+'%' and estado = 1";

                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Parameters.Add(new SqlParameter("@valor", System.Data.SqlDbType.VarChar));
                        cmd.Parameters["@valor"].Value = valor;
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader srd = cmd.ExecuteReader())
                        {
                            while (srd.Read())
                            {
                                articulos.Add(new articulosPedidos
                                {
                                    Id = Convert.ToInt32(srd["id"]),
                                    Codigo = Convert.ToInt32(srd["codigo"]),
                                    Descripcion = srd["descripcion"] + "",
                                    und = srd["und"] + "",
                                    cuenta = srd["cuenta"] + ""
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
                General.CrearLogError(sf.GetMethod().Name, "articulos_af", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return articulos;
        }


    }
}
