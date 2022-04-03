namespace DotnetWebUtils.Model
{
    public class MailServerSettings
    {
        public string FromEmailAddress { get; set; }
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string SecureEmailCertificateOid { get; set; }
        public string EmailEncryptionAlgorithmOid { get; set; }
    }

    public class MessageContents
    {
        public string BodyText { get; set; }
        public string Subject { get; set; }
    }
}