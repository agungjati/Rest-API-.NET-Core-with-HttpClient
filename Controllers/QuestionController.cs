using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace web_api.Controllers
{
    public class QuestionResponse
    {

        public int id { get; set; }
        public int category { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string footer { get; set; }
        public IEnumerable<string> tags { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class QuestionItemResponse
    {

        public int id { get; set; }
        public int category { get; set; }
        public IEnumerable<QuestionResponse> items { get; set; }
        public DateTime createdAt { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<QuestionController> _logger;

        public QuestionController(ILogger<QuestionController> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [Route("one")]
        public async Task<object> GetOne()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://screening.moduit.id/backend/question/one"));

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<QuestionResponse>(contentStream);
                return new
                {
                    id = data.id,
                    title = data.title,
                    description = data.description,
                    footer = data.footer,
                    createdAt = data.createdAt
                };
            }
            return null;

        }

        [HttpGet]
        [Route("two")]
        public async Task<object> GetTwo()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://screening.moduit.id/backend/question/two"));

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<List<QuestionResponse>>(contentStream);
                var questions = data.FindAll(x => (x.title.Contains("Ergonomic") || x.description.Contains("Ergonomic")) && (x.tags != null && x.tags.Contains("Sports"))).OrderByDescending(x => x.id).Take(3).ToList();
                return questions;
            }
            return null;

        }

        [HttpGet]
        [Route("three")]
        public async Task<object> GetThree()
        {
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponseMessage = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://screening.moduit.id/backend/question/three"));

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                var data = await JsonSerializer.DeserializeAsync<IEnumerable<QuestionItemResponse>>(contentStream);
                Func<int, int, int> adder = (m, n) => m + n;
                
                Func<QuestionItemResponse, IEnumerable<object>> selectMany = (x) => {
                   return x.items == null ? new object[]{} : x.items.Select(item => new {
                       id = x.id,
                       category = x.category,
                       title = item.title,
                       description = item.description,
                       footer = item.footer,
                       createdAt = x.createdAt
                   }).ToArray();
                };

                var questions = data.SelectMany(selectMany).ToList();
                return questions;
            }
            return null;

        }

    }
}
