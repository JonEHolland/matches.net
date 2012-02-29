using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using GrayHills.Matches.Core;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace GrayHills.Matches
{
  public class Room : IRoom
  {
    private readonly ISite site;

    public ISite Site { get { return site; } }
    public int ID { get; set; }
    public string Name { get; set; }
    public string Topic { get; set; }
    public List<User> Users { get; set; }
    public List<User> AllUsers { get; set; }
    public List<UploadObject> RecentUploads { get; set; }

    public Room(ISite site)
    {
      this.site = site;

      Users = new List<User>();
      AllUsers = new List<User>();
      RecentUploads = new List<UploadObject>();
    }

    public void Join()
    {
      new CampfireRequest(this.site, this.site.Credentials)
          .Post(this.site.ApiUrlBuilder.Join(ID));

      this.site.GetRoom(ID).Users.ForEach(x =>
      {
        if (!this.Users.Any(u => u.ID == x.ID))
          this.Users.Add(x);
      });

      RecentUploads.AddRange(GetRecentlyUploadedFiles());
    }

    public void Leave()
    {
      new CampfireRequest(this.site, this.site.Credentials)
          .Post(this.site.ApiUrlBuilder.Leave(ID));
    }

    public Message Say(string messageText)
    {
      return this.Speak(messageText, null);
    }

    public void PlaySound(CampfireSound sound)
    {      
        this.PlaySound(Enum.GetName(typeof(CampfireSound), sound).ToLower());
    }

    public void PlaySound(string sound)
    {       
        var data = new
        {
            message = new
            {
                type = "SoundMessage",
                body = sound
            }
        };

        new CampfireRequest(this.site, this.site.Credentials)
            .Post(this.site.ApiUrlBuilder.Speak(ID), data);
    }
    public void Paste(string pasteText)
    {
      this.Speak(pasteText, "PasteMessage");
    }

    public void ShareTweet(string twitterStatusUrl)
    {
      this.Speak(twitterStatusUrl, "TweetMessage");
    }

    public void Lock()
    {
      new CampfireRequest(this.site, this.site.Credentials)
          .Post(this.site.ApiUrlBuilder.Lock(ID));
    }

    public void Unlock()
    {
      new CampfireRequest(this.site, this.site.Credentials)
          .Post(this.site.ApiUrlBuilder.Unlock(ID));
    }

    public void Update(string newName = null, string newTopic = null)
    {
      var data = new
      {
        room = new
        {
          name = newName,
          topic = newTopic
        }
      };

      new CampfireRequest(this.site, this.site.Credentials)
          .Post(this.site.ApiUrlBuilder.UpdateRoom(ID), data);
    }

    public List<Message> GetTranscript()
    {
      return new CampfireRequest(this.site, this.site.Credentials)
        .GetMany(this.site.ApiUrlBuilder.GetTranscript(ID), (json, site) => Message.Load(json, site)).ToList();
    }
    
    public List<Message> GetTranscript(DateTime specificDate)
    {
      return new CampfireRequest(this.site, this.site.Credentials)
       .GetMany(this.site.ApiUrlBuilder.GetTranscript(ID, specificDate), (json, site) => Message.Load(json, site)).ToList();
    }

    public UploadObject GetUploadObject(int messageID)
    {
      return new CampfireRequest(this.site, this.site.Credentials)
          .GetOne(this.site.ApiUrlBuilder.GetUploadObject(ID, messageID), UploadObject.Load);
    }

    public IEnumerable<UploadObject> GetRecentlyUploadedFiles()
    {
      return new CampfireRequest(this.site, this.site.Credentials)
          .GetMany(this.site.ApiUrlBuilder.GetRecentlyUploadedFiles(ID), UploadObject.Load);
    }

    public IEnumerable<Message> GetRecentMessages(int? limit = null, int? sinceMessageID = null)
    {
      return new CampfireRequest(this.site, this.site.Credentials)
          .GetMany(this.site.ApiUrlBuilder.GetRecentMessages(ID, limit, sinceMessageID), Message.Load);
    }

    public void UploadFile(Stream stream, string filename)
    {
      // TODO: replace the .Replace call
      new CampfireRequest(this.site, this.site.Credentials)
          .UploadFile(this.site.ApiUrlBuilder.UploadFile(ID).Replace("json", "xml"), stream, filename, GetMIMEType(filename));
    }

    private string GetMIMEType(string filepath)
    {
        RegistryKey classesRoot = Registry.ClassesRoot;
        FileInfo fi = new FileInfo(filepath);
        string dotExt = fi.Extension.ToLower();
        RegistryKey typeKey = classesRoot.OpenSubKey(@"MIME\Database\Content Type");

        foreach (string keyname in typeKey.GetSubKeyNames())
        {
            RegistryKey curKey = classesRoot.OpenSubKey(@"MIME\Database\Content Type\" + keyname);
            object val = curKey.GetValue("Extension");
            
            if (val != null && val.ToString().ToLower() == dotExt)
            {
                return keyname;
            }
        }

        return string.Empty;
    }

    public StreamingContext GetStreamingContext(Action<Message> callback)
    {
      return new StreamingContext(this.site, this, callback);
    }

    private Message Speak(string messageBody, string messageType)
    {
      var data = new
      {
        message = new
        {
          body = messageBody,
          type = messageType
        }
      };

      JToken response = new CampfireRequest(this.site, this.site.Credentials)
          .Post(this.site.ApiUrlBuilder.Speak(ID), data);

      return Message.Load(response.First.First, this.site);
    }

    public static Room Load(JToken json, ISite campfireSite)
    {
      return new Room(campfireSite)
      {
        ID = json.Value<int>("id"),
        Name = json.Value<string>("name"),
        Topic = json.Value<string>("topic")
      };
    }

    public static Room LoadWithUsers(JToken json, ISite campfireSite, Func<JToken, ISite, User> userBuilder)
    {
      Room room = Room.Load(json, campfireSite);

      foreach (JToken token in (JArray)json.SelectToken("users"))
      {
        User user = userBuilder(token, campfireSite);

        room.Users.Add(user);
        room.AllUsers.Add(user);
      }

      return room;
    }
  }
}
