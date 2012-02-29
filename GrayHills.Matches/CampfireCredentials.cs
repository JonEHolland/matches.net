using System.Net;

namespace GrayHills.Matches
{
    ///<summary>
    /// Encapsulates credentials for authenticating with Campfire.
    ///</summary>
    public class CampfireCredentials : NetworkCredential
    {
        ///<summary>
        /// Initializes Campfire credentials using a user's API token.
        ///</summary>
        ///<param name="apiToken">An API token provided by Campfire.</param>
        public CampfireCredentials(string apiToken)
            : base(apiToken, "X") { }

        ///<summary>
        /// Initializes Campfire credentials using a user's username and password.
        ///</summary>
        ///<param name="username">The user's 37signals username.</param>
        ///<param name="password">The user's 37signals password.</param>
        ///<remarks>
        /// It is recommended that you only use this constructor when attempting to retrieve the
        /// user's API token. The API token is a much more secure way to authenticate with the API.
        ///</remarks>
        public CampfireCredentials(string username, string password)
            : base(username, password) { }
    }
}
