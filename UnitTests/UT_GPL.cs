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
    the Free Software Foundation, either version of the License.

    GPL is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with GPL.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPL;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Data.Common;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json;
using System.Drawing;

namespace GPL.UnitTests
{
    [TestClass]
    public class UT_GPL
    {
        const String SQL_SQLSERVER_LOCALDB_CONNECTIONSTRING = @"Packet Size=32767;Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\DataBases\Northwind.mdf;Database=Northwind;Integrated Security=True;Connect Timeout=30;";
        const String SQL_OLEDB_LOCALDB_CONNECTIONSTRING = @"Provider=sqloledb;Packet Size=32767;Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Northwind.mdf;Database=Northwind;Integrated Security=True;Connect Timeout=30;";
        const String SQL_ODBC_LOCALDB_CONNECTIONSTRING = @"Provider=Odbc;Driver={SQL Server};Packet Size=32767;Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Northwind.mdf;Database=Northwind;Integrated Security=True;Connect Timeout=30;";


        public UT_GPL()
        {
            // Set the data folder for this application.
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data");

            AppDomain.CurrentDomain.SetData("DataDirectory", a);
        }

        [TestMethod]
        public void LimitedConcurrencyLevelTaskScheduler_T001()
        {
            // Create a scheduler that uses two threads. 
            LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(2);
            List<Task> tasks = new List<Task>();

            // Create a TaskFactory and pass it our custom scheduler. 
            TaskFactory factory = new TaskFactory(lcts);
            CancellationTokenSource cts = new CancellationTokenSource();

            // Use our factory to run a set of tasks. 
            Object lockObj = new Object();
            int outputItem = 0;

            for (int tCtr = 0; tCtr <= 4; tCtr++)
            {
                int iteration = tCtr;
                Task t = factory.StartNew(() =>
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        lock (lockObj)
                        {
                            //Console.Write("{0} in task t-{1} on thread {2}   ", i, iteration, Thread.CurrentThread.ManagedThreadId);
                            Debug.Write(string.Format("{0} in task t-{1} on thread {2}   ", i, iteration, Thread.CurrentThread.ManagedThreadId));
                            outputItem++;
                            if (outputItem % 3 == 0)
                                Debug.WriteLine("");
                        }
                    }
                }, cts.Token);
                tasks.Add(t);
            }
            // Use it to run a second set of tasks.                       
            for (int tCtr = 0; tCtr <= 4; tCtr++)
            {
                int iteration = tCtr;
                Task t1 = factory.StartNew(() =>
                {
                    for (int outer = 0; outer <= 10; outer++)
                    {
                        for (int i = 0x21; i <= 0x7E; i++)
                        {
                            lock (lockObj)
                            {
                                //Console.Write("'{0}' in task t1-{1} on thread {2}   ", Convert.ToChar(i), iteration, Thread.CurrentThread.ManagedThreadId);
                                Debug.Write(string.Format("'{0}' in task t1-{1} on thread {2}   ", Convert.ToChar(i), iteration, Thread.CurrentThread.ManagedThreadId));
                                outputItem++;
                                if (outputItem % 3 == 0)
                                    Debug.WriteLine("");
                            }
                        }
                    }
                }, cts.Token);
                tasks.Add(t1);
            }

            // Wait for the tasks to complete before displaying a completion message.
            Task.WaitAll(tasks.ToArray());
            cts.Dispose();
            Debug.WriteLine("\n\nSuccessful completion.");
        }

        [TestMethod]
        public void Utility_T001_GetDataTabletFromDelimitedFile()
        {
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data\Files\DelimitedFiles\Sample_Pipe_and_Quote_With_Headers.txt");

            var r = Utility.GetDataTableFromDelimitedFile(a, true, true, '|', '"');
            Assert.ReferenceEquals(r, new DataTable());
        }
        [TestMethod]
        public void Utility_T002_FileToString()
        {
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data\Files\DelimitedFiles\Sample_Pipe_and_Quote_With_Headers.txt");

            var r = Utility.FileToString(a);
            Assert.IsInstanceOfType(r, typeof(string));
            Assert.IsNotNull(r);
            Assert.AreEqual(228, r.Length);

        }

        [TestMethod]
        public void Utility_T003_GetCurrentExecutablePath()
        {
            var r = Utility.GetCurrentExecutablePath();

            Assert.IsInstanceOfType(r, typeof(string));
            Assert.IsNotNull(r);
            Assert.IsTrue(r.Contains(@"GPL.dll"));
            //Assert.AreEqual(228, r.Length);

        }

        [TestMethod]
        public void Utility_T004_RetryMethod()
        {
            const long RowsToRead = 90;

            string CmdTextOK = "select top " + RowsToRead.ToString() + " * from [Northwind].[dbo].[Customers]";
            string CmdTextWRONG = "select top " + RowsToRead.ToString() + " * [Northwind].[dbo].[XXXX]";


            //var Provider = DBHelper.Providers.OleDB;   // 12 sec
            var Provider = DBHelper.Providers.SqlServer; // 08 sec
            //var Provider = DBHelper.Providers.ODBC; // 20 sec

            string Cnstring = string.Empty;

            switch (Provider)
            {
                case DBHelper.Providers.SqlServer:
                    Cnstring = SQL_SQLSERVER_LOCALDB_CONNECTIONSTRING;
                    break;
                case DBHelper.Providers.OleDB:
                    Cnstring = SQL_OLEDB_LOCALDB_CONNECTIONSTRING;
                    break;
                case DBHelper.Providers.ODBC:
                    Cnstring = SQL_ODBC_LOCALDB_CONNECTIONSTRING;
                    break;
                case DBHelper.Providers.Oracle:
                    break;
                default:
                    break;
            }

            // create a connection object
            using (var dbh = new DBHelper(false))
            {
                dbh.CreateDBObjects(Cnstring, Provider, null);

                // Example without retries.
                //var rdr = dbh.ExecuteReader(CmdText, CommandType.Text, ConnectionState.Open);

                // Example with retries defining the delegate before and invoking in the Utility.RetryMethod note that the return type 'DataSet' is declared at the end, and the parameters types before.
                var newfuction = new Func<string, CommandType, ConnectionState, DataSet>(dbh.GetDataSet); // You can define the delegate before or inside of the RetryMethod.
                DataSet rdr = (DataSet)Utility.RetryMethod(newfuction, 3, 3, CmdTextOK, CommandType.Text, ConnectionState.Open);
                //rdr = (DataSet)Utility.RetryMethod(newfuction, 3, 3, CmdTextWRONG, CommandType.Text, ConnectionState.Open);

                // Example with retries defining the delegate inside and imvoking the Utility.RetryMethod note that the return type 'DbDataReader' is declared at the end and the parameters types before.
                // DbDataReader rdr = (DbDataReader)Utility.RetryMethod(new Func<string, CommandType, ConnectionState, DbDataReader>(dbh.ExecuteReader), 3, 3, CmdTextOK, CommandType.Text, ConnectionState.Open);

                Assert.IsInstanceOfType(rdr, typeof(DataSet));
                Assert.AreEqual(RowsToRead, rdr.Tables[0].Rows.Count);
            }
        }

        [TestMethod]
        public void Utility_T005_InvokeProcess()
        {
            //const string FileName = @"C:\Program Files\7-Zip\7zG.exe";
            //const string Arguments = @"a -mx1 -tzip ""C:\Temp\test.ZIP""  ""C:\Temp\test.csv""";

            const string FileName = @"cmd";
            const string Arguments = @" /c " + "dir";

            string r;

            r = Utility.InvokeProcess(FileName, Arguments);
            r = Utility.InvokeProcess(FileName, Arguments, true);
            r = Utility.InvokeProcess(FileName, Arguments, true, false);
            r = Utility.InvokeProcess(FileName, Arguments, true, false, System.Diagnostics.ProcessWindowStyle.Maximized, 50000);

            Assert.IsInstanceOfType(r, typeof(object));

        }

        [TestMethod]
        public void Utility_T006_URLDomainExist()
        {
            Assert.IsTrue(Utility.URLDomainExist("https://www.google.com/"));
            Assert.IsFalse(Utility.URLDomainExist("https://www.UUUUgoogle.com/"));
            Assert.IsFalse(Utility.URLDomainExist("@#$%^"));
        }

        [TestMethod]
        public void Utility_T007_GetDataSetFromJson()
        {
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data\Files\DataFiles\Computer.json");

            string json = System.IO.File.ReadAllText(a);

            var Result = Utility.GetDataSetFromJson(json);

            Assert.IsInstanceOfType(Result, typeof(DataSet));
        }

        [TestMethod]
        public void Extensions_T001_TextReader_ReadLines()
        {
            //string a = @"\\app.diablo.corelogic.com\LTL\FULFILLMENTS\CMAS\Ohio Housing Finance Agency\AKZA-8N1JW\OHFA_HHF_CoreLogic_20170719\master_pii_table.dat";

            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data\Files\DelimitedFiles\Sample_Pipe_and_Quote_With_Headers.txt");
            bool r = false;

            using (StreamReader sr = new StreamReader(a, Encoding.Default))
            {
                foreach (var line in sr.ReadLines('\n'))
                {
                    r = line.EndsWith("\r");
                    break;

                }

                Assert.IsTrue(r);

            }


        }

        [TestMethod]
        public void Extensions_T002_DirectoryInfo_CreateDirectory()
        {
            string a = @"C:\temp\dir1";
            string b = @"C:\temp\dir1\dir2\dir3";

            //string a = @"\\App.Diablo.Corelogic.com\LTL\TEST\RPM Direct";
            //string b = @"\\App.Diablo.Corelogic.com\LTL\TEST\RPM Direct\AKZA-7SNEPC\CountFiles";

            var adi = new DirectoryInfo(a);
            var bdi = new DirectoryInfo(b);

            if (adi.Exists)
            {
                adi.Delete(true);
                adi.Refresh();
            }

            Assert.IsFalse(bdi.Exists);

            bdi.CreateDirectory();

            // Refresh the DirectoryInfo
            bdi.Refresh();
            adi.Refresh();

            Assert.IsTrue(bdi.Exists);

            if (adi.Exists)
                adi.Delete(true);
        }

        [TestMethod]
        public void Extensions_T003_IEnumerable_ToCSV()
        {
            List<string> L = new List<string>();

            L.Add("A");
            L.Add("B");
            L.Add("C");
            L.Add("D");

            var R = L.ToCSV();

            Assert.AreEqual("A,B,C,D", R);

            // Test the overload.
            R = L.ToCSV("|"); // pasing optional parameter separator.

            Assert.AreEqual("A|B|C|D", R);
        }

        [TestMethod]
        public void Extensions_T004_IDataReader_ToDelimitedFile()
        {
            const long RowsToRead = 50;

            string CmdText = "select top " + RowsToRead.ToString() + " * from [Northwind].[dbo].[Customers]";

            const int BufferSize = 1024 * 4;

            const string TestFile = "c:\\temp\\test.csv";

            //var Provider = DBHelper.Providers.OleDB;   // 12 sec
            var Provider = DBHelper.Providers.SqlServer; // 08 sec
            //var Provider = DBHelper.Providers.ODBC; // 20 sec

            string Cnstring = string.Empty;

            switch (Provider)
            {
                case DBHelper.Providers.SqlServer:
                    //Cnstring = SQL_PROD_CONNECTIONSTRING;
                    Cnstring = SQL_SQLSERVER_LOCALDB_CONNECTIONSTRING;

                    break;
                case DBHelper.Providers.OleDB:
                    Cnstring = SQL_OLEDB_LOCALDB_CONNECTIONSTRING;
                    break;
                case DBHelper.Providers.ODBC:
                    Cnstring = SQL_ODBC_LOCALDB_CONNECTIONSTRING;
                    break;
                case DBHelper.Providers.Oracle:
                    break;
                default:
                    break;
            }

            long FileRows = 0;

            // create a connection object
            using (var dbh = new DBHelper(false))
            {
                dbh.CreateDBObjects(Cnstring, Provider, null);

                DbDataReader rdr = (DbDataReader)Utility.RetryMethod(new Func<string, CommandType, ConnectionState, DbDataReader>(dbh.ExecuteReader), 3, 3, CmdText, CommandType.Text, ConnectionState.Open);

                FileRows = rdr.ToDelimitedFile(TestFile, false, Encoding.Default, BufferSize, true, textQualifier: "\"", columnDelimiter: "|");
            }

            var r = File.Exists(TestFile);

            Assert.IsInstanceOfType(r, typeof(bool));
            Assert.AreEqual(true, r);
            Assert.AreEqual(RowsToRead, FileRows);

        }

        [TestMethod]
        public void Extensions_T005_string_GetSubstringByString()
        {
            string Mystring = "Edit: Subscriber (54vtpmcj7l8)";

            var Myresult = Mystring.GetSubstringByString("(", ")");

            Assert.IsTrue(Myresult.Equals("54vtpmcj7l8"));

        }

        [TestMethod]
        public void Extensions_T006_StartAndWaitAllThrottled()
        {
            // StartAndWaitAllThrottledAsync

            // Create a list of tasks to control
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 25; i++)
            {
                var t = new Task(() =>
                {
                    CountAndWait(10, 5000);
                });


                tasks.Add(t);

            }


            Task T = Utility.StartAndWaitAllThrottledAsync(tasks, 10);

            Task.WaitAll(tasks.ToArray());

            string waithere = "";
        }

        [TestMethod]
        public void Extensions_T007_Stream_ToDataTable()
        {
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data\Files\DelimitedFiles\Sample_CSV_File.csv");

            // get a Stream from the file.
            using (FileStream stream = File.Open(a, FileMode.Open))
            {
                var r = stream.ToDataTable(true, true, ',', '"');

                Assert.ReferenceEquals(r, new DataTable());
            }
        }

        [TestMethod]
        public void Extensions_T008_ParseFromJson()
        {
            using (HttpClient client = new HttpClient())
            {
                const string url = "http://www.json-generator.com/api/json/get/bVHreokWmq?indent=2";
                //using (HttpResponseMessage response =  client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result)
                using (HttpResponseMessage response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult())
                //using (Stream streamToReadFrom = response.Content.ReadAsStreamAsync().Result)
                using (Stream streamToReadFrom = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    dynamic dynamicObj = streamToReadFrom.DeserializeJsonFromStream<dynamic>();

                    string validString = JsonConvert.SerializeObject(dynamicObj);

                    Assert.IsTrue(validString.Length > 500);

                    Assert.IsTrue(validString.IsValidJson());
                }
            }
        }

        [TestMethod]
        public void Extensions_T009_ImageToString()
        {
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data\Files\Images\Habibi.jpg");

            // Load the image from a file
            Image i = Image.FromFile(a);

            Assert.IsInstanceOfType(i, typeof(Image));

            // Now convert the Image to string.
            var strImage = i.ImageToString();

            Assert.IsInstanceOfType(strImage, typeof(string));

            // Now load the Image from the string
            var ii = strImage.StringToImage();

            Assert.IsInstanceOfType(ii, typeof(Image));

        }

        [TestMethod]
        public void Extensions_T010_In()
        {
            bool RetVal = false;

            string myStr = "str3";

            RetVal = myStr.In("str1", "str2", "str3", "str4");

            Assert.IsTrue(RetVal);
        }

        private void CountAndWait(int count = 10, int milisecondsToWait = 5000)
        {
            for (int i = 0; i < count; i++)
            {
                Trace.WriteLine($"count = {i} milisecondsToWait = {milisecondsToWait}");
                Thread.Sleep(milisecondsToWait);
            }
        }


        /// <summary>
        /// This is the destructor of this class.
        /// </summary>
        ~UT_GPL()
        {
            // Detach the Northwind database.

            var CommandText = string.Format(@"
USE MASTER;
	
IF db_id('{0}') IS NOT NULL
    BEGIN
        ALTER DATABASE {0} SET OFFLINE WITH ROLLBACK IMMEDIATE;
        EXEC sp_detach_db '{0}', 'true';
    END
", "Northwind");

            using (var dbh = new DBHelper(false))
            {
                dbh.CreateDBObjects(SQL_SQLSERVER_LOCALDB_CONNECTIONSTRING, DBHelper.Providers.SqlServer, null);

                var newfuction = new Func<string, CommandType, ConnectionState, int>(dbh.ExecuteNonQuery); // You can define the delegate before or inside of the RetryMethod.
                Utility.RetryMethod(newfuction, 3, 3, CommandText, CommandType.Text, ConnectionState.Closed);
                //rdr = (DataSet)Utility.RetryMethod(newfuction, 3, 3, CmdTextWRONG, CommandType.Text, ConnectionState.Open);

                // Example with retries defining the delegate inside and imvoking the Utility.RetryMethod note that the return type 'DbDataReader' is declared at the end and the parameters types before.
                // DbDataReader rdr = (DbDataReader)Utility.RetryMethod(new Func<string, CommandType, ConnectionState, DbDataReader>(dbh.ExecuteReader), 3, 3, CmdTextOK, CommandType.Text, ConnectionState.Open);

            }
        }

    }
}
