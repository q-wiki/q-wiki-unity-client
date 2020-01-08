using UnityEngine;

namespace Minigame
{
    public class MinigameImage
    {
        private Sprite _sprite;
        private string _license;

        public MinigameImage(Sprite sprite, string license)
        {
            _sprite = sprite;
            _license = license;
        }

        public Sprite Sprite
        {
            get => _sprite;
            set => _sprite = value;
        }

        public string License
        {
            get => _license;
            set => _license = value;
        }
    }
}