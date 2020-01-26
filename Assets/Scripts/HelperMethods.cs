using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class HelperMethods {

    private static List<Sprite> defaultSprites = new List<Sprite>();
    private static Sprite botAvatarSprite;

    /// <summary>
    /// Loads default textures to use as avatars.
    /// </summary>
    public static void LoadDefaultTextures() {
        defaultSprites.AddRange(Resources.LoadAll<Sprite>("Animals/"));
        botAvatarSprite = Resources.Load<Sprite>("robot");
    }
    
    /// <summary>
    /// Performs a null-check if string contains another string
    /// </summary>
    /// <param name="source">Source string</param>
    /// <param name="toCheck">string to look for</param>
    /// <param name="comparison">comparison options</param>
    /// <returns>If source string contains another string</returns>
    public static bool Contains(string source, string toCheck, StringComparison comparison)
    {
        return source?.IndexOf(toCheck, comparison) >= 0;
    }
    
    /// <summary>
    /// Replaces all german umlauts with their respective meanings.
    /// </summary>
    /// <param name="str">Input string</param>
    /// <returns>String with replaced umlauts</returns>
    public static string ReplaceGermanUmlauts(string str) {
        var result = str;
        result = result.Replace( "ä", "ae" );
        result = result.Replace( "ö", "oe" );
        result = result.Replace( "ü", "ue" );
        result = result.Replace( "Ä", "Ae" );
        result = result.Replace( "Ö", "Oe" );
        result = result.Replace( "Ü", "Ue" );
        result = result.Replace( "ß", "ss" );
        return result;
    }

    /// <summary>
    /// Generates a 'readable' string of the source string.
    /// This is achieved by replacing all characters which are not in our basic font.
    /// </summary>
    /// <param name="str">Source string</param>
    /// <returns>'readable' string</returns>
    public static string Readable(string str)
    {
        String normalizedString = str.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Finds an image belonging to a username and sets it
    /// </summary>
    /// <param name="img">Image placeholder</param>
    /// <param name="username">Username</param>
    public static void SetImage(Image img, string username) {
        // img.color = GetColorFromUsername(username);
        img.sprite = GetAvatarSpriteFromUsername(username);
    }
    

    /// <summary>
    /// Calculates an image dependent on the username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>Sprite that holds the avatar</returns>
    private static Sprite GetAvatarSpriteFromUsername(string username) {
        if(username == "AI Bot") {
            return botAvatarSprite;
        }
        int index = (username.Length + Math.Abs(username.GetHashCode())) % defaultSprites.Count;
        return defaultSprites[index];
    }
    
    
    /// <summary>
    /// Calculates a color dependent on the username
    /// </summary>
    /// <param name="username">username</param>
    /// <returns>A color hash that was calculated using the username</returns>
    private static Color GetColorFromUsername(string username) {
        var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(username));
        Color color = new Color32(hash[0], hash[1], hash[2], 0xFF);
        return color;
    }

    /// <summary>
    /// Use this to receive a username without its anon-prefix
    /// </summary>
    /// <param name="username">The original username</param>
    /// <returns>Username without prefix</returns>
    public static string GetUsernameWithoutPrefix(string username) {
        return username.Contains("anon-") ? username.Substring(5) : username;
    }
    
    /// <summary>
    /// Generates a password of the specified length.
    /// </summary>
    public static string CreatePassword(int length) {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        string pw = "";
        while (pw.Length < length) {
            pw += valid[UnityEngine.Random.Range(0, valid.Length)];
        }
        return pw;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="userName"></param>
    public static void SendEmail(string userId, string userName)
    {
        string email = "playstore@maltegoetz.com";
        string subject = ConstructEscapeURL("[Q-Wiki] Abuse / Spam Report");
        string body = ConstructEscapeURL($"User ID: {userId}\r\nUsername: {userName}\r\n");
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static string ConstructEscapeURL(string url)
    {
        return UnityWebRequest.EscapeURL(url).Replace("+","%20");
    }
}
