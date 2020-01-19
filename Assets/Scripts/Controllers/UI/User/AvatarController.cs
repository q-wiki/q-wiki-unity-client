using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.UI.User
{
    public class AvatarController : Singleton<AvatarController>
    {
        public List<Sprite> sprites;
        
        /// <summary>
        /// Finds an image belonging to a username and sets it
        /// </summary>
        /// <param name="img">Image placeholder</param>
        /// <param name="username">Username</param>
        public void SetImage(Image img, string username) {
            // img.color = GetColorFromUsername(username);
            var nameWithoutPrefix = GetUsernameWithoutPrefix(username);
            img.sprite = GetAvatarSpriteFromUsername(nameWithoutPrefix);
        }

        /// <summary>
        /// Calculates an image dependent on the username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Sprite that holds the avatar</returns>
        private Sprite GetAvatarSpriteFromUsername(string username) {
            var index = (username.Length + Math.Abs(username.GetHashCode()) ) % sprites.Count;
            return sprites[index];
        }

        /// <summary>
        /// Calculates a color dependent on the username
        /// </summary>
        /// <param name="username">username</param>
        /// <returns>A color hash that was calculated using the username</returns>
        private Color GetColorFromUsername(string username) {
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
        private string GetUsernameWithoutPrefix(string username) {
            return username.Contains("anon-") ? username.Substring(5) : username;
        }
    }
}