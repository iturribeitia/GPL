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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using GPL;

namespace PoppuloAPI
{
    /// <summary>
    /// This class is designed to handle all the web methods that the REST WEB API poppulo exposes.
    /// Please read the Poppulo documentation here: https://developer.poppulo.com/
    /// </summary>
    ///<remarks>
    /// This Assembly must be compiled with C# 6.0 or greater and the target framework is .NET Framework 3.5 which can be find in Visual estudios 2017.
    ///</remarks>
    public sealed class PoppuloAPIClient
    {
        /// <summary>
        /// Hold the Base64 encoded credential using the UserName and Password.
        /// </summary>
        private String _Credentials = string.Empty;

        /// <summary>
        /// Hold the Data to use for Subscribers sync.
        /// </summary>
        private DataTable _DataSource;

        /// <summary>
        /// Hold a representation of a Subscriber Import Job (https://developer.poppulo.com/api-entities/api-subscriber-import-job.html)
        /// </summary>
        private XmlDocument _SubscriberImportJob;

        /// <summary>
        /// Template to create a SubscriberImportJob.
        /// </summary>
        private string _XML_SubscriberImportJob_Template = @"<subscriber_import_job>
    <accept_terms>true</accept_terms>
    <update_existing>true</update_existing>
    <reactivate_api_removed>false</reactivate_api_removed>
    <reactivate_admin_removed>false</reactivate_admin_removed>
    <reactivate_bounced_removed>false</reactivate_bounced_removed>
    <!--<set_fields>
        <set_field name=""source""><![CDATA[UltiPro]]></set_field>
    </set_fields>-->
    <subscriber_data>
        <columns><![CDATA[{columns}]]></columns>
        <skip_first_line>true</skip_first_line>
        <field_separator>comma</field_separator>
        <data><![CDATA[{data}]]></data>
    </subscriber_data>
</subscriber_import_job>";

        /// <summary>
        /// Template to create a Subscriber.
        /// </summary>
        private static string _XML_Subscriber_Template = @"<subscriber date_modified=""{date_modified}"" uri=""https://api.us.newsweaver.com/v2/ultimateapitest/subscriber/{subscriber id}"">
<link href = ""https://api.us.newsweaver.com/v2/ultimateapitest/subscriber/{subscriber id}/tags"" rel=""http://api-info.newsweaver.com/v2/rels/subscriber.tags"" title=""Subscriber ({subscriber id}) Tags""/>
<link href = ""https://api.us.newsweaver.com/v2/ultimateapitest/subscriber/{subscriber id}/topics"" rel=""http://api-info.newsweaver.com/v2/rels/subscriber.topics"" title=""Subscriber ({subscriber id}) Topics""/>
<link href = ""https://api.us.newsweaver.com/v2/ultimateapitest/subscriber/{subscriber id}/reviewer_groups"" rel=""http://api-info.newsweaver.com/v2/rels/subscriber.reviewer_groups"" title=""Subscriber ({subscriber id}) Reviewer Groups""/>
<city><![CDATA[{city}]]></city>
<email><![CDATA[{email}]]></email>
<status><![CDATA[{status}]]></status>
<surname><![CDATA[{surname}]]></surname>
<company><![CDATA[{company}]]></company>
<address1><![CDATA[{address1}]]></address1>
<address2><![CDATA[{address2}]]></address2>
<address3><![CDATA[{address3}]]></address3>
<first_name><![CDATA[{first_name}]]></first_name>
<salutation><![CDATA[{salutation}]]></salutation>
<middle_name><![CDATA[{middle_name}]]></middle_name>
<postal_code><![CDATA[{postal_code}]]></postal_code>
<external_id><![CDATA[{external_id}]]></external_id>
<phone_number><![CDATA[{phone_number}]]></phone_number>
<county_state><![CDATA[{county_state}]]></county_state>
<date_created><![CDATA[{date_created}]]></date_created>
<position_company><![CDATA[{position_company}]]></position_company>
<personal_details>
	<preferred_name><![CDATA[{preferred_name}]]></preferred_name>
	<gender><![CDATA[{gender}]]></gender>
	<date_of_birth><![CDATA[{date_of_birth}]]></date_of_birth>
	<employee_start_date><![CDATA[{employee_start_date}]]></employee_start_date>
</personal_details>
<work_location>
		<region><![CDATA[{region}]]></region>
</work_location>
</subscriber>";

        /// <summary>
        /// Constant with the User Agent value.
        /// </summary>
        private const string USERAGENT = "Ultimate Software";

        /// <summary>
        /// Get Set the Base URL of the Poppulo end point.
        /// </summary>
        public Uri BaseURL { get; }

        /// <summary>
        /// Get Set the Poppulo Account Code.
        /// </summary>
        public string AccountCode { get; set; }

        /// <summary>
        /// Get Set the Poppulo API User Name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Get Set the Poppulo API Password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Get Set the CompressContent property.
        /// </summary>
        public bool CompressContent { get; set; }

        /// <summary>
        /// Get Set the KeepRequestAlive property.
        /// </summary>
        public bool KeepRequestAlive { get; set; }

        /// <summary>
        /// SQL connection string for data source.
        /// </summary>
        public string ConnectionStringForDataSource { get; set; }

        /// <summary>
        /// Stored Procedure for data source.
        /// </summary>
        public string StoredProcedureNameForDataSource { get; set; }

        /// <summary>
        /// Get a string XML that represent a Subscriber Entity (https://developer.poppulo.com/api-entities/api-subscriber.html)
        /// You must to replace the {Values} with your values before to use it.
        /// </summary>
        public static string XMLSubscriber_Template { get { return _XML_Subscriber_Template; } }

        /// <summary>
        /// Get the last StatusCode from the last HttpWebResponse
        /// </summary>
        public HttpStatusCode LastHttpStatusCode { get; private set; }

        /// <summary>
        /// Get the last StatusDescription from the last HttpWebResponse
        /// </summary>
        public string LastHttpStatusDescription { get; private set; }

        /// <summary>
        /// Get the last stream from the HttpWebResponse.
        /// </summary>
        public string LastHttpWebResponseStream { get; private set; }

        /// <summary>
        /// Get the las stream from the HttpWebRequest.
        /// </summary>
        public string LastHttpWebRequestStream { get; private set; }

        /// <summary>
        /// Get the las httpVerb from the HttpWebRequest.
        /// </summary>
        public HttpVerb LastHttpVerb { get; private set; }


        /// <summary>
        /// Class constructor.
        /// </summary>
        /// <param name="endPointBaseURL"></param>
        /// <param name="accountCode"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="compressContent"></param>
        /// <param name="keepRequestAlive"></param>
        public PoppuloAPIClient(string endPointBaseURL, string accountCode, string userName, string password, bool compressContent = false, bool keepRequestAlive = true)
        {
            // validate the input parameters.

            // validate the endPointBaseURL.
            if (string.IsNullOrEmpty(endPointBaseURL) || !Utility.URLDomainExist(endPointBaseURL))
            {
                throw new ArgumentException($"Value: {(string.IsNullOrEmpty(endPointBaseURL) ? "'IsNullOrEmpty'" : endPointBaseURL)} passed to parameter 'endPointBaseURL' is not a valid URL.");
            }

            // validate the accountCode.
            if (string.IsNullOrEmpty(accountCode))
            {
                throw new ArgumentException($"Value: {(string.IsNullOrEmpty(accountCode) ? "'IsNullOrEmpty'" : accountCode)} passed to parameter 'accountCode' is not valid.");
            }

            // validate the userName.
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentException($"Value: {(string.IsNullOrEmpty(userName) ? "'IsNullOrEmpty'" : userName)} passed to parameter 'userName' is not valid.");
            }

            // validate the password.
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException($"Value: {(string.IsNullOrEmpty(password) ? "'IsNullOrEmpty'" : password)} passed to parameter 'password' is not valid.");
            }

            // set the properties
            BaseURL = new Uri(endPointBaseURL);
            AccountCode = accountCode;
            UserName = userName;
            Password = password;
            CompressContent = compressContent;
            KeepRequestAlive = keepRequestAlive;

            // encode the credentials for authentication.
            _Credentials = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(UserName + ":" + Password));
        }
        /// <summary>
        /// Get the Poppulo Account 
        /// </summary>
        /// <returns></returns>
        public string GetAccount()
        {
            string MyReturn = string.Empty;

            // https://api.newsweaver.com/v2/{account code}
            string MyURL = BaseURL + AccountCode;

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }

        /// <summary>
        /// List the Poppulo Accounts
        /// </summary>
        /// <returns></returns>
        public string ListAccount()
        {
            string MyReturn = string.Empty;

            // https://api.newsweaver.com/v2/

            return MakeHttpWebRequest(BaseURL.ToString(), HttpVerb.GET);
        }

        /// <summary>
        /// Get Poppulo subscriber By Email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public string GetSubscriberByEmail(string email)
        {
            string MyReturn = string.Empty;

            // Example:
            // GET https://api.newsweaver.com/v2/{account code}/subscriber/{subscriber email}

            string MyURL = BaseURL + AccountCode + $"/subscriber/{email}";

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }

        /// <summary>
        /// Get a subscriber by external_id.
        /// </summary>
        /// <param name="external_id"></param>
        /// <param name="subscriberStatus"></param>
        /// <returns></returns>
        public string GetSubscriberByExternal_id(string external_id, SubscriberStatus subscriberStatus = SubscriberStatus.active)
        {
            string MyReturn = string.Empty;

            // Example:
            // GET https://api.us.newsweaver.com/v2/{account code}/subscribers?ext_id=11019&status=all

            string MyURL = BaseURL + AccountCode + $"/subscribers/?ext_id={external_id}&status={subscriberStatus}";

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }


        /// <summary>
        /// Get Poppulo subscriber permissions By Email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public string ListSubscriberPermissionsByEmail(string email)
        {
            string MyReturn = string.Empty;

            // Example:
            // GET https://api.newsweaver.com/v2/{account code}/subscriber/{subscriber email}/permissions
            string MyURL = BaseURL + AccountCode + $"/subscriber/{email}/permissions";

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }

        /// <summary>
        /// Private method to make HttpWebRequests to the end point.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="requestMethod"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        private string MakeHttpWebRequest(string endPoint, HttpVerb requestMethod, string postData = null)
        {
            // https://binarythistleblog.wordpress.com/2017/10/12/posting-to-a-rest-api-with-c/

            string MyReturn = string.Empty;

            LastHttpWebRequestStream = postData;

            LastHttpVerb = requestMethod;

            HttpWebRequest MyHttpWebRequest = HttpWebRequest.Create(endPoint) as HttpWebRequest;

            MyHttpWebRequest.Method = requestMethod.ToString();

            if (CompressContent)
                MyHttpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            // HTTP Basic Authentication.
            MyHttpWebRequest.Headers.Add("Authorization", "Basic " + _Credentials);

            MyHttpWebRequest.UserAgent = USERAGENT;

            //NTLM authentication scheme.  
            //MyHttpWebRequest.Credentials = new NetworkCredential(UserName, Password);


            //MyHttpWebRequest.Accept = "application/xml";
            //MyHttpWebRequest.Accept = "application/*";
            //MyHttpWebRequest.Accept = "*/*";
            MyHttpWebRequest.Accept = "*.*";

            //MyHttpWebRequest.ContentType = "application/xml;charset=UTF-8";
            MyHttpWebRequest.ContentType = "application/xml";

            MyHttpWebRequest.KeepAlive = KeepRequestAlive;

            if (MyHttpWebRequest.Method == HttpVerb.POST.ToString())
            {
                // Write the POST data.
                if (!string.IsNullOrEmpty(postData))
                {
                    using (StreamWriter MyStreamWriter = new StreamWriter(MyHttpWebRequest.GetRequestStream()))
                    {
                        MyStreamWriter.Write(postData);
                        MyStreamWriter.Close();
                    }
                }
            }

            // Get the response.
            try
            {
                using (HttpWebResponse exResponse = MyHttpWebRequest.GetResponse() as HttpWebResponse)
                {
                    LastHttpStatusCode = exResponse.StatusCode;
                    LastHttpStatusDescription = exResponse.StatusDescription;

                    Stream MyStream = exResponse.GetResponseStream();

                    if (exResponse.ContentEncoding.ToLower().Contains("gzip"))
                        MyStream = new GZipStream(MyStream, CompressionMode.Decompress);

                    else if (exResponse.ContentEncoding.ToLower().Contains("deflate"))
                        MyStream = new DeflateStream(MyStream, CompressionMode.Decompress);

                    StreamReader MyStreamReader = new StreamReader(MyStream, Encoding.Default);

                    MyReturn = MyStreamReader.ReadToEnd();

                    exResponse.Close();
                    MyStream.Close();
                }
            }

            catch (WebException ex)
            {
                using (HttpWebResponse exResponse = ex.Response as HttpWebResponse)
                {
                    LastHttpStatusCode = exResponse.StatusCode;
                    LastHttpStatusDescription = exResponse.StatusDescription;

                    Stream MyStream = exResponse.GetResponseStream();

                    if (exResponse.ContentEncoding.ToLower().Contains("gzip"))
                        MyStream = new GZipStream(MyStream, CompressionMode.Decompress);

                    else if (exResponse.ContentEncoding.ToLower().Contains("deflate"))
                        MyStream = new DeflateStream(MyStream, CompressionMode.Decompress);

                    StreamReader MyStreamReader = new StreamReader(MyStream, Encoding.Default);

                    MyReturn = MyStreamReader.ReadToEnd();

                    exResponse.Close();
                    MyStream.Close();
                }
            }
            catch (Exception ex)
            {
                // this is a JSON response.
                //MyReturn = "{\"errorMessages\":[\"" + ex.Message.ToString() + "\"],\"errors\":{}}";

                // this is an XML response,
                MyReturn = $"<root><errorMessage>{ex.Message.ToString()}</errorMessages></root>";

                // this is in plain text response.
                //MyReturn = ex.Message.ToString();

            }

            LastHttpWebResponseStream = MyReturn;

            return MyReturn;
        }

        /// <summary>
        /// Get the data source to import rows as Poppulo subscribers.
        /// </summary>
        private void GetDataSource()
        {
            _DataSource = new DataTable("DataSource_For_Poppulo_Subscribers");

            using (var con = new SqlConnection(ConnectionStringForDataSource))
            using (var cmd = new SqlCommand(StoredProcedureNameForDataSource, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                //cmd.Parameters.Add("@EmployeeAddressEmail", SqlDbType.NVarChar, 75).Value = "aaron_johnson@ultimatesoftware.com";

                da.Fill(_DataSource);
            }
        }

        /// <summary>
        /// Synchronize Data Source to Poppulo.
        /// </summary>
        public void SynchronizeDataSourceAsSubscribers()
        {
            // Load the data source.
            GetDataSource();

            // Create the subscriber_import_job using the _DataSource object.
            SetSubscriberImportJobEntity();

            // POST the Subscriber Import Job Entity to the Poppulo WEB API.
            string strStatusEntity = CreateSubscriberImportJob(_SubscriberImportJob.InnerXml);
        }

        /// <summary>
        /// Create the SubscriberImportJob using the WEB API subscriber_imports.
        /// </summary>
        /// <param name="subscriberImportJob"></param>
        /// <returns>A XML string of a Poppulo Status Entity</returns>
        public string CreateSubscriberImportJob(string subscriberImportJob)
        {
            string MyReturn = string.Empty;

            // Example:
            // POST https://api.newsweaver.com/v2/{account code}/subscriber_imports

            string MyURL = BaseURL + $"{AccountCode}/subscriber_imports";

            return MakeHttpWebRequest(MyURL, HttpVerb.POST, subscriberImportJob);
        }

        /// <summary>
        /// Set the Subscriber Import Job Entity using the data source previously settled.
        /// </summary>
        private void SetSubscriberImportJobEntity()
        {
            // New instance.
            _SubscriberImportJob = new XmlDocument();

            // Load the template.
            StringBuilder sbXML = new StringBuilder(_XML_SubscriberImportJob_Template);

            // Generate the fields.
            StringBuilder sbCSVcolumns = new StringBuilder();
            _DataSource.ToCSV(recivingStringBuilder: ref sbCSVcolumns, characterUsedToDelimit: Convert.ToChar(44), includeHeaders: true, includeValues: false);
            sbXML.Replace("{columns}", sbCSVcolumns.ToString());

            // Generate the data.
            StringBuilder sbCSVdata = new StringBuilder();
            _DataSource.ToCSV(recivingStringBuilder: ref sbCSVdata, characterUsedToDelimit: ',', includeHeaders: true, includeValues: true);


            sbXML.Replace("{data}", sbCSVdata.ToString());
            //sbXML.Replace("{data}", @"<![CDATA[" + sbCSVdata.ToString() + @"]]>");

            // Load the sbXML as XmlDocument to ensure that it is a valid XML document.
            _SubscriberImportJob.LoadXml(sbXML.ToString());
        }

        /// <summary>
        /// Create Subscriber.
        /// </summary>
        public string CreateSubscriber(XmlDocument xmlSubscriber)
        {
            string MyReturn = string.Empty;

            // Example:
            // GET https://api.newsweaver.com/v2/{account code}/subscriber/{subscriber email}

            string MyURL = BaseURL + AccountCode + $"/subscribers/";

            return MakeHttpWebRequest(MyURL, HttpVerb.POST, xmlSubscriber.InnerXml);
        }

    }
}
