using System;
using System.ComponentModel.DataAnnotations.Schema;
using vue_spotify_app.Classes;

/// <summary>
/// Represents a Spotify access token for a user.
/// </summary>
public class SpotifyToken
{
    /// <summary>
    /// The ID used to identify the user.
    /// </summary>
    public Guid ID { get; set; }
    /// <summary>
    /// The token used to authenticate API requests.
    /// </summary>
    public string AccessToken { get; set; } = "";
    /// <summary>
    /// The refresh token used to get a new access token when the current one expires.
    /// </summary>
    public string RefreshToken { get; set; } = "";
    /// <summary>
    /// The UTC time when the access token expires, based on the "expires_in" value returned by Spotify.
    /// </summary>
    [Column(TypeName = "datetime2")]
    public DateTime ExpirationUTC { get; set; }

    public string TokenType { get; set; } = "";

    public string Scope { get; set; } = "";

    public User User { get; set; } = null!;

    public SpotifyToken()
	{
	}
}
