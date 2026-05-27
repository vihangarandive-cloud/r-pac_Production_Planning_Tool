using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RPACProductionPlanner.Services
{
    // Handles communication with the SAP Business One Service Layer REST API.
    // The Service Layer base URL is read from web.config in a production deployment.
    public class SapIntegrationService
    {
        private static readonly string ServiceLayerUrl = System.Configuration.ConfigurationManager
            .AppSettings["SapServiceLayerUrl"] ?? "https://localhost:50000/b1s/v1/";

        private string _sessionContext;

        public async Task<bool> LoginAsync(string username, string password, string companyDb)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var loginData = new { UserName = username, Password = password, CompanyDB = companyDb };
                    var content = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(ServiceLayerUrl + "Login", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(json);
                        _sessionContext = result.SessionId;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SAP login failed: " + ex.Message);
            }
            return false;
        }

        public async Task<dynamic> GetSalesOrderAsync(int docEntry)
        {
            if (string.IsNullOrEmpty(_sessionContext))
                throw new InvalidOperationException("Not authenticated with SAP Service Layer.");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Cookie", string.Format("B1SESSION={0}", _sessionContext));
                var response = await client.GetAsync(ServiceLayerUrl + string.Format("Orders({0})", docEntry));
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject(json);
                }
            }
            return null;
        }

        public async Task<dynamic> GetItemStockAsync(string itemCode)
        {
            if (string.IsNullOrEmpty(_sessionContext))
                throw new InvalidOperationException("Not authenticated with SAP Service Layer.");

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Cookie", string.Format("B1SESSION={0}", _sessionContext));
                var url = ServiceLayerUrl + string.Format("Items('{0}')?$select=ItemCode,ItemName,QuantityOnStock", itemCode);
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject(json);
                }
            }
            return null;
        }
    }
}
