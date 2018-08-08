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
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;

namespace GPL
{
    /// <summary>
    /// Email Class.
    /// </summary>
    public class Email : IDisposable
    {
        #region privateobjects
        private readonly SmtpClient _smtpClient = new SmtpClient();
        #endregion privateobjects

        #region Properties

        /// <summary>
        /// Gets or sets From property.
        /// </summary>
        /// <value>
        /// From.
        /// </value>
        public string From
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets To property.
        /// </summary>
        /// <value>
        /// To.
        /// </value>
        public string To
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the CC property.
        /// </summary>
        /// <value>
        /// The cc.
        /// </value>
        public string Cc
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Subject property.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Body property.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the IsBodyHtml property.
        /// </summary>
        /// <value>
        /// <c>true</c> if the body is HTML; otherwise, <c>false</c>.
        /// </value>
        public bool IsBodyHtml { get; set; }

        /// <summary>
        /// Gets or sets the templatepath.
        /// </summary>
        /// <value>
        /// The template path.
        /// </value>
        public string TemplatePath { get; set; }

        /// <summary>
        /// Gets or sets the email attachments.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        public List<string> Attachments
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable SSL].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable SSL]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSsl
        {
            get
            {
                return _smtpClient.EnableSsl;
            }

             set
            {
                 _smtpClient.EnableSsl = value;
            }
        }

        #endregion Properties

        #region PublicMethods

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        public Email()
        {
            // It reads its configuration from the application or machine configuration file.
            Attachments = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="smtpServer">The SMTP server.</param>
        /// <param name="port">The port.</param>
        public Email(String smtpServer, int port = 25)
            : this()
        {
            _smtpClient = new SmtpClient(smtpServer, port);
        }

        /// <summary>
        /// Load the template file into the Body and replace parameters with the args values.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <exception cref="System.IO.FileNotFoundException">File name in TemplatePath property doesn't exist, Email body can not be loaded from the template.</exception>
        public void LoadTemplate(params object[] args)
        {
            // Chech if the template is valid and exist.
            if (!File.Exists(TemplatePath))
                throw new FileNotFoundException("File name in TemplatePath property doesn't exist, Email body can not be loaded from the template.");

            // Load the template file into the Body.
            Body = Utility.FileToString(TemplatePath);

            // Replace the parameters with the values supplied.
            if (args != null && args.Length != 0)
                Body = Body.FormatString(args);
        }

        /// <summary>
        /// Creates a Mail object and sends the email if the SMTPServer variable is not
        /// empty or null
        /// </summary>
        /// <param name="timesToRetry">Number of retries</param>
        /// <param name="retryTimeout">Miliseconds between each retry.</param>
        public void SendEmail(int timesToRetry = 0, int retryTimeout = 5000)
        {
            // Create Mail object
            var oMailMessage = new MailMessage();
            try
            {
                // Set properties needed for the email
                oMailMessage.From = new MailAddress(From);
                //oMailMessage.To.Add(new MailAddress(To));

                var splitChar = To.Contains(",") ? "," : ";";

                var emTo = To.Trim().Split(splitChar.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                if (emTo.Length == 0)
                    throw new ArgumentException("Argument 'To' must be a valid email repository, please review It.");

                foreach (string toAdd in emTo)
                {
                    oMailMessage.To.Add(toAdd);
                }


                if (!string.IsNullOrEmpty(Cc))
                {
                    //oMailMessage.CC.Add(new MailAddress(Cc));

                    splitChar = Cc.Contains(",") ? "," : ";";

                    var emCc = Cc.Trim().Split(splitChar.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    if (emCc.Length == 0)
                        throw new ArgumentException("Argument 'Cc' must be a valid email repository, please review It.");

                    foreach (string CcAdd in emCc)
                    {
                        //oMailMessage.To.Add(CcAdd);
                        oMailMessage.CC.Add(CcAdd);
                    }
                }

                oMailMessage.Subject = ((string.IsNullOrEmpty(Subject) ? "" : Subject));
                oMailMessage.Body = ((string.IsNullOrEmpty(Body) ? "" : Body));
                oMailMessage.IsBodyHtml = IsBodyHtml;

                if (Attachments != null && Attachments.Count > 0)
                {
                    foreach (string attachment in Attachments)
                    {
                        var mailAttachment = new Attachment(attachment.Trim());
                        oMailMessage.Attachments.Add(mailAttachment);
                    }
                }
                //Utility.RetryAction(() => _smtpClient.Send(oMailMessage), timesToRetry, retryTimeout);
                Utility.RetryMethod(new Action<MailMessage>(_smtpClient.Send), timesToRetry, retryTimeout, oMailMessage);
            }
            finally
            {
                //destroy the oMailMessage object to release attachements from usage by the process
                oMailMessage.Dispose();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _smtpClient.Dispose();
        }

        #endregion PublicMethods

    }
}
