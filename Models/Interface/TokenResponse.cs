namespace LINELoginOIDCImplementationDemo.Models.Interface;

/// <summary>
/// <see href="https://developers.line.biz/en/reference/line-login/#issue-token-response">Documentation</see> 
/// </summary>
public interface ITokenResponse
{
    /// <summary>
    /// Access token. Valid for 30 days.
    /// </summary>
    string access_token { get; set; }
    /// <summary>
    /// Number of seconds until the access token expires.
    /// </summary>
    int expires_in { get; set; }
    /// <summary>
    /// JSON Web Token (JWT) (opens new window)with information about the user. This property is returned only if you requested the openid scope. For more information about ID tokens, see Get profile information from ID tokens.
    /// </summary>
    string? id_token { get; set; }
    /// <summary>
    /// Token used to get a new access token (refresh token). Valid for 90 days after the access token is issued.
    /// </summary>
    string refresh_token { get; set; }
    /// <summary>
    /// Permissions granted to the access token. For more information on scopes, see Scopes. Note that the email scope isn't returned as a value of the scope property even if access to it has been granted.
    /// </summary>
    string scope { get; set; }
    /// <summary>
    /// Bearer
    /// </summary>
    string token_type { get; set; }
}

public class TokenResponse : ITokenResponse
{
    public string access_token { get; set; } = "";
    public int expires_in { get; set; }
    public string? id_token { get; set; }
    public string refresh_token { get; set; } = "";
    public string scope { get; set; } = "";
    public string token_type { get; set; } = "";
}
