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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

// https://www.codeguru.com/csharp/.net/net_general/article.php/c4643/Giving-a-NET-Assembly-a-Strong-Name.htm

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("KeyFile.snk")]

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
        /// HttpClient to make request to any end point.
        /// </summary>
        private HttpClient _httpClient;

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
        /// Template to create a Tag.
        /// </summary>
        private const string _XML_Subscriber_Tag_Template = @"<tag uri=""https://api.newsweaver.com/v2/{AccountCode}/tag/{tagName}""></tag>";

        /// <summary>
        /// Constant with the User Agent value.
        /// </summary>
        private const string USERAGENT = "Ultimate Software";

        private readonly Encoding PoppuloBodyDataEncoding = Encoding.UTF8;
        private const string PoppuloBodyDataMediaType = @"application/xml";
        private const int PoppuloAttemptsForHTTPRequests = 5;
        private const int PoppuloWaitMillisecondsForNextAttempt = 6000;

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
        /// Get a string XML that represent a Tag Entity (https://developer.poppulo.com/api-entities/api-tag.html)
        /// You must to replace the {Values} with your values before to use it.
        /// </summary>
        public static string XML_Subscriber_Tag_Template { get { return _XML_Subscriber_Tag_Template.Replace(@"^^", (Convert.ToChar(93)).ToString() + (Convert.ToChar(93)).ToString()); } }

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

            if (compressContent)
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                _httpClient = new HttpClient(handler);
            }
            else
            {
                _httpClient = new HttpClient();

            }

            SetDefaultRequestHeadersForPoppulo();
        }

        /// <summary>
        /// Set Default Request Headers For Poppulo end point.
        /// </summary>
        private void SetDefaultRequestHeadersForPoppulo()
        {
            // Set the _httpClient to work with Poppulo end point. 

            // Clean all the DefaultRequestHeaders
            _httpClient.DefaultRequestHeaders.Clear();

            // Headers
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + _Credentials);

            // UserAgent
            _httpClient.DefaultRequestHeaders.Add("User-Agent", USERAGENT);

            // Encoder
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("UTF8"));

            if (CompressContent)
            {
                _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }

            // ContentType
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(PoppuloBodyDataMediaType));

            // KeepAlive
            _httpClient.DefaultRequestHeaders.Add("Connection", KeepRequestAlive ? "keep-alive" : "close");

            // Retry-After
            _httpClient.DefaultRequestHeaders.Add("Retry-After", "10");
        }

        /// <summary>
        /// Get the Poppulo Account
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccountAsync()
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-get-account.html

            string MyURL = BaseURL + AccountCode;

            using (HttpResponseMessage response = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Get))
            {
                using (HttpContent content = response.Content)
                {
                    return await content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// List the Poppulo Accounts
        /// </summary>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public Task<List<XmlNode>> ListAccountsAsync(int? pageSize = null)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-list-accounts.html

            return GetListEntitiesAsync(BaseURL.ToString(), PoppuloListType.Accounts, pageSize);
        }

        /// <summary>
        /// List Tags
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public Task<List<XmlNode>> ListTagsAsync(int? pageSize = null)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-list-tags.html

            string MyURL = BaseURL + AccountCode + @"/tags";

            return GetListEntitiesAsync(MyURL, PoppuloListType.Tags, pageSize);
        }

        /// <summary>
        /// Get the Tag by Name.
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public async Task<string> GetTagAsync(string tagName)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-get-tag.html

            string MyURL = BaseURL + AccountCode + @"/tag/" + tagName;

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Get))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Create Tag
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        /// <param name="tagDescription">Description of the tag</param>
        /// <returns></returns>
        public async Task<string> CreateTagAsync(string tagName, string tagDescription)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-create-tag.html

            string MyURL = BaseURL + AccountCode + @"/tags/";

            StringBuilder MyTag = new StringBuilder(PoppuloAPIClient.XML_Tag_Template);

            MyTag.Replace("{Name}", tagName);
            MyTag.Replace("{Description}", tagDescription);

            if (!PoppuloAPIClient.IsXML(MyTag.ToString()))
                throw new Exception("XML for CreateTagAsync is not a XML document Please review.");

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Post, bodyData: MyTag.ToString(), bodyDataEncoding: PoppuloBodyDataEncoding, bodyDataMediaType: PoppuloBodyDataMediaType))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Update Tag
        /// </summary>
        /// <param name="tagName">Existing tag</param>
        /// <param name="newTagName">New tag name</param>
        /// <param name="newTagDescription">New tag description</param>
        /// <returns></returns>
        public async Task<string> UpdateTagAsync(string tagName, string newTagName, string newTagDescription)
        {
            // Documentation : https://developer.poppulo.com/api-calls/api-update-tag.html

            string MyURL = BaseURL + AccountCode + @"/tag/" + tagName;

            StringBuilder MyTag = new StringBuilder(PoppuloAPIClient.XML_Tag_Template);

            MyTag.Replace("{Name}", newTagName);
            MyTag.Replace("{Description}", newTagDescription);

            if (!PoppuloAPIClient.IsXML(MyTag.ToString()))
                throw new Exception("XML for UpdateTagAsync is not a XML document Please review.");

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Put, bodyData: MyTag.ToString(), bodyDataEncoding: PoppuloBodyDataEncoding, bodyDataMediaType: PoppuloBodyDataMediaType))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Delete Tag
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        /// <returns></returns>
        public async Task<string> DeleteTagAsync(string tagName)
        {
            // Documentation : https://developer.poppulo.com/api-calls/api-delete-tag.html

            string MyURL = BaseURL + AccountCode + @"/tag/" + tagName;

            using (HttpResponseMessage r = await HTTPClientSendAsync(MyURL, HttpMethod.Delete))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Get Poppulo subscriber By Email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<string> GetSubscriberByEmailAsync(string email)
        {
            string MyReturn = string.Empty;

            // Documentation: https://developer.poppulo.com/api-calls/api-get-subscriber.html

            //string MyURL = BaseURL + AccountCode + $"/subscriber/{email}";
            string MyURL = BaseURL + AccountCode + string.Format(@"/subscriber/{0}", email);

            using (HttpResponseMessage r = await HTTPClientSendAsync(MyURL, HttpMethod.Get))
            {
                using (HttpContent c = r.Content)
                {
                    MyReturn = await c.ReadAsStringAsync();

                    LastSubscriberIdentifier = ((r.IsSuccessStatusCode) && MyReturn.Contains("Edit: Subscriber")) ? MyReturn.GetSubstringByString("(", ")") : string.Empty;
                }
            }

            return MyReturn;

        }

        /// <summary>
        /// Get Poppulo subscriber permissions By Email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<string> ListSubscriberPermissionsByEmailAsync(string email)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-list-subscriber-permissions.html

            string MyURL = BaseURL + AccountCode + string.Format(@"/subscriber/{0}/permissions", email);

            using (HttpResponseMessage r = await HTTPClientSendAsync(MyURL, HttpMethod.Get))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Send HTTPClient.SendAsync.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="attempts">The attempts.</param>
        /// <param name="waitMillisecondsForNextAttempt">The wait milliseconds for next attempt.</param>
        /// <param name="bodyData">The body data.</param>
        /// <param name="bodyDataEncoding">The body data encoding.</param>
        /// <param name="bodyDataMediaType">Type of the body data media.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Parameter attempts is <= 0, please review It.</exception>
        private async Task<HttpResponseMessage> HTTPClientSendAsync(string url, HttpMethod method, int attempts = PoppuloAttemptsForHTTPRequests, int waitMillisecondsForNextAttempt = PoppuloWaitMillisecondsForNextAttempt, string bodyData = null, Encoding bodyDataEncoding = null, string bodyDataMediaType = null)
        {
            HttpResponseMessage Myreturn;

            HttpContent content = bodyData == null ? null : new StringContent(bodyData, bodyDataEncoding, bodyDataMediaType);

            if (attempts <= 0)
                throw new Exception(@"Parameter attempts is <= 0, please review It.");

            Boolean Retrying = false;
            do
            {
                // Need to recreate the HttpRequestMessage for each attempt to avoid this error:
                // The request message was already sent. Cannot send the same request message multiple times.
                var request = new HttpRequestMessage(method, url)
                {
                    Content = content
                };

                if (Retrying)
                    Thread.Sleep(waitMillisecondsForNextAttempt);

                Myreturn = await _httpClient.SendAsync(request);

                attempts--;

                Retrying = true;

            } while (attempts > 0 && Myreturn.StatusCode == HttpStatusCode.ServiceUnavailable);

            return Myreturn;
        }

        /// <summary>
        /// Create Subscriber Tag
        /// </summary>
        /// <param name="subscriberEmailAddress"></param>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public async Task<string> CreateSubscriberTagAsync(string subscriberEmailAddress, string tagName)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-create-subscriber-tag.html

            string MyURL = BaseURL + AccountCode + @"/subscriber/" + subscriberEmailAddress + @"/tags";

            StringBuilder MyTag = new StringBuilder(PoppuloAPIClient.XML_Subscriber_Tag_Template);

            MyTag.Replace("{AccountCode}", AccountCode);
            MyTag.Replace("{tagName}", Uri.EscapeDataString(tagName));

            if (!PoppuloAPIClient.IsXML(MyTag.ToString()))
                throw new Exception("XML for CreateSubscriberTagAsync is not a XML document Please review.");

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Post, bodyData: MyTag.ToString(), bodyDataEncoding: PoppuloBodyDataEncoding, bodyDataMediaType: PoppuloBodyDataMediaType))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Removes the subscriber tag.
        /// </summary>
        /// <param name="subscriberEmailAddress">The subscriber email address.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public async Task<string> RemoveSubscriberTagAsync(string subscriberEmailAddress, string tagName)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-remove-subscriber-tag.html

            string MyURL = BaseURL + AccountCode + @"/subscriber/" + subscriberEmailAddress + @"/tag/" + tagName;

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Delete))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
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
        /// Gets the data source asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task GetDataSourceAsync()
        {
            return Task.Factory.StartNew(() =>
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
            });
        }

        /// <summary>
        /// Synchronize Data Source to Poppulo.
        /// </summary>
        /// <remarks>Tags must be created before using the SynchronizeDataSourceAsTags method if the tag is not created then it will be ignored in the subscriber.</remarks>  
        public async Task<string> SynchronizeDataSourceAsSubscribersAsync()
        {
            // Load the data source.
            if (null == _DataSource)
            {
                await GetDataSourceAsync();
            }

            // Create the subscriber_import_job using the _DataSource object.
            SetSubscriberImportJobEntity();

            // POST the Subscriber Import Job Entity to the Poppulo WEB API.
            return await CreateSubscriberImportJobAsync(_SubscriberImportJob.InnerXml);
        }

        /// <summary>
        /// Create the SubscriberImportJob using the WEB API subscriber_imports.
        /// </summary>
        /// <param name="subscriberImportJob"></param>
        /// <returns>A XML string of a Poppulo Status Entity</returns>
        public async Task<string> CreateSubscriberImportJobAsync(string subscriberImportJob)
        {
            string MyReturn = string.Empty;

            // Documentation: https://developer.poppulo.com/api-calls/api-create-subscriber-import-job.html

            string MyURL = BaseURL + string.Format(@"{0}/subscriber_imports", AccountCode);

            // load the subscriberImportJob to a xml
            if (!PoppuloAPIClient.IsXML(subscriberImportJob))
                throw new Exception("XML for CreateSubscriberImportJobAsync is not a XML document Please review.");

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Post, bodyData: subscriberImportJob, bodyDataEncoding: PoppuloBodyDataEncoding, bodyDataMediaType: PoppuloBodyDataMediaType))
            {
                using (HttpContent c = r.Content)
                {
                    MyReturn = await c.ReadAsStringAsync();
                }
            }

            // Update the SubscriberImportLink
            SubscriberImportLink = string.Empty;

            // load the returned xml
            XmlDocument XmlSubscriberImport = new XmlDocument();
            XmlSubscriberImport.LoadXml(MyReturn);

            // Get the SubscriberImportLink
            SubscriberImportLink = XmlSubscriberImport.SelectSingleNode("/status/resources_created/link/@href").Value;

            return MyReturn;
        }

        /// <summary>
        /// Get Subscriber Import
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetSubscriberImportAsync(string subscriberImportLink)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-get-subscriber-import.html

            if (string.IsNullOrEmpty(subscriberImportLink))
            {
                return string.Empty;
            }
            else
            {
                using (HttpResponseMessage r = await HTTPClientSendAsync(url: subscriberImportLink, method: HttpMethod.Get))
                {
                    using (HttpContent c = r.Content)
                    {
                        return await c.ReadAsStringAsync();
                    }
                }
            }
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
        public async Task<List<string>> SynchronizeDataSourceAsTagsAsync(int maxDegreeOfParallelism = 5)
        {
            List<Task> tasks = new List<Task>();

            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxDegreeOfParallelism))
            {
                List<string> AllTags = await GetTagsNamesFromDataSourceAsync();

                // Try to delete & create the new tags.
                foreach (var tag in AllTags)
                {
                    concurrencySemaphore.Wait();

                    //await CreateTagAsync(t, @"(API Created) " + t);
                    Task<string> t = CreateTagAsync(tag, @"(API Created) " + tag);

                    tasks.Add(t);

                    concurrencySemaphore.Release();
                }

                // Wait until the end of all tasks
                Task.WaitAll(tasks.ToArray());

                return AllTags;
            }
        }


        /// <summary>
        /// Get Tags names from data source.
        /// </summary>
        /// <returns>A list of tags names from data source.</returns>
        public async Task<List<string>> GetTagsNamesFromDataSourceAsync()
        {
            // Load the data source.
            if (null == _DataSource)
            {
                await GetDataSourceAsync();
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

            // Return the tags.
            return AllTags;
        }

        /// <summary>
        /// Delete API Created Tags.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns> A List<XmlNode> of the deleted tags.</returns>
        public async Task<List<Task>> DeleteAPICreatedTagsAsync(int? pageSize = null, int maxDegreeOfParallelism = 5)
        {
            using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(maxDegreeOfParallelism))
            {
                //List<Task> MyReturn = new List<Task>();
                // Get all the tags.
                List<XmlNode> ListTags = await ListTagsAsync(pageSize);

                string TagUri;

                List<Task> tasks = new List<Task>();

                foreach (XmlNode n in ListTags)
                {
                    if (n.Attributes != null)
                    {
                        var nameAttribute = n.Attributes["uri"];
                        if (nameAttribute != null)
                        {
                            TagUri = nameAttribute.Value;

                            string strTag;

                            using (HttpResponseMessage r = await HTTPClientSendAsync(url: TagUri, method: HttpMethod.Get))
                            {
                                using (HttpContent c = r.Content)
                                {
                                    strTag = await c.ReadAsStringAsync();
                                }
                            }

                            // Load the Tag
                            XmlDocument XMLTag = new XmlDocument();

                            XMLTag.LoadXml(strTag);

                            // Get the description

                            var descriptionTag = XMLTag.SelectSingleNode("/tag/description");
                            if (descriptionTag != null)
                            {
                                string strdescriptionTag = descriptionTag.InnerText;

                                if (strdescriptionTag.Contains(@"(API Created)"))
                                {
                                    concurrencySemaphore.Wait();

                                    Task<HttpResponseMessage> t = HTTPClientSendAsync(url: TagUri, method: HttpMethod.Delete);

                                    tasks.Add(t);

                                    concurrencySemaphore.Release();
                                }
                            }
                        }
                    }
                }

                // Wait until the end of all tasks
                Task.WaitAll(tasks.ToArray());

                return tasks;
            }
        }

        /// <summary>
        /// Get a subscriber by external_id.
        /// </summary>
        /// <param name="external_id"></param>
        /// <param name="subscriberStatus"></param>
        /// <returns></returns>
        public async Task<string> GetSubscriberByExternal_idAsync(string external_id, SubscriberStatus subscriberStatus = SubscriberStatus.active)
        {
            //Documentation: https://developer.poppulo.com/api-calls/api-list-subscribers.html

            string MyURL = BaseURL + AccountCode + string.Format(@"/subscribers/?ext_id={0}&status={1}", external_id, subscriberStatus);

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Get))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Create a Subscriber
        /// </summary>
        /// <param name="xmlSubscriber">xml object that represent a subscriber</param>
        /// <returns></returns>
        public async Task<string> CreateSubscriberAsync(XmlDocument xmlSubscriber)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-create-subscriber.html

            string MyURL = BaseURL + AccountCode + @"/subscribers/";

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Post, bodyData: xmlSubscriber.InnerXml, bodyDataEncoding: PoppuloBodyDataEncoding, bodyDataMediaType: PoppuloBodyDataMediaType))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Update Subscriber
        /// </summary>
        /// <param name="subscriberIdentifier">Subscriber Identifier</param>
        /// <param name="xmlSubscriber">xml Subscriber</param>
        /// <returns></returns>
        public async Task<string> UpdateSubscriberAsync(string subscriberIdentifier, XmlDocument xmlSubscriber)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-update-subscriber.html

            string MyURL = BaseURL + AccountCode + @"/subscriber/" + subscriberIdentifier;

            using (HttpResponseMessage r = await HTTPClientSendAsync(url: MyURL, method: HttpMethod.Put, bodyData: xmlSubscriber.InnerXml, bodyDataEncoding: PoppuloBodyDataEncoding, bodyDataMediaType: PoppuloBodyDataMediaType))
            {
                using (HttpContent c = r.Content)
                {
                    return await c.ReadAsStringAsync();
                }
            }
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
        public static async Task<string> GetUltiSafeSecretAsync(string url, string appId, string userId, string secretPath)
        {
            string MyReturn = string.Empty;

            // this is mandatory in the SSIS package)

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                   | SecurityProtocolType.Tls11
                   | SecurityProtocolType.Tls12
                   | SecurityProtocolType.Ssl3;

            Uri myUri = new Uri(new Uri(url), "auth/app-id/login/" + appId);

            // Set the client.
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Set the content.
                var content = new StringContent("{\"user_id\":\"" + userId + "\"}", Encoding.UTF8, "application/json");

                // Make the request to get the client_token.
                using (HttpResponseMessage r = await httpClient.PostAsync(myUri.ToString(), content))
                {
                    using (HttpContent c = r.Content)
                    {
                        MyReturn = await c.ReadAsStringAsync();
                    }
                }

                // Extract the token.
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
                    throw new Exception("Can not get the client_token from the vault...");
                }

                // now try to get the secret.
                //-------------------------------------
                httpClient.DefaultRequestHeaders.Add("X-Vault-Token", client_token);

                myUri = new Uri(new Uri(url), secretPath.TrimStart("/".ToCharArray()));

                using (HttpResponseMessage r = await httpClient.GetAsync(myUri.ToString()))
                {
                    using (HttpContent c = r.Content)
                    {
                        MyReturn = await c.ReadAsStringAsync();
                    }
                }
            }

            return MyReturn;
        }

        /// <summary>
        /// List Subscribers
        /// </summary>
        /// <param name="pageSize">Page Size</param>
        /// <returns></returns>
        public async Task<List<XmlNode>> ListSubscribersAsync(int? pageSize = null)
        {
            // Documentation: https://developer.poppulo.com/api-calls/api-list-subscribers.html

            string MyURL = BaseURL + AccountCode + @"/subscribers";

            return await GetListEntitiesAsync(MyURL, PoppuloListType.Subscribers, pageSize);
        }

        /// <summary>
        /// Get a list of Populo entities
        /// </summary>
        /// <param name="url"></param>
        /// <param name="poppuloListType"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private async Task<List<XmlNode>> GetListEntitiesAsync(string url, PoppuloListType poppuloListType, int? pageSize = null)
        {
            // Documentation: https://developer.poppulo.com/api-basics/api-list.html

            List<XmlNode> ListEntities = new List<XmlNode>();

            url += (pageSize == null || pageSize < 1) ? "" : string.Format(@"?page_size={0}", pageSize);

            string strEntityTypeName, strEntitiesTypeName;

            switch (poppuloListType)
            {
                case PoppuloListType.Accounts:
                    strEntityTypeName = "account";
                    strEntitiesTypeName = "accounts";
                    break;
                case PoppuloListType.Tags:
                    strEntityTypeName = "tag";
                    strEntitiesTypeName = "tags";
                    break;
                case PoppuloListType.Subscribers:
                    strEntityTypeName = "subscriber";
                    strEntitiesTypeName = "subscribers";
                    break;
                default:
                    throw new Exception("Unsupported PoppuloListType, please review.");
                    break;
            }

            XmlNode EntitiesNode;

            int Page = 0;

            int TotalResults = 0;

            do
            {
                Page++;

                string PageURL = url + ((url.Contains(@"?")) ? @"&" : @"?") + string.Format(@"page={0}", Page);

                string strEntities;

                using (HttpResponseMessage r = await HTTPClientSendAsync(url: PageURL, method: HttpMethod.Get))
                {
                    using (HttpContent c = r.Content)
                    {
                        strEntities = await c.ReadAsStringAsync();
                    }
                }

                XmlDocument XMLPageEntities = new XmlDocument();

                XMLPageEntities.LoadXml(strEntities);

                // extract the nodes
                XmlNodeList EntitiesType = XMLPageEntities.GetElementsByTagName(strEntitiesTypeName);

                if (EntitiesType != null && EntitiesType.Count > 0)
                    EntitiesNode = XMLPageEntities.GetElementsByTagName(strEntitiesTypeName)[0];
                else
                    throw new Exception("No EntitiesType type returned, Please review.");

                // Get the TotalResults
                var TotalResultsNodes = EntitiesNode.SelectNodes("totalResults");

                if (TotalResultsNodes != null)

                    int.TryParse(TotalResultsNodes[0].InnerText, out TotalResults);

                XmlNodeList EntityNodes = XMLPageEntities.GetElementsByTagName(strEntityTypeName);

                foreach (XmlNode n in EntityNodes)
                {
                    ListEntities.Add(n);
                }

            } while (ListEntities.Count < TotalResults);

            return ListEntities;
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
