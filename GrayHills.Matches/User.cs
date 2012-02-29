using Newtonsoft.Json.Linq;

namespace GrayHills.Matches
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ApiToken { get; set; }

        public static User Load(JToken json, ISite campfireSite)
        {
            return new User
                {
                    Name = json.Value<string>("name"),
                    ID = json.Value<int>("id")
                };
        }

        public static User LoadMe(JToken json, ISite campfireSite)
        {
            return new User
                {
                    ID = json.Value<int>("id"),
                    Name = json.Value<string>("name"),
                    ApiToken = json.Value<string>("api_auth_token")
                };
        }
    }
}
