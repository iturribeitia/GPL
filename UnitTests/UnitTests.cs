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

namespace GPL.UnitTests
{
    [TestClass]
    public class UnitTests
    {
        const String SQL_SQLSERVER_LOCALDB_CONNECTIONSTRING = @"Packet Size=32767;Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\DataBases\Northwind.mdf;Database=Northwind;Integrated Security=True;Connect Timeout=30;";
        const String SQL_OLEDB_LOCALDB_CONNECTIONSTRING = @"Provider=sqloledb;Packet Size=32767;Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Northwind.mdf;Database=Northwind;Integrated Security=True;Connect Timeout=30;";
        const String SQL_ODBC_LOCALDB_CONNECTIONSTRING = @"Provider=Odbc;Driver={SQL Server};Packet Size=32767;Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Northwind.mdf;Database=Northwind;Integrated Security=True;Connect Timeout=30;";


        public UnitTests()
        {
            // Set the data folder for this application.
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data");

            AppDomain.CurrentDomain.SetData("DataDirectory", a);
        }

        [TestMethod]
        public void T001_Utility_GetDataTabletFromDelimitedFile()
        {
            var a = Utility.GetCurrentExecutablePath();
            var b = new FileInfo(a).Name;
            a = a.Replace(@"\bin\Debug\" + b, @"\App_Data\Files\DelimitedFiles\Sample_Pipe_and_Quote_With_Headers.txt");

            var r = Utility.GetDataTableFromDelimitedFile(a, true, true, '|', '"');
            Assert.ReferenceEquals(r, new DataTable());
        }
        [TestMethod]
        public void T002_Utility_FileToString()
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
        public void T003_Utility_GetCurrentExecutablePath()
        {
            var r = Utility.GetCurrentExecutablePath();

            Assert.IsInstanceOfType(r, typeof(string));
            Assert.IsNotNull(r);
            Assert.IsTrue(r.Contains(@"GPL.dll"));
            //Assert.AreEqual(228, r.Length);

        }

        [TestMethod]
        public void T004_Utility_RetryMethod()
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
        public void T005_Utility_InvokeProcess()
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
        public void T001_Extensions_TextReader_ReadLines()
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
        public void T002_Extensions_DirectoryInfo_CreateDirectory()
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
        public void T003_Extensions_IEnumerable_ToCSV()
        {
            List<string> L = new List<string>();

            L.Add("A");
            L.Add("B");
            L.Add("C");
            L.Add("D");

            var R = L.ToCSV();

            Assert.AreEqual("A,B,C,D", R);

            // Test the overload.
            R = L.ToCSV('|'); // pasing optional parameter separator.

            Assert.AreEqual("A|B|C|D", R);
        }

        [TestMethod]
        public void T004_Extensions_IDataReader_ToDelimitedFile()
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
        /// <summary>
        /// This is the destructor of this class.
        /// </summary>
        ~UnitTests()
        {
            // Detach the Northwind database.

            var CommandText = string.Format(@"
    USE MASTER;
    ALTER DATABASE {0} SET OFFLINE WITH ROLLBACK IMMEDIATE;
    EXEC sp_detach_db '{0}', 'true';", "Northwind");

            using (var dbh = new DBHelper(false))
            {
                dbh.CreateDBObjects(SQL_SQLSERVER_LOCALDB_CONNECTIONSTRING, DBHelper.Providers.SqlServer, null);

                var newfuction = new Func<string, CommandType, ConnectionState, int>(dbh.ExecuteNonQuery); // You can define the delegate before or inside of the RetryMethod.
                Utility.RetryMethod(newfuction, 3, 3, CommandText, CommandType.Text, ConnectionState.Open);
                //rdr = (DataSet)Utility.RetryMethod(newfuction, 3, 3, CmdTextWRONG, CommandType.Text, ConnectionState.Open);

                // Example with retries defining the delegate inside and imvoking the Utility.RetryMethod note that the return type 'DbDataReader' is declared at the end and the parameters types before.
                // DbDataReader rdr = (DbDataReader)Utility.RetryMethod(new Func<string, CommandType, ConnectionState, DbDataReader>(dbh.ExecuteReader), 3, 3, CmdTextOK, CommandType.Text, ConnectionState.Open);

            }
        }

    }
}
