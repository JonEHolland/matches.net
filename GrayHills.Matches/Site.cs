using System.Collections.Generic;
using System.Linq;
using System.Net;
using GrayHills.Matches.Core;

namespace GrayHills.Matches
{
  public class Site : ISite
  {
    public int MeID { get; private set; }
    public CampfireUrlBuilder ApiUrlBuilder { get; set; }
    public ICredentials Credentials { get; set; }
    public List<User> Users { get; set; }
    public List<Room> Rooms { get; set; }

    public string Name { get; set; }
    public string ApiToken { get; set; }
    public string Username { get; set; }

    public Site(string name, ICredentials credentials)
    {
      // TODO: control should be inverted somehow
      ApiUrlBuilder = new CampfireUrlBuilder(this, ApiFormat.Json);

      Name = name;
      Credentials = credentials;

      Users = new List<User>();
      Rooms = new List<Room>();
    }

    public IRoom GetRoom(int roomId)
    {
      IRoom room = null;

      lock (this)
      {
        if (!this.Rooms.Any(r => r.ID == roomId))
        {
          Rooms.Add(new CampfireRequest(this, Credentials)
              .GetOne(ApiUrlBuilder.GetRoom(roomId), (json, site) => Room.LoadWithUsers(json, site, User.Load)));
        }

        room = Rooms.Single(r => r.ID == roomId);
      }

      return room;
    }

    public List<Message> Search(string term)
    {
      return new CampfireRequest(this, Credentials)
        .GetMany(ApiUrlBuilder.Search(term), (json, site) => Message.Load(json, site)).ToList();
    }

    public User GetMe()
    {
        if (MeID == 0)
        {
            User me = new CampfireRequest(this).GetOne(ApiUrlBuilder.GetMe(), User.LoadMe);
            MeID = me.ID;

            if (!Users.Any(u => u.ID == me.ID))
            {
                Users.Add(me);
            }
        }

        return Users.Single(u => u.ID == MeID);
    }

    public User GetUser(int userID)
    {
      lock (this)
      {
        if (this.Users.SingleOrDefault(u => u.ID == userID) == null)
        {
          Users.Add(new CampfireRequest(this, Credentials)
              .GetOne(ApiUrlBuilder.GetUser(userID), User.Load));
        }
      }

      return Users.Single(u => u.ID == userID);
    }

    public IEnumerable<Room> GetRooms()
    {
      return new CampfireRequest(this, Credentials)
          .GetMany(ApiUrlBuilder.GetRooms(), Room.Load);
    }

    public IEnumerable<Room> GetPresence()
    {
      return new CampfireRequest(this, Credentials)
          .GetMany(ApiUrlBuilder.GetPresence(), Room.Load);
    }

    /// <summary>
    /// Gets the user's API token using the current credentials.
    /// </summary>
    /// <returns>The API token.</returns>
    public string GetApiToken()
    {
      var me = new CampfireRequest(this, Credentials)
        .GetOne(ApiUrlBuilder.GetMe(), User.LoadMe);

      return me.ApiToken;
    }
  }
}
