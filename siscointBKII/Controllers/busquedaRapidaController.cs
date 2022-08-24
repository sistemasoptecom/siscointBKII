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
            }
            return Ok(buscados);
        }


        private  List<dataBusqueda> searchBusquedaRapida(List<busquedaRapidaModel> abusqueda)
        {
            List<dataBusqueda> buscadosS = new List<dataBusqueda>();
            string valor = "";
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
                            if (valor != "")
                            {
                                List<objeto> Objetos = busquedaActivoDisponible(valor);
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

                    }
                }
                catch (Exception e)
                {

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

            }

            return articulosFijo;
        }
        public List<objeto> busquedaActivoDisponible(string valor)
        {
            List<objeto> Objetos = new List<objeto>();
            Objetos = _context.objeto.Where(x => x.af == valor 
                                            || x.imei == valor 
                                            || x.descripcion.Contains(valor) 
                                            && x.estado == 1 
                                            && x.tipo_articulo == 2).ToList();
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

            }
            return Empleados;
        }
    }
}
