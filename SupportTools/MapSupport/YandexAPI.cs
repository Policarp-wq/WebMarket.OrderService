using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace WebMarket.OrderService.SupportTools.MapSupport
{
    public class YandexAPI : IMapGeocoder
    {
        public static string YandexGeoAPIKeyConfigName = "YandexGeoAPIKey";
        private const string AddressPath = "response.GeoObjectCollection.featureMember[0].GeoObject.metaDataProperty.GeocoderMetaData.text";
        private string _apiKey;
        public static string BasicPath = @"https://geocode-maps.yandex.ru/1.x";
        private readonly HttpClient _httpClient;
        private readonly ILogger<YandexAPI> _logger;
        private IDatabase _redis;
        public YandexAPI(IHttpClientFactory httpClientFactory, IConnectionMultiplexer multiplexer, ILogger<YandexAPI> logger, string apikey)
        {
            _apiKey = apikey;
            _httpClient = httpClientFactory.CreateClient("yandexapi");
            _httpClient.BaseAddress = new Uri($"{BasicPath}?apikey={_apiKey}&lang=ru_RU");
            _logger = logger;
            _redis = multiplexer.GetDatabase();
        }
        private bool IsGeoCoordValid(double c) => c <= 180 && c >= -180;
        //benchmark!
        public async Task<string> GetAddressByLongLat(double longitude, double latitude) //долгота и широта
        {
            string coordinatedKey = $"{longitude},{latitude}";
            if (!(IsGeoCoordValid(longitude) && IsGeoCoordValid(latitude)))
                throw new ArgumentException($"Provided coordinates are invalid! long: {longitude}, lat: {latitude}");
            var cachedAddress = await _redis.StringGetAsync(coordinatedKey);
            if (cachedAddress != RedisValue.Null)
                return cachedAddress.ToString();
            var httpGetRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BasicPath}?apikey={_apiKey}&lang=ru_RU&geocode={longitude},{latitude}&sco=longlat&format=json")
            };
            _logger.LogDebug("Send request to yandex api {Uri}", httpGetRequest.RequestUri);
            var response = await _httpClient.SendAsync(httpGetRequest);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Bad response for {Uri}", httpGetRequest.RequestUri);
                throw new HttpRequestException($"Bad status code {response.StatusCode}");
            }
            //api key exposing
            _logger.LogDebug("Got response for {Uri}", httpGetRequest.RequestUri);
            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(body))
                throw new HttpRequestException($"Body was null for {httpGetRequest.RequestUri}");
            try
            {
                JObject jobject = (JObject)JsonConvert.DeserializeObject(body);
                var address = jobject.SelectToken(AddressPath)!.Value<string>();
                if (address == null)
                    return string.Empty;
                await _redis.StringSetAsync(coordinatedKey, address);
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

        public async Task<string> GetAddressByLongLat(Point point)
        {
            return await GetAddressByLongLat(point.X, point.Y);
        }
    }
}
