using System;
using System.Net.Security;
using SharpIRC.API;
using jabber;
using jabber.client;

namespace SharpIRC {
    public class Gtalk {
        public static JabberClient jabberClient = new JabberClient();

        public Gtalk() {
            jabberClient.User = "";
            jabberClient.Password = "";
            jabberClient.Server = "gmail.com";
            jabberClient.NetworkHost = "talk.l.google.com";
            jabberClient.OnInvalidCertificate += jc_OnInvalidCertificate;
            jabberClient.OnError += jc_OnError;
            jabberClient.OnAuthenticate += jc_OnAuthenticate;
            jabberClient.OnReadText += jc_OnReadText;
            jabberClient.OnWriteText += jc_OnWriteText;
            jabberClient.AutoRoster = true;
            jabberClient.AutoPresence = true;
            jabberClient.AutoLogin = true;
            jabberClient.Connect();
        }
        void jc_OnError(object sender, Exception ex) {
            
        }

        void jc_OnAuthenticate(object sender) {
            
        }

        void jc_OnReadText(object sender, string message) {
            
        }

        void jc_OnWriteText(object sender, string message) {
            
        }

        bool jc_OnInvalidCertificate(object sender,
                             System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                             System.Security.Cryptography.X509Certificates.X509Chain chain,
                             SslPolicyErrors sslPolicyErrors) {
            if (sslPolicyErrors == SslPolicyErrors.None) {
                Program.OutputConsole(String.Format("SSL Certificate approved.\r\n" +
                                                    "Name: {0}\r\n" +
                                                    "Issued by: {1}\r\n" +
                                                    "Expires: {2}\r\n", certificate.Subject, certificate.Issuer, certificate.GetExpirationDateString()), ConsoleMessageType.Information);
                return true;
            }

            Program.OutputConsole("SSL Certificate Error: " + sslPolicyErrors, ConsoleMessageType.Error);

            // Do not allow this client to communicate with unauthenticated servers. 
            return false;
        }
    }
}