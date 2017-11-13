using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;

namespace WorkWithBpmByWebServices
{
    /// <summary>
    /// AuthService response helper class.
    /// </summary>
    class ResponseStatus
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Exception { get; set; }
        public object PasswordChangeUrl { get; set; }
        public object RedirectUrl { get; set; }
    }

    /// <summary>
    /// Main program class.
    /// </summary>
    class Program
    {
		// Change URL below to point to specific bpm'online application.
        private const string baseUri = "http://my.bpmonline.com";
        private const string authServiceUri = baseUri + @"/ServiceModel/AuthService.svc/Login";
        private const string processServiceUri = baseUri + @"/0/ServiceModel/ProcessEngineService.svc/";
        private static ResponseStatus status = null;

        public static CookieContainer AuthCookie = new CookieContainer();

        /// <summary>
        /// Attempts to authenticate.
        /// </summary>
        /// <param name="userName">Bpm'online user name.</param>
        /// <param name="userPassword">Bpm'online user password.</param>
        /// <returns>True if authenticated. Otherwise returns false.</returns>
        public static bool TryLogin(string userName, string userPassword)
        {
            var authRequest = HttpWebRequest.Create(authServiceUri) as HttpWebRequest;
            authRequest.Method = "POST";
            authRequest.ContentType = "application/json";
            authRequest.CookieContainer = AuthCookie;

            using (var requesrStream = authRequest.GetRequestStream())
            {
                using (var writer = new StreamWriter(requesrStream))
                {
                    writer.Write(@"{
                    ""UserName"":""" + userName + @""",
                    ""UserPassword"":""" + userPassword + @"""
                    }");
                }
            }

            using (var response = (HttpWebResponse)authRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    status = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<ResponseStatus>(responseText);
                }
            }

            if (status != null)
            {
                if (status.Code == 0)
                {
                    return true;
                }
                Console.WriteLine(status.Message);
            }
            return false;
        }

        /// <summary>
        /// Adds new contact to bpm'online.
        /// </summary>
        /// <param name="contactName">Name of the contact.</param>
        /// <param name="contactPhone">Phone of the contact.</param>
        public static void AddContact(string contactName, string contactPhone)
        {
            string requestString = string.Format(processServiceUri +
                    "UsrAddNewExternalContact/Execute?ContactName={0}&ContactPhone={1}",
                                     contactName, contactPhone);
            HttpWebRequest request = HttpWebRequest.Create(requestString) as HttpWebRequest;
            request.Method = "GET";
            request.CookieContainer = AuthCookie;
            using (var response = request.GetResponse())
            {
                Console.WriteLine(response.ContentLength);
                Console.WriteLine(response.Headers.Count);
            }
        }

        /// <summary>
        /// Reads all bpm'online contacts and displays them.
        /// </summary>
        public static void GetAllContacts()
        {
            string requestString = processServiceUri +
                               "UsrGetAllContacts/Execute?ResultParameterName=ContactList";
            HttpWebRequest request = HttpWebRequest.Create(requestString) as HttpWebRequest;
            request.Method = "GET";
            request.CookieContainer = AuthCookie;
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    Console.WriteLine(responseText);
                }
            }
        }

        static void Main(string[] args)
        {
            if (!TryLogin("Supervisor", "Supervisor"))
            {
                Console.WriteLine("Wrong login or password. Application will be terminated.");
            }
            else
            {
                try
                {
                    AddContact("John Johanson", "+1 111 111 1111");
                    GetAllContacts();
                }
                catch (Exception)
                {
                    // Process exception here. Or throw it further.
                    throw;
                }

            };

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
