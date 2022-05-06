using HowToUseAuthentication;
using HowToUseAuthentication.Dtos;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TestClientConsole
{
    internal class MyHttpClient
    {
        private RestClient _restClient;
        private string _accessToken;
        private string _refreshToken;

        public int TokenStep { get; private set; }
      
        public void SetApiUrl(string url)
        {
            _restClient = new RestClient(url);
        }

        public async Task<bool> CreateNewUser(string userName, string password)
        {
            var dto = new UserDto { UserName = userName, Password = password };
            var request = new RestRequest("auth/register").AddBody(dto);
            
            return await _restClient.PostAsync<bool>(request);
        }

        public async Task<bool> Authenticate(string userName, string password)
        {
            var dto = new UserDto { UserName = userName, Password = password };
            var request = new RestRequest("auth/login").AddBody(dto);
            var tokenInfo = await _restClient.PostAsync<TokenInfoDto>(request);

            if (tokenInfo == null)
                return false;

            _accessToken = tokenInfo.AccessToken;
            _refreshToken = tokenInfo.RefreshToken;
            TokenStep = 1;

            return true;
        }

        private async Task<bool> RefreshToken()
        {
            if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_refreshToken))
                return false;

            var dto = new TokenInfoDto { AccessToken = _accessToken, RefreshToken = _refreshToken };
            var request = new RestRequest("auth/refresh").AddBody(dto);
            var tokenInfo = await _restClient.PostAsync<TokenInfoDto>(request);

            if (tokenInfo == null)
                return false;

            _accessToken = tokenInfo.AccessToken;
            _refreshToken = tokenInfo.RefreshToken;
            TokenStep++;

            return !string.IsNullOrEmpty(_accessToken) && !string.IsNullOrEmpty(_refreshToken);
        }

        public async Task<WeatherForecast[]> GetWeatherInfo()
        {
            return await GetAsync<WeatherForecast[]>("weatherforecast/getweatherforecast");
        }

        private async Task<TRes> PostAsync<TRes, TForm> (string apiMethod, TForm dto) where TRes : class
        {
            var request = new RestRequest(apiMethod).AddBody(dto);

            request.AddHeader("authorization", $"Bearer {_accessToken}");

            return await _restClient.PostAsync<TRes>(request);
        }

        private async Task<TRes> GetAsync<TRes>(string apiMethod) where TRes : class
        {
            var request = new RestRequest(apiMethod);

            request.AddHeader("authorization", $"Bearer {_accessToken}");

            var response = await _restClient.GetAsync(request);

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                if (!await RefreshToken())
                    return default(TRes);

                return await GetAsync<TRes>(apiMethod);
            }

            return JsonConvert.DeserializeObject<TRes>(response.Content);
        }
    }
}
