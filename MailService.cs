using DotnetWebUtils.Model;
using System.Net.Mail;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;

namespace DotnetWebUtils
{
    /// <summary>
    /// Abstract implementation of an SMTP encrypted email service, for use inside an organization with an LDAP-based directory.
    /// Assumes emails will be encrypted with a recipient's S/MIME certificate, but implementers can also use this to send unencrypted as well.
    /// </summary>
    public abstract class MailService
    {
        protected readonly MailServerSettings _settings;
        protected const string _smimeMediaType = "application/pkcs7-mime; smime-type=signed-data;name=smime.p7m";

        public MailService(MailServerSettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Send email to a directory user
        /// </summary>
        /// <param name="recipient">recipient directory identifier</param>
        public abstract void SendEmail(UserQuery recipient);

        /// <summary>
        /// Send email with custom message content not configured in this instance.
        /// </summary>
        /// <param name="recipient">recipient directory identifier</param>
        public abstract void SendEmailWithAlternateMessage(UserQuery recipient, MailMessage message);

        /// <summary>
        /// Abstract method for implementer to define the HTML body of their email
        /// </summary>
        protected abstract string CreateEmailBody(string body = "");

        protected virtual MailMessage GetEncryptedMailMessage(User recipient, MessageContents messageContents)
        {
            var message = new MailMessage()
            {
                From = new MailAddress(_settings.FromEmailAddress),
                Subject = messageContents.Subject
            };

            var cert = GetEmailCertificate(recipient.CertificateList);
            message.To.Add(new MailAddress(recipient.Email));
            return EncryptMailMessageWithContent(message, cert, messageContents.BodyText);
        }

        /// <summary>
        /// Add content to MailMessage object and encrypt
        /// </summary>
        protected virtual MailMessage EncryptMailMessageWithContent(MailMessage message, X509Certificate2 cert, string bodyText = "")
        {
            string body = CreateEmailBody(bodyText);
            var envelope = CreateMailEnvelope(body);
            envelope.Encrypt(new CmsRecipient(SubjectIdentifierType.IssuerAndSerialNumber, cert));
            message.AlternateViews.Add(new AlternateView(new MemoryStream(envelope.Encode()), _smimeMediaType));
            return message;
        }

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

        protected virtual bool IsCorrectKey(X509Certificate2 cert)
        {
            return cert.Extensions.OfType<X509KeyUsageExtension>().Any(cx => cx.KeyUsages.HasFlag(X509KeyUsageFlags.KeyEncipherment))
            || cert.Extensions.OfType<X509EnhancedKeyUsageExtension>().Any(cx => cx.EnhancedKeyUsages.OfType<Oid>().Any(k => k.Value == _settings.SecureEmailCertificateOid));
        }

        /// <summary>
        /// Default encryption headers to be set in CreateEmailBody. Override if modifications are needed.
        /// </summary>
        protected virtual StringBuilder SetEncryptionHeaders(StringBuilder message)
        {
            message.AppendLine("content-type: multipart/mixed; boundary=unique-boundary-1");
            message.AppendLine();
            message.AppendLine("--unique-boundary-1");
            message.AppendLine("Content-Type: text/html");
            message.AppendLine("Content-Transfer-Encoding: 7bit");
            message.AppendLine();
            return message;
        }

        /// <summary>
        /// Default encryption footer to be set in CreateEmailBody. Override if modifications are needed.
        /// </summary>
        protected virtual StringBuilder SetEncryptionFooter(StringBuilder message)
        {
            message.AppendLine("--unique-boundary-1");
            return message;
        }

        /// <summary>
        /// Create email payload to be encrypted
        /// </summary>
        /// <param name="body">body text/html of email to be sent</param>
        /// <returns>EnvelopedCms object with message and encryption algorithm metadata</returns>
        protected EnvelopedCms CreateMailEnvelope(string body)
        {
            return new EnvelopedCms(new ContentInfo(Encoding.ASCII.GetBytes(body)),
                                    new AlgorithmIdentifier(new Oid(_settings.EmailEncryptionAlgorithmOid)));
        }

        /// <summary>
        /// Send MailMessage using your organization's mail server and port
        /// </summary>
        protected virtual void SendEmail(MailMessage message)
        {
            using (var mailClient = new SmtpClient(_settings.MailServer, _settings.MailPort))
            {
                mailClient.Send(message);
            }
        }
    }
}