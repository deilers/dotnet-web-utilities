
namespace DotnetWebUtils.Model
{
    public class OAuthToken
    {
        public string token { get; set; }
        public DateTime expiration { get; set; }
    }

    public class OAuthTokenDto
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
}