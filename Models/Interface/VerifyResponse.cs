namespace LINELoginOIDCImplementationDemo.Models.Interface;

/// <summary>
/// <see href="https://developers.line.biz/en/docs/line-login/verify-id-token/#payload">Documentation</see> 
/// </summary>
public interface IVerifyResponse
{
    /// <summary>
    /// URL used to generate the ID token.
    /// </summary>
    string iss { get; set; }
    /// <summary>
    /// User ID for which the ID token was generated.
    /// </summary>
    string sub { get; set; }
    /// <summary>
    /// Channel ID
    /// </summary>
    string aud { get; set; }
    /// <summary>
    /// The expiry date of the ID token in UNIX time.
    /// </summary>
    int exp { get; set; }
    /// <summary>
    /// Time when the ID token was generated in UNIX time.
    /// </summary>
    int iat { get; set; }
    /// <summary>
    /// Time the user was authenticated in UNIX time. Not included if the max_age value wasn't specified in the authorization request.
    /// </summary>
    int? auth_time { get; set; }
    /// <summary>
    /// The nonce value specified in the authorization URL. Not included if the nonce value wasn't specified in the authorization request.
    /// </summary>
    string? nonce { get; set; }
    /// <summary>
    /// A list of authentication methods used by the user. Not included in the payload under certain conditions.
    /// </summary>
    List<string>? amr { get; set; }
    /// <summary>
    /// User's display name. Not included if the profile scope wasn't specified in the authorization request.
    /// </summary>
    string? name { get; set; }
    /// <summary>
    /// User's profile image URL. Not included if the profile scope wasn't specified in the authorization request.
    /// </summary>
    string? picture { get; set; }
    /// <summary>
    /// User's email address. Not included if the email scope wasn't specified in the authorization request.
    /// </summary>
    string? email { get; set; }
}

public class VerifyResponse : IVerifyResponse
{
    public string iss { get; set; } = "https://access.line.me";
    public string sub { get; set; } = "";
    public string aud { get; set; } = "";
    public int exp { get; set; }
    public int iat { get; set; }
    public int? auth_time { get; set; }
    public string? nonce { get; set; }
    public List<string>? amr { get; set; }
    public string? name { get; set; }
    public string? picture { get; set; }
    public string? email { get; set; }
}
