using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace siscointBKII
{
    public class General
    {
        
       
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

        public static void CrearLogError(string tipo, string entidad, string mensaje, string conexion)
        {
            using(SqlConnection connection = new SqlConnection(conexion))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "insert into log_error (entidad, tipo, mensaje, FechaCreacion) values(@entidad, @tipo, @mensaje, @fechacreacion)";
                    command.Parameters.AddWithValue("@entidad", entidad);
                    command.Parameters.AddWithValue("@tipo", tipo);
                    command.Parameters.AddWithValue("@mensaje", mensaje);
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

       
    }
}
