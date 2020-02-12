using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ProxmoxSpiceLauncher
{
    class ProxmoxAPI
    {
        public string Username;
        public string Password;
        public string Host;
        public int Port = 8006;
        private string Ticket;
        private string CSRF;

        public ProxmoxAPI()
        {

        }

        public string GetSpiceCommand(string Node, string VMId)
        {

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpWebRequest req;
            HttpWebResponse response;


            string PostString = string.Format("proxy={0}", Host);
            string Url = string.Format("https://{0}:{1}/api2/spiceconfig/nodes/{2}/qemu/{3}/spiceproxy", Host, Port, Node, VMId);
            req = WebRequest.Create(Url) as HttpWebRequest;
            req.Headers.Add("Cookie", "PVEAuthCookie=" + Ticket);
            req.Headers.Add("CSRFPreventionToken", CSRF);
            req.Method = "POST";

            var data = Encoding.ASCII.GetBytes(PostString);
            req.ContentLength = data.Length;
            using (var stream = req.GetRequestStream())
                stream.Write(data, 0, data.Length);

            req.Accept = "*/*";
            req.KeepAlive = false;

            response = (HttpWebResponse)req.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;

        }

            

        public string RefreshTicket()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            HttpWebRequest req;
            HttpWebResponse response;


            string PostString = string.Format("username={0}&password={1}", Username, Password);
            string Url = string.Format("https://{0}:{1}/api2/json/access/ticket", Host, Port);
            req = WebRequest.Create(Url) as HttpWebRequest;
            req.Method = "POST";
            req.Accept = "*/*";
            req.KeepAlive = false;
            var data = Encoding.ASCII.GetBytes(PostString);
            req.ContentLength = data.Length;
            using (var stream = req.GetRequestStream())
                stream.Write(data, 0, data.Length);

            response = (HttpWebResponse)req.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            JavaScriptSerializer aSer = new JavaScriptSerializer();
            dynamic aJson = aSer.DeserializeObject(responseString);
            Ticket = aJson["data"]["ticket"];
            CSRF = aJson["data"]["CSRFPreventionToken"];
            return Ticket;

        }
    }
}
