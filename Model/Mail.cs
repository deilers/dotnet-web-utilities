namespace DotnetWebUtils.Model
{
    public class MailServerSettings
    {
        public string FromEmailAddress { get; set; }
        public string EnrollmentSubject { get; set; }
        public string MailServer { get; set; }
        public int MailPort { get; set; }
        public string EnrollmentBody { get; set; }
        public string SecureEmailCertificateOid { get; set; }
        public string EmailEncryptionAlgorithmOid { get; set; }
    }
}