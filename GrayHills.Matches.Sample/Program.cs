using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrayHills.Matches.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var site = new Site("site_name", 
                new NetworkCredential("37signals_username", "37signals_password"));

            // get the users API token in order to avoid saving their password
            var apiToken = site.GetApiToken();
            site.ApiToken = apiToken;
            site.Credentials = new NetworkCredential(apiToken, "X");

            //Find the General chat room and join
            var room = site.GetRooms().Where(r => r.Name == "General").FirstOrDefault();
            room.Join();

            //Streaming context monitors the room for new messages from other users outputs to console
            var streamingContext = room.GetStreamingContext(m => Console.WriteLine("{0}:{1}", m.User.Name, m.Body));
            var startStream = new Task(streamingContext.BeginStreaming);
            startStream.Start();

            room.PlaySound(CampfireSound.Trombone);
            room.Say("Hello, World!");
            room.UploadFile(File.Open(@"C:\path\to\file", FileMode.Open), "file_name");

            while (true)
            {
                var message = Console.ReadLine();

                if (message == "/leave")
                {
                    room.Leave();
                    break;
                }
                room.Say(message);
            }

            var closeStream = new Task(streamingContext.EndStreaming);
            closeStream.Start();
            closeStream.Wait();
        }
    }
}
