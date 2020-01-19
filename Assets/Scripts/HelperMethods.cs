using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class HelperMethods {

    private static List<Sprite> defaultSprites = new List<Sprite>();

    /// <summary>
    /// Loads default textures to use as avatars.
    /// </summary>
    public static void LoadDefaultTextures() {
        defaultSprites.AddRange(Resources.LoadAll<Sprite>("Animals/"));
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
}
