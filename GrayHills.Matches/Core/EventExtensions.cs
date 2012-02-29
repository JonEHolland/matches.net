using System;

namespace GrayHills.Matches.Core
{
  public static class EventExtensions
  {
    public static void Fire<T>(this EventHandler<T> eventHandler, object sender, T e) where T : EventArgs
    {
      if (eventHandler != null)
        eventHandler(sender, e);
    }
  }
}
