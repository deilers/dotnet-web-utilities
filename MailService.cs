using DotnetWebUtils.Model;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;

namespace DotnetWebUtils
{
    public abstract class MailService
    {
        protected readonly MailServerSettings _settings;
        protected const string _smimeMediaType = "application/pkcs7-mime; smime-type=signed-data;name=smime.p7m";

        public MailService(MailServerSettings settings)
        {
            _settings = settings;
        }

        protected abstract string CreateEmailBody(string body);

        /// <summary>
        /// Returns certificate needed to encrypt email message
        /// </summary>
        /// <param name="collection">user certificate collection from Active Directory</param>
        /// <returns></returns>
        protected virtual X509Certificate2 GetEmailCertificate(X509Certificate2Collection collection)
        {
            foreach (X509Certificate2 cert in collection)
            {
                if (IsCorrectKey(cert))
                {
                    return cert;
                }
            }

            throw new ArgumentException();
        }

        protected StringBuilder SetEncryptionHeaders(StringBuilder message)
        {
            message.AppendLine("content-type: multipart/mixed; boundary=unique-boundary-1");
            message.AppendLine();
            message.AppendLine("--unique-boundary-1");
            message.AppendLine("Content-Type: text/html");
            message.AppendLine("Content-Transfer-Encoding: 7bit");
            message.AppendLine();
            return message;
        }

        protected StringBuilder SetEncryptionFooter(StringBuilder message)
        {
            message.AppendLine("--unique-boundary-1");
            return message;
        }

        private bool IsCorrectKey(X509Certificate2 cert)
        {
            return cert.Extensions.OfType<X509KeyUsageExtension>().Any(cx => cx.KeyUsages.HasFlag(X509KeyUsageFlags.KeyEncipherment))
            || cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().Any(cx => cx.EnhancedKeyUsages.OfType<Oid>().Any(k => k.Value == _settings.SecureEmailCertificateOid));
        }
    }
}