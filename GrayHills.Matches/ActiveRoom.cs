using System;
using System.Collections.Generic;
using GrayHills.Matches.Core;

namespace GrayHills.Matches
{
  public class ActiveRoom : IActiveRoom
  {
    public event EventHandler<UserEventArgs> UserJoined;
    public event EventHandler<UserEventArgs> UserLeft;
    public event EventHandler<UserEventArgs> UserKicked;
    public event EventHandler<MessageEventArgs> MessageReceived;
    public event EventHandler<MessageEventArgs> FileUploaded;

    private StreamingContext streamingContext;

    public IRoom Room { get; private set; }
    public List<Message> Messages { get; private set; }
    public List<User> Users { get; private set; }

    public ActiveRoom(IRoom room)
    {
      this.Room = room;

      this.Messages = new List<Message>();
      this.Users = new List<User>();

      this.Messages.AddRange(this.Room.GetRecentMessages(limit: 50));

      this.streamingContext = this.Room.GetStreamingContext(
        m => OnMessageReceived(new MessageEventArgs(m)));
      this.streamingContext.BeginStreaming();
    }

    protected virtual void OnUserJoined(UserEventArgs e)
    {
      this.UserJoined.Fire(this, e);
    }

    protected virtual void OnUserLeft(UserEventArgs e)
    {
      this.UserLeft.Fire(this, e);
    }

    protected virtual void OnUserKicked(UserEventArgs e)
    {
      this.UserKicked.Fire(this, e);
    }

    protected virtual void OnFileUploaded(MessageEventArgs e)
    {
      this.FileUploaded.Fire(this, e);
    }

    protected virtual void OnMessageReceived(MessageEventArgs e)
    {
      this.Messages.Add(e.Message);

      switch (e.Message.Type)
      {
        case MessageType.Leave:
          OnUserLeft(new UserEventArgs(e.Message.User));
          break;
        case MessageType.Kick:
          OnUserKicked(new UserEventArgs(e.Message.User));
          break;
        case MessageType.Enter:
          OnUserJoined(new UserEventArgs(e.Message.User));
          break;
        case MessageType.Upload:
          OnFileUploaded(new MessageEventArgs(e.Message));
          break;
      }

      this.MessageReceived.Fire(this, e);
    }
  }
}
