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

    This Class is the Utility Class.
*/

using GenericParsing;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace GPL
{

    /// <summary>
    /// pubilc class that has utility methods.
    /// </summary>
    static public class Utility
    {
        #region HttpWebRequest
        /// <summary>
        /// Get and write certificate from URL into file in path
        /// </summary>
        /// <param name="_URL">URL of website with certficate</param>
        /// <param name="_path">Path where you want to store certificate</param>
        private static void SaveCertificate(String _URL, String _path)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_URL);
                request.AllowAutoRedirect = false;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();

                X509Certificate2 cert = new X509Certificate2(request.ServicePoint.Certificate);

                File.WriteAllText(_path, ExportToPEM(cert));
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Export a certificate to a PEM format string
        /// </summary>
        /// <param name="_cert">The certificate to export</param>
        /// <returns>A PEM encoded string</returns>
        public static string ExportToPEM(X509Certificate2 _cert)
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                builder.AppendLine("-----BEGIN CERTIFICATE-----");
                builder.AppendLine(Convert.ToBase64String(_cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                builder.AppendLine("-----END CERTIFICATE-----");

            }
            catch (Exception)
            {
            }

            return builder.ToString();
        }
        #endregion HttpWebRequest

        #region Microsoft SQL Operations

        /// <summary>
        /// Get a IEnumerable<string> of the Columns names from a SQL table, don't use brackets [].
        /// </summary>
        /// <param name="connectionString">The SQL connection string</param>
        /// <param name="tableCatalog">The name of the data base</param>
        /// <param name="tableSchema">The name of the table schema example dbo.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetColumnsNamesFromSQLTable(string connectionString, string tableCatalog, string tableSchema, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText =
                    @"SELECT COLUMN_NAME
                      FROM INFORMATION_SCHEMA.COLUMNS
                      WHERE
                      TABLE_CATALOG = @TABLE_CATALOG
                      AND TABLE_SCHEMA = @TABLE_SCHEMA
                      AND TABLE_NAME = @TABLE_NAME";

                command.CommandType = CommandType.Text;

                command.Parameters.AddWithValue("@TABLE_CATALOG", tableCatalog);
                command.Parameters.AddWithValue("@TABLE_SCHEMA", tableSchema);
                command.Parameters.AddWithValue("@TABLE_NAME", tableName);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return (string)reader["COLUMN_NAME"];
                    }
                }
            }
        }

        public static IEnumerable<string> GetColumnsNamesFromDelimitedFile(string fileFullpathName, string delimiter = ",", bool hasFieldsEnclosedInQuotes = true)
        {
            DataTable DelimitedData = new DataTable();

            using (TextFieldParser DelimitedReader = new TextFieldParser(fileFullpathName))
            {
                DelimitedReader.SetDelimiters(new string[] { delimiter });
                DelimitedReader.HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;
                //read column names
                return DelimitedReader.ReadFields().ToList<string>();

            }
        }



            /// <summary>
            /// Check if a SQL tables exist in the specified database.
            /// </summary>
            /// <param name="connectString">The connect string.</param>
            /// <param name="database">The database.</param>
            /// <param name="tableSchema">The table schema.</param>
            /// <param name="tableName">Name of the table.</param>
            /// <returns></returns>
            public static bool TableExist(string connectString, string database, string tableSchema, string tableName)
        {
            using (var dbh = new DBHelper(false))
            {
                dbh.CreateDBObjects(connectString, DBHelper.Providers.SqlServer, null);

                var ct = @"use {0}; 
                           if exists (select top 1 table_name from information_schema.tables where table_schema like '{1}' and table_name like '{2}') 
                            select cast(1 as bit); 
                           else 
                            select cast(0 as bit);".FormatString(database, tableSchema, tableName);

                return dbh.ExecuteScalar(ct, CommandType.Text, ConnectionState.Closed).ToString().Parse<bool>();
            }
        }
        #endregion Microsoft SQL Operations

        /// <summary>
        /// Gets the current executable path.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentExecutablePath()
        {
            //return System.Reflection.Assembly.GetEntryAssembly().Location;
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Sends the message email.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="isHtml">if set to <c>true</c> [is HTML].</param>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="numRetries">The number retries.</param>
        /// <param name="retryTimeout">The retry timeout.</param>
        /// <exception cref="System.ArgumentException">Argument To must be a valid email repository, please review It.</exception>
        public static void SendMessageEmail(string to, string @from, string subject, string body, bool isHtml, string host = "", int port = 25, int numRetries = 3, int retryTimeout = 1000)
        {
            var msg = new MailMessage();

            var splitChar = to.Contains(",") ? "," : ";";

            string[] emTo = to.Trim().Split(splitChar.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (emTo.Length == 0)
                throw new ArgumentException("Argument To must be a valid email repository, please review It.");

            foreach (string toAdd in emTo)
            {
                msg.To.Add(toAdd);
            }
            msg.From = new MailAddress(@from);
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = isHtml;

            var client = new SmtpClient();
            if (!string.IsNullOrEmpty(host))
                client.Host = host;

            client.Port = port;

            //client.Send(msg);

            // TODO 'RetryAction is deprecated, please use RetryMethod instead.'

            RetryAction(() => client.Send(msg), numRetries, retryTimeout);
        }

        /// <summary>
        /// Upload a file to a FTP server.
        /// </summary>
        /// <param name="ftpAddress">The FTP address.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public static void FTPuploadFile(string ftpAddress, string filePath, string username, string password)
        {
            //Create FTP request
            FtpWebRequest request = null;
            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(ftpAddress + "/" + Path.GetFileName(filePath));

                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(username, password);
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;

                //Load the file
                FileStream stream = File.OpenRead(filePath);
                byte[] buffer = new byte[stream.Length];

                stream.Read(buffer, 0, buffer.Length);
                stream.Close();

                //Upload file
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(buffer, 0, buffer.Length);
                reqStream.Close();

                //MessageBox.Show("Uploaded Successfully");
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Queues the action in the ThreadPool.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <example>Example: Utility.QueueAction(() => SomeFunctionToExecute(Parm1, Parmm2, ...));</example>
        /// <exception cref="System.ArgumentNullException">action must be not null...</exception>
        public static void QueueAction(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action must be not null...");

            //new Task(() => { action(); }).Start();

            /*
             new Thread(() =>
             {
                 Thread.CurrentThread.IsBackground = true;
                 // run your code here.
                 action();
             }).Start();
             */

            ThreadPool.QueueUserWorkItem(o => action());
        }

        /// <summary>
        /// Method for retry any other method if It fail.
        /// </summary>
        /// <example>
        /// C# example: Utility.RetryAction( () => SomeFunctionThatCanFail(), 3, 1000 );
        /// VB example: Utility.RetryAction(AddressOf SomeFunctionThatCanFail, 3, 1000)
        /// </example>
        /// <param name="action">Name of the Method you want to retry</param>
        /// <param name="timesToRetry">Number of retries</param>
        /// <param name="retryTimeout">Miliseconds between each retry.</param>
        [Obsolete("RetryAction is deprecated, please use RetryMethod instead.")]
        public static void RetryAction(Action action, int timesToRetry, int retryTimeout)
        {
            // Validate Parameters.
            if (action == null)
                throw new ArgumentNullException("action");

            if (timesToRetry < 0)
                throw new ArgumentException("timesToRetry is < 0.");

            while (true)
            {
                try
                {
                    action();
                    break; // success!
                }
                catch
                {
                    if (--timesToRetry <= -1) throw;
                    else Thread.Sleep(retryTimeout);
                }
            }
        }

        /// <summary>
        /// Retries the execution of a function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="func">The function.</param>
        /// <param name="timesToRetry">The times to retry.</param>
        /// <param name="retryTimeout">The retry timeout.</param>
        /// <returns></returns>
        [Obsolete("RetryFunc<T, TException> is deprecated, please use RetryMethod instead.")]
        public static T RetryFunc<T, TException>(Func<T> func, int timesToRetry, int retryTimeout)
where TException : Exception
        {
            // Start at 1 instead of 0 to allow for final attempt
            for (int i = 1; i < timesToRetry; i++)
            {
                try
                {
                    return func();
                }
                catch (TException)
                {
                    Thread.Sleep(retryTimeout);
                }
            }

            return func(); // Final attempt, let exception bubble up
        }


        /// <summary>
        /// Retry a method.
        /// </summary>
        /// <remarks>
        /// This method retry a method until It runs successfully or reach the timesToRetry failing
        /// </remarks>
        /// <param name="method">Name of the method</param>
        /// <param name="timesToRetry">Times to retry</param>
        /// <param name="retryTimeout">Timeout between retries</param>
        /// <param name="args">Arguments needed by the method</param>
        /// <returns>The object returned by the method.</returns>
        /// <example>
        /// <code>
        /// /* This below example shows how to invoke a method without parameters that returns void. */
        /// Utility.RetryMethod(new Action(SomeMethodWithoutParametersReturnsVoid), 2, 500);
        ///
        /// /* This below example shows ow to invoke a method with parameters that returns void. */
        /// Utility.RetryMethod(new Action&lt;string, string, string, DatabaseManufacter, string, string, string, Int32, int, int&gt;(DAHelper.DoSqlBulkCopy), 2, 500, scs, spn, tn, sdsmEnum, dcs, dpn, ftn, BulkRowsBuffer, BulkCommandTimeout, BulkBatchSize);
        /// 
        /// /* A method that return a value. 
        /// You can define the delegate before or inside of the RetryMethod.
        ///
        /// Example with retries defining the delegate before and invoking in the Utility.RetryMethod 
        /// note that the return type 'DataSet' is declared at the end, and the parameters types before.
        /// */
        /// var newfuction = new Func&#60;string, CommandType, ConnectionState, DataSet&#62;(dbh.GetDataSet);
        ///
        /// /* Inside note that the return type is converted from the object return type to DataSet type. */
        /// DataSet rdr = (DataSet)Utility.RetryMethod(newfuction, 3, 3, CmdTextOK, CommandType.Text, ConnectionState.Open);
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">Thrown when any parameter is invalid.</exception>
        /// <exception cref="System.AggregateException">Thrown when the method fails more than the value of timesToRetry parameter.</exception>
        public static object RetryMethod(Delegate method, int timesToRetry, int retryTimeout, params object[] args)
        {
            // http://stackoverflow.com/questions/380198/how-to-pass-a-function-as-a-parameter-in-c

            // Validate Parameters.
            if (method == null)
                throw new ArgumentNullException("method");

            if (timesToRetry < 0)
                throw new ArgumentException("timesToRetry is < 0.");

            if (retryTimeout < 0)
                throw new ArgumentException("retryTimeout is < 0.");

            var exceptions = new List<Exception>();

            while (true)
            {
                try
                {
                    return method.DynamicInvoke(args);
                    // success!
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    if (--timesToRetry <= -1) throw new AggregateException(exceptions);
                    else Thread.Sleep(retryTimeout);
                }
            }

        }

        /// <summary>
        /// Gets a list of the ODBC drivers.
        /// </summary>
        /// <returns></returns>
        public static List<String> GetODBCDrivers()
        {
            List<string> names = new List<string>();
            // get system dsn's
            Microsoft.Win32.RegistryKey reg = (Microsoft.Win32.Registry.LocalMachine).OpenSubKey("Software");
            if (reg != null)
            {
                reg = reg.OpenSubKey("ODBC");
                if (reg != null)
                {
                    reg = reg.OpenSubKey("ODBCINST.INI");
                    if (reg != null)
                    {
                        reg = reg.OpenSubKey("ODBC Drivers");
                        if (reg != null)
                        {
                            // Get all DSN entries defined in DSN_LOC_IN_REGISTRY.
                            foreach (string sName in reg.GetValueNames())
                            {
                                names.Add(sName);
                            }
                        }
                        try
                        {
                            reg.Close();
                        }
                        catch { /* ignore this exception if we couldn't close */ }
                    }
                }
            }

            return names;
        }

        /// <summary>
        /// Detect if this OS runs in a virtual machine
        ///
        /// http://blogs.msdn.com/b/virtual_pc_guy/archive/2005/10/27/484479.aspx
        ///
        /// Microsoft themselves say you can see that by looking at the motherboard via wmi
        /// </summary>
        /// <returns>false</returns> if it runs on a fysical machine
        public static bool DetectVirtualMachine()
        {
            //bool result = false;

            using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (var items = searcher.Get())
                {
                    foreach (var item in items)
                    {
                        string manufacturer = item["Manufacturer"].ToString().ToLower();
                        if ((manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                            || manufacturer.Contains("vmware")
                            || item["Model"].ToString() == "VirtualBox")
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #region Validate Server Listering.

        public static bool CheckServer(string host, int port)
        {
            try
            {
                IPHostEntry IPHost = new IPHostEntry();
                //IPHost = Dns.Resolve(host);
                IPHost = Dns.GetHostEntry(host);

                IPAddress IPAddr = IPHost.AddressList[0];

                TcpClient TcpCli = new TcpClient();
                TcpCli.Connect(IPAddr, port);
                TcpCli.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ping the host of the given URL to see if it is responding.
        /// </summary>
        /// <param name="strURL"></param>
        /// <returns></returns>
        public static bool URLDomainExist(string strURL)
        {
            bool MyReturn = false;
            try
            {
                Uri myUri = new Uri(strURL);
                string host = myUri.Host;

                Ping pingSender = new Ping();

                PingReply reply = pingSender.Send(host);

                if (reply.Status == IPStatus.Success)

                    MyReturn = true;
                else
                    MyReturn = true;
            }
            catch (Exception ex)
            {
                MyReturn = false;
            }

            return MyReturn;
        }

        #endregion Validate Server Listering.

        #region File Operations

        /// <summary>
        /// Finds the MIME from data.
        /// </summary>
        /// <param name="pBC">The p BC.</param>
        /// <param name="pwzUrl">The PWZ URL.</param>
        /// <param name="pBuffer">The p buffer.</param>
        /// <param name="cbSize">Size of the cb.</param>
        /// <param name="pwzMimeProposed">The PWZ MIME proposed.</param>
        /// <param name="dwMimeFlags">The dw MIME flags.</param>
        /// <param name="ppwzMimeOut">The PPWZ MIME out.</param>
        /// <param name="dwReserved">The dw reserved.</param>
        /// <returns></returns>
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        private static extern int FindMimeFromData(IntPtr pBC,
            [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1, SizeParamIndex = 3)] byte[] pBuffer,
            int cbSize, [MarshalAs(UnmanagedType.LPWStr)]  string pwzMimeProposed,
            int dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

        /// <summary>
        /// Gets the MIME from file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns>get ContentType for file</returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public static string GetMimeFromFile(string file)
        {
            IntPtr mimeout;
            if (!System.IO.File.Exists(file))
                throw new FileNotFoundException(file + " not found");

            int MaxContent = (int)new FileInfo(file).Length;
            if (MaxContent > 4096) MaxContent = 4096;
            FileStream fs = File.OpenRead(file);

            byte[] buf = new byte[MaxContent];
            fs.Read(buf, 0, MaxContent);
            fs.Close();
            int result = FindMimeFromData(IntPtr.Zero, file, buf, MaxContent, null, 0, out mimeout, 0);

            if (result != 0)
                throw Marshal.GetExceptionForHR(result);
            string mime = Marshal.PtrToStringUni(mimeout);
            Marshal.FreeCoTaskMem(mimeout);
            return mime;
        }

        /// <summary>
        /// Deletes older files from the given Directory comparing the LastWriteTime property of the file and the current date time.
        /// </summary>
        /// <param name="dirName">Name of the dir.</param>
        /// <param name="days">The days from Now to calculate.</param>
        public static void DeleteOlderFiles(string dirName, string searchPattern, double days = -90)
        {
            string[] files = Directory.GetFiles(dirName, searchPattern);

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastWriteTime < DateTime.Now.AddDays(days))
                    fi.Delete();
            }
        }

        /// <summary>
        /// Gets the file size on disk.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        /// <exception cref="Win32Exception"></exception>
        public static long GetFileSizeOnDisk(string file)
        {
            FileInfo info = new FileInfo(file);
            uint dummy, sectorsPerCluster, bytesPerSector;
            int result = GetDiskFreeSpaceW(info.Directory.Root.FullName, out sectorsPerCluster, out bytesPerSector, out dummy, out dummy);
            if (result == 0) throw new Win32Exception();
            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint hosize;
            uint losize = GetCompressedFileSizeW(file, out hosize);
            long size;
            size = (long)hosize << 32 | losize;
            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        /// <summary>
        /// Gets the compressed file size w.
        /// </summary>
        /// <param name="lpFileName">Name of the lp file.</param>
        /// <param name="lpFileSizeHigh">The lp file size high.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern uint GetCompressedFileSizeW([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
           [Out, MarshalAs(UnmanagedType.U4)] out uint lpFileSizeHigh);

        /// <summary>
        /// Gets the disk free space w.
        /// </summary>
        /// <param name="lpRootPathName">Name of the lp root path.</param>
        /// <param name="lpSectorsPerCluster">The lp sectors per cluster.</param>
        /// <param name="lpBytesPerSector">The lp bytes per sector.</param>
        /// <param name="lpNumberOfFreeClusters">The lp number of free clusters.</param>
        /// <param name="lpTotalNumberOfClusters">The lp total number of clusters.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, PreserveSig = true)]
        private static extern int GetDiskFreeSpaceW([In, MarshalAs(UnmanagedType.LPWStr)] string lpRootPathName,
           out uint lpSectorsPerCluster, out uint lpBytesPerSector, out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);

        /// <summary>
        /// Checks the file has copied.
        /// </summary>
        /// <param name="FilePath">The file path.</param>
        /// <returns></returns>
        public static bool CheckFileHasCopied(string FilePath)
        {
            // http://www.dotnetfunda.com/articles/article1312-detect-file-copy-completion-in-filesystemwatcher.aspx
            try
            {
                if (File.Exists(FilePath))
                    using (File.OpenRead(FilePath))
                    {
                        return true;
                    }
                else
                    return false;
            }
            catch (Exception)
            {
                //Thread.Sleep(100);
                //return CheckFileHasCopied(FilePath);
                return false;
            }
        }

        /// <summary>
        /// Writes text to file.
        /// </summary>
        /// <param name="FilePath">The file path.</param>
        /// <param name="text">The text.</param>
        public static void WriteTextToFile(string FilePath, string text)
        {
            using (StreamWriter sw = File.AppendText(FilePath))
            {
                sw.WriteLine(text);
            }
        }

        /// <summary>
        /// Load the content of a files to a  string variable.
        /// </summary>
        /// <param name="filePath">The full file path.</param>
        /// <returns>A string with the content of the file.</returns>
        public static string FileToString(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(filePath))
            {
                String line;
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the file lines count.
        /// </summary>
        /// <param name="fileFullPath">The file full path.</param>
        /// <param name="bufferZise">The buffer zise.</param>
        /// <returns></returns>
        public static long GetFileLinesCount(string fileFullPath, int bufferZise = 1024 * 1024)
        {
            using (var fs = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
            {
                long lineCount = 0;
                byte[] buffer = new byte[bufferZise];
                int bytesRead;

                do
                {
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < bytesRead; i++)
                        if (buffer[i] == '\n')
                            lineCount++;
                }
                while (bytesRead > 0);

                return lineCount;

            }
        }

        /// <summary>
        /// Gets a DataTable from a delimited file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="skipEmptyRows">if set to <c>true</c> [skip empty rows].</param>
        /// <param name="hasHeaderRecord">if set to <c>true</c> [has header record].</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="textQualifier">The text qualifier.</param>
        /// <param name="escapeCharacter">The escape character.</param>
        /// <returns></returns>
        [Obsolete("GetDataTableFromDelimitedFile is deprecated, please use the new GetDataTableFromDelimitedFile that use the TextFieldParser instead.", true)]
        public static DataTable GetDataTableFromDelimitedFile(string filePath, bool skipEmptyRows, bool hasHeaderRecord, char? delimiter = null, char? textQualifier = null, char? escapeCharacter = null)
        {
            using (GenericParserAdapter parser = new GenericParserAdapter())
            {
                parser.SetDataSource(filePath);

                parser.ColumnDelimiter = delimiter;
                parser.FirstRowHasHeader = hasHeaderRecord;
                parser.TextQualifier = textQualifier;
                parser.SkipEmptyRows = skipEmptyRows;
                parser.EscapeCharacter = escapeCharacter;

                return parser.GetDataTable();
            }
        }

        /// <summary>
        /// Get a DataTable from a delimited file.
        /// </summary>
        /// <param name="fileFullpathName">File full path name</param>
        /// <param name="delimiter">Delimiter</param>
        /// <param name="hasFieldsEnclosedInQuotes">Has fields enclosed in quotes</param>
        /// <returns></returns>
        public static DataTable GetDataTableFromDelimitedFile(string fileFullpathName, string delimiter = ",", bool hasFieldsEnclosedInQuotes = true)
        {
            DataTable DelimitedData = new DataTable();
            try
            {
                using (TextFieldParser DelimitedReader = new TextFieldParser(fileFullpathName))
                {
                    DelimitedReader.SetDelimiters(new string[] { delimiter });
                    DelimitedReader.HasFieldsEnclosedInQuotes = hasFieldsEnclosedInQuotes;
                    //read column names
                    string[] colFields = DelimitedReader.ReadFields();
                    foreach (string column in colFields)
                    {
                        DataColumn datecolumn = new DataColumn(column);
                        datecolumn.AllowDBNull = true;
                        DelimitedData.Columns.Add(datecolumn);
                    }
                    while (!DelimitedReader.EndOfData)
                    {
                        string[] fieldData = DelimitedReader.ReadFields();
                        //Making empty value as null
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == "")
                            {
                                fieldData[i] = null;
                            }
                        }
                        DelimitedData.Rows.Add(fieldData);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DelimitedData;
        }

        /// <summary>
        /// Convert a json string to a DataSet
        /// </summary>
        /// <param name="jsonMessage">The json message</param>
        /// <param name="createRootNode">Create a root node</param>
        /// <param name="rootNodeName">Name of the root node</param>
        /// <returns>A DataSet with the data from the json message</returns>
        public static DataSet GetDataSetFromJson(string jsonMessage, bool createRootNode = false, string rootNodeName = "root")
        {
            // If you get errors see It to understand.
            // https://stackoverflow.com/questions/57990303/xmlnodeconverter-can-only-convert-json-that-begins-with-an-object-path-line

            if (createRootNode)
                jsonMessage = "{\"" + rootNodeName + "\":" + jsonMessage + "}";

            /* https://blogs.msdn.microsoft.com/dilkushp/2013/10/31/easiest-way-of-loading-json-data-in-sql-using-c/ */


            XmlDocument xml = createRootNode ? JsonConvert.DeserializeXmlNode(jsonMessage, rootNodeName) : JsonConvert.DeserializeXmlNode(jsonMessage);

            DataSet ds = new DataSet("DataSetFromJson");

            XmlReader xr = new XmlNodeReader(xml);

            ds.ReadXml(xr);

            return ds;
        }

        #endregion File Operations

        /// <summary>
        /// Gets the name of the machine.
        /// </summary>
        /// <returns></returns>
        public static string GetMachineName()
        {
            return System.Environment.MachineName;
        }

        /// <summary>
        /// Combines multiple strings in a format suitable to be used as a web URI
        /// </summary>
        /// <param name="uriParts">String array containing the different parts of the final URI</param>
        /// <returns>A valid web link string</returns>
        public static string CombineUri(params string[] uriParts)
        {
            string uri = string.Empty;
            if (uriParts != null && uriParts.Length > 0)
            {
                char[] trims = new char[] { '\\', '/' };
                uri = (uriParts[0] ?? string.Empty).TrimEnd(trims);
                for (int i = 1; i < uriParts.Length; i++)
                {
                    uri = string.Format("{0}/{1}", uri.TrimEnd(trims), (uriParts[i] ?? string.Empty).TrimStart(trims));
                }
            }
            return uri;
        }

        /// <summary>
        /// Gets the base URL of the current request.
        /// </summary>
        /// <returns>a string with the Base Url of the current request.</returns>
        public static string GetBaseUrl()
        {
            return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        }

        /// <summary>
        /// Gets the client ip address.
        /// </summary>
        /// <returns></returns>
        public static string GetClientIPAddress()
        {
            //return ((RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name]).Address;
            return HttpContext.Current.Request.UserHostAddress;
        }

        #region Random operations

        /// <summary>
        /// Generate a Random string.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="allowedChars">The allowed chars.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">length;length cannot be less than zero.</exception>
        /// <exception cref="System.ArgumentException">
        /// allowedChars may not be empty.
        /// or
        /// </exception>
        public static string RandomString(int length, string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            if (string.IsNullOrEmpty(allowedChars)) throw new ArgumentException("allowedChars may not be empty.");

            const int byteSize = 0x100;
            var allowedCharSet = new HashSet<char>(allowedChars).ToArray();
            if (byteSize < allowedCharSet.Length) throw new ArgumentException(String.Format("allowedChars may contain no more than {0} characters.", byteSize));

            // Guid.NewGuid and System.Random are not particularly random. By using a
            // cryptographically-secure random number generator, the caller is always
            // protected, regardless of use.
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var result = new StringBuilder();
                var buf = new byte[128];
                while (result.Length < length)
                {
                    rng.GetBytes(buf);
                    for (var i = 0; i < buf.Length && result.Length < length; ++i)
                    {
                        // Divide the byte into allowedCharSet-sized groups. If the
                        // random value falls into the last group and the last group is
                        // too small to choose from the entire allowedCharSet, ignore
                        // the value in order to avoid biasing the result.
                        var outOfRangeStart = byteSize - (byteSize % allowedCharSet.Length);
                        if (outOfRangeStart <= buf[i]) continue;
                        result.Append(allowedCharSet[buf[i] % allowedCharSet.Length]);
                    }
                }
                return result.ToString();
            }
        }

        #endregion Random operations

        #region Service operations

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        public static ServiceController GetService(string serviceName)
        {
            ServiceController[] services = ServiceController.GetServices();
            //return services.FirstOrDefault(_ => Contracts.Extensions.CompareStrings(_.ServiceName, serviceName));
            return services.FirstOrDefault(s => s.ServiceName == serviceName);
        }

        /// <summary>
        /// Determines whether [is service running] [the specified service name].
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns>
        ///   <c>true</c> if [is service running] [the specified service name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsServiceRunning(string serviceName)
        {
            ServiceControllerStatus status;
            uint counter = 0;
            do
            {
                ServiceController service = GetService(serviceName);
                if (service == null)
                {
                    return false;
                }

                Thread.Sleep(100);
                status = service.Status;
            } while (!(status == ServiceControllerStatus.Stopped ||
                       status == ServiceControllerStatus.Running) &&
                     (++counter < 30));
            return status == ServiceControllerStatus.Running;
        }

        /// <summary>
        /// Determines whether [is service installed] [the specified service name].
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns>
        ///   <c>true</c> if [is service installed] [the specified service name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsServiceInstalled(string serviceName)
        {
            return GetService(serviceName) != null;
        }

        /// <summary>
        /// Starts the service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public static void StartService(string serviceName)
        {
            ServiceController controller = GetService(serviceName);
            if (controller == null)
            {
                return;
            }

            controller.Start();
            controller.WaitForStatus(ServiceControllerStatus.Running);
        }

        /// <summary>
        /// Stops the service.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        public static void StopService(string serviceName)
        {
            ServiceController controller = GetService(serviceName);
            if (controller == null)
            {
                return;
            }

            controller.Stop();
            controller.WaitForStatus(ServiceControllerStatus.Stopped);
        }

        #endregion Service operations

        #region Control operations

        /// <summary>
        /// Recursive FindControl method, to search a control and all child
        /// controls for a control with the specified ID.
        /// </summary>
        /// <returns>Control if found or null</returns>
        public static Control FindControlRecursive(Control root, string id)
        {
            if (id == string.Empty)
                return null;

            if (root.ID == id)
                return root;

            foreach (Control c in root.Controls)
            {
                Control t = FindControlRecursive(c, id);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        #endregion Control operations

        /// <summary>
        /// Gets the current method Name.
        /// </summary>
        /// <returns>The Name of the current method</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethodName()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        /// <summary>
        /// Sets the value from application settings.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="variableToReceiveValue">The variable to receive value.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="KeyIsOptional">if set to <c>true</c> [key is optional].</param>
        /// <param name="valueCouldBeEmpty">if set to <c>true</c> [value could be empty].</param>
        /// <exception cref="Exception">
        /// Fatal error: Variable To Receive Value must be NOT nullable.
        /// or
        /// or
        /// or
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Fatal error: Variable To Receive Value must be Initialized with not null value.
        /// or
        /// Fatal error: The keyName Variable can not be null or empty.
        /// </exception>
        public static void SetValueFromAppSettings<T>(ref T variableToReceiveValue, string keyName, bool KeyIsOptional = true, bool valueCouldBeEmpty = true)
        {
            // Validate the parameters.

            // Validate <T> is not nullable
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                // It's nullable
                throw new Exception("Fatal error: Variable To Receive Value must be NOT nullable.");
            }

            // Initialize only if == typeof(String).
            if (typeof(T) == typeof(String)) variableToReceiveValue = (T)(object)String.Empty;

            // variableToReceiveValue must be not null.
            if (variableToReceiveValue == null)
                throw new ArgumentNullException("Fatal error: Variable To Receive Value must be Initialized with not null value.");

            // keyName must be not null.
            if (string.IsNullOrEmpty(keyName.Trim()))
                throw new ArgumentNullException("Fatal error: The keyName Variable can not be null or empty.");

            // Try to get the value of the Key in the appSettings and validate the value.

            // if !KeyIsOptional then validate that the key exist. 
            if ((!KeyIsOptional) && (ConfigurationManager.AppSettings[keyName] == null))
            {
                throw new Exception((string.Format(@"Fatal error: The application setting keyName = '{0}' is not optional and can not be loaded or does not exist in the .config file, Please review.", keyName)));
            }

            // Get the key value.
            string strValue = ConfigurationManager.AppSettings.Get(keyName);

            // if !valueCouldBeEmpty then validate the value.
            if (!valueCouldBeEmpty && (string.IsNullOrEmpty(strValue)))
                throw new Exception((string.Format(@"Fatal error: The application setting keyName = '{0}' value could not be empty or null, can not be loaded or does not exist in the .config file, Please review.", keyName)));

            // Get and Validate the variable type.
            switch (variableToReceiveValue.GetType().BaseType.ToString())
            {
                // Convert the value to a Enums Type.
                case "System.Enum":
                    variableToReceiveValue = (T)Enum.Parse(typeof(T), strValue, true);
                    break;

                case "System.Object":
                case "System.ValueType":
                    variableToReceiveValue = (T)Convert.ChangeType(strValue, typeof(T));
                    break;

                default:
                    // If this exception happend you must to try to implement the object.type here.
                    throw new Exception(String.Format(@"Vatiable of type:{0} is not supported in this method, Please update this method.", variableToReceiveValue.GetType().BaseType.ToString()));
            }
        }

        /// <summary>
        /// Gets the connection string settings.
        /// </summary>
        /// <param name="name">The name of the connection string.</param>
        /// <returns>a ConnectionStringSettings</returns>
        /// <exception cref="System.Exception"></exception>
        public static ConnectionStringSettings GetConnectionStringSettings(string name)
        {
            ConnectionStringSettings RetObj = ConfigurationManager.ConnectionStrings[name];
            if (RetObj == null || string.IsNullOrEmpty(RetObj.ConnectionString))
                throw new Exception(string.Format(@"Fatal error: missing connecting string name=""""{0}"""" in web.config file", name));
            return RetObj;
        }

        /// <summary>
        /// Invokes the process.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="useShellExecute">if set to <c>true</c> [use shell execute].</param>
        /// <param name="createNoWindow">if set to <c>true</c> [create no window].</param>
        /// <param name="windowStyle">The window style.</param>
        /// <param name="waitForExit">Wait for exit.</param>
        /// <returns></returns>
        public static string InvokeProcess(string fileName, string arguments, bool useShellExecute = false, bool createNoWindow = true, ProcessWindowStyle windowStyle = ProcessWindowStyle.Normal, int? waitForExit = null)
        {
            // create the ProcessStartInfo.
            ProcessStartInfo procStartInfo =
                new ProcessStartInfo(fileName, arguments);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = useShellExecute ? false : true;
            //procStartInfo.RedirectStandardOutput = true;

            procStartInfo.UseShellExecute = useShellExecute;
            //procStartInfo.UseShellExecute = false;

            // Do not create the black window.
            procStartInfo.CreateNoWindow = createNoWindow;

            procStartInfo.WindowStyle = windowStyle;

            // Now we create a process, assign its ProcessStartInfo and start it
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();

            if (waitForExit == null)
                proc.WaitForExit();
            else
                proc.WaitForExit((int)waitForExit);

            // Get the output into a string
            return procStartInfo.RedirectStandardOutput ? proc.StandardOutput.ReadToEnd() : string.Empty;
        }

        #region Tasks operations

        /// <summary>
        /// Starts the given tasks and waits for them to complete. This will run, at most, the specified number of tasks in parallel.
        /// <para>NOTE: If one of the given tasks has already been started, an exception will be thrown.</para>
        /// </summary>
        /// <param name="tasksToRun">The tasks to run.</param>
        /// <param name="maxTasksToRunInParallel">The maximum number of tasks to run in parallel.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task StartAndWaitAllThrottledAsync(IEnumerable<Task> tasksToRun, int maxTasksToRunInParallel, CancellationToken cancellationToken = new CancellationToken())
        {
            await StartAndWaitAllThrottledAsync(tasksToRun, maxTasksToRunInParallel, -1, cancellationToken);
        }

        /// <summary>
        /// Starts the given tasks and waits for them to complete. This will run the specified number of tasks in parallel.
        /// <para>NOTE: If a timeout is reached before the Task completes, another Task may be started, potentially running more than the specified maximum allowed.</para>
        /// <para>NOTE: If one of the given tasks has already been started, an exception will be thrown.</para>
        /// </summary>
        /// <param name="tasksToRun">The tasks to run.</param>
        /// <param name="maxTasksToRunInParallel">The maximum number of tasks to run in parallel.</param>
        /// <param name="timeoutInMilliseconds">The maximum milliseconds we should allow the max tasks to run in parallel before allowing another task to start. Specify -1 to wait indefinitely.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task StartAndWaitAllThrottledAsync(IEnumerable<Task> tasksToRun, int maxTasksToRunInParallel, int timeoutInMilliseconds, CancellationToken cancellationToken = new CancellationToken())
        {
            // Convert to a list of tasks so that we don't enumerate over it multiple times needlessly.
            var tasks = tasksToRun.ToList();

            using (var throttler = new SemaphoreSlim(maxTasksToRunInParallel))
            {
                var postTaskTasks = new List<Task>();

                // Have each task notify the throttler when it completes so that it decrements the number of tasks currently running.
                tasks.ForEach(t => postTaskTasks.Add(t.ContinueWith(tsk => throttler.Release())));

                // Start running each task.
                foreach (var task in tasks)
                {
                    // Increment the number of tasks currently running and wait if too many are running.
                    await throttler.WaitAsync(timeoutInMilliseconds, cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                    task.Start();
                }

                // Wait for all of the provided tasks to complete.
                // We wait on the list of "post" tasks instead of the original tasks, otherwise there is a potential race condition where the throttler&#39;s using block is exited before some Tasks have had their "post" action completed, which references the throttler, resulting in an exception due to accessing a disposed object.
                await Task.WhenAll(postTaskTasks.ToArray());
            }
        }

        #endregion Tasks operations
    }
}