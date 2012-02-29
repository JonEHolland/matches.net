using System;
using System.Collections.Generic;

namespace GrayHills.Matches
{
  public interface IActiveRoom
  {
    event EventHandler<UserEventArgs> UserJoined;
    event EventHandler<UserEventArgs> UserLeft;
    event EventHandler<UserEventArgs> UserKicked;
    event EventHandler<MessageEventArgs> MessageReceived;
    event EventHandler<MessageEventArgs> FileUploaded;

    IRoom Room { get; }
    List<Message> Messages { get; }
    List<User> Users { get; }
  }

  public class MessageEventArgs : EventArgs
  {
    public Message Message { get; private set; }

    public MessageEventArgs(Message message)
    {
      this.Message = message;
    }
  }

  public class UserEventArgs : EventArgs
  {
    public User User { get; private set; }

    public UserEventArgs(User user)
    {
      this.User = user;
    }
  }
}
