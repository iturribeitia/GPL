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

using GPL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace PoppuloAPI
{
    /// <summary>
    /// This class is designed to handle all the web methods that the REST WEB API Poppulo exposes.
    /// Please read the Poppulo documentation here: https://developer.poppulo.com/
    /// </summary>
    ///<remarks>
    /// This Assembly must be compiled with C# 6.0 or greater and the target framework is .NET Framework 4.6 which can be find in Visual studio 2017.
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
        private const string _XML_SubscriberImportJob_Template = @"<subscriber_import_job>
    <accept_terms>true</accept_terms>
    <update_existing>true</update_existing>
    <reactivate_api_removed>false</reactivate_api_removed>
    <reactivate_admin_removed>false</reactivate_admin_removed>
    <reactivate_bounced_removed>false</reactivate_bounced_removed>
    <!--<set_fields>
        <set_field name=""source""><![CDATA[UltiPro^^></set_field>
    </set_fields>-->
    <subscriber_data>
        <columns><![CDATA[{columns}^^></columns>
        <skip_first_line>true</skip_first_line>
        <field_separator>comma</field_separator>
        <data><![CDATA[{data}^^></data>
    </subscriber_data>
</subscriber_import_job>";

        /// <summary>
        /// Template to create a Subscriber.
        /// </summary>
        private const string _XML_Subscriber_Template = @"<subscriber date_modified=""{date_modified}"">
<city><![CDATA[{city}^^></city>
<email><![CDATA[{email}^^></email>
<status><![CDATA[{status}^^></status>
<surname><![CDATA[{surname}^^></surname>
<company><![CDATA[{company}^^></company>
<address1><![CDATA[{address1}^^></address1>
<address2><![CDATA[{address2}^^></address2>
<address3><![CDATA[{address3}^^></address3>
<first_name><![CDATA[{first_name}^^></first_name>
<salutation><![CDATA[{salutation}^^></salutation>
<middle_name><![CDATA[{middle_name}^^></middle_name>
<postal_code><![CDATA[{postal_code}^^></postal_code>
<external_id><![CDATA[{external_id}^^></external_id>
<phone_number><![CDATA[{phone_number}^^></phone_number>
<county_state><![CDATA[{county_state}^^></county_state>
<date_created><![CDATA[{date_created}^^></date_created>
<custom_fields>
        <custom_field name = ""Employee Type""><![CDATA[{Employee Type}^^></custom_field>
        <custom_field name = ""Full / Part Time""><![CDATA[{Full / Part Time}^^></custom_field>
        <custom_field name = ""Level""><![CDATA[{Level}^^></custom_field>
        <custom_field name = ""People Manager""><![CDATA[{People Manager}^^></custom_field>
    </custom_fields>
<position_company><![CDATA[{position_company}^^></position_company>
<personal_details>
	<preferred_name><![CDATA[{preferred_name}^^></preferred_name>
	<gender><![CDATA[{gender}^^></gender>
	<date_of_birth><![CDATA[{date_of_birth}^^></date_of_birth>
	<employee_start_date><![CDATA[{employee_start_date}^^></employee_start_date>
</personal_details>
<work_location>
		<region><![CDATA[{region}^^></region>
</work_location>
</subscriber>";

        /// <summary>
        /// Template to create a Tag.
        /// </summary>
        private const string _XML_Tag_Template = @"<tag>
<name><![CDATA[{Name}^^></name>
<description><![CDATA[{Description}^^></description>
</tag>
";

        /// <summary>
        /// Constant with the User Agent value.
        /// </summary>
        private const string USERAGENT = "Ultimate Software";

        /// <summary>
        /// Constant to define the number of retries when Service is responding Service Unavailable.
        /// </summary>
        private const int RetriesWhenServiceIsRespondingServiceUnavailable = 10;

        /// <summary>
        /// Constant to define the milliseconds to wait when service is responding Service Unavailable
        /// </summary>
        private const int MillisecondsWaitWhenServiceIsRespondingServiceUnavailable = 6000;

        /// <summary>
        /// Get Set the Base URL of the Poppulo end point.
        /// </summary>
        public Uri BaseURL { get; private set; }

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
        /// Get the DataSource
        /// </summary>
        public DataTable DataSource { get { return _DataSource; } }


        /// <summary>
        /// Get a string XML that represent a Subscriber Entity (https://developer.poppulo.com/api-entities/api-subscriber.html)
        /// You must to replace the {Values} with your values before to use it.
        /// </summary>
        public static string XML_Subscriber_Template { get { return _XML_Subscriber_Template.Replace(@"^^", (Convert.ToChar(93)).ToString() + (Convert.ToChar(93)).ToString()); } }

        /// <summary>
        /// Get a string XML that represent a Subscriber Import Job Entity (https://developer.poppulo.com/api-entities/api-subscriber-import-job.html)
        /// You must to replace the {Values} with your values before to use it.
        /// </summary>
        public static string XML_SubscriberImportJob_Template { get { return _XML_SubscriberImportJob_Template.Replace(@"^^", (Convert.ToChar(93)).ToString() + (Convert.ToChar(93)).ToString()); } }

        /// <summary>
        /// Get a string XML that represent a Tag Entity (https://developer.poppulo.com/api-entities/api-tag.html)
        /// You must to replace the {Values} with your values before to use it.
        /// </summary>
        public static string XML_Tag_Template { get { return _XML_Tag_Template.Replace(@"^^", (Convert.ToChar(93)).ToString() + (Convert.ToChar(93)).ToString()); } }

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
        /// Get the SubscriberImportLink of the last CreateSubscriberImportJob execution.
        /// </summary>
        public string SubscriberImportLink { get; private set; }

        /// <summary>
        /// Get the Last Subscriber Identifier
        /// </summary>
        public string LastSubscriberIdentifier { get; private set; }

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
            if (string.IsNullOrEmpty(endPointBaseURL) || !URLDomainExist(endPointBaseURL))
            {
                //throw new ArgumentException($"Value: {(string.IsNullOrEmpty(endPointBaseURL) ? "'IsNullOrEmpty'" : endPointBaseURL)} passed to parameter 'endPointBaseURL' is not a valid URL.");
                throw new ArgumentException(string.Format(@"Value: {0} passed to parameter 'endPointBaseURL' is not a valid URL.", string.IsNullOrEmpty(endPointBaseURL) ? "'IsNullOrEmpty'" : endPointBaseURL));
            }

            // validate the accountCode.
            if (string.IsNullOrEmpty(accountCode))
            {
                //throw new ArgumentException($"Value: {(string.IsNullOrEmpty(accountCode) ? "'IsNullOrEmpty'" : accountCode)} passed to parameter 'accountCode' is not valid.");
                throw new ArgumentException(string.Format(@"Value: {0} passed to parameter 'accountCode' is not valid.", string.IsNullOrEmpty(accountCode) ? "'IsNullOrEmpty'" : accountCode));
            }

            // validate the userName.
            if (string.IsNullOrEmpty(userName))
            {
                //throw new ArgumentException($"Value: {(string.IsNullOrEmpty(userName) ? "'IsNullOrEmpty'" : userName)} passed to parameter 'userName' is not valid.");
                throw new ArgumentException(string.Format(@"Value: {0} passed to parameter 'userName' is not valid.", string.IsNullOrEmpty(userName) ? "'IsNullOrEmpty'" : userName));
            }

            // validate the password.
            if (string.IsNullOrEmpty(password))
            {
                //throw new ArgumentException($"Value: {(string.IsNullOrEmpty(password) ? "'IsNullOrEmpty'" : password)} passed to parameter 'password' is not valid.");
                throw new ArgumentException(string.Format(@"Value: {0} passed to parameter 'password' is not valid.", string.IsNullOrEmpty(password) ? "'IsNullOrEmpty'" : password));
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
            // https://api.newsweaver.com/v2/{account code}

            string MyURL = BaseURL + AccountCode;

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }

        /// <summary>
        /// List the Poppulo Accounts
        /// </summary>
        /// <returns> a string representing a Poppulo Account List (https://developer.poppulo.com/api-entities/api-account-list.html)</returns>
        public string ListAccount()
        {
            string MyReturn = string.Empty;

            // https://api.newsweaver.com/v2/

            return MakeHttpWebRequest(BaseURL.ToString(), HttpVerb.GET);
        }

        /// <summary>
        /// List the Poppulo Tags
        /// </summary>
        /// <returns></returns>
        public string ListTags()
        {
            // https://api.newsweaver.com/v2/{account code}/tags

            string MyURL = BaseURL + AccountCode + @"/tags";

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }

        /// <summary>
        /// Get the Tag by Name.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public string GetTag(string tagName)
        {
            // GET https://api.newsweaver.com/v2/{account code}/tag/{tag name}

            string MyURL = BaseURL + AccountCode + @"/tag/" + tagName;

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }

        /// <summary>
        /// Create Tag
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        /// <param name="tagDescription">Description of the tag</param>
        /// <returns></returns>
        public string CreateTag(string tagName, string tagDescription)
        {
            string MyReturn = string.Empty;

            // Example:
            // POST https://api.newsweaver.com/v2/{account code}/tags

            //string MyURL = BaseURL + AccountCode + $"/subscribers/";
            string MyURL = BaseURL + AccountCode + @"/tags/";

            StringBuilder MyTag = new StringBuilder(PoppuloAPIClient.XML_Tag_Template);

            MyTag.Replace("{Name}", tagName);
            MyTag.Replace("{Description}", tagDescription);

            XmlDocument xmlTag = new XmlDocument();
            xmlTag.LoadXml(MyTag.ToString());

            return MakeHttpWebRequest(MyURL, HttpVerb.POST, xmlTag.InnerXml);
        }

        /// <summary>
        /// Update Tag
        /// </summary>
        /// <param name="tagName">Existing tag</param>
        /// <param name="newTagName">New tag name</param>
        /// <param name="newTagDescription">New tag description</param>
        /// <returns></returns>
        public string UpdateTag(string tagName, string newTagName, string newTagDescription)
        {
            // Documentation : https://developer.poppulo.com/api-calls/api-update-tag.html

            string MyReturn = string.Empty;

            string MyURL = BaseURL + AccountCode + @"/tag/" + tagName;

            StringBuilder MyTag = new StringBuilder(PoppuloAPIClient.XML_Tag_Template);

            MyTag.Replace("{Name}", newTagName);
            MyTag.Replace("{Description}", newTagDescription);

            XmlDocument xmlTag = new XmlDocument();
            xmlTag.LoadXml(MyTag.ToString());

            return MakeHttpWebRequest(MyURL, HttpVerb.PUT, xmlTag.InnerXml);
        }

        /// <summary>
        /// Delete Tag
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        /// <returns></returns>
        public string DeleteTag(string tagName)
        {
            // Documentation : https://developer.poppulo.com/api-calls/api-delete-tag.html

            string MyURL = BaseURL + AccountCode + @"/tag/" + tagName;

            return MakeHttpWebRequest(MyURL, HttpVerb.DELETE);
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

            //string MyURL = BaseURL + AccountCode + $"/subscriber/{email}";
            string MyURL = BaseURL + AccountCode + string.Format(@"/subscriber/{0}", email);

            MyReturn = MakeHttpWebRequest(MyURL, HttpVerb.GET);

            // title="Edit: Subscriber 
            LastSubscriberIdentifier = (LastHttpStatusCode.Equals(HttpStatusCode.OK) && MyReturn.Contains("Edit: Subscriber")) ? MyReturn.GetSubstringByString("(", ")") : string.Empty;

            return MyReturn;
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
            //string MyURL = BaseURL + AccountCode + $"/subscriber/{email}/permissions";
            string MyURL = BaseURL + AccountCode + string.Format(@"/subscriber/{0}/permissions", email);

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

            // Check if the HttpVerb can trasport data in the boddy.
            if (MyHttpWebRequest.Method == HttpVerb.POST.ToString() || MyHttpWebRequest.Method == HttpVerb.PUT.ToString())
            {
                // Write the Body data data if any .
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
                //MyReturn = $"<root><errorMessage>{ex.Message.ToString()}</errorMessages></root>";
                MyReturn = string.Format(@"<root><errorMessage>{0}</errorMessages></root>", ex.Message.ToString());

                // this is in plain text response.
                //MyReturn = ex.Message.ToString();

            }

            LastHttpWebResponseStream = MyReturn;

            return MyReturn;
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

        /// <summary>
        /// Validate is a string is a XML.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns>Return a boolean object type indicating true = the string XML parameter is and Xml Document.</returns>
        public static bool IsXML(string xml)
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
        /// Get the data source to import rows as Poppulo subscribers.
        /// </summary>
        public void GetDataSource()
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
        /// <remarks>Tags must be created before using the SynchronizeDataSourceAsTags method if the tag is not created then it will be ignored in the subscriber.</remarks>  
        public void SynchronizeDataSourceAsSubscribers()
        {
            // Load the data source.
            if (null == _DataSource)
            {
                GetDataSource();
            }

            // Create the subscriber_import_job using the _DataSource object.
            SetSubscriberImportJobEntity();

            // POST the Subscriber Import Job Entity to the Poppulo WEB API.
            CreateSubscriberImportJob(_SubscriberImportJob.InnerXml);
        }

        /// <summary>
        /// Create the SubscriberImportJob using the WEB API subscriber_imports.
        /// </summary>
        /// <param name="subscriberImportJob"></param>
        /// <returns>A XML string of a Poppulo Status Entity</returns>
        public void CreateSubscriberImportJob(string subscriberImportJob)
        {
            string MyReturn = string.Empty;

            // Example:
            // POST https://api.newsweaver.com/v2/{account code}/subscriber_imports

            //string MyURL = BaseURL + $"{AccountCode}/subscriber_imports";
            string MyURL = BaseURL + string.Format(@"{0}/subscriber_imports", AccountCode);

            MakeHttpWebRequest(MyURL, HttpVerb.POST, subscriberImportJob);

            // Update the SubscriberImportLink
            SubscriberImportLink = string.Empty;

            // load the returned xml
            XmlDocument XmlSubscriberImport = new XmlDocument();

            XmlSubscriberImport.LoadXml(LastHttpWebResponseStream);

            // Get the SubscriberImportLink
            SubscriberImportLink = XmlSubscriberImport.SelectSingleNode("/status/resources_created/link/@href").Value;
        }

        /// <summary>
        /// Get Subscriber Import
        /// </summary>
        /// <returns></returns>
        public string GetSubscriberImport(string subscriberImportLink)
        {
            // https://developer.poppulo.com/api-calls/api-get-subscriber-import.html

            // https://api.newsweaver.com/v2/{account code}/subscriber_import/{subscriber import id}

            return string.IsNullOrEmpty(subscriberImportLink) ? string.Empty : MakeHttpWebRequest(subscriberImportLink, HttpVerb.GET);
        }

        /// <summary>
        /// Set the Subscriber Import Job Entity using the data source previously settled.
        /// </summary>
        private void SetSubscriberImportJobEntity()
        {
            // New instance.
            _SubscriberImportJob = new XmlDocument();

            // Load the template.
            StringBuilder sbXML = new StringBuilder(XML_SubscriberImportJob_Template);

            // Generate the fields.
            StringBuilder sbCSVcolumns = new StringBuilder();
            _DataSource.ToCSV(recivingStringBuilder: ref sbCSVcolumns, characterUsedToDelimit: Convert.ToChar(44), includeHeaders: true, includeValues: false);
            sbXML.Replace("{columns}", sbCSVcolumns.ToString());

            // Generate the data.
            StringBuilder sbCSVdata = new StringBuilder();
            _DataSource.ToCSV(recivingStringBuilder: ref sbCSVdata, characterUsedToDelimit: ',', includeHeaders: true, includeValues: true);

            sbXML.Replace("{data}", sbCSVdata.ToString());

            // Load the sbXML as XmlDocument to ensure that it is a valid XML document.
            _SubscriberImportJob.LoadXml(sbXML.ToString());
        }
        /// <summary>
        /// Synchronize Data Source As Tags.
        /// </summary>
        /// <returns>A list of the tags processed.</returns>
        public List<string> SynchronizeDataSourceAsTags()
        {
            int Retries;

            // Load the data source.
            if (null == _DataSource)
            {
                GetDataSource();
            }

            // Verify the data source have the columns needed.
            if (!(_DataSource.Columns.Contains("tags")))
                throw new Exception("The data source must contains the 'tags' column in order to create the tags, please review the stored procedure used to get the data source.");

            // Create all the distinct tags in Poppulo.
            List<string> AllTags = new List<string>();

            // Get the tags.
            foreach (DataRow r in _DataSource.Rows)
            {
                AllTags.AddRange(r["Tags"].ToString().Replace(", ", ",").Split(',').Distinct().ToList());
            }

            // Dedup and sort the Tags.
            AllTags = AllTags.Distinct().OrderBy(q => q).ToList<string>();

            // Try to create the new tags.
            foreach (var t in AllTags)
            {
                Retries = 0;

                do
                {
                    Retries++;

                    if (Retries > 1)
                        Thread.Sleep(MillisecondsWaitWhenServiceIsRespondingServiceUnavailable);

                    CreateTag(t, t);

                } while (Retries < RetriesWhenServiceIsRespondingServiceUnavailable && this.LastHttpStatusCode == HttpStatusCode.ServiceUnavailable);
            }
            return AllTags;
        }

        /// <summary>
        /// Create Subscriber Tags
        /// </summary>
        /// <param name="email"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        //private List<string> CreateSubscriberTags(string email, List<string> tags)
        //{
        //    // TODO THIS IS NOT WORKING IT IS RETURNING 400 Bad Request Error.

        //    List<string> MyReturn = new List<string>();

        //    // Documentation:
        //    // https://developer.poppulo.com/api-calls/api-create-subscriber-tag.html

        //    //string MyURL = BaseURL + AccountCode + $"/subscribers/";
        //    string MyURL = BaseURL + AccountCode + @"/subscriber/" + email + @"/tags";

        //    foreach (var Tag in tags)
        //    {
        //        StringBuilder MyTag = new StringBuilder(PoppuloAPIClient.XML_Tag_Template);

        //        MyTag.Replace("{Name}", Tag);
        //        MyTag.Replace("{Description}", Tag);

        //        XmlDocument xmlTag = new XmlDocument();
        //        xmlTag.LoadXml(MyTag.ToString());

        //        int Retries = 0;

        //        do
        //        {
        //            Retries++;

        //            if (Retries > 1)
        //                Thread.Sleep(MillisecondsWaitWhenServiceIsRespondingServiceUnavailable);

        //            MyReturn.Add(MakeHttpWebRequest(MyURL, HttpVerb.POST, xmlTag.InnerXml));

        //        } while (Retries < RetriesWhenServiceIsRespondingServiceUnavailable && this.LastHttpStatusCode == HttpStatusCode.ServiceUnavailable);
        //    }

        //    return MyReturn;
        //}

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

            //string MyURL = BaseURL + AccountCode + $"/subscribers/?ext_id={external_id}&status={subscriberStatus}";
            string MyURL = BaseURL + AccountCode + string.Format(@"/subscribers/?ext_id={0}&status={1}", external_id, subscriberStatus);

            return MakeHttpWebRequest(MyURL, HttpVerb.GET);
        }

        /// <summary>
        /// Create a Subscriber
        /// </summary>
        /// <param name="xmlSubscriber">xml object that represent a subscriber</param>
        /// <returns></returns>
        public string CreateSubscriber(XmlDocument xmlSubscriber)
        {
            string MyReturn = string.Empty;

            // Example:
            // GET https://api.newsweaver.com/v2/{account code}/subscriber/{subscriber email}

            //string MyURL = BaseURL + AccountCode + $"/subscribers/";
            string MyURL = BaseURL + AccountCode + @"/subscribers/";

            return MakeHttpWebRequest(MyURL, HttpVerb.POST, xmlSubscriber.InnerXml);
        }

        /// <summary>
        /// Update Subscriber
        /// </summary>
        /// <param name="subscriberIdentifier">Subscriber Identifier</param>
        /// <param name="xmlSubscriber">xml Subscriber</param>
        /// <returns></returns>
        public string UpdateSubscriber(string subscriberIdentifier, XmlDocument xmlSubscriber)
        {
            string MyReturn = string.Empty;

            // Documentation: https://developer.poppulo.com/api-calls/api-update-subscriber.html

            string MyURL = BaseURL + AccountCode + @"/subscriber/" + subscriberIdentifier;

            return MakeHttpWebRequest(MyURL, HttpVerb.PUT, xmlSubscriber.InnerXml);
        }

        /// <summary>
        /// Create a Random string
        /// </summary>
        /// <param name="length">Set the length of the returned string.</param>
        /// <returns>A random string.</returns>
        public static string RandomString(int length)
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Get the secret from the UltiSave hashicorp vault
        /// </summary>
        /// <param name="url"></param>
        /// <param name="appId"></param>
        /// <param name="userId"></param>
        /// <param name="secretPath"></param>
        /// <returns></returns>
        public static string GetUltiSafeSecret(string url, string appId, string userId, string secretPath)
        {
            // https://binarythistleblog.wordpress.com/2017/10/12/posting-to-a-rest-api-with-c/

            string MyReturn = string.Empty;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Uri myUri = new Uri(new Uri(url), "auth/app-id/login/" + appId);

            HttpWebRequest MyHttpWebRequest = HttpWebRequest.Create(myUri) as HttpWebRequest;

            MyHttpWebRequest.Method = HttpVerb.POST.ToString();

            //if (CompressContent)
            MyHttpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            //MyHttpWebRequest.Accept = "*.*";

            //MyHttpWebRequest.ContentType = "application/xml;charset=UTF-8";
            //MyHttpWebRequest.ContentType = "application/xml";

            //MyHttpWebRequest.KeepAlive = KeepRequestAlive;

            if (MyHttpWebRequest.Method == HttpVerb.POST.ToString())
            {
                // Write the POST data.
                //if (!string.IsNullOrEmpty(postData))
                {
                    using (StreamWriter MyStreamWriter = new StreamWriter(MyHttpWebRequest.GetRequestStream()))
                    {
                        string postData = "{\"user_id\":\"" + userId + "\"}";

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
                MyReturn = "{\"errorMessages\":[\"" + ex.Message.ToString() + "\"],\"errors\":{}}";

                // this is an XML response,
                //MyReturn = $"<root><errorMessage>{ex.Message.ToString()}</errorMessages></root>";
                //MyReturn = string.Format(@"<root><errorMessage>{0}</errorMessages></root>", ex.Message.ToString());

                // this is in plain text response.
                //MyReturn = ex.Message.ToString();

            }

            // BuildMyString.com generated code. Please enjoy your string responsibly.
            string sb = "(?:\"client_token\":\")(.*?)(?:\")";

            var m = Regex.Match(MyReturn, sb);

            string client_token = string.Empty;

            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions
            if (m.Success)
            {
                client_token = m.Groups[1].ToString();
            }
            else
            {
                throw new Exception("Can not get the client_token...");
            }

            // now try to get the secret.
            //-------------------------------------
            // https://binarythistleblog.wordpress.com/2017/10/12/posting-to-a-rest-api-with-c/


            myUri = new Uri(new Uri(url), secretPath.TrimStart("/".ToCharArray()));

            MyHttpWebRequest = HttpWebRequest.Create(myUri) as HttpWebRequest;

            MyHttpWebRequest.Method = HttpVerb.GET.ToString();

            MyHttpWebRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            MyHttpWebRequest.Headers.Add("X-Vault-Token", client_token);

            //MyHttpWebRequest.Accept = "*.*";

            // Get the response.
            try
            {
                using (HttpWebResponse exResponse = MyHttpWebRequest.GetResponse() as HttpWebResponse)
                {
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
                MyReturn = "{\"errorMessages\":[\"" + ex.Message.ToString() + "\"],\"errors\":{}}";

                // this is an XML response,
                //MyReturn = $"<root><errorMessage>{ex.Message.ToString()}</errorMessages></root>";
                //MyReturn = string.Format(@"<root><errorMessage>{0}</errorMessages></root>", ex.Message.ToString());

                // this is in plain text response.
                //MyReturn = ex.Message.ToString();

            }

            return MyReturn;
        }

        /// <summary>
        /// Get Value From a JSON string
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValueFromJSON(string jsonString, string key)
        {
            // BuildMyString.com generated code. Please enjoy your string responsibly.
            string sb = string.Format("(?:\"{0}\":\")(.*?)(?:\")", key);

            var m = Regex.Match(jsonString, sb);

            string MyReturn = string.Empty;

            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions
            if (m.Success)
            {
                MyReturn = m.Groups[1].ToString();
            }

            return MyReturn;
        }
    }
}
