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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PoppuloAPI;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace GPL.UnitTests
{
    [TestClass]
   public class UT_PoppuloAPIClient
    {
        const string POPPULO_WEB_API_BASE_URL = @"https://api.us.newsweaver.com/v2/";
        const string POPPULO_ACCOUNTCODE = @"********";
        const string POPPULO_USERNAME = @"******";
        const string POPPULO_PASSWORD = @"****";

        /*
        Y29uc3Qgc3RyaW5nIFBPUFBVTE9fV0VCX0FQSV9CQVNFX1VSTCA9IEAiaHR0cHM6Ly9hcGkudXMubmV3c3dlYXZlci5jb20vdjIvIjsNCiAgICAgICAgY29uc3Qgc3RyaW5nIFBPUFBVTE9fQUNDT1VOVENPREUgPSBAInVsdGltYXRlYXBpdGVzdCI7DQogICAgICAgIGNvbnN0IHN0cmluZyBQT1BQVUxPX1VTRVJOQU1FID0gQCJhZC1hcGkyQHVsdGltYXRlc29mdHdhcmUuY29tIjsNCiAgICAgICAgY29uc3Qgc3RyaW5nIFBPUFBVTE9fUEFTU1dPUkQgPSBAImVmZEd5UnRzZnRkWjNoTDIiOw==
        */

        const string CONNECTION_STRING_FOR_EMPLOYEES_DATASOURCE = @"Server=FL1MUSGDBCL01\IETL;Database=ETLDB;Trusted_Connection=True;";
        //const string STORED_PROCEDURE_NAME_FOR_EMPLOYEES_DATASOURCE = @"[ETLDB].[Poppulo].[usp_Ultimate_Employee_To_Poppulo_Sel]";
        const string STORED_PROCEDURE_NAME_FOR_EMPLOYEES_DATASOURCE = @"[ETLDB].[Poppulo].[usp_Ultimate_Employee_To_Poppulo_Sel_With_Tags]";

        [TestInitialize]
        public void Initialize()
        {
            // Get the Ulti Safe Secret.
            //string strSafeSecret = PoppuloAPIClient.GetUltiSafeSecret(url, appId, userId, secretPath);

            // Extract Sensitive data from the secret.

            // You must to mannually remove this Assert when you are testing with ULTISAFE_PRODUCTION
            //var Comment = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "Comment");
            //Assert.IsTrue(Comment == "This are the credentials to consume the Poppulo development WEB API");

            //POPPULO_WEB_API_BASE_URL = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_WEB_API_BASE_URL");
            //POPPULO_ACCOUNTCODE = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_ACCOUNTCODE");
            //POPPULO_USERNAME = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_USERNAME");
            //POPPULO_PASSWORD = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_PASSWORD");
        }

        [TestMethod]
        public void TM_0010_PoppuloAPIClient_Constructor_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (Normal)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD);

            Type expectedType = typeof(PoppuloAPIClient);
            Assert.IsInstanceOfType(PC, expectedType);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "A endPointBaseURL of null was inappropriately allowed.")]
        public void TM_0020_PoppuloAPIClient_Constructor_endPointBaseURL_IsNull()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid baseURL parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(null, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "A AccountCode of null was inappropriately allowed.")]
        public void TM_0030_PoppuloAPIClient_Constructor_AccountCode_IsNull()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid AccountCode parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, null, POPPULO_USERNAME, POPPULO_PASSWORD);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "A UserName of null was inappropriately allowed.")]
        public void TM_0040_PoppuloAPIClient_Constructor_UserName_IsNull()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid UserName parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, null, POPPULO_PASSWORD);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "A Password of null was inappropriately allowed.")]
        public void TM_0050_PoppuloAPIClient_Constructor_Password_IsNull()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, null);
        }

        //
        [TestMethod]
        public void TM_0060_PoppuloAPIClient_Constructor_Using_HTTP()
        {
            // replace the HTTPS by HTTP to check if it is possible to authenticate over http.
            var HTTP_POPPULO_WEB_API_BASE_URL = Regex.Replace(POPPULO_WEB_API_BASE_URL, "https", "http", RegexOptions.IgnoreCase);

            // Create a new instance of the PoppuloAPIClient class.
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(HTTP_POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD);

            // Now do a GET operation to verify if it is possible over http not https.
            string Myresults = PC.GetAccount();

            // Assert by ExpectedHttpStatusCodes


            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.NotFound, HttpStatusCode.Forbidden, HttpStatusCode.InternalServerError };

            Assert.IsTrue(ExpectedHttpStatusCodes.Contains(PC.LastHttpStatusCode));
        }
        //

        [TestMethod]
        public void TM_0070_URLDomainExist()
        {
            Assert.IsTrue(PoppuloAPIClient.URLDomainExist("https://www.google.com/"));
            Assert.IsFalse(PoppuloAPIClient.URLDomainExist("https://www.UUUUgoogle.com/"));
            Assert.IsFalse(PoppuloAPIClient.URLDomainExist("@#$%^"));
        }

        [TestMethod]
        public void TM_0080_GetAccount_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // Execute the method.
            string Myresults = PC.GetAccount();

            Type expectedType = typeof(string);
            Assert.IsInstanceOfType(Myresults, expectedType);     //passes
            Assert.AreEqual(200, (int)PC.LastHttpStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, PC.LastHttpStatusCode);
            Assert.AreEqual(@"OK", PC.LastHttpStatusDescription);
            Assert.IsTrue(Myresults.Contains(@"<name>Ultimate Software API Test Account</name>"));
            Assert.IsTrue(PoppuloAPIClient.IsXML(Myresults));

        }

        [TestMethod]
        public void TM_0090_ListAccount_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (No errors expected for this Unit Test method.)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // Execute the method.
            string MyRetVal = PC.ListAccount();


            // Assert by ExpectedValues
            string[] ExpectedValues = new[] { @"<accounts total=" };


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);     //passes

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);

        }

        [TestMethod]
        public void TM_0091_ListTags_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (No errors expected for this Unit Test method.)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // Execute the method.
            string MyRetVal = PC.ListTags();


            // Assert by ExpectedValues
            string[] ExpectedAnyOfThisValues = new[] { @"<tags total=" };


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            bool ExpectedAnyOfTheValueFound = false;

            foreach (var item in ExpectedAnyOfThisValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedAnyOfTheValueFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedAnyOfTheValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);

        }

        [TestMethod]
        public void TM_0092_GetTag_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (No errors expected for this Unit Test method.)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // first create the unit test tag.
            PC.CreateTag(@"Tag_For_Unit_Test_Name", @"Tag_For_Unit_Test_Description");

            // Execute the method.
            string MyRetVal = PC.GetTag(@"Tag_For_Unit_Test_Name");

            // Assert by ExpectedValues
            string[] ExpectedAnyOfThisValues = new[] { @"<tag name=" };

            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            bool ExpectedAnyOfTheValueFound = false;

            foreach (var item in ExpectedAnyOfThisValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedAnyOfTheValueFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedAnyOfTheValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);

            PC.DeleteTag(@"Tag_For_Unit_Test_Name");

        }

        [TestMethod]
        public void TM_0093_CreateTag_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (No errors expected for this Unit Test method.)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // Delete the tag first if it exist.
            string MyRetVal = PC.DeleteTag(@"Tag_For_Unit_Test_Name");

            // Execute the method.
            MyRetVal = PC.CreateTag(@"Tag_For_Unit_Test_Name", @"Tag_For_Unit_Test_Name");


            // Assert by ExpectedValues
            string[] ExpectedAnyOfThisValues = new[] { @"<code>201</code><resources_created>" };


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            bool ExpectedAnyOfTheValueFound = false;

            foreach (var item in ExpectedAnyOfThisValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedAnyOfTheValueFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedAnyOfTheValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.Created };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);

            // Delete the tag first if it exist.
            PC.DeleteTag(@"Tag_For_Unit_Test_Name");

        }

        [TestMethod]
        public void TM_0094_UpdateTag_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (No errors expected for this Unit Test method.)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // first create the unit test tag.
            string MyRetVal = PC.CreateTag(@"Tag_For_Unit_Test_Name", @"Tag_For_Unit_Test_Description");

            // Execute the method.
            MyRetVal = PC.UpdateTag(@"Tag_For_Unit_Test_Name", @"Tag_For_Unit_Test_Name_Updated", @"Tag_For_Unit_Test_Description_Updated");

            // Assert by ExpectedValues
            string[] ExpectedAnyOfThisValues = new[] { @"<code>200</code><resources_updated>" };

            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            bool ExpectedAnyOfTheValueFound = false;

            foreach (var item in ExpectedAnyOfThisValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedAnyOfTheValueFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedAnyOfTheValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);

            MyRetVal = PC.DeleteTag(@"Tag_For_Unit_Test_Name_Updated");
        }

        [TestMethod]
        public void TM_0095_DeleteTag_OK()
        {
            // Create a new instance of the PoppuloAPIClient class. (No errors expected for this Unit Test method.)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // first create the unit test tag.
            string MyRetVal = PC.CreateTag(@"Tag_For_Unit_Test_Name", @"Tag_For_Unit_Test_Description");

            // Execute the method.
            MyRetVal = PC.DeleteTag(@"Tag_For_Unit_Test_Name");

            // Assert by ExpectedValues
            string[] ExpectedAnyOfThisValues = new[] { @"<code>200</code><resources_deleted>" };


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            bool ExpectedAnyOfTheValueFound = false;

            foreach (var item in ExpectedAnyOfThisValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedAnyOfTheValueFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedAnyOfTheValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);
        }

        [TestMethod]
        public void TM_0100_GetAccount_Account_No_Exist()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid AccountCode parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, "CheckNoexist" + POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Execute the method.
            string MyRetVal = PC.GetAccount();

            string[] ExpectedValues = new[] { @"<status><code>403</code>" };


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);     //passes

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.Forbidden };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);
        }

        [TestMethod]
        public void TM_0110_GetSubscriberByEmail()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Execute the method.
            string MyRetVal = PC.GetSubscriberByEmail(@"testsubscriber01@testdomain.com");
            //string MyRetVal = PC.GetSubscriberByEmail(@"TEST_20180607_01@ultimatesoftware.com");



            string[] ExpectedValues = new[] { @"<email>testsubscriber01@testdomain.com</email>" };


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);     //passes

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);
        }

        [TestMethod]
        public void TM_0111_GetSubscriberByexternal_id_DoNotExists()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Execute the method.
            string MyRetVal = PC.GetSubscriberByExternal_id(@"xxxSomeValue", SubscriberStatus.all);

            string[] ExpectedValues = new[] { @"<totalResults>0</totalResults>" };

            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);
        }

        [TestMethod]
        public void TM_0112_GetSubscriberByexternal_id_Exists()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Execute the method.
            string MyRetVal = PC.GetSubscriberByExternal_id(@"SomeValue", SubscriberStatus.all);

            // Verify the returned type
            Type ExpectedType = typeof(string);
            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            // verify that the returned string is a XML.
            Assert.IsTrue(PoppuloAPIClient.IsXML(MyRetVal));

            // verify that returned value contains one of the expected values.
            string[] ExpectedValues = new[] { @"<email>testsubscriber01@testdomain.com</email>" };
            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.OK };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(MyRetVal);

            // Validate the <subscribers total="1" attribute 
            string attrVal = doc.SelectSingleNode("/subscribers/@total").Value;

            Assert.IsTrue(attrVal == "1");

            // Validate the <totalResults>1</totalResults> Node 
            string nodeVal = doc.SelectSingleNode("/subscribers/totalResults").InnerText;

            Assert.IsTrue(nodeVal == "1");

        }


        [TestMethod]
        public void TM_0120_GetSubscriberByEmail_NoExist()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Execute the method.
            string MyRetVal = PC.GetSubscriberByEmail(@"NoExist@testdomain.com");

            string[] ExpectedValues = new[] { @"<status><code>404</code>" }; //No Found.


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);     //passes

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.NotFound };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);
        }

        [TestMethod]
        public void TM_0130_ListSubscriberPermissionsByEmail_OK()
        {
            // todo review this test why API account does not have access and return 403.

            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Execute the method.
            string MyRetVal = PC.ListSubscriberPermissionsByEmail(@"testsubscriber01@testdomain.com");

            string[] ExpectedValues = new[] { @"<status><code>403</code>", @"<status><code>404</code>" };


            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);     //passes

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.Forbidden, HttpStatusCode.NotFound };

            bool ExpectedHttpStatusCodeFound = false;

            foreach (var item in ExpectedHttpStatusCodes)
            {
                if (PC.LastHttpStatusCode.Equals(item))
                {
                    ExpectedHttpStatusCodeFound = true;
                    break;
                }
            }
            Assert.IsTrue(ExpectedHttpStatusCodeFound);

        }

        [TestMethod]
        public void TM_0140_ListSubscriberPermissionsByEmail_NoExist()
        {
            // Create a new instance of the PoppuloAPIClient class. (Invalid Password parameter)
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Execute the method.
            string MyRetVal = PC.ListSubscriberPermissionsByEmail(@"NoExist@testdomain.com");

            string[] ExpectedValues = new[] { @"<status><code>403</code>", @"<status><code>404</code>" };

            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);     //passes

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes ++++
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.NotFound, HttpStatusCode.Forbidden };

            Assert.IsTrue(ExpectedHttpStatusCodes.Contains(PC.LastHttpStatusCode));

        }

        [TestMethod]
        public void TM_0150_CreateSubscriber()
        {
            // Create a instance on the PoppuloAPIClient.
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true);

            // Get the string from the template
            StringBuilder MySubscriber = new StringBuilder(PoppuloAPIClient.XML_Subscriber_Template);

            // Get today date in ISO 8601 date in string format.
            DateTime TodayDateTime = DateTime.UtcNow;
            string TodayDateTime_ISO_8601 = DateTime.UtcNow.ToISO8601();

            string subscriberID = null;

            if (string.IsNullOrEmpty(subscriberID))
                subscriberID = PoppuloAPIClient.RandomString(10);

            // Start replacing the template with values.

            MySubscriber.Replace("{date_modified}", TodayDateTime_ISO_8601);
            MySubscriber.Replace("{subscriber id}", subscriberID);
            MySubscriber.Replace("{city}", "city_Value");
            MySubscriber.Replace("{email}", $"Test_{subscriberID}@TestDomain.com");
            MySubscriber.Replace("{status}", SubscriberStatus.active.ToString()); // todo test this with the other status. 
            MySubscriber.Replace("{surname}", "surname_Value"); // this is the Last Name
            MySubscriber.Replace("{company}", "company_Value");
            MySubscriber.Replace("{address1}", "address1_Value");
            MySubscriber.Replace("{address2}", "address2_Value");
            MySubscriber.Replace("{address3}", "address3_Value");
            MySubscriber.Replace("{first_name}", "first_name_Value");
            MySubscriber.Replace("{salutation}", "salutation_Value");
            MySubscriber.Replace("{middle_name}", "middle_name_Value");
            MySubscriber.Replace("{postal_code}", "postal_code_Value");
            MySubscriber.Replace("{external_id}", "external_id_Value"); // This is the Employee Number
            MySubscriber.Replace("{phone_number}", "phone_number_Value");
            MySubscriber.Replace("{county_state}", "county_state_Value");
            MySubscriber.Replace("{date_created}", TodayDateTime_ISO_8601);
            MySubscriber.Replace("{position_company}", "position_company_Value");
            MySubscriber.Replace("{preferred_name}", "preferred_name_Value");
            MySubscriber.Replace("{gender}", SubscriberGender.Male.ToString()); // must be Male or Female or empty
            MySubscriber.Replace("{date_of_birth}", TodayDateTime_ISO_8601);
            MySubscriber.Replace("{employee_start_date}", TodayDateTime_ISO_8601);
            MySubscriber.Replace("{region}", "region_Value");

            // Set the custom_fields.
            MySubscriber.Replace("{Employee Type}", "Employee Type_Value");
            MySubscriber.Replace("{Full / Part Time}", "Full / Part Time_Value");
            MySubscriber.Replace("{Level}", "Level_Value");
            MySubscriber.Replace("{People Manager}", "false");


            XmlDocument MyXMLSubscriber = new XmlDocument();
            MyXMLSubscriber.LoadXml(MySubscriber.ToString());

            // Call the API
            string MyRetVal = PC.CreateSubscriber(MyXMLSubscriber);



            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);     //passes

            bool ExpectedValueFound = false;

            string[] ExpectedValues = new[] { @"<status><code>200</code>", @"<status><code>201</code>" };

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Assert by ExpectedHttpStatusCodes 
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.Created };

            Assert.IsTrue(ExpectedHttpStatusCodes.Contains(PC.LastHttpStatusCode));
        }

        [TestMethod]
        public void TM_0160_Synchronize_Employees()
        {
            //Create a new instance of the PoppuloAPIClient class.
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // Set the connection string and the stored procedure to get Employees data
            PC.ConnectionStringForDataSource = CONNECTION_STRING_FOR_EMPLOYEES_DATASOURCE;
            PC.StoredProcedureNameForDataSource = STORED_PROCEDURE_NAME_FOR_EMPLOYEES_DATASOURCE;

            //Execute the method.
            PC.SynchronizeDataSourceAsSubscribers();

            string MyRetVal = PC.LastHttpWebResponseStream;

            // Set the expected values & types. 
            string[] ExpectedValues = new[] { @"<status><code>202</code>" };

            Type ExpectedType = typeof(string);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            bool ExpectedValueFound = false;

            foreach (var item in ExpectedValues)
            {
                if (MyRetVal.Contains(item))
                {
                    ExpectedValueFound = true;
                    break;
                }
            }

            Assert.IsTrue(ExpectedValueFound);

            // Set the expected HttpStatusCodes.
            HttpStatusCode[] ExpectedHttpStatusCodes = new[] { HttpStatusCode.Accepted };

            Assert.IsTrue(ExpectedHttpStatusCodes.Contains(PC.LastHttpStatusCode));
        }

        //[TestMethod]
        //public void TM_0200_GetUltiSafeSecret()
        //{
        //    // Get the Ulti Safe Secret.
        //    string strSafeSecret = PoppuloAPIClient.GetUltiSafeSecret(url, appId, userId, secretPath);

        //    // Extract Sensitive data from the secret.
        //    string POPPULO_WEB_API_BASE_URL = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_WEB_API_BASE_URL");
        //    string POPPULO_ACCOUNTCODE = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_ACCOUNTCODE");
        //    string POPPULO_USERNAME = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_USERNAME");
        //    string POPPULO_PASSWORD = PoppuloAPIClient.GetValueFromJSON(strSafeSecret, "POPPULO_PASSWORD");

        //    Assert.IsFalse(string.IsNullOrEmpty(strSafeSecret));
        //    Assert.IsFalse(string.IsNullOrEmpty(POPPULO_WEB_API_BASE_URL));
        //    Assert.IsFalse(string.IsNullOrEmpty(POPPULO_ACCOUNTCODE));
        //    Assert.IsFalse(string.IsNullOrEmpty(POPPULO_USERNAME));
        //    Assert.IsFalse(string.IsNullOrEmpty(POPPULO_PASSWORD));
        //}

        [TestMethod]
        public void TM_0210_SynchronizeDataSourceAsTags()
        {
            //Create a new instance of the PoppuloAPIClient class.
            PoppuloAPIClient PC = new PoppuloAPI.PoppuloAPIClient(POPPULO_WEB_API_BASE_URL, POPPULO_ACCOUNTCODE, POPPULO_USERNAME, POPPULO_PASSWORD, true, true);

            // Set the connection string and the stored procedure to get Employees data
            PC.ConnectionStringForDataSource = CONNECTION_STRING_FOR_EMPLOYEES_DATASOURCE;
            PC.StoredProcedureNameForDataSource = STORED_PROCEDURE_NAME_FOR_EMPLOYEES_DATASOURCE;

            //Execute the method.
            var MyRetVal = PC.SynchronizeDataSourceAsTags();

            Type ExpectedType = typeof(System.Collections.Generic.List<string>);

            Assert.IsInstanceOfType(MyRetVal, ExpectedType);

            Assert.IsTrue(MyRetVal.Count > 0);
        }

    }
}
