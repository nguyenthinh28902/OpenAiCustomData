using Newtonsoft.Json;
using System.Text;

namespace ChatOpenAi.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public static class Library
    {
        public static List<Doc> LoadJson(string PathJsonData)
        {
            try
            {
                using (StreamReader r = new StreamReader(PathJsonData))
                {
                    string json = r.ReadToEnd();
                    List<Doc> items = JsonConvert.DeserializeObject<List<Doc>>(json);
                    return items;
                }
            }
            catch (Exception)
            {

               return new List<Doc>();
            }
           
        }
        public static Stream Createfilejson(List<Doc> docs)
        {
            var stream = new MemoryStream();
            using (var streamWriter = new StreamWriter(stream: stream, encoding: Encoding.UTF8, bufferSize: 4096, leaveOpen: true)) // last parameter is important
            using (var jsonWriter = new JsonTextWriter(streamWriter))
            {
                var serializer = new Newtonsoft.Json.JsonSerializer();
                serializer.Serialize(jsonWriter, docs);
                streamWriter.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }
        }
        public static void SaveFileStream(string path, Stream stream)
        {
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
            fileStream.Dispose();
        }

        public static double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1.Length != vector2.Length)
            {
                throw new ArgumentException("Vectors must have the same length.");
            }

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
            {
                return 0;
            }

            return dotProduct / (magnitude1 * magnitude2);
        }

      
    }
    public class Doc
    {
        public string text { get; set; }
        public float[] vector { get; set; }
        public double CosineSimilarity { get; set; }
    }

    public  class ChatGpt
    {
        public string Id { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public int Index { get; set; }
        public string Content { get; set; }
    }

    public class OpenAi { 
    
        public ChatGpt Data { get; set; }

    }

    public class Document
    {
        public List<string> Doc { get; set; }
        public bool IsDocTest { get; set; }
    }

    public class CustomHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly CustomHeadersToAddAndRemove _headers;


        public CustomHeadersMiddleware(RequestDelegate next, CustomHeadersToAddAndRemove headers)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            _next = next;
            _headers = headers;
        }

        public async Task Invoke(HttpContext context)
        {
            foreach (var headerValuePair in _headers.HeadersToAdd)
            {
                context.Response.Headers[headerValuePair.Key] = headerValuePair.Value;
            }
            foreach (var header in _headers.HeadersToRemove)
            {
                context.Response.Headers.Remove(header);
            }

            await _next(context);
        }
    }

    public class Setting
    {
        public string PathJsonData { get; set; }
        public string OpenAiKey { get; set; }
    }

    public class CustomHeadersToAddAndRemove
    {
        public Dictionary<string, string> HeadersToAdd = new();
        public HashSet<string> HeadersToRemove = new();
    }
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Enable the Customer Headers middleware and specify the headers to add and remove.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="addHeadersAction">
        /// Action to allow you to specify the headers to add and remove.
        ///
        /// Example: (opt) =>  opt.HeadersToAdd.Add("header","value"); opt.HeadersToRemove.Add("header");</param>
        /// <returns></returns>
        public static IApplicationBuilder UseCustomHeaders(this IApplicationBuilder builder, Action<CustomHeadersToAddAndRemove> addHeadersAction)
        {
            var headers = new CustomHeadersToAddAndRemove();
            addHeadersAction?.Invoke(headers);

            builder.UseMiddleware<CustomHeadersMiddleware>(headers);
            return builder;
        }
    }
}