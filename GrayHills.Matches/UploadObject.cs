using System;
using System.IO;
using GrayHills.Matches.Core;
using Newtonsoft.Json.Linq;

namespace GrayHills.Matches
{
  public class UploadObject
  {
    public int ByteCount { get; set; }
    public string ContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ID { get; set; }
    public string Name { get; set; }
    public IRoom Room { get; set; }
    public User User { get; set; }
    public string FullUrl { get; set; }

    public Stream GetDownloadStream()
    {
        return new CampfireRequest(this.Room.Site)
            .CreateRequest(FullUrl, HttpMethod.GET).GetResponse().GetResponseStream();
    }

    public static UploadObject Load(JToken json, ISite campfireSite)
    {
      return new UploadObject
      {
        ByteCount = json.Value<int>("byte_size"),
        ContentType = json.Value<string>("content_type"),
        CreatedAt = json.Value<DateTime>("created_at"),
        ID = json.Value<int>("id"),
        Name = json.Value<string>("name"),
        Room = campfireSite.GetRoom(json.Value<int>("room_id")),
        User = campfireSite.GetUser(json.Value<int>("user_id")),
        FullUrl = json.Value<string>("full_url")
      };
    }
  }
}
