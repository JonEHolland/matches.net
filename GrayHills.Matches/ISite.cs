using System.Collections.Generic;
using System.Net;
using GrayHills.Matches.Core;

namespace GrayHills.Matches
{
  public interface ISite
  {
    CampfireUrlBuilder ApiUrlBuilder { get; set; }
    ICredentials Credentials { get; set; }
    List<User> Users { get; set; }
    List<Room> Rooms { get; set; }
    List<Message> Search(string term);
    string Name { get; set; }
    string ApiToken { get; set; }
    string Username { get; set; }
    IRoom GetRoom(int roomID);
    User GetMe();
    User GetUser(int userID);
    IEnumerable<Room> GetRooms();
    IEnumerable<Room> GetPresence();
    string GetApiToken();
  }
}