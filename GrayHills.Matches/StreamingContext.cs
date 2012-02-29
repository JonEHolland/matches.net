using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using GrayHills.Matches.Core;
using Newtonsoft.Json.Linq;

namespace GrayHills.Matches
{
  public class StreamingContext
  {
    private ISite site;
    private Room room;
    private Action<Message> callback;
    private WebRequest request;
    private WebResponse response;
    private Stream responseStream;
    private StreamReader reader;
    private Action del;

    public bool IsConnected { get; private set; }

    internal StreamingContext(ISite site, Room room, Action<Message> callback)
    {
      this.site = site;
      this.room = room;
      this.callback = callback;
    }

    public void BeginStreaming()
    {
      try
      {
        request = new CampfireRequest(this.site)
            .CreateRequest(this.site.ApiUrlBuilder.Stream(this.room.ID), HttpMethod.GET);
        request.Timeout = -1;

        // yes, this is needed. regular authentication using the Credentials property does not work for streaming
        string token = string.Format("{0}:X", this.site.ApiToken);
        string encoding = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        request.Headers.Add("Authorization", "Basic " + encoding);

        response = request.GetResponse();
        responseStream = response.GetResponseStream();
        reader = new StreamReader(responseStream, Encoding.UTF8);

        del = new Action(() =>
        {
          ReadNextMessage(reader);
        });

        del.BeginInvoke(LineRead, null);
      }
      catch
      {
        Thread.Sleep(2500);
        BeginStreaming();
      }
    }

    private void ReadNextMessage(StreamReader reader)
    {
      try
      {
        string jsonString = reader.ReadLine();
        JObject json = JObject.Parse(jsonString);
        Message newMsg = Message.Load(json, this.site);
        callback(newMsg);
      }
      catch (ObjectDisposedException)
      {
        //stream closed; don't attempt to restart the stream
      }
      catch (Exception)
      {
        Thread.Sleep(2500);
        BeginStreaming();
      }
    }

    private void LineRead(IAsyncResult iar)
    {
      try
      {
        del.EndInvoke(iar);

        del.BeginInvoke(LineRead, null);
      }
      catch
      {
        // todo: figure out what to do here
      }
    }

    public void EndStreaming()
    {
      if (reader != null)
        reader.Close();
    }
  }
}
