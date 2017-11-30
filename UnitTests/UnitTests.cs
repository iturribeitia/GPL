using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPL;
using System.IO;
using System.Data;
using System.Text;

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
        public void T001_Extensions_ReadLines()
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
    }
}
