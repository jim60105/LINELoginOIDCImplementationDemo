namespace LINELoginOIDCImplementationDemo.Models.Interface;

#pragma warning disable CS8766 // 傳回型別中參考型別是否可為 NULL 的情況，與隱含實作的成員不相符 (可能的原因是屬性可為 NULL )。

/// <summary>
/// <see href="https://developers.line.biz/en/docs/line-login/integrate-line-login/#receiving-the-authorization-code">Documentation</see> 
/// </summary>
public interface ISigninOIDCResponse : ISigninOIDCResponse_Success, ISigninOIDCResponse_Error
{
}


public interface ISigninOIDCResponse_Success
{
    /// <summary>
    /// Authorization code used to get an access token. Valid for 10 minutes. This authorization code can only be used once.
    /// </summary>
    string code { get; set; }
    /// <summary>
    /// A unique alphanumeric string used to prevent cross-site request forgery (opens new window). Verify that this matches the value of the state parameter given to the authorization URL.
    /// </summary>
    string state { get; set; }
    /// <summary>
    /// true if the friendship status between the user and the LINE Official Account linked to the channel has changed when the user logs in. Otherwise, the value is false. This parameter is only returned if you specify the bot_prompt query parameter when authenticating users and making authorization requests and the user was given the option to add your LINE Official Account as a friend when they logged in. For more information, see Add a LINE Official Account as a friend when logged in (bot links).
    /// </summary>
    bool? friendship_status_changed { get; set; }
    /// <summary>
    /// LINE Login channel ID. This parameter is returned only when the login process is performed using the liff.login() method in the LIFF app. To ensure proper operation of the LIFF app, don't change this parameter.
    /// </summary>
    string? liffClientId { get; set; }
    /// <summary>
    /// URL displayed in the LIFF app after login. Value specified in the redirectUri property of the liff.login() method. This parameter is returned only when the login process is performed using the liff.login() method in the LIFF app. To ensure proper operation of the LIFF app, don't change this parameter.
    /// </summary>
    string? liffRedirectUri { get; set; }
}

public interface ISigninOIDCResponse_Error
{
    /// <summary>
    /// Error code.
    /// </summary>
    string error { get; set; }
    /// <summary>
    /// A description of the error.
    /// </summary>
    string? error_description { get; set; }
    /// <summary>
    /// The state parameter included in the authorization URL. You can use this value to determine which process was denied.
    /// </summary>
    string? state { get; set; }
}

public class SigninOIDCResponse : ISigninOIDCResponse
{
    public string? code { get; set; }
    public string? state { get; set; }
    public bool? friendship_status_changed { get; set; }
    public string? liffClientId { get; set; }
    public string? liffRedirectUri { get; set; }

    public string? error { get; set; }
    public string? error_description { get; set; }
}
public class SigninOIDCResponse_Success : ISigninOIDCResponse_Success
{
    public string code { get; set; } = "";
    public string state { get; set; } = "";
    public bool? friendship_status_changed { get; set; }
    public string? liffClientId { get; set; }
    public string? liffRedirectUri { get; set; }
}
public class SigninOIDCResponse_Error : ISigninOIDCResponse_Error
{

    public string error { get; set; } = "";
    public string? error_description { get; set; }
    public string? state { get; set; }
}
