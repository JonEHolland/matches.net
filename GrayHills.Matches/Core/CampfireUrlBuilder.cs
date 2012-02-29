using System;

namespace GrayHills.Matches.Core
{
  public class CampfireUrlBuilder
  {
    #region Properties

    private ISite CampfireSite { get; set; }
    private string Format { get; set; }

    public string BaseUrl
    {
      get
      {
        return string.Format("https://{0}.campfirenow.com/", CampfireSite.Name);
      }
    }

    #endregion

    public CampfireUrlBuilder(ISite campfireSite, ApiFormat format)
    {
      this.CampfireSite = campfireSite;

      switch (format)
      {
        case ApiFormat.Xml:
          Format = "xml";
          break;
        default:
          Format = "json";
          break;
      }
    }

    #region General URL Methods

    public string Stream(int roomID)
    {
      return string.Format("https://streaming.campfirenow.com/room/{0}/live.{1}", roomID, Format);
    }

    #endregion

    #region Site URL Methods

    public string Search(string searchTerm)
    {
      return string.Format("{0}search/{1}.{2}", BaseUrl, searchTerm, Format);
    }

    #endregion

    #region Room URL Methods

    public string GetRooms()
    {
      return string.Format("{0}rooms.{1}", BaseUrl, Format);
    }

    public string GetRoom(int roomID)
    {
      return string.Format("{0}room/{1}.{2}", BaseUrl, roomID, Format);
    }

    public string GetPresence()
    {
      return string.Format("{0}presence.{1}", BaseUrl, Format);
    }

    public string UploadFile(int roomID)
    {
      return string.Format("{0}room/{1}/uploads.{2}", BaseUrl, roomID, Format);
    }

    public string UpdateRoom(int roomID)
    {
      return string.Format("{0}room/{1}.{2}", BaseUrl, roomID, Format);
    }

    public string GetRecentMessages(int roomID, int? limit = null, int? sinceMessageID = null)
    {
      string queryString = string.Empty;

      // TODO: shorten this up a bit
      if (limit.HasValue)
      {
        queryString = "limit=" + limit.Value.ToString();
      }
      if (sinceMessageID.HasValue)
      {
        queryString = queryString + (string.IsNullOrWhiteSpace(queryString) ? string.Empty : "&") + "since_message_id=" + sinceMessageID.Value.ToString();
      }
      if (!string.IsNullOrWhiteSpace(queryString))
      {
        queryString = "?" + queryString;
      }

      return string.Format("{0}room/{1}/recent.{2}{3}", BaseUrl, roomID, Format, queryString);
    }

    public string GetRecentlyUploadedFiles(int roomID)
    {
      return string.Format("{0}room/{1}/uploads.{2}", BaseUrl, roomID, Format);
    }

    public string GetUploadObject(int roomID, int uploadMessageID)
    {
      return string.Format("{0}room/{1}/messages/{2}/upload.{3}", BaseUrl, roomID, uploadMessageID, Format);
    }

    public string Join(int roomID)
    {
      return string.Format("{0}room/{1}/join.{2}", BaseUrl, roomID, Format);
    }

    public string Leave(int roomID)
    {
      return string.Format("{0}room/{1}/leave.{2}", BaseUrl, roomID, Format);
    }

    public string Lock(int roomID)
    {
      return string.Format("{0}room/{1}/lock.{2}", BaseUrl, roomID, Format);
    }

    public string Unlock(int roomID)
    {
      return string.Format("{0}room/{1}/unlock.{2}", BaseUrl, roomID, Format);
    }

    #endregion

    #region User URL Methods

    public string GetMe()
    {
      return string.Format("{0}users/me.{1}", BaseUrl, Format);
    }

    public string GetUser(int userID)
    {
      return string.Format("{0}users/{1}.{2}", BaseUrl, userID, Format);
    }

    #endregion

    #region Message URL Methods

    public string Speak(int roomID)
    {
      return string.Format("{0}room/{1}/speak.{2}", BaseUrl, roomID, Format);
    }

    public string Star(int messageID)
    {
      return string.Format("{0}messages/{1}/star.{2}", BaseUrl, messageID, Format);
    }

    #endregion

    #region Transcript URL Methods

    public string GetTranscript(int roomID)
    {
      return string.Format("{0}room/{1}/transcript.{2}", BaseUrl, roomID, Format);
    }

    public string GetTranscript(int roomID, DateTime specificDate)
    {
      return string.Format("{0}room/{1}/transcript/{2}/{3}/{4}.{5}", BaseUrl, roomID, specificDate.Year, specificDate.Month, specificDate.Day, Format);
    }

    #endregion
  }
}
