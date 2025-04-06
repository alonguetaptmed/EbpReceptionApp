using Newtonsoft.Json;

namespace EbpReceptionApp.Models
{
    public class ApiResponse<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}