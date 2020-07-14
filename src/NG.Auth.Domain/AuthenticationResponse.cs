namespace NG.Auth.Domain
{
    public class AuthenticationResponse
    {
        public AuthenticationResponse(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public override bool Equals(object obj)
        {
            return obj is AuthenticationResponse authenticationResponse
                && AccessToken.Equals(authenticationResponse.AccessToken)
                && RefreshToken.Equals(authenticationResponse.RefreshToken);
        }

        public override int GetHashCode()
        {
            return new { AccessToken, RefreshToken }.GetHashCode();
        }
    }
}
