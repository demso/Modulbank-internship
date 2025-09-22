using DotNet.Testcontainers.Containers;
using System.Net.Http.Headers;

namespace BankAccounts.Tests.Integration.Testcontainers.Utility
{
    public class ClientBuilder
    {
        private static string _userToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiIxZGUzNWZiZi1kNDEwLTQyMDYtODM4NC03MGRhMGJkMDM4MGIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdCIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiZTU4ZjRhYTktYjFjYy1hNjViLTljNGMtMDg3M2QzOTFlOTg3IiwiZXhwIjoyNTEyODU0MzYyLCJpc3MiOiJCYW5rQWNjb3VudEF1dGhvcml6YXRpb24iLCJhdWQiOiJCYW5rQWNjb3VudHNXZWJBUEkifQ.QJG-5p1bj5KZdpxvcCvDII6-fiC3gWRqMKH12WKNIM0";

        public static HttpClient GetTestApiClient(IContainer apiCont)
        {
            ushort bankApiPort = apiCont.GetMappedPublicPort(80);
            HttpClient apiClient = new();
            apiClient.BaseAddress = new Uri($"http://localhost:{bankApiPort}/");
            apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _userToken);

            return apiClient;
        }
    }
}
