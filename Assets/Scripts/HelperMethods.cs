using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class HelperMethods {

    private static List<Sprite> defaultSprites = new List<Sprite>();

    public static void LoadDefaultTextures() {
        defaultSprites.AddRange(Resources.LoadAll<Sprite>("Animals/"));
    }

    public static void SetImage(Image img, string username) {
        // img.color = GetColorFromUsername(username);
        img.sprite = GetAvatarSpriteFromUsername(username);
    }

    private static Sprite GetAvatarSpriteFromUsername(string username) {
        int index = (username.Length + Math.Abs(username.GetHashCode())) % defaultSprites.Count;
        return defaultSprites[index];
    }

    //private static Color GetColorFromUsername(string username) {
    //    MD5 md5 = MD5.Create();
    //    byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(username));
    //    Color color = new Color32(hash[0], hash[1], hash[2], 0xFF);
    //    return color;
    //}

    public static string GetPrefixFreeUsername(string username) {
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
