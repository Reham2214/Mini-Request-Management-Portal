using Newtonsoft.Json;

namespace Mini_Request_Management_Portal.Models
{
    public class AiSuggestionResult
    {
        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("suggestedCategory")]
        public string SuggestedCategory { get; set; }

        [JsonProperty("suggestedPriority")]
        public string SuggestedPriority { get; set; }
    }
}