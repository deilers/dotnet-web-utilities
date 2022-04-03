using System.Security.Cryptography.X509Certificates;

namespace DotnetWebUtils.Model
{
    /// <summary>
    /// LDAP-based directory query parameters for a user
    /// </summary>
    public class UserQuery
    {
        public string IdentifierType { get; set; }
        public string Id { get; set; }
    }

    /// <summary>
    /// Models common directory user object properties 
    /// </summary>
    public class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string samAccountName { get; set; }
        public string DistinguishedName { get; set; }
        public string Domain { get; set; }
        public X509Certificate2Collection CertificateList { get; set; }
    }
}