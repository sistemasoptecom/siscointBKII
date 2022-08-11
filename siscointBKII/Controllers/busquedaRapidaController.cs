using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class busquedaRapidaController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        public busquedaRapidaController(AplicationDbContext context)
        {
            _context = context;
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
            foreach(busquedaRapidaModel item in abusqueda)
            {
                var busqueda = new Object();
                try
                {
                    switch (item.entidad)
                    {
                        case "area_ccosto":
                            List<area_ccosto> area_ccostos = new List<area_ccosto>();
                            string valor = item.valor;
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

                                    buscadosS.Add(data);
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
    }
}
