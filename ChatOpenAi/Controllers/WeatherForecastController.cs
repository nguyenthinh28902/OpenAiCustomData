using ChatOpenAi.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenAI_API.Models;
using System.Text;

namespace ChatOpenAi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly Setting SettingModel;
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IOptions<Setting> SettingModel)
        {
            _logger = logger;
            this.SettingModel = SettingModel.Value;
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [EnableCors("MyPolicy")]
        [HttpPost("CreaeteEmbeddings")]
        public async Task<bool> CreaeteEmbeddings([FromBody] Document document)
        {
            try
            {
                var api = new OpenAI_API.OpenAIAPI(SettingModel.OpenAiKey);
                if (document.IsDocTest)
                {
                    document.Doc = new List<string>() {
                "Hôm nay ngày bao nhiêu? Hôm nay ngày 2023 08 14",
                "Vnresource bao gồm các phân hệ:\r\nLương: tính lương.\r\nNhân sự: thông tin nhân viên.\r\nBảo hiểm: quản lý bảo hiểm.",
                "VnResource được biết đến là đơn vị chuyên cung cấp giải pháp phần mềm hàng \r\nđầu,chất lượng, uy tín cho các doanh nghiệp trong nước và ngoài nước.VnResource \r\ncóhơn 18 năm kinh nghiêm triển khai phần mềm quản lý nhân sự cho các tập đoàn \r\nhàngđầu Việt Nam. Giải pháp ưu việt và linh hoạt với các chức năng chuyên biệt.Giải \r\npháp đặc thù cho từng công ty giúp tối ưu thời gian, chi phí và hiệu suất công việc\r\nĐược thành lập ngày 12/09/2005, Công ty TNHH Tài Nguyên Tri Thức Việt Năng -\r\nVnResource với trụ sở chính đặt tại TP. Hồ Chí Minh, được chính thức thành lập với mục tiêu \r\nmang lại những Giải Pháp và Dịch vụ CNTT giá trị giúp quản lý tổ chức một cách hiệu quả và \r\ntốt nhất cho cộng đồng Doanh Nghiệp Việt Nam.Năm 2020: Giải pháp Phần mềm Quản lý Nhân sự VnResource HRM Pro của VnResource rất vinh \r\ndự khi được bình chọn là giải pháp xuất sắc của ngành phần mềm CNTT Việt Nam và được công \r\nnhận Danh hiệu Sao Khuê 2020. Lễ công bố và trao danh hiệu diễn ra ngày 16/05/2020 tại Hà Nội, \r\ndo Hiệp hội Phần mềm và Dịch vụ CNTT Việt Nam tổ chức.\r\nNăm 2019: Trải qua hơn một thập kỷ hình thành và phát triển, VnResource luôn đi cùng với triết lý: \r\n“Your Trust, Our Success”, định hình trở thành công ty hàng đầu chuyên về lĩnh vực tư vấn, cung \r\ncấp dịch vụ và giải pháp phần mềm tại Việt Nam. VnResource tự hào xây dựng một đội ngũ mạnh \r\nvề chuyên môn, phong cách chuyên nghiệp, phục vụ tận tâm và “LUÔN ĐẶT MÌNH Ở VỊ TRÍ \r\nKHÁCH HÀNG” để thực hiện giải pháp và dịch vụ một cách tốt nhất. Một bước đánh dấu quan \r\ntrọng khi sản phẩm của VnResource đã xuất khẩu ra thị trường nước ngoài, đây là niềm khích lệ và \r\nvững tin rằng sản phẩm của người Việt sẽ vươn ra biển lớn.\r\nSứ mệnh: Mang đến cho thị trường các sản phẩm phần mềm và dịch vụ công nghệ\r\nthông tin có chất lượng cao với chi phí cạnh tranh để khẳng định sức sáng tạo và nă\r\nng lực công nghệ của con người Việt Nam trong thời đại công nghệ 4.0. Phát triển các \r\ndự án thương mại điện tử trong các lĩnh vực tiềm năng tại Việt Nam và xuất khẩu ra \r\ncác nước trong khu vực. Xây dựng đội ngũ nhân viên năng động, chuyên nghiệp \r\ncùng trình độ chuyên môn cao. Phát triển và nâng cao giá trị của công ty và khách \r\nhàng. Thiết lập và tham gia những chương trình hỗ trợ xã hội.",
              };
                }
                List<Doc> docs = new List<Doc>();
                foreach (var item in document.Doc)
                {
                    if (string.IsNullOrEmpty(item)) continue;
                    Doc docobj = new Doc();
                    docobj.text = item;
                    docobj.vector = await api.Embeddings.GetEmbeddingsAsync(item);
                    docs.Add(docobj);

                }
                var datas = Library.LoadJson(SettingModel.PathJsonData);
                datas.AddRange(docs);
                var jsonfile = Library.Createfilejson(datas);
                Library.SaveFileStream(SettingModel.PathJsonData, jsonfile);
            }
            catch (Exception ex) { return false; }
            return true;
        }

        [EnableCors("MyPolicy")]
        [HttpGet("Chat")]
        public async Task Chat(string input)
        {
            var api = new OpenAI_API.OpenAIAPI(SettingModel.OpenAiKey);
            var datas = Library.LoadJson(SettingModel.PathJsonData);
            var vectorSearch = await api.Embeddings.GetEmbeddingsAsync(input);
            string datasearch = string.Empty;
            foreach (var item in datas)
            {
                double similarity = Library.CalculateCosineSimilarity(vectorSearch, item.vector);
                if (similarity > 0.8)
                {
                    datasearch += item.text + " ";
                }
            }

            string prompt = $"Document: {datasearch}\nUser Question: {input}\nAnswer question:";
            var result = string.Empty;
            Response.ContentType = "text/event-stream";
            try
            {
                var chat = api.Chat.CreateConversation();
                chat.AppendUserInput(prompt);
                StringBuilder sb = new StringBuilder();
                int index = 0;
                var openres = new OpenAi() { Data = new ChatGpt() };
                await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
                {
                    openres.Data.Id = "Vnr_OpenAi";
                    openres.Data.Index = index++;
                    openres.Data.Content = res.ToString();
                    var chatres = JsonConvert.SerializeObject(openres.Data);
                    await HttpContext.Response.WriteAsync(res);
                    await HttpContext.Response.Body.FlushAsync();
                };
            }
            catch (Exception ex)
            {
                HttpContext.Response.Body.Close();
            }

            HttpContext.Response.Body.Close();
        }
    }
}