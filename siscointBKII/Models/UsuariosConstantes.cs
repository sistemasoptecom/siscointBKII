using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Models
{
    
    public class UsuariosConstantes
    {
       
        public static List<UsuariosModel> user = new List<UsuariosModel>()
        {
            new UsuariosModel(){id=1, codigo="1045687883", nombre_usuario = "Antonio Jose Linero Florez", username = "alinero", password = "aajjllff", id_tipo_usuario = 1, estado = 1, cargo = "", area="", modulo = 1},
            new UsuariosModel(){id=2, codigo="1002022028", nombre_usuario = "Juan Jose Perez", username = "aperez", password = "123456", id_tipo_usuario = 1002, estado = 1, cargo = "", area="", modulo = 1},
        };

        public static List<UsuariosModel> userModel = new List<UsuariosModel>()
        {
            new UsuariosModel()
        };

       
    }
}
