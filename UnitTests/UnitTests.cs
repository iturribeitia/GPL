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
            // Create the reader.
            const String SQL_TEST_CONNECTIONSTRING = @"Data Source=EDGQN1PDDMSQL06;Initial Catalog=Temporary;Integrated Security=true;";
            const String SQLOLEDB_TEST_CONNECTIONSTRING = @"Provider=sqloledb;Packet Size=32767;Data Source=EDGQN1PDDMSQL06;Initial Catalog=Temporary;Integrated Security=SSPI;";
            const String SQL_PROD_CONNECTIONSTRING = @"Data Source=EDGQN1CPDMSQL09;Initial Catalog=Temporary;Integrated Security=true;";
            const String SQLOLEDB_PROD_CONNECTIONSTRING = @"Provider=sqloledb;Packet Size=32767;Data Source=EDGQN1CPDMSQL09;Initial Catalog=Temporary;Integrated Security=SSPI;";

            // create a connection object
            //var conn = new SqlConnection(SQL_TEST_CONNECTIONSTRING);
            var conn = new SqlConnection(SQL_PROD_CONNECTIONSTRING);
            //var conn = new OleDbConnection(SQLOLEDB_TEST_CONNECTIONSTRING);
            //var conn = new OleDbConnection(SQLOLEDB_PROD_CONNECTIONSTRING);

            // create a command object
            var cmd = new SqlCommand("select 'mar,cos\"iturri|beitia' as [Tes,tCo\"lu|mn], * from [Temporary].[dbo].[RPM_7SNEPC_Ols_Delivery]", conn);
            //var cmd = new OleDbCommand("select 'mar,cos\"iturri|beitia' as [Tes,tCo\"lu|mn], * from [Temporary].[dbo].[RPM_7SNEPC_Ols_Delivery]", conn);

            // open the connection
            conn.Open();

            // get an instance of the SqlDataReader
            var rdr = cmd.ExecuteReader();


            // export the reader 
            string TestFile = "c:\\temp\\test.csv";
            // CSV sample no textQualifier
            //rdr.ToDelimitedFile(TestFile, false, Encoding.Default, 1024 * 8, true);

            // CSV sample textQualifier = '"'
            //rdr.ToDelimitedFile(TestFile, false, Encoding.Default, 1024 * 8, true,textQualifier: "\"");

            // CSV sample textQualifier = '"'
            //rdr.ToDelimitedFile(TestFile, false, Encoding.Default, 1024 * 8, true, columnDelimiter: "|");
            //rdr.ToDelimitedFile(TestFile, false, Encoding.Default, 1024 * 8, true,textQualifier: "\"", columnDelimiter: "|");

           Int64 rows =  rdr.ToDelimitedFile(TestFile, false, Encoding.Default, 1024 * 8, true, columnDelimiter: "|");


            // open the connection
            rdr.Close();

            cmd.Dispose();

            conn.Close();
            conn.Dispose();

            var r = File.Exists(TestFile);
            Assert.IsInstanceOfType(r, typeof(bool));
            Assert.AreEqual(true, r);
        }
    }
}
