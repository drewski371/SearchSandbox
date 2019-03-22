using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace SearchSandbox.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var elasticClient = _httpClientFactory.CreateClient(HttpClients.ElasticClient);
            var response = await elasticClient.GetStringAsync($"{elasticClient.BaseAddress}/profile/_doc/{id}");

            var json = JObject.Parse(response);
            if (!json["found"].Value<bool>())
                return NotFound();

            var profile = new Profile
            {
                id = json["_id"].Value<string>(),
                name = json["_source"]["name"].Value<string>(),
                title = json["_source"]["title"].Value<string>(),
                ato = json["_source"]["ato"].Value<bool>()
            };

            return Ok(profile);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var elasticClient = _httpClientFactory.CreateClient(HttpClients.ElasticClient);
            var response = await elasticClient.GetStringAsync($"{elasticClient.BaseAddress}/profile/_search?q=*");

            var json = JObject.Parse(response);
            var profiles = json["hits"]["hits"]
                .Select(p => new Profile
                {
                    id = p["_id"].Value<string>(),
                    name = p["_source"]["name"].Value<string>(),
                    title = p["_source"]["title"].Value<string>(),
                    ato = p["_source"]["ato"].Value<bool>()
                });

            return Ok(profiles);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Profile profile)
        {
            var elasticClient = _httpClientFactory.CreateClient(HttpClients.ElasticClient);
            var response = await elasticClient.PostAsJsonAsync($"{elasticClient.BaseAddress}/profile/_doc/{profile.id}", profile);

            var content = await response.Content.ReadAsStringAsync();
            return Ok();
        }

    }

}
