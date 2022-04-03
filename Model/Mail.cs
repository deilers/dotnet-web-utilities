namespace DotnetWebUtils.Model
{
    public class MailServerSettings
    {
        public string FromEmailAddress { get; set; }
        public string EmailSubject { get; set; }
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string EmailBody { get; set; }
        public string SecureEmailCertificateOid { get; set; }
        public string EmailEncryptionAlgorithmOid { get; set; }
    }
}