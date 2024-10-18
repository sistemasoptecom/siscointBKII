using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using siscointBKII.ModelosQ;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace siscointBKII
{
    public class General
    {

        private static string cadena_conexion_1 = "Data Source=sql5108.site4now.net;Initial Catalog=DB_A642ED_prueba; user id=DB_A642ED_prueba_admin;password=S1st3m##C4cH0n;MultipleActiveResultSets=True;";
        private static string cadena_conexion_2 = "Data Source=AF1002522FTTHBG\\SQLEXPRESS;Initial Catalog=SISCOINT_PRUEBAS_II;Integrated Security=True;";
        //public static string EncriptarPassword(string cadenaNombre, string password)
        //{
        //    byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(cadenaNombre);
        //    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        //    passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
        //    byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
        //    string encryptedResult = Convert.ToBase64String(bytesEncrypted);
        //    return encryptedResult;
        //}

        //public static string DesencriptarPassword(string passwordEncriptado, string password)
        //{
        //    byte[] bytesToBeDecrypted = Convert.FromBase64String(passwordEncriptado);
        //    byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes(password);

        //    passwordBytesdecrypt = SHA256.Create().ComputeHash(passwordBytesdecrypt);
        //    byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytesdecrypt);
        //    string decryptedResult = Encoding.UTF8.GetString(bytesDecrypted);
        //    return decryptedResult;
        //}

        //public static string EncriptarPassword_V2(string password)
        //{
        //    string passEncriptado = "";

        //    try
        //    {
        //        using(RijndaelManaged myRijndael = new RijndaelManaged())
        //        {
        //            myRijndael.GenerateKey();
        //            myRijndael.GenerateIV();
        //            byte[] encryptado = EncriptarStringToBytes(password, myRijndael.Key, myRijndael.IV);
        //            passEncriptado = Convert.ToBase64String(encryptado);
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        //aqui en catch
        //        passEncriptado = e.Message;
        //    }
        //    return passEncriptado;
        //}

        //public static string DesencriptarPassword_V2(string passEncript)
        //{
        //    string Encrip_pass = "";
        //    try
        //    {
        //        using (RijndaelManaged myRijndael = new RijndaelManaged())
        //        {
        //            myRijndael.GenerateKey();
        //            myRijndael.GenerateIV();


        //            byte[] encrypted = Convert.FromBase64String(passEncript);
        //            Encrip_pass = DesencriptarStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        Encrip_pass = e.Message;
        //    }
        //    return Encrip_pass;
        //}

        //private static byte[] EncriptarStringToBytes(string texto, byte[] Key, byte[] IV)
        //{
        //    if (texto == null || texto.Length <= 0)
        //        throw new ArgumentNullException("PassWord");
        //    if (Key == null || Key.Length <= 0)
        //        throw new ArgumentNullException("Key");
        //    if (IV == null || IV.Length <= 0)
        //        throw new ArgumentNullException("IV");
        //    byte[] encrypted;

        //    using (RijndaelManaged rijAlg = new RijndaelManaged())
        //    {
        //        rijAlg.Key = Key;
        //        rijAlg.IV = IV;

        //        //rijAlg.Padding = PaddingMode.None;
        //        ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
        //        using (MemoryStream msEncrypt = new MemoryStream())
        //        {
        //            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        //            {
        //                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        //                {
        //                    swEncrypt.Write(texto);
        //                }
        //                encrypted = msEncrypt.ToArray();
        //            }
        //        }
        //    }

        //    return encrypted;
        //}

        //private static string DesencriptarStringFromBytes(byte[] textByte, byte[] key, byte[] IV)
        //{
        //    if (textByte == null || textByte.Length <= 0)
        //        throw new ArgumentNullException("cipherText");
        //    if (key == null || key.Length <= 0)
        //        throw new ArgumentNullException("Key");
        //    if (IV == null || IV.Length <= 0)
        //        throw new ArgumentNullException("IV");

        //    string plaintext = null;
        //    using (RijndaelManaged rijAlg = new RijndaelManaged())
        //    {
        //        rijAlg.Key = key;
        //        rijAlg.IV = IV;
        //        //rijAlg.Padding = PaddingMode.Zeros;
        //        //rijAlg.Mode = CipherMode.CFB;


        //        //rijAlg.Padding = PaddingMode.Zeros;
        //        ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
        //        using (MemoryStream msDecrypt = new MemoryStream(textByte))
        //        {
        //            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
        //                {
        //                    plaintext = srDecrypt.ReadToEnd();
        //                }
        //            }
        //        }
        //    }
        //    return plaintext;
        //}

        //private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        //{
        //    byte[] encryptedBytes = null;

        //    // Set your salt here, change it to meet your flavor:
        //    // The salt bytes must be at least 8 bytes.
        //    //byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };
        //    byte[] saltBytes = new byte[8];


        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (RijndaelManaged AES = new RijndaelManaged())
        //        {
        //            AES.KeySize = 256;
        //            AES.BlockSize = 128;
        //            RandomNumberGenerator.Create().GetBytes(saltBytes);
        //            //saltBytes = GenerateRandomSalt();
        //            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
        //            AES.Key = key.GetBytes(AES.KeySize / 8);
        //            AES.IV = key.GetBytes(AES.BlockSize / 8);
        //            //AES.Padding = PaddingMode.Zeros;
        //            //AES.Mode = CipherMode.CBC;

        //            using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
        //                cs.Close();
        //            }
        //            encryptedBytes = ms.ToArray();
        //        }
        //    }

        //    return encryptedBytes;
        //}

        //private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        //{
        //    byte[] decryptedBytes = null;

        //    // Set your salt here, change it to meet your flavor:
        //    // The salt bytes must be at least 8 bytes.
        //    //byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };
        //    //byte[] saltBytes = new byte[] { 1,2,3,4,5,6,7,8 };
        //    byte[] saltBytes = new byte[8];


        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (RijndaelManaged AES = new RijndaelManaged())
        //        {
        //            AES.KeySize = 256;
        //            AES.BlockSize = 128;
        //            RandomNumberGenerator.Create().GetBytes(saltBytes);
        //            //saltBytes = GenerateRandomSalt();
        //            var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
        //            AES.Key = key.GetBytes(AES.KeySize / 8);
        //            AES.IV = key.GetBytes(AES.BlockSize / 8);
        //            AES.Mode = CipherMode.CBC;
        //            //AES.Padding = PaddingMode.Zeros;
        //            //AES.Padding = PaddingMode.PKCS7;
        //            //AES.Mode = CipherMode.CBC;

        //            using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
        //            {
        //                cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
        //                cs.Close();
        //            }
        //            decryptedBytes = ms.ToArray();
        //        }
        //    }

        //    return decryptedBytes;
        //}

        //private static byte[] GenerateRandomSalt()
        //{
        //    byte[] data = new byte[8];

        //    using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        //    {
        //        for (int i = 0; i < 10; i++)
        //        {
        //            // Fill the buffer with the generated data
        //            rng.GetBytes(data);
        //        }
        //    }
        //    return data;
        //}


        public static string cifrarTextoAES(string textoCifrar, string palabraPaso,
            string valorRGBSalt, string algoritmoEncriptacionHASH,
            int iteraciones, string vectorInicial, int tamanoClave)
        {
            try
            {
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(vectorInicial);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(valorRGBSalt);
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(textoCifrar);

                PasswordDeriveBytes password =
                    new PasswordDeriveBytes(palabraPaso, saltValueBytes,
                        algoritmoEncriptacionHASH, iteraciones);

                byte[] keyBytes = password.GetBytes(tamanoClave / 8);

                RijndaelManaged symmetricKey = new RijndaelManaged();

                symmetricKey.Mode = CipherMode.CBC;

                ICryptoTransform encryptor =
                    symmetricKey.CreateEncryptor(keyBytes, InitialVectorBytes);

                MemoryStream memoryStream = new MemoryStream();

                CryptoStream cryptoStream =
                    new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);

                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);

                cryptoStream.FlushFinalBlock();

                byte[] cipherTextBytes = memoryStream.ToArray();

                memoryStream.Close();
                cryptoStream.Close();

                string textoCifradoFinal = Convert.ToBase64String(cipherTextBytes);

                return textoCifradoFinal;
            }
            catch
            {
                return null;
            }
        }


        public static string descifrarTextoAES(string textoCifrado, string palabraPaso,
           string valorRGBSalt, string algoritmoEncriptacionHASH,
           int iteraciones, string vectorInicial, int tamanoClave)
        {
            try
            {
                byte[] InitialVectorBytes = Encoding.ASCII.GetBytes(vectorInicial);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(valorRGBSalt);

                byte[] cipherTextBytes = Convert.FromBase64String(textoCifrado);

                PasswordDeriveBytes password =
                    new PasswordDeriveBytes(palabraPaso, saltValueBytes,
                        algoritmoEncriptacionHASH, iteraciones);

                byte[] keyBytes = password.GetBytes(tamanoClave / 8);

                RijndaelManaged symmetricKey = new RijndaelManaged();

                symmetricKey.Mode = CipherMode.CBC;

                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, InitialVectorBytes);

                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);

                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);

                byte[] plainTextBytes = new byte[cipherTextBytes.Length];

                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                memoryStream.Close();
                cryptoStream.Close();

                string textoDescifradoFinal = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);

                return textoDescifradoFinal;
            }
            catch
            {
                return null;
            }
        }


        public static string Decryption(string srtText)
        {
            //MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCPP+NlfOS/cYdJ1dkbg3EMGY/8JJgl2Op89RNUIB6zJ8O3vD1dwmR4f/zIYx9tOOMgMxm3LmlhoF2LoYuC0mUuPcnXbgY2VPVYWC73DE82Ejn31YDGz79K9ufmPiyT6Sxnx6V0PQFJIQf1SMQaSoaKdUe9BSIn0ODKC1XiBJBefwIDAQAB
            var publicKey = "<RSAKeyValue><Modulus>/MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCPP+NlfOS/cYdJ1dkbg3EMGY/8JJgl2Op89RNUIB6zJ8O3vD1dwmR4f/zIYx9tOOMgMxm3LmlhoF2LoYuC0mUuPcnXbgY2VPVYWC73DE82Ejn31YDGz79K9ufmPiyT6Sxnx6V0PQFJIQf1SMQaSoaKdUe9BSIn0ODKC1XiBJBefwIDAQAB</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            //var publicKey = "<RSAKeyValue><Modulus>21wEnTU+mcD2w0Lfo1Gv4rtcSWsQJQTNa6gio05AOkV/Er9w3Y13Ddo5wGtjJ19402S71HUeN0vbKILLJdRSES5MHSdJPSVrOqdrll/vLXxDxWs/U0UT1c8u6k/Ogx9hTtZxYwoeYqdhDblof3E75d9n2F0Zvf6iTb4cI7j6fMs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            var testData = Encoding.UTF8.GetBytes(srtText);
            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    var base64Encrypted = srtText;
                    rsa.FromXmlString(publicKey);
                    var resultBytes = Convert.FromBase64String(base64Encrypted);
                    var decryptedBytes = rsa.Decrypt(resultBytes, true);
                    var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                    return decryptedData.ToString();
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }

        public static string DecryptionV2(string textExcrypt)
        {
            string decryptedDataS = "";
            try 
            {
                ASCIIEncoding ByteConverter = new ASCIIEncoding();

                string dataString = textExcrypt;
                byte[] dataToEncrypt = ByteConverter.GetBytes(dataString);
                byte[] encryptedData;
                byte[] decryptedData;

                RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();
                encryptedData = RSAalg.Encrypt(dataToEncrypt, false);
                decryptedData = RSAalg.Decrypt(encryptedData, false);
                decryptedDataS = ByteConverter.GetString(decryptedData);
            }
            catch(CryptographicException e)
            {
                string excepMsj = e.Message;
            }

            
            return decryptedDataS;
        }



        public static void CrearLogError(string tipo, string entidad, string mensaje, string source, string stacktrace, string targetsite, string conexion)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into log_error (entidad, tipo, mensaje,source,stacktrace,targetsite, FechaCreacion) values(@entidad, @tipo, @mensaje, @source, @stacktrace, @targetsite ,@fechacreacion)";
                    command.Parameters.AddWithValue("@entidad", entidad);
                    command.Parameters.AddWithValue("@tipo", tipo);
                    command.Parameters.AddWithValue("@mensaje", mensaje);
                    command.Parameters.AddWithValue("@source", source);
                    command.Parameters.AddWithValue("@stacktrace", stacktrace);
                    command.Parameters.AddWithValue("@targetsite", targetsite);
                    command.Parameters.AddWithValue("@fechacreacion", DateTime.Now);

                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }



        public static void EjecutarProcedimientoAlmacenado_validatePresupuesto(string nombre_procedimiento, string conexion, string parametro)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                connection.Open();
                SqlCommand sql_cmnd = new SqlCommand(nombre_procedimiento, connection);
                sql_cmnd.CommandType = CommandType.StoredProcedure;
                sql_cmnd.Parameters.AddWithValue("@nro_ped", SqlDbType.NVarChar).Value = parametro;
                sql_cmnd.ExecuteNonQuery();
                connection.Close();
            }
        }

        public static void EjecutarProcedimientoAlmacenado_validateAFDiff(string nombre_procedimiento, string conexion, string numero_pedido, int cantidad, string cod_articulo)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                connection.Open();
                SqlCommand sql_cmnd = new SqlCommand(nombre_procedimiento, connection);
                sql_cmnd.CommandType = CommandType.StoredProcedure;
                sql_cmnd.Parameters.AddWithValue("@nro_ped", SqlDbType.NVarChar).Value = numero_pedido;
                sql_cmnd.Parameters.AddWithValue("@cantidadPedido", SqlDbType.Int).Value = cantidad;
                sql_cmnd.Parameters.AddWithValue("@codi_articulo", SqlDbType.NVarChar).Value = cod_articulo;
                connection.Close();
            }
        }

        public static async Task<string> crearEmpleadosV2(string cedula, string nombres, string cargo, string nombre_empresa ,  string contrato, string conexion)
        {
            string msj = "";
            string[] result = nombres.Split(" ");
            string primer_nombre = "";
            string segundo_nombre = "";
            string primer_apellido = "";
            string segundo_apellido = "";
            if(result.Length >= 4)
            {
               primer_nombre = result[0];
               segundo_nombre = result[1];
               primer_apellido = result[2];
               segundo_apellido = result[3];
            }
            else
            {
                primer_nombre = result[0];
                
                primer_apellido = result[1];
                segundo_apellido = result[2];
            }
            //valido el nombre de la empresa
            Int32 codigo_empresa = get_codigo_empresa(nombre_empresa,conexion);

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into empleado (cedula_emp,nombre,snombre,ppellido,spellido,area,cargo,estado,permiso,ccosto,empresa,correo) values(@cedula_emp,@nombre,@snombre,@ppellido,@spellido,@area,@cargo,@estado,@permiso,@ccosto,@empresa,@correo)";
                    command.Parameters.AddWithValue("@cedula_emp", cedula);
                    command.Parameters.AddWithValue("@nombre", primer_nombre);
                    command.Parameters.AddWithValue("@snombre", segundo_nombre);
                    command.Parameters.AddWithValue("@ppellido", primer_apellido);
                    command.Parameters.AddWithValue("@spellido", segundo_apellido);
                    command.Parameters.AddWithValue("@area", cargo);
                    command.Parameters.AddWithValue("@cargo", cargo);
                    command.Parameters.AddWithValue("@estado", 1);
                    command.Parameters.AddWithValue("@permiso", 0);
                    command.Parameters.AddWithValue("@ccosto", 0);
                    command.Parameters.AddWithValue("@empresa", codigo_empresa);
                    command.Parameters.AddWithValue("@correo", "");
                    try
                    {
                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return  msj;
        }

        public static async Task<string> crearEmpleadosV3(string cedula, 
                                                          string nombres, 
                                                          string area,
                                                          string cargo, 
                                                          
                                                          string contrato,
                                                          Int32 ccosto,
                                                          Int32 cod_empresa,
                                                          string correo,
                                                          
                                                          string conexion)
        {
            string msj = "";
            string[] result = nombres.Split(" ");
            string primer_nombre = "";
            string segundo_nombre = "";
            string primer_apellido = "";
            string segundo_apellido = "";
            if (result.Length >= 4)
            {
                primer_nombre = result[0];
                segundo_nombre = result[1];
                primer_apellido = result[2];
                segundo_apellido = result[3];
            }
            else if(result.Length == 3)
            {
                primer_nombre = result[0];

                primer_apellido = result[1];
                segundo_apellido = result[2];
            }else if(result.Length == 2)
            {
                primer_nombre = result[0];
                primer_apellido = result[1];
            }
            //valido el nombre de la empresa
            //Int32 codigo_empresa = get_codigo_empresa(nombre_empresa, conexion);

            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into empleado (cedula_emp," +
                                                                "nombre," +
                                                                "snombre," +
                                                                "ppellido," +
                                                                "spellido," +
                                                                "area," +
                                                                "cargo," +
                                                                "contrato,"+
                                                                "estado," +
                                                                "permiso," +
                                                                "ccosto," +
                                                                "empresa," +
                                                                "correo) " +
                                                                "values(" +
                                                                "@cedula_emp," +
                                                                "@nombre," +
                                                                "@snombre," +
                                                                "@ppellido," +
                                                                "@spellido," +
                                                                "@area," +
                                                                "@cargo," +
                                                                "@contrato,"+
                                                                "@estado," +
                                                                "@permiso," +
                                                                "@ccosto," +
                                                                "@empresa," +
                                                                "@correo)";
                    command.Parameters.AddWithValue("@cedula_emp", cedula);
                    command.Parameters.AddWithValue("@nombre", primer_nombre);
                    command.Parameters.AddWithValue("@snombre", segundo_nombre);
                    command.Parameters.AddWithValue("@ppellido", primer_apellido);
                    command.Parameters.AddWithValue("@spellido", segundo_apellido);
                    command.Parameters.AddWithValue("@area", area);
                    command.Parameters.AddWithValue("@cargo", cargo);
                    command.Parameters.AddWithValue("@contrato", contrato);
                    command.Parameters.AddWithValue("@estado", 1);
                    command.Parameters.AddWithValue("@permiso", 0);
                    command.Parameters.AddWithValue("@ccosto", ccosto);
                    command.Parameters.AddWithValue("@empresa", cod_empresa);
                    command.Parameters.AddWithValue("@correo", correo);
                    try
                    {
                        connection.Open();
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return msj;
        }

        public static int get_codigo_empresa(string nombre_empresa, string conexion)
        {
            int codigo_empresa = 0;
            string[] empresa_nom = nombre_empresa.Split(" ");
            string nombre_1 = empresa_nom[0];
            string query = "select isnull(id,2) as codigo_empresa from empresa where nombre like '%"+ nombre_1 + "%'";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader srd = cmd.ExecuteReader())
                    {
                        while (srd.Read()) 
                        {
                            codigo_empresa = Convert.ToInt32(srd["codigo_empresa"]);
                        }
                    }
                    con.Close();
                }
            }
            return codigo_empresa;
        }

        public static void CrearEmpleados(string cedula,
                                          string primer_nombre,
                                          string segundo_nombre,
                                          string primer_apellido,
                                          string segundo_apellido,
                                          string cargo,
                                          string conexion)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into empleado (cedula_emp,nombre,snombre,ppellido,spellido,area,cargo,estado,permiso,ccosto,empresa,correo) values(@cedula_emp,@nombre,@snombre,@ppellido,@spellido,@area,@cargo,@estado,@permiso,@ccosto,@empresa,@correo)";
                    command.Parameters.AddWithValue("@cedula_emp", cedula);
                    command.Parameters.AddWithValue("@nombre", primer_nombre);
                    command.Parameters.AddWithValue("@snombre", segundo_nombre);
                    command.Parameters.AddWithValue("@ppellido", primer_apellido);
                    command.Parameters.AddWithValue("@spellido", segundo_apellido);
                    command.Parameters.AddWithValue("@area", "");
                    command.Parameters.AddWithValue("@cargo", cargo);
                    command.Parameters.AddWithValue("@estado", 1);
                    command.Parameters.AddWithValue("@permiso", 0);
                    command.Parameters.AddWithValue("@ccosto", 0);
                    command.Parameters.AddWithValue("@empresa", 2);
                    command.Parameters.AddWithValue("@correo", "");
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        public static void crearPeriodo(string periodo, string conexion)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into liq_periodo_comision_v2 (periodo, estado, fecha_creacion, EsPublicado, EsCerrado) values(@periodo, @estado, @fecha_creacion, @EsPublicado, @EsCerrado)";
                    command.Parameters.AddWithValue("@periodo", periodo);
                    command.Parameters.AddWithValue("@estado", 1);
                    command.Parameters.AddWithValue("@fecha_creacion", DateTime.Now);
                    command.Parameters.AddWithValue("@EsPublicado", 0);
                    command.Parameters.AddWithValue("@EsCerrado", 0);
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        public static void crearLoteImporte(Int64 consecutivo_lote, string tipo_importe, string ruta, string usuario, string conexion)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into lote_importe (consecutivo_lote, tipo_importe, ruta_archivo,usuario,estado,fecha_creacion) values(@consecutivo_lote, @tipo_importe, @ruta,@usuario,@estado, @fecha_creacion)";
                    command.Parameters.AddWithValue("@consecutivo_lote", consecutivo_lote);
                    command.Parameters.AddWithValue("@tipo_importe", tipo_importe);
                    command.Parameters.AddWithValue("@ruta", ruta);
                    command.Parameters.AddWithValue("@usuario", usuario);
                    command.Parameters.AddWithValue("@estado", 1);
                    command.Parameters.AddWithValue("@fecha_creacion", DateTime.Now);
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        public static void recalcular_subtotales(string cedula_asesor, string periodo, string conexion)
        {
            string query = "";
            string query_2 = "";
            double valor_total = 0;
            query = "select isnull(sum((total_valor_mega_1+" +
                                      " total_valor_mega_2+ " +
                                      " total_valor_mega_3+" +
                                      " total_valor_mega_4+" +
                                      " total_valor_mega_5+" +
                                      " total_valor_mega_6+" +
                                      " total_valor_duos +" +
                                      " total_valor_naked +" +
                                      " total_valor_trios +" +
                                      " total_migracion + " +
                                      " total_plan_movil +" +
                                      " total_valor_preferencial +" +
                                      " total_valor_dedicado +" +
                                      " total_venta_base +" +
                                      " total_venta_c2c )" +
                                      
                                      " -" +
                                      " (total_nunca_pago_movil)+ " +
                                      " (total_otros_conceptos)),0) as total " +
                                      " from liq_comision_asesor " +
                                      " where cedula_asesor = '"+ cedula_asesor + "' and periodo = '"+ periodo + "' and estado = 1";

            using (SqlConnection con = new SqlConnection(conexion))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            valor_total = Convert.ToDouble(sdr["total"]);
                        }
                    }
                    con.Close();
                }
            }
            //ahora despues del calculo actualizamos
            query_2 = "update liq_comision_asesor set " +
                      "sub_total_comision = @valor_total ," +
                      "total_comision = @valor_total " +
                      "where cedula_asesor = @cedula_asesor " +
                      "and periodo = @periodo and estado = 1";
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = query_2;
                    command.Parameters.AddWithValue("@valor_total", valor_total);
                    command.Parameters.AddWithValue("@cedula_asesor", cedula_asesor);
                    command.Parameters.AddWithValue("@periodo", periodo);
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
        public static void crearDataValidoProceso(string proceso, Int32 valor,Int32 consecutivo_lote,string nombre_tabla, string usuario, string conexion)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into data_valido_proceso (proceso,cantidad,estado,usuario,fecha_inicio_proceso,consecutivo_lote,nombre_tabla) values(@proceso,@cantidad,@estado,@usuario,@fecha_inicio_proceso,@consecutivo_lote,@nombre_tabla)";
                    command.Parameters.AddWithValue("@proceso", proceso);
                    command.Parameters.AddWithValue("@cantidad", valor);
                    command.Parameters.AddWithValue("@estado", 0);
                    command.Parameters.AddWithValue("@usuario", usuario);
                    command.Parameters.AddWithValue("@fecha_inicio_proceso", DateTime.Now);
                    command.Parameters.AddWithValue("@consecutivo_lote", consecutivo_lote);
                    command.Parameters.AddWithValue("@nombre_tabla", nombre_tabla);
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public static void crearImprimeMensajeLog(string mensaje, string proceso, string conexion)
        {
            using (SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into data_imprime_mensaje (mensaje,proceso,fecha,estado) values(@mensaje,@proceso,@fecha,@estado)";
                    command.Parameters.AddWithValue("@mensaje", mensaje);
                    command.Parameters.AddWithValue("@proceso", proceso);
                    command.Parameters.AddWithValue("@fecha", DateTime.Now);
                    command.Parameters.AddWithValue("@estado", 1);
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {

                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

        public static Boolean enviarCorreo(string subjectMensaje , string cuerpoMensaje, string correo_saliente, string cadena_conexion)
        {
            Boolean Envio = false;
            string outMss = "";
            //var fromAddress = new MailAddress("antoniojlfz2010@gmail.com");
            //var toAddres = new MailAddress("alinero@optecom.com.co");
            string correo_entrante = getParametroVariable("correo_entrante", cadena_conexion);
            string contrasena_entrante = getParametroVariable("contrasena_entrante", cadena_conexion);
            string servidor_entrante = getParametroVariable("servidor_entrante", cadena_conexion);
            string puerto_entrante = getParametroVariable("puerto_entrante", cadena_conexion);
            string fromAddress = correo_entrante;
            string toAddres = correo_saliente;
            string fromPassword = contrasena_entrante;
            //string subjetc = subjectMensaje;
            //string body = cuerpoMensaje;

            var smtp = new SmtpClient
            {
                Host = servidor_entrante,
                Port = Convert.ToInt32(puerto_entrante),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword)
            };

            //using (var message = new MailMessage(fromAddress, toAddres)
            //{
            //    Subject = subjetc,
            //    Body = body
            //})
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromAddress);
            mail.Sender = new MailAddress(toAddres);
            mail.To.Add(toAddres);
            mail.IsBodyHtml = true;
            mail.Subject = subjectMensaje;
            mail.Body = cuerpoMensaje;
            try{
                smtp.Send(mail);
                Envio = true;
            }
            catch (SmtpException e)
            {
                outMss = e.Message;
            }
            return Envio;
        }

        public static Boolean EnviarCorreoAuth(string subjectMensaje, string cuerpoMensaje, string correo_saliente, string cadena_conexion, out string outMss)
        {
            outMss = "";
            Boolean Envio = false;
            string correo_entrante = getParametroVariable("correo_envios_login", cadena_conexion);
            string contrasena_entrante = getParametroVariable("contrasena_envios_login", cadena_conexion);
            string servidor_entrante = getParametroVariable("servidor_entrante_login", cadena_conexion);
            string puerto_entrante = getParametroVariable("puerto_entrante_login", cadena_conexion);
            string fromAddress = correo_entrante;
            string toAddres = correo_saliente;
            string fromPassword = contrasena_entrante;
            var smtp = new SmtpClient
            {
                Host = servidor_entrante,
                Port = Convert.ToInt32(puerto_entrante),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword)
            };

            //using (var message = new MailMessage(fromAddress, toAddres)
            //{
            //    Subject = subjetc,
            //    Body = body
            //})
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromAddress);
            mail.Sender = new MailAddress(toAddres);
            mail.To.Add(toAddres);
            mail.IsBodyHtml = true;
            mail.Subject = subjectMensaje;
            mail.Body = cuerpoMensaje;
            try
            {
                smtp.Send(mail);
                Envio = true;
            }
            catch (SmtpException e)
            {
                outMss = e.Message;
            }
            return Envio;
            
        }

        public static string getNombreCompletoEmpleado(string cedula_empleado, string conexion)
        {
            string nombre_completo = "";
            string query = "select concat(e.nombre,' ',e.snombre,' ',e.ppellido,' ',e.spellido) as nombreCompleto " +
                           " from empleado e where e.cedula_emp = '"+cedula_empleado+"'";


            using(SqlConnection con = new SqlConnection(conexion))
            {
                using(SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using(SqlDataReader srd = cmd.ExecuteReader())
                    {
                        while(srd.Read())
                        {
                            nombre_completo = srd["nombreCompleto"] + "";
                        }
                    }
                    con.Close();
                }
            }
            return nombre_completo;
        }
        public static string getNombreTipoEsquema(Int32 codigo_tipo_esquema, string conexion)
        {
            string nombre_tipo_esquema = "";
            string query = "select esquema from liq_tipo_esquema where codigo_valor = '"+codigo_tipo_esquema+"'";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader srd = cmd.ExecuteReader())
                    {
                        while (srd.Read())
                        {
                            nombre_tipo_esquema = srd["esquema"] + "";
                        }
                    }
                    con.Close();
                }
            }
            return nombre_tipo_esquema;
        }

        public static List<listar_tmp_solicitud_np> ListarPendientesNpgroup(string periodo, string tipo_operacion ,string conexion)
        {
            
            List<listar_tmp_solicitud_np> _listar_pendientes_nunca_pagos_g = new List<listar_tmp_solicitud_np>();
          
            string query = "select count(*) as total, cedula_asesor, periodo_np from liq_tmp_solicitud_np where periodo_cm = '" + periodo + "' group by cedula_asesor, periodo_np";
            using (SqlConnection con = new SqlConnection(conexion))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            _listar_pendientes_nunca_pagos_g.Add(new listar_tmp_solicitud_np
                            {
                                TOTAL = Convert.ToInt32(sdr["total"]),
                                CEDULA_ASESOR = sdr["cedula_asesor"] + "",
                                PERIODO_NP = sdr["periodo_np"] + ""
                            });
                        }
                    }
                    con.Close();
                }
            }
            return _listar_pendientes_nunca_pagos_g;
        }

        public static string getParametroVariable(string nombre_parametro, string cadena_conexion)
        {
            string valor_parametro = "";
            string query = "select valor_variable from variable where codigo_variable = '"+ nombre_parametro + "'";
            using (SqlConnection con = new SqlConnection(cadena_conexion))
            {
                using (SqlCommand cmd = new SqlCommand(query))
                {
                    cmd.Connection = con;
                    con.Open();
                    using (SqlDataReader srd = cmd.ExecuteReader())
                    {
                        while (srd.Read())
                        {
                            valor_parametro = srd["valor_variable"] + "";
                        }
                    }
                    con.Close();
                }
            }
            return valor_parametro;
        }

        public static void recalcular_saldos(string periodo, string conexion)
        {
            SqlConnection conex = null;
            SqlDataReader rd = null;
            try
            {
                conex = new SqlConnection(conexion);
                conex.Open();
                SqlCommand cmd = new SqlCommand("dbo.recalcular_saldos_comision", conex);
                cmd.Parameters.AddWithValue("@periodo", periodo);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                rd = cmd.ExecuteReader();
            }
            finally
            {
                if (conex != null)
                {
                    conex.Close();
                }
                else
                {
                    conex.Close();
                }
            }
        }

    }

    
}
