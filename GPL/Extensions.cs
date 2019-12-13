/*
 ____                                                         _   _               
|  _ \ _ __ ___   __ _ _ __ __ _ _ __ ___  _ __ ___   ___  __| | | |__  _   _   _ 
| |_) | '__/ _ \ / _` | '__/ _` | '_ ` _ \| '_ ` _ \ / _ \/ _` | | '_ \| | | | (_)
|  __/| | | (_) | (_| | | | (_| | | | | | | | | | | |  __/ (_| | | |_) | |_| |  _ 
|_|   |_|  \___/ \__, |_|  \__,_|_| |_| |_|_| |_| |_|\___|\__,_| |_.__/ \__, | (_)
                 |___/                                                  |___/     
 __  __                         
|  \/  | __ _ _ __ ___ ___  ___ 
| |\/| |/ _` | '__/ __/ _ \/ __|
| |  | | (_| | | | (_| (_) \__ \
|_|  |_|\__,_|_|  \___\___/|___/

 ___ _                   _ _          _ _   _       
|_ _| |_ _   _ _ __ _ __(_) |__   ___(_) |_(_) __ _ 
 | || __| | | | '__| '__| | '_ \ / _ \ | __| |/ _` |
 | || |_| |_| | |  | |  | | |_) |  __/ | |_| | (_| |
|___|\__|\__,_|_|  |_|  |_|_.__/ \___|_|\__|_|\__,_|
 
*/

/* This file is part of GPL DLL.

    GPL is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    GPL is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with GPL.  If not, see <http://www.gnu.org/licenses/>.

    This Class is the Extensions repository.
*/

using GenericParsing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Xml;
using System.Xml.Serialization;

namespace GPL
{
    /// <summary>
    /// Class for Extensions.
    /// </summary>
    /// <remarks>
    /// http://www.extensionmethod.net/csharp
    /// </remarks>
    public static class Extensions
    {
        #region object extensions

        public static bool IsNull(this object source)
        {
            return source == null;
        }

        #endregion object extensions

        #region type extensions

        /// <summary>
        /// Get the SqlDbType equivalent of the current type.
        /// </summary>
        /// <param name="type">The Type</param>
        /// <returns>The SqlDbType equivalent</returns>
        /// <remarks>
        /// If the extension throw an exceptions is because this Type does not have a SqlDbType equivalent.
        /// </remarks>
        public static SqlDbType GetSqlDbType(this Type type)
        {
            // https://stackoverflow.com/questions/1574867/convert-datacolumn-datatype-to-sqldbtype

            if (type == typeof(string))
                return SqlDbType.NVarChar;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);

            var param = new SqlParameter("", Activator.CreateInstance(type));
            return param.SqlDbType;
        }

        #endregion type extensions

        #region DirectoryInfo extensions

        /// <summary>
        /// Recursively create directory
        /// </summary>
        /// <remarks>
        /// Example:
        /// string path = @"C:\temp\one\two\three";
        ///  
        /// var dir = new DirectoryInfo(path);
        /// dir.CreateDirectory();
        /// </remarks>
        /// <param name="dirInfo">Folder path to create.</param>
        public static void CreateDirectory(this DirectoryInfo dirInfo)
        {
            if (dirInfo.Parent != null) CreateDirectory(dirInfo.Parent);
            if (!dirInfo.Exists) dirInfo.Create();
        }

        #endregion DirectoryInfo extensions

        #region string extensions

        /// <summary>
        /// Convert a Image to string.
        /// </summary>
        /// <param name="image">This Image</param>
        /// <returns>A string of the Image</returns>
        public static string ImageToString(this Image image)
        {
            if (image == null)
                return String.Empty;

            var stream = new MemoryStream();
            image.Save(stream, image.RawFormat);
            var bytes = stream.ToArray();

            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Convert a string to Image.
        /// </summary>
        /// <param name="base64String">This string</param>
        /// <returns>A Image from the string</returns>
        public static Image StringToImage(this string base64String)
        {
            if (String.IsNullOrWhiteSpace(base64String))
                return null;

            var bytes = Convert.FromBase64String(base64String);
            var stream = new MemoryStream(bytes);
            return Image.FromStream(stream);
        }

        /// <summary>
        /// Get Substring By String
        /// </summary>
        /// <param name="thisString"></param>
        /// <param name="startWith">String to start</param>
        /// <param name="endWith">String to end</param>
        /// <returns></returns>
        public static string GetSubstringByString(this string thisString, string startWith, string endWith)
        {
            return thisString.Substring((thisString.IndexOf(startWith) + startWith.Length), (thisString.IndexOf(endWith) - thisString.IndexOf(startWith) - startWith.Length));
        }

        /// <summary>
        /// Validate is a string is a XML.
        /// </summary>
        /// <param name="xml">This XML string</param>
        /// <returns></returns>
        public static bool IsXML(this string xml)
        {
            try
            {
                new XmlDocument().LoadXml(xml);

                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Validate is this string is a json.
        /// </summary>
        /// <param name="json">This string as a json</param>
        /// <returns>True is It can be deserialize to a dynamic type</returns>
        public static bool IsValidJson(this string json)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                dynamic result = serializer.DeserializeObject(json);
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Determines whether the curent string has any single tag. i.e. <b> (it doesn't have to be closed).
        /// </summary>
        /// <param name="s">The current string.</param>
        /// <returns></returns>
        public static bool HasHTMLTags(this string s)
        {
            return s.Match(@"<[^>]+>");
        }

        /// <summary>
        /// Determines whether [is valid email address] [the specified s].
        /// </summary>
        /// <param name="s">string to validate.</param>
        /// <returns></returns>
        public static bool IsValidEmailAddress(this string s)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }

        /// <summary>
        /// Determines whether the input is [T].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static bool Is<T>(this string input)
        {
            try
            {
                TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// remove white space, not line end
        /// Useful when parsing user input such phone,
        /// price int.Parse("1 000 000".RemoveSpaces(),.....
        /// </summary>
        /// <param name="s">string without spaces</param>
        public static string RemoveSpaces(this string s)
        {
            return s.Replace(" ", "");
        }

        /// <summary>
        /// Get From the right the specified value.
        /// </summary>
        /// <param name="sValue">The value.</param>
        /// <param name="iMaxLength">Maximum length to return.</param>
        /// <returns></returns>
        public static string Right(this string sValue, int iMaxLength)
        {
            //Check if the value is valid
            if (string.IsNullOrEmpty(sValue))
            {
                //Set valid empty string as string could be null
                sValue = string.Empty;
            }
            else if (sValue.Length > iMaxLength)
            {
                //Make the string no longer than the max length
                sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
            }

            //Return the string
            return sValue;
        }

        /// <summary>
        /// Removes line endings.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static String RemoveLineEndings(this String text)
        {
            StringBuilder newText = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsControl(text, i))
                    newText.Append(text[i]);
            }
            return newText.ToString();
        }

        public static string FormatString(this string format, params object[] args)
        {
            // string message = "Welcome {0} (Last login: {1})".FormatString(userName, lastLogin);
            return string.Format(format, args);
        }
        //[Obsolete("ToBoolean is deprecated, please use Parse<T> instead.")]
        //public static Boolean ToBoolean(this string value)
        //{
        //    Boolean result = true;

        //    Boolean.TryParse(value, out result);

        //    return result;
        //}

        //[Obsolete("ToInt is deprecated, please use Parse<T> instead.")]
        //public static int ToInt(this string value)
        //{
        //    int result = 0;

        //    if (!string.IsNullOrEmpty(value))
        //        int.TryParse(value, out result);

        //    return result;
        //}

        //[Obsolete("ToInt16 is deprecated, please use Parse<T> instead.")]
        //public static long ToInt16(this string value)
        //{
        //    Int16 result = 0;

        //    if (!string.IsNullOrEmpty(value))
        //        Int16.TryParse(value, out result);

        //    return result;
        //}

        //[Obsolete("ToInt32 is deprecated, please use Parse<T> instead.")]
        //public static long ToInt32(this string value)
        //{
        //    Int32 result = 0;

        //    if (!string.IsNullOrEmpty(value))
        //        Int32.TryParse(value, out result);

        //    return result;
        //}

        //[Obsolete("ToInt64 is deprecated, please use Parse<T> instead.")]
        //public static long ToInt64(this string value)
        //{
        //    Int64 result = 0;

        //    if (!string.IsNullOrEmpty(value))
        //        Int64.TryParse(value, out result);

        //    return result;
        //}

        public static bool Match(this string value, string pattern)
        {
            Regex regex = new Regex(pattern);
            return regex.IsMatch(value);
        }

        public static string Base64Encode(this string data)
        {
            byte[] encData_byte = new byte[data.Length];
            encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
            string encodedData = Convert.ToBase64String(encData_byte);
            return encodedData;
        }

        public static string Base64Decode(this string data)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();

            byte[] todecode_byte = Convert.FromBase64String(data);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

        public static string ComputeHash(this string Message)
        {
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Message));
            return Convert.ToBase64String(TDESKey);
        }

        /// <summary>
        /// Replaces only the first instance of a string within another string
        /// </summary>
        /// <param name="text">The original string</param>
        /// <param name="search">The string to be replaced</param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        #region "3DES Encoding/decoding"

        public static string EncryptString(this string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToEncrypt = UTF8.GetBytes(Message);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(Results);
        }

        public static string DecryptString(this string Message, string Passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToDecrypt = Convert.FromBase64String(Message);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(Results);
        }

        #endregion "3DES Encoding/decoding"

        #region Compress/Decompress

        /// <summary>
        /// Compresses the specified string.
        /// </summary>
        /// <param name="s">The string to Compress.</param>
        /// <returns></returns>
        public static string Compress(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        /// <summary>
        /// Decompresses the specified string.
        /// </summary>
        /// <param name="s">The string to Decompress.</param>
        /// <returns></returns>
        public static string Decompress(this string s)
        {
            var bytes = Convert.FromBase64String(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }

        #endregion Compress/Decompress

        /// <summary>
        /// Parses the string to a Enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        #endregion string extensions

        #region TextReader extensions

        /// <summary>
        /// Reads a TextReader line by line expecifying the character used as line feed.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns></returns>
        /// <example>
        /// This sample shows how to call the <see cref="ReadLines"/> method.
        /// <code>
        /// using (StreamReader sr = new StreamReader(FileName, Encoding.Default))
        /// {
        ///     foreach (var line in sr.ReadLines ('\n'))
        ///           Console.WriteLine (line);
        ///}
        /// </code>
        /// </example>
        public static IEnumerable<string> ReadLines(this TextReader reader, char delimiter)
        {
            List<char> chars = new List<char>();
            while (reader.Peek() >= 0)
            {
                char c = (char)reader.Read();

                if (c == delimiter)
                {
                    yield return new String(chars.ToArray());
                    chars.Clear();
                    continue;
                }

                chars.Add(c);
            }
        }

        #endregion TextReader extensions

        #region this IEnumerable<T>

        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        /// Get the CSV Comma Separated Value representation of this IEnumerable<T>.
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string ToCSV<T>(this IEnumerable<T> instance, string separator = ",")
        {
        return String.Join(separator, instance);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        /// <summary>
        /// Convert the current IEnumerable<T> to a DataTable.
        /// </summary>
        /// <typeparam name="T">Type of IEnumerable<T></typeparam>
        /// <param name="items">This IEnumerable<T></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> items)
        {
            // Create the result table, and gather all properties of a T        
            DataTable table = new DataTable(typeof(T).Name);
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Add the properties as columns to the datatable
            foreach (var prop in props)
            {
                Type propType = prop.PropertyType;

                // Is it a nullable type? Get the underlying type 
                if (propType.IsGenericType && propType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    propType = new NullableConverter(propType).UnderlyingType;

                table.Columns.Add(prop.Name, propType);
            }

            // Add the property values per T as rows to the datatable
            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                    values[i] = props[i].GetValue(item, null);

                table.Rows.Add(values);
            }

            return table;

        }

        #endregion this IEnumerable<T>

        #region <T>

        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T Parse<T>(this string value)
        {
            // Get default value for type so if string
            // is empty then we can return default value.
            T result = default(T);
            if (!string.IsNullOrEmpty(value))
            {
                // we are not going to handle exception here
                // if you need SafeParse then you should create
                // another method specially for that.
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
                result = (T)tc.ConvertFrom(value);
            }
            return result;
        }

        /// <summary>
        /// Return true if the params T[] list contains the this T value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool In<T>(this T value, params T[] list)
        {
            return list.Contains(value);
        }

        public static bool Between<T>(this T value, T from, T to) where T : IComparable<T>
        {
            return value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0;
        }

        // http://www.dennispoint.com/2010/10/c-extension-methods-to-serialize-and.html
        /// <summary>
        /// Serializes the specified obj.
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <returns>A string representing serialized data</returns>
        public static string Serialize(this object obj)
        {
            //Check is object is serializable before trying to serialize
            if (obj.GetType().IsSerializable)
            {
                using (var stream = new MemoryStream())
                {
                    var serializer = new XmlSerializer(obj.GetType());
                    serializer.Serialize(stream, obj);
                    var bytes = new byte[stream.Length];
                    stream.Position = 0;
                    stream.Read(bytes, 0, bytes.Length);

                    return Encoding.UTF8.GetString(bytes);
                }
            }
            throw new NotSupportedException(string.Format("{0} is not serializable.", obj.GetType()));
        }

        /// <summary>
        /// Deserializes the specified serialized data.
        /// </summary>
        /// <param name="serializedData">The serialized data.</param>
        /// <returns></returns>
        public static T Deserialize<T>(this string serializedData)
        {
            var serializer = new XmlSerializer(typeof(T));
            var reader = new XmlTextReader(new StringReader(serializedData));

            return (T)serializer.Deserialize(reader);
        }

        #endregion <T>

        #region DateTime Extensions

        public static long ToUnixTimestamp(this DateTime d)
        {
            var duration = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)duration.TotalSeconds;
        }

        public static DateTime UnixTimeStampToDateTime(this long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static DateTime JavaTimeStampToDateTime(this double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(Math.Round(javaTimeStamp / 1000)).ToLocalTime();
            return dtDateTime;
        }
        /// <summary>
        /// Determines whether the specified input is date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        //[Obsolete("IsDate is deprecated, please use Is<T> instead.")]
        //public static bool IsDate(this string input)
        //{
        //    if (!string.IsNullOrEmpty(input))
        //    {
        //        DateTime dt;
        //        return (DateTime.TryParse(input, out dt));
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public static string ToISO8601(this DateTime d)
        {
            //return d.ToString(@"yyyy-MM-ddTHH\:mm\:ss.fffffffzzz");
            return d.ToString(@"yyyy-MM-ddTHH\:mm\:ss");
        }

        #endregion DateTime Extensions

        #region System.Web.UI.Control extensions

        /// <summary>
        /// Similar to Control.FindControl, but recurses through child controls.
        /// </summary>
        public static T FindControlR<T>(this Control startingControl, string id) where T : Control
        {
            T found = startingControl.FindControl(id) as T;

            if (found == null)
            {
                found = FindChildControl<T>(startingControl, id);
            }

            return found;
        }

        /// <summary>     
        /// Similar to Control.FindControl, but recurses through child controls.
        /// Assumes that startingControl is NOT the control you are searching for.
        /// </summary>
        public static T FindChildControl<T>(this Control startingControl, string id) where T : Control
        {
            T found = null;

            foreach (Control activeControl in startingControl.Controls)
            {
                found = activeControl as T;

                if (found == null || (string.Compare(id, found.ID, true) != 0))
                {
                    found = FindChildControl<T>(activeControl, id);
                }

                if (found != null)
                {
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Finds the type of the controls of.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent">The parent.</param>
        /// <returns></returns>
        public static IEnumerable<T> FindControlsOfType<T>(this Control parent) where T : Control
        {
            foreach (Control child in parent.Controls)
            {
                if (child is T)
                {
                    yield return (T)child;
                }
                else if (child.Controls.Count > 0)
                {
                    foreach (T grandChild in child.FindControlsOfType<T>())
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        // http://stackoverflow.com/questions/7362482/c-sharp-get-all-web-controls-on-page

        /// <summary>
        /// Find the first ancestor of the selected control in the control tree
        /// </summary>
        /// <typeparam name="TControl">Type of the ancestor to look for</typeparam>
        /// <param name="control">The control to look for its ancestors</param>
        /// <returns>The first ancestor of the specified type, or null if no ancestor is found.</returns>
        public static TControl FindAncestor<TControl>(this Control control) where TControl : Control
        {
            if (control == null) throw new ArgumentNullException("control");

            Control parent = control;
            do
            {
                parent = parent.Parent;
                var candidate = parent as TControl;
                if (candidate != null)
                {
                    return candidate;
                }
            } while (parent != null);
            return null;
        }

        /// <summary>
        /// Finds all descendants of a certain type of the specified control.
        /// </summary>
        /// <typeparam name="TControl">The type of descendant controls to look for.</typeparam>
        /// <param name="parent">The parent control where to look into.</param>
        /// <returns>All corresponding descendants</returns>
        public static IEnumerable<TControl> FindDescendants<TControl>(this Control parent) where TControl : Control
        {
            if (parent == null) throw new ArgumentNullException("control");

            if (parent.HasControls())
            {
                foreach (Control childControl in parent.Controls)
                {
                    var candidate = childControl as TControl;
                    if (candidate != null) yield return candidate;

                    foreach (var nextLevel in FindDescendants<TControl>(childControl))
                    {
                        yield return nextLevel;
                    }
                }
            }
        }

        #endregion System.Web.UI.Control extensions

        #region DataTable

        /// <summary>
        /// Deletes rows in the specified table using the filter.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static DataTable Delete(this DataTable table, string filter, bool acceptChanges = false)
        {
            table.Select(filter).Delete();
            if (acceptChanges)
                table.AcceptChanges();

            return table;
        }
        /// <summary>
        /// Deletes all the rows of  the specified IEnumerable<DataRow>.
        /// </summary>
        /// <param name="rows">The rows.</param>
        public static void Delete(this IEnumerable<DataRow> rows)
        {
            foreach (var row in rows)
                row.Delete();
        }

        public static bool AreAllColumnsEmpty(this DataRow rows)
        {
            if (rows == null)
            {
                return true;
            }
            else
            {
                foreach (var value in rows.ItemArray)
                {
                    if (value != null && !string.IsNullOrEmpty(value.ToString().Trim()))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// compare two DataTables and determine if the following is true:
        /// Is the number of data columns the same in both DataTables
        /// For each data column in the first dataTable does a column exist in the other table that also is of the same type regardless of order.
        /// </summary>
        /// <param name="dt">The DataTable.</param>
        /// <param name="value">The DataTable to compare.</param>
        /// <returns>
        /// true is are Equals, false if there are some difference.
        /// </returns>
        /// <remarks>
        /// http://stackoverflow.com/questions/7313282/check-to-see-if-2-datatable-have-same-schema
        /// </remarks>
        public static bool SchemaEquals(this DataTable dt, DataTable value)
        {
            if (dt.Columns.Count != value.Columns.Count)
                return false;

            var dtColumns = dt.Columns.Cast<DataColumn>();
            var valueColumns = value.Columns.Cast<DataColumn>();


            var exceptCount = dtColumns.Except(valueColumns, DataColumnEqualityComparer.Instance).Count();
            return (exceptCount == 0);


        }

        /// <summary>
        /// Convert to a CSV delimited the DataTable and write it ti the ref StringBuilder.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="recivingStringBuilder"></param>
        /// <param name="includeHeaders"></param>
        /// <param name="includeValues"></param>
        public static void ToCSV(this DataTable dataTable, ref StringBuilder recivingStringBuilder, char characterUsedToDelimit = ',', bool includeHeaders = true, bool includeValues = true)
        {

            //headers  
            if (includeHeaders)
            {
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    recivingStringBuilder.Append(dataTable.Columns[i]);
                    if (i < dataTable.Columns.Count - 1)
                    {
                        recivingStringBuilder.Append(characterUsedToDelimit);
                    }
                }
            }

            // Values
            if (includeValues)
            {
                recivingStringBuilder.Append(Environment.NewLine);
                foreach (DataRow dr in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (!Convert.IsDBNull(dr[i]))
                        {
                            string value = dr[i].ToString();
                            if (value.Contains(characterUsedToDelimit))
                            {
                                value = String.Format("\"{0}\"", value);
                                recivingStringBuilder.Append(value);
                            }
                            else
                            {
                                recivingStringBuilder.Append(dr[i].ToString());
                            }
                        }
                        if (i < dataTable.Columns.Count - 1)
                        {
                            recivingStringBuilder.Append(characterUsedToDelimit);
                        }
                    }
                    recivingStringBuilder.Append(Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// Returns a string with a SQL CREATE TABLE command for this DataTable.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="sqlTableName"></param>
        /// <returns></returns>
        public static string GetSQLScriptToCreateTable(this DataTable dataTable, string sqlTableName)
        {
            if (string.IsNullOrEmpty(sqlTableName))
                throw new ArgumentException("tableName Parameter is null or empty.");

            StringBuilder sql = new StringBuilder();
            StringBuilder alterSql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", sqlTableName);

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                bool isNumeric = false;
                bool usesColumnDefault = true;

                sql.AppendFormat("\n\t[{0}]", dataTable.Columns[i].ColumnName);

                switch (dataTable.Columns[i].DataType.ToString().ToUpper())
                {
                    case "SYSTEM.INT16":
                        sql.Append(" [smallint]");
                        isNumeric = true;
                        break;
                    case "SYSTEM.INT32":
                        sql.Append(" [int]");
                        isNumeric = true;
                        break;
                    case "SYSTEM.INT64":
                        sql.Append(" [bigint]");
                        isNumeric = true;
                        break;
                    case "SYSTEM.DATETIME":
                        sql.Append(" [datetime]");
                        usesColumnDefault = false;
                        break;
                    case "SYSTEM.STRING":
                        sql.AppendFormat(" [nvarchar]({0})", dataTable.Columns[i].MaxLength.Equals(-1) ? "MAX" : dataTable.Columns[i].MaxLength.ToString());
                        break;
                    case "SYSTEM.SINGLE":
                        sql.Append(" [single]");
                        isNumeric = true;
                        break;
                    case "SYSTEM.DOUBLE":
                        sql.Append(" [double]");
                        isNumeric = true;
                        break;
                    case "SYSTEM.DECIMAL":
                        sql.AppendFormat(" [decimal](18, 6)");
                        isNumeric = true;
                        break;
                    default:
                        sql.AppendFormat(" [UKNOW_REVIEW_IT]({0})", "MAX");
                        break;
                }

                if (dataTable.Columns[i].AutoIncrement)
                {
                    sql.AppendFormat(" IDENTITY({0},{1})",
                        dataTable.Columns[i].AutoIncrementSeed,
                        dataTable.Columns[i].AutoIncrementStep);
                }
                else
                {
                    // DataColumns will add a blank DefaultValue for any AutoIncrement column. 
                    // We only want to create an ALTER statement for those columns that are not set to AutoIncrement. 
                    if (dataTable.Columns[i].DefaultValue != System.DBNull.Value)
                    {
                        if (usesColumnDefault)
                        {
                            if (isNumeric)
                            {
                                alterSql.AppendFormat("\nALTER TABLE [{0}] ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                    sqlTableName,
                                    dataTable.Columns[i].ColumnName,
                                    dataTable.Columns[i].DefaultValue);
                            }
                            else
                            {
                                alterSql.AppendFormat("\nALTER TABLE [{0}] ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ('{2}') FOR [{1}];",
                                    sqlTableName,
                                    dataTable.Columns[i].ColumnName,
                                    dataTable.Columns[i].DefaultValue);
                            }
                        }
                        else
                        {
                            // Default values on Date columns, e.g., "DateTime.Now" will not translate to SQL.
                            // This inspects the caption for a simple XML string to see if there is a SQL compliant default value, e.g., "GETDATE()".
                            try
                            {
                                System.Xml.XmlDocument xml = new System.Xml.XmlDocument();

                                xml.LoadXml(dataTable.Columns[i].Caption);

                                alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                                    sqlTableName,
                                    dataTable.Columns[i].ColumnName,
                                    xml.GetElementsByTagName("defaultValue")[0].InnerText);
                            }
                            catch
                            {
                                // Handle
                            }
                        }
                    }
                }

                if (!dataTable.Columns[i].AllowDBNull)
                {
                    sql.Append(" NOT NULL");
                }

                sql.Append(",");
            }

            if (dataTable.PrimaryKey.Length > 0)
            {
                StringBuilder primaryKeySql = new StringBuilder();

                primaryKeySql.AppendFormat("\n\tCONSTRAINT PK_{0} PRIMARY KEY (", sqlTableName);

                for (int i = 0; i < dataTable.PrimaryKey.Length; i++)
                {
                    primaryKeySql.AppendFormat("{0},", dataTable.PrimaryKey[i].ColumnName);
                }

                primaryKeySql.Remove(primaryKeySql.Length - 1, 1);
                primaryKeySql.Append(")");

                sql.Append(primaryKeySql);
            }
            else
            {
                sql.Remove(sql.Length - 1, 1);
            }

            sql.AppendFormat("\n);\n{0}", alterSql.ToString());

            return sql.ToString();
        }

        #endregion DataTable

        #region IDataReader

        /// <summary>
        /// Export the IDataReader to a delimited file.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="fileFullName">Full name of the file.</param>
        /// <param name="append">if set to <c>true</c> [append].</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="includeHeaderAsFirstRow">if set to <c>true</c> [include header as first row].</param>
        /// <param name="textQualifier">The text qualifier.</param>
        /// <param name="rowDelimiter">The row delimiter.</param>
        /// <param name="columnDelimiter">The column delimiter.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// Arguments: rowDelimiter & columnDelimiter can not be null.
        /// or
        /// Arguments: textQualifier, columnDelimiter and rowDelimiter must have differents values.
        /// </exception>
        public static long ToDelimitedFile(this IDataReader dataReader, string fileFullName, bool append, Encoding encoding, int bufferSize = 1024 * 4, bool includeHeaderAsFirstRow = true, string textQualifier = null, string rowDelimiter = "\r\n", string columnDelimiter = ",")
        {
            // Original code: http://www.extensionmethod.net/2085/csharp/list-string/datareader-to-csv

            // rowDelimiter & columnDelimiter must be different to null.
            if (string.IsNullOrEmpty(rowDelimiter) || string.IsNullOrEmpty(columnDelimiter))
                throw new ArgumentException(@"Arguments: rowDelimiter & columnDelimiter can not be null.");

            textQualifier = (string.IsNullOrEmpty(textQualifier) ? string.Empty : textQualifier);

            // Validate input Arguments values.
            if (textQualifier.Equals(columnDelimiter) || (textQualifier.Equals(rowDelimiter) || rowDelimiter.Equals(columnDelimiter)))
                throw new ArgumentException(@"Arguments: textQualifier, columnDelimiter and rowDelimiter must have differents values.");

            long rowsExported = 0;

            bool textQualifierIsDoubleCuoteOrEmpty = (textQualifier == "\"" || textQualifier == string.Empty) ? true : false;

            bool textQualifierIsNullOrEmpty = (string.IsNullOrEmpty(textQualifier)) ? true : false;

            using (StreamWriter outfile = new StreamWriter(fileFullName, append, encoding, bufferSize))
            {
                StringBuilder sb = null;
                bool ValueAlreadyEnclosed;

                if (includeHeaderAsFirstRow)
                {
                    sb = new StringBuilder();
                    for (int index = 0; index < dataReader.FieldCount; index++)
                    {
                        if (dataReader.GetName(index) != null)
                        {
                            string value = dataReader.GetName(index);

                            ValueAlreadyEnclosed = false;

                            if (textQualifierIsDoubleCuoteOrEmpty)
                            {
                                //If double quotes are used in value, ensure each are replaced but 2.
                                if (value.IndexOf("\"") >= 0)
                                    value = value.Replace("\"", "\"\"");

                                //If columnDelimiter is in value, ensure it is put in double quotes.
                                if (value.IndexOf(columnDelimiter) >= 0)
                                {
                                    value = "\"" + value + "\"";
                                    ValueAlreadyEnclosed = true;
                                }
                            }
                            // Apply the textQualifier to the value if it is supplied.
                            if (!ValueAlreadyEnclosed)
                                value = textQualifier + value + textQualifier;

                            sb.Append(value);
                        }
                        if (index < dataReader.FieldCount - 1)
                            sb.Append(columnDelimiter);
                    }
                    outfile.Write(sb + rowDelimiter);
                }

                while (dataReader.Read())
                {
                    sb = new StringBuilder();
                    for (int index = 0; index < dataReader.FieldCount - 1; index++)
                    {
                        if (!dataReader.IsDBNull(index))
                        {
                            string value = dataReader.GetValue(index).ToString();
                            //if (dataReader.GetFieldType(index) == typeof(String))
                            {
                                ValueAlreadyEnclosed = false;

                                if (textQualifierIsDoubleCuoteOrEmpty)
                                {
                                    //If double quotes are used in value, ensure each are replaced but 2.
                                    if (value.IndexOf("\"") >= 0)
                                        value = value.Replace("\"", "\"\"");

                                    //If columnDelimiter is in value, ensure it is put in double quotes.
                                    if (value.IndexOf(columnDelimiter) >= 0)
                                    {
                                        value = "\"" + value + "\"";
                                        ValueAlreadyEnclosed = true;
                                    }
                                }
                                // Apply the textQualifier to the value if it is supplied.
                                if (!ValueAlreadyEnclosed)
                                    value = textQualifier + value + textQualifier;
                            }
                            sb.Append(value);
                        }

                        if (index < dataReader.FieldCount - 1)
                            sb.Append(columnDelimiter);
                    }

                    if (!dataReader.IsDBNull(dataReader.FieldCount - 1))
                        sb.Append(dataReader.GetValue(dataReader.FieldCount - 1).ToString().Replace(columnDelimiter, " "));

                    outfile.Write(sb + rowDelimiter);
                    rowsExported++;
                }
                dataReader.Close();

                sb = null;
            }
            return rowsExported;
        }

        #endregion IDataReader

        #region stream

        /// <summary>
        /// COnvert this string to a Stream
        /// </summary>
        /// <param name="str">This strim</param>
        /// <returns>A Stream from the string</returns>
        public static Stream ToStream(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            //byte[] byteArray = Encoding.ASCII.GetBytes(str);
            return new MemoryStream(byteArray);
        }
        /// <summary>
        /// Copy from one stream to another.
        /// Example:
        /// using(var stream = response.GetResponseStream())
        /// using(var ms = new MemoryStream())
        /// {
        ///     stream.CopyTo(ms);
        ///      // Do something with copied data
        /// }
        /// </summary>
        /// <param name="fromStream">From stream.</param>
        /// <param name="toStream">To stream.</param>
        public static void CopyTo(this Stream fromStream, Stream toStream)
        {
            if (fromStream == null)
                throw new ArgumentNullException("fromStream");
            if (toStream == null)
                throw new ArgumentNullException("toStream");
            var bytes = new byte[8092];
            int dataRead;
            while ((dataRead = fromStream.Read(bytes, 0, bytes.Length)) > 0)
                toStream.Write(bytes, 0, dataRead);
        }

        /// <summary>
        /// Deserialize Json From Stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T DeserializeJsonFromStream<T>(this Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        /// <summary>
        /// Get a DataTable from This stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="skipEmptyRows"></param>
        /// <param name="hasHeaderRecord"></param>
        /// <param name="delimiter"></param>
        /// <param name="textQualifier"></param>
        /// <param name="escapeCharacter"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this Stream stream, bool skipEmptyRows, bool hasHeaderRecord, char? delimiter = null, char? textQualifier = null, char? escapeCharacter = null)
        {
            using (GenericParserAdapter parser = new GenericParserAdapter())
            {
                stream.Position = 0;

                parser.SetDataSource(new StreamReader(stream));

                parser.ColumnDelimiter = delimiter;
                parser.FirstRowHasHeader = hasHeaderRecord;
                parser.TextQualifier = textQualifier;
                parser.SkipEmptyRows = skipEmptyRows;
                parser.EscapeCharacter = escapeCharacter;

                return parser.GetDataTable();
            }
        }

        #endregion stream

        #region HttpContent
        /// <summary>
        /// Read a HttpContent and save to a file.
        /// </summary>
        /// <param name="content">this HttpContent</param>
        /// <param name="filename">File name to save the HttpContent</param>
        /// <param name="overwrite">true = overwrite existing files</param>
        /// <returns></returns>
        public static Task ReadAsFileAsync(this HttpContent content, string filename, bool overwrite)
        {
            string pathname = Path.GetFullPath(filename);
            if (!overwrite && File.Exists(filename))
            {
                throw new InvalidOperationException(string.Format("File {0} already exists.", pathname));
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(fileStream).ContinueWith(
                    (copyTask) =>
                    {
                        fileStream.Close();
                    });
            }
            catch
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                throw;
            }
        }
        #endregion HttpContent

        #region ZipArchive
        /// <summary>
        /// Extract from zip file to a directory.
        /// </summary>
        /// <param name="archive">The ZIP file</param>
        /// <param name="destinationDirectoryName">The destination directory</param>
        /// <param name="overwrite">true = overwrite existing files</param>
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
            string destinationDirectoryFullPath = di.FullName;

            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                }

                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }

        #endregion ZipArchive
    }

    /// <summary>
    /// Class that implements IEqualityComparer interface used on the SchemaEquals extension.
    /// </summary>
    class DataColumnEqualityComparer : IEqualityComparer<DataColumn>
    {
        #region IEqualityComparer Members

        private DataColumnEqualityComparer() { }
        public static DataColumnEqualityComparer Instance = new DataColumnEqualityComparer();


        public bool Equals(DataColumn x, DataColumn y)
        {
            if (x.ColumnName != y.ColumnName)
                return false;
            if (x.DataType != y.DataType)
                return false;

            return true;
        }

        public int GetHashCode(DataColumn obj)
        {
            int hash = 17;
            hash = 31 * hash + obj.ColumnName.GetHashCode();
            hash = 31 * hash + obj.DataType.GetHashCode();

            return hash;
        }

        #endregion
    }
}
