using DotnetWebUtils.Model;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace DotnetWebUtils
{
    /// <summary>
    /// Base class for interfacing with RESTful APIs with authentication
    /// </summary>
    public class RestHelper : IRestHelper
    {
        protected OAuthConfig _config;
        protected OAuthHelper _oAuthHelper;
        protected static HttpClient _client;

        public RestHelper(OAuthConfig config, HttpClient client)
        {
            _config = config;
            _client = client;
            _oAuthHelper = new OAuthHelper(_config);
        }

        public virtual async Task<T> GetApiPayloadAsync<T>(HttpRequestMessage request)
        {
            using HttpResponseMessage response = await _client.SendAsync(request);
            var responsePayload = JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            return responsePayload;
        }

        public virtual async Task<T> PostDataAndGetPayloadAsync<T>(HttpRequestMessage request, object dataToPost)
        {
            if (dataToPost != null)
            {
                request.Content = FormatPostBody(dataToPost);
            }

            return await GetApiPayloadAsync<T>(request);
        }

        public virtual async Task<bool> GetApiBooleanResultAsync(HttpRequestMessage request)
        {
            using HttpResponseMessage response = await _client.SendAsync(request);
            return IsSuccessResult(response);
        }

        public virtual async Task<bool> PostDataAndGetBooleanAsync(HttpRequestMessage request, object dataToPost = null)
        {
            if (dataToPost != null)
            {
                var json = JsonConvert.SerializeObject(dataToPost);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = content;
            }

            using HttpResponseMessage response = await _client.SendAsync(request);
            return IsSuccessResult(response);
        }

        public virtual async Task<HttpRequestMessage> SetupHttpRequestMessageAsync(string endpoint, HttpMethod method)
        {
            var token = (await _oAuthHelper.GetToken()).token;

            HttpRequestMessage request = new(method, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return request;
        }

        private bool IsSuccessResult(HttpResponseMessage response)
        {
            if (response == null) return false;

            return response.StatusCode == HttpStatusCode.Accepted ||
                    response.StatusCode == HttpStatusCode.NoContent ||
                    response.StatusCode == HttpStatusCode.OK;
        }

        private StringContent FormatPostBody(object dataToPost)
        {
            var json = JsonConvert.SerializeObject(dataToPost);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }

    public interface IRestHelper
    {
        Task<T> GetApiPayloadAsync<T>(HttpRequestMessage request);
        Task<bool> GetApiBooleanResultAsync(HttpRequestMessage request);
        Task<bool> PostDataAndGetBooleanAsync(HttpRequestMessage request, object dataToPost);
        Task<T> PostDataAndGetPayloadAsync<T>(HttpRequestMessage request, object dataToPost);
        Task<HttpRequestMessage> SetupHttpRequestMessageAsync(string endpoint, HttpMethod method);
    }
}