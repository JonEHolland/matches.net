using System;
using GrayHills.Matches.Core;
using Newtonsoft.Json.Linq;

namespace GrayHills.Matches
{
    public class Message
    {
        public int ID { get; set; }
        public IRoom Room { get; set; }
        public User User { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageType Type { get; set; }
        public ISite CampfireSite { get; private set; }

        public Message(ISite campfireSite)
        {
            this.CampfireSite = campfireSite;
        }

        public void Star()
        {
            new CampfireRequest(this.CampfireSite, this.CampfireSite.Credentials)
                .Post(this.CampfireSite.ApiUrlBuilder.Star(ID), null);
        }

        public void Unstar()
        {
            new CampfireRequest(this.CampfireSite, this.CampfireSite.Credentials)
                .Delete(this.CampfireSite.ApiUrlBuilder.Star(ID));
        }

        public static Message Load(JToken json, ISite campfireSite)
        {
            int? userID = json.Value<int?>("user_id");

            User user = null;
            
            if (userID.HasValue)
                user = campfireSite.GetUser(userID.Value);

            return new Message(campfireSite)
                {
                    ID = json.Value<int>("id"),
                    Body = json.Value<string>("body"),
                    User = user,
                    Room = campfireSite.GetRoom(json.Value<int?>("room_id").GetValueOrDefault()),
                    Type = (MessageType)Enum.Parse(typeof(MessageType), json.Value<string>("type").Replace("Message", ""), true),
                    CreatedAt = json.Value<DateTime>("created_at")
                };
        }
    }
}
