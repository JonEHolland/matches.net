using System;
using System.Collections.Generic;
using System.IO;

namespace GrayHills.Matches
{
  public interface IRoom
  {
    ISite Site { get; }
    int ID { get; set; }
    string Name { get; set; }
    string Topic { get; set; }
    List<User> AllUsers { get; set; }
    List<User> Users { get; set; }
    List<UploadObject> RecentUploads { get; set; }
    List<Message> GetTranscript();
    List<Message> GetTranscript(DateTime specificDate);
    void Join();
    void Leave();
    Message Say(string messageText);
    void PlaySound(CampfireSound sound);
    void PlaySound(string sound);
    void Paste(string pasteText);
    void ShareTweet(string twitterStatusUrl);
    void Lock();
    void Unlock();
    void Update(string newName = null, string newTopic = null);
    UploadObject GetUploadObject(int messageID);
    IEnumerable<UploadObject> GetRecentlyUploadedFiles();
    IEnumerable<Message> GetRecentMessages(int? limit = null, int? sinceMessageID = null);
    void UploadFile(Stream stream, string filename);
    StreamingContext GetStreamingContext(Action<Message> callback);
  }
}