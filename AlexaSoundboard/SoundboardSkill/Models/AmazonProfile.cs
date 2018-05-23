using Newtonsoft.Json;

namespace AlexaSoundboard.SoundboardSkill.Models
{
    public class AmazonProfile
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
    }
}
