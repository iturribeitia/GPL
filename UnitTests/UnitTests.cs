using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPL;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace GPL.UnitTests
{
    [TestClass]
    public class UnitTests
    {
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
            // Create the connection strings.
            
            // XX sec
            const String SQL_PROD_CONNECTIONSTRING = @"Data Source=EDGQN1CPDMSQL09;Initial Catalog=Temporary;Integrated Security=true;";

            // XX sec
            const String SQLOLEDB_PROD_CONNECTIONSTRING = @"Provider=sqloledb;Packet Size=32767;Data Source=EDGQN1CPDMSQL09;Initial Catalog=Temporary;Integrated Security=SSPI;";

            // 21 sec
            //const string ODBCSQLServer = "Driver={SQL Server};Server=EDGQN1CPDMSQL09;Database=Temporary;Trusted_Connection = Yes; ";

            // 22 sec
            //const string ODBCSQLServer = "Driver={ODBC Driver 13 for SQL Server};Server=EDGQN1CPDMSQL09;Database=Temporary;Trusted_Connection=yes;";

            // 19 sec
            const string ODBCSQLServer = "Driver={SQL Server Native Client 11.0};Server=EDGQN1CPDMSQL09;Database=Temporary;Trusted_Connection=yes;";

            const long RowsToRead = 100000;

            string CmdText = "select top " + RowsToRead.ToString() + " * from[Temporary].[dbo].[RPM_7SNEPC_Ols_Delivery]";

            const int BufferSize = 1024 * 4;

            const string TestFile = "c:\\temp\\test.csv";

            //var Provider = DBHelper.Providers.OleDB;   // 12 sec
            var Provider = DBHelper.Providers.SqlServer; // 08 sec
            //var Provider = DBHelper.Providers.ODBC; // 20 sec

            string Cnstring = string.Empty;

            switch (Provider)
            {
                case DBHelper.Providers.SqlServer:
                    Cnstring = SQL_PROD_CONNECTIONSTRING;
                    break;
                case DBHelper.Providers.OleDB:
                    Cnstring = SQLOLEDB_PROD_CONNECTIONSTRING;
                    break;
                case DBHelper.Providers.ODBC:
                    Cnstring = ODBCSQLServer;
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

                var rdr = dbh.ExecuteReader(CmdText, CommandType.Text, ConnectionState.Open);

                FileRows = rdr.ToDelimitedFile(TestFile, false, Encoding.Default, BufferSize, true, columnDelimiter: "|");

            }

            var r = File.Exists(TestFile);

            Assert.IsInstanceOfType(r, typeof(bool));
            Assert.AreEqual(true, r);
            Assert.AreEqual(RowsToRead, FileRows);

        }
    }
}
