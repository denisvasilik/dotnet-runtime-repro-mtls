using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace ReproMtls
{
    class Program
    {
        const String CertFilepath =  @"";

        const String KeyFilepath = @"";

        const String Hostname = "";

        const Int32 Port = 27015;

        static void Main(string[] args)
        {
            var client = new TcpClient(Hostname, 443);

            var sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                new LocalCertificateSelectionCallback(SelectClientCertificate),
                EncryptionPolicy.RequireEncryption
            );

            try
            {
                sslStream.AuthenticateAsClient(
                    Hostname,
                    GetClientCertificates(),
                    SslProtocols.Tls12,
                    false
                );
            }
            catch (AuthenticationException ex)
            {
                Console.WriteLine("AuthenticationException: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            finally
            {
                client.Close();
            }
        }

        private static X509Certificate SelectClientCertificate(
            object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssuers)
        {
            //
            // * Is only called once when running on Linux and does *not* provide
            //   acceptable issuers.
            //
            // * Is called twice when running on Windows and does provide
            //   acceptable issuers.
            //
            return localCertificates[0];
        }

        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        protected static X509Certificate2Collection GetClientCertificates()
        {
            var certPem = File.ReadAllText(CertFilepath);
            var eccPem = File.ReadAllText(KeyFilepath);

            return new X509Certificate2Collection(
                X509Certificate2.CreateFromPem(certPem, eccPem)
            );
        }
    }
}
