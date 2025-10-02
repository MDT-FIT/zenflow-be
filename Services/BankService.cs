using FintechStatsPlatform.Filters;
using FintechStatsPlatform.Models;

namespace FintechStatsPlatform.Services
{
    public class BankService
    {

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;


        public BankService(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _httpClient = new HttpClient();
        }


        public  List<Transaction> listTransactions(TransactionFilter filter) 
        {
            return new List<Transaction>();
        }

        public List<BankConfig> listBankConfigs(string userdId)
        {
            return new List<BankConfig>();
        }

        public Balance getBalance(BalanceFilter filter)
        { 
            return new Balance();
        }

        public  void connectMono() { }

        public void connectOtherBank(string userdId, BankConfig config)
        {
            
        }
        public string GetTinkAccessToken()
        {
                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "authorization:grant,user:create"),
                });

            var response = _httpClient.PostAsync("https://api.tink.com/api/v1/oauth/token", content).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }

    }
}
