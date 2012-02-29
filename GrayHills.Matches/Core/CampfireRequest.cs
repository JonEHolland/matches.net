using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace GrayHills.Matches.Core
{
    internal class CampfireRequest
    {
        private ICredentials Credentials { get; set; }
        public ISite CampfireSite { get; private set; }

        public CampfireRequest(ISite campfireSite)
            : this(campfireSite, campfireSite.Credentials)
        {
            // intentionally left blank
        }

        public CampfireRequest(ISite campfireSite, ICredentials credentials)
        {
            this.Credentials = credentials;
            this.CampfireSite = campfireSite;
        }

        public T GetOne<T>(string url, Func<JToken, ISite, T> builder)
        {
            using (Stream responseStream = CreateRequest(url, HttpMethod.GET).GetResponse().GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                string responseString = reader.ReadToEnd();

                JObject json = JObject.Parse(responseString);

                return builder(json.First.First, CampfireSite);
            }
        }

        public IEnumerable<T> GetMany<T>(string url, Func<JToken, ISite, T> builder)
        {
            using (Stream responseStream = CreateRequest(url, HttpMethod.GET).GetResponse().GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                string responseString = reader.ReadToEnd();

                JObject json = JObject.Parse(responseString);

                return json.First.Children().Children().Select(j => builder(j, CampfireSite)).ToList();
            }
        }

        public void Post(string url)
        {
            this.Post(url, null);
        }

        public JToken Post(string url, object data)
        {
            WebRequest request = CreateRequest(url, HttpMethod.POST);

            if (data != null)
            {
                JToken json = JObject.FromObject(data);
                request.ContentLength = Encoding.UTF8.GetByteCount(json.ToString()); 

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(Encoding.UTF8.GetBytes(json.ToString()), 0, (int)request.ContentLength);
                requestStream.Flush();
            }

            using (Stream responseStream = request.GetResponse().GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                string responseString = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(responseString)) return null;

                return JObject.Parse(responseString);
            }
        }

        public void Delete(string url)
        {
            CreateRequest(url, HttpMethod.DELETE).GetResponse();
        }

        public void UploadFile(string connectURL, Stream stream, string filename, String mimeType)
        {
            String lineEnd = "\r\n";
            String twoHyphens = "--";
            String boundary = "---------------------------XXX";

            // Unlike other parts of the API, this must be posted to the .xml endpoint, not the .json
            // This seems to be because .json endpoints require a Content-Type of application/json,
            // and with a multipart post it must be multipart/form-data.
            // I consider this a bug, since it is inconsistent with the rest of the API, and undocumented.;

            var request = CreateRequest(connectURL, HttpMethod.POST) as HttpWebRequest;

            // authentication

            request.UserAgent = "GrayHills.Matches";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            
            Stream dos = request.GetRequestStream();

            // header for the file itself
            dos.Write(Encoding.UTF8.GetBytes(twoHyphens + boundary + lineEnd), 0, Encoding.UTF8.GetByteCount(twoHyphens + boundary + lineEnd));
            // OH MY GOD the space between the semicolon and "filename=" is ABSOLUTELY NECESSARY
            dos.Write(Encoding.UTF8.GetBytes("Content-Disposition: form-data; name=\"upload\"; filename=\"" + filename + "\"" + lineEnd), 0, Encoding.UTF8.GetByteCount("Content-Disposition: form-data; name=\"upload\"; filename=\"" + filename + "\"" + lineEnd));
            dos.Write(Encoding.UTF8.GetBytes("Content-Transfer-Encoding: binary" + lineEnd), 0, Encoding.UTF8.GetByteCount("Content-Transfer-Encoding: binary" + lineEnd));
            dos.Write(Encoding.UTF8.GetBytes("Content-Type: " + mimeType + lineEnd), 0, Encoding.UTF8.GetByteCount("Content-Type: " + mimeType + lineEnd));
            dos.Write(Encoding.UTF8.GetBytes(lineEnd), 0, Encoding.UTF8.GetByteCount(lineEnd));

            // insert file
            int bytesAvailable = (int)stream.Length;
            int maxBufferSize = 1024;
            int bufferSize = Math.Min(bytesAvailable, maxBufferSize);
            byte[] buffer = new byte[bufferSize];
            int bytesRead = stream.Read(buffer, 0, bufferSize);
            while (bytesRead > 0)
            {
                dos.Write(buffer, 0, bufferSize);
                bytesAvailable -= bufferSize;
                bufferSize = Math.Min(bytesAvailable, maxBufferSize);
                bytesRead = stream.Read(buffer, 0, bufferSize);
            }

            // file closer
            dos.Write(Encoding.UTF8.GetBytes(lineEnd), 0, Encoding.UTF8.GetByteCount(lineEnd));

            // end multipart request
            dos.Write(Encoding.UTF8.GetBytes(twoHyphens + boundary + twoHyphens + lineEnd), 0, Encoding.UTF8.GetByteCount(twoHyphens + boundary + twoHyphens + lineEnd));

            // close streams
            stream.Close();
            dos.Flush();
            dos.Close();

            request.GetResponse();

        }

        internal WebRequest CreateRequest(string url, HttpMethod method)
        {
            var request = HttpWebRequest.Create(url);

            request.ContentType = "application/json";
            request.Credentials = Credentials;
            request.Method = Enum.GetName(typeof(HttpMethod), method);

            ((HttpWebRequest)request).ServicePoint.ConnectionLimit = 20;

            return request;
        }
    }
}
