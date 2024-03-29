using System.Net.Http.Headers;
using DotnetWebUtils.Model;
using Newtonsoft.Json;

namespace DotnetWebUtils
{
    /// <summary>
    /// Base class for managing OAuth bearer tokens within an instance of RestHelper
    /// </summary>
    public class OAuthHelper
    {
        protected readonly OAuthConfig _config;

        /// <summary>
        /// Singleton instance of HttpClient exclusively for requesting authentication tokens
        /// </summary>
        protected static HttpClient _client = new();

        /// <summary>
        /// Token storage singleton, so it persists across multiple instances
        /// </summary>
        private static Dictionary<string, OAuthToken> _tokenDictionary = new();

        private OAuthToken _token
        {
            get
            {
                return _tokenDictionary.ContainsKey(GetType().Name)
                ? _tokenDictionary[GetType().Name]
                : null;
            }

            set => _tokenDictionary[GetType().Name] = value;
        }

        public OAuthHelper(OAuthConfig config)
        {
            _config = config;
        }

        protected bool TokenExpired()
        {
            return _token.expiration < DateTime.Now;
        }

        protected Dictionary<string, string> GetOAuthCredentials()
        {
            return new Dictionary<string, string>
            {
                { Constants.GRANT_TYPE_KEY, Constants.CLIENT_CREDENTIALS },
                { Constants.CLIENT_ID_KEY, _config.ClientId },
                { Constants.CLIENT_SECRET_KEY, _config.ClientSecret }
            };
        }

        protected HttpContent GetTokenRequestContent()
        {
            HttpContent content = new FormUrlEncodedContent(GetOAuthCredentials());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            content.Headers.ContentType.CharSet = "UTF-8";
            return content;
        }

        public async Task<OAuthToken> GetToken()
        {
            if (_token == null || TokenExpired())
            {
                using HttpResponseMessage res = await _client.PostAsync(_config.TokenEndpoint, GetTokenRequestContent());
                string contents = await res.Content.ReadAsStringAsync();
                OAuthTokenDto dto = JsonConvert.DeserializeObject<OAuthTokenDto>(contents);
                OAuthToken token = CreateTokenObjectAndSetTimeout(dto);
                _token = token;
                return token;
            }
            else
            {
                return _token;
            }
        }

        protected static OAuthToken CreateTokenObjectAndSetTimeout(OAuthTokenDto tokenDto)
        {
            return new OAuthToken()
            {
                token = tokenDto.access_token,
                expiration = DateTime.Now.AddMinutes(59)
            };
        }
    }
}