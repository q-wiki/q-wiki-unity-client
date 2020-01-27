using UnityEngine;
using WikidataGame.Models;

namespace Minigame
{
    public class MinigameImage
    {
        private Sprite _sprite;
        private ImageInfo _imageInfo;

        public MinigameImage(Sprite sprite, ImageInfo imageInfo)
        {
            _sprite = sprite;
            _imageInfo = imageInfo;
        }

        public Sprite Sprite
        {
            get => _sprite;
        }

        public ImageInfo ImageInfo
        {
            get => _imageInfo;
        }
    }
}