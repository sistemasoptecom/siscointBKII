﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace siscointBKII
{
    public class General
    {
        public static string EncriptarPassword(string cadenaNombre, string password)
        {
            byte[] bytesToBeEncrypted = Encoding.UTF8.GetBytes(cadenaNombre);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesEncrypted = AES_Encrypt(bytesToBeEncrypted, passwordBytes);
            string encryptedResult = Convert.ToBase64String(bytesEncrypted);
            return encryptedResult;
        }

        public static string DesencriptarPassword(string passwordEncriptado, string password)
        {
            byte[] bytesToBeDecrypted = Convert.FromBase64String(passwordEncriptado);
            byte[] passwordBytesdecrypt = Encoding.UTF8.GetBytes(password);

            passwordBytesdecrypt = SHA256.Create().ComputeHash(passwordBytesdecrypt);
            byte[] bytesDecrypted = AES_Decrypt(bytesToBeDecrypted, passwordBytesdecrypt);
            string decryptedResult = Encoding.UTF8.GetString(bytesDecrypted);
            return decryptedResult;
        }

        private static byte[] AES_Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }

            return encryptedBytes;
        }

        private static byte[] AES_Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;

            // Set your salt here, change it to meet your flavor:
            // The salt bytes must be at least 8 bytes.
            byte[] saltBytes = new byte[] { 2, 1, 7, 3, 6, 4, 8, 5 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    AES.KeySize = 256;
                    AES.BlockSize = 128;

                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);

                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }
                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }

        public static DataTable ConvertJSONToDataTable(string cadena)
        {
            DataTable result = new DataTable();
            string[] jsonParts = System.Text.RegularExpressions.Regex.Split(cadena.Replace("[", "").Replace("]", ""), "},{");

            List<string> dtColumns = new List<string>();
            foreach (string jp in jsonParts)
            {
                string[] propData = System.Text.RegularExpressions.Regex.Split(jp.Replace("{", "").Replace("}", ""), ",");
                foreach (string rowData in propData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string n = rowData.Substring(0, idx - 1).Trim();
                        string v = rowData.Substring(idx + 1).Trim();
                        if (!dtColumns.Contains(n))
                        {
                            dtColumns.Add(n.Replace("\"", ""));
                        }
                    }
                    catch
                    {
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", rowData));
                    }

                }
                break;
            }

            //foreach (string c in dtColumns)
            //{
            //    DetalleDocumentoCartera d = new DetalleDocumentoCartera();
            //    try
            //    {
            //        result.Columns.Add(c, d.getFieldtype(c));
            //    }
            //    catch
            //    {
            //        result.Columns.Add(c);
            //    }
            //}

            foreach (string jp in jsonParts)
            {
                string[] propData = System.Text.RegularExpressions.Regex.Split(jp.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = result.NewRow();
                foreach (string rowData in propData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string n = rowData.Substring(0, idx - 1).Replace("\"", "").Trim();
                        string v = rowData.Substring(idx + 1).Replace("\"", "").Trim();
                        nr[n] = v;
                    }
                    catch
                    {
                        continue;
                    }

                }
                result.Rows.Add(nr);
            }
            return result;
        }
    }
}