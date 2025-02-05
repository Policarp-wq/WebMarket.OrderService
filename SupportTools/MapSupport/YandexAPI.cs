using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebMarket.OrderService.SupportTools.MapSupport
{
    public class YandexAPI : IMapGeocoder
    {
        public static string YandexGeoAPIKeyConfigName = "YandexGeoAPIKey";
        private const string AddressPath = "response.GeoObjectCollection.featureMember[0].GeoObject.metaDataProperty.GeocoderMetaData.text";
        private string _apiKey;
        public static string BasicPath = @"https://geocode-maps.yandex.ru/1.x";
        private readonly HttpClient _httpClient;
        public YandexAPI(IHttpClientFactory httpClientFactory, string apikey)
        {
            _apiKey = apikey;
            _httpClient = httpClientFactory.CreateClient("yandexapi");
            _httpClient.BaseAddress = new Uri($"{BasicPath}?apikey={_apiKey}&lang=ru_RU");
        }

        public async Task<string> GetAddressByLongLat(double longitude, double latitude) //долгота и широта
        {
            var httpGetRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BasicPath}?apikey={_apiKey}&lang=ru_RU&geocode={longitude},{latitude}&sco=longlat&format=json")
            };
            var response = await _httpClient.SendAsync(httpGetRequest);
            if (!response.IsSuccessStatusCode)
            {
                return $"{longitude},{latitude}";
            }
            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
                throw new HttpRequestException($"Body was null for {httpGetRequest.RequestUri}");
            try
            {
                JObject jobject = (JObject)JsonConvert.DeserializeObject(body);
                var address = jobject.SelectToken(AddressPath)!.Value<string>();
                return address;
            }
            catch (ArgumentNullException ex)
            {
                throw new HttpRequestException($"Exception while trying deserialize req body for {httpGetRequest.RequestUri}", ex);
            }

        }

        public Task<string> GetLongLatByAddress(string address)
        {
            throw new NotImplementedException();
        }
    }
}
