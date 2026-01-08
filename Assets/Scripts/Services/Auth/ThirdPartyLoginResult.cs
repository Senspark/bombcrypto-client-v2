namespace App {
    public class ThirdPartyLoginResult {
        public readonly string AccessToken;
        public readonly string UserId;

        public ThirdPartyLoginResult(string accessToken, string id) {
            AccessToken = accessToken;
            UserId = id;
        }
    }
}