namespace DotnetWebUtils.Model
{
    public class OAuthConfig
    {
        public string GrantType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TokenEndpoint { get; set; }
    }
}