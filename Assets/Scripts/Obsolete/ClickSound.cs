using System;
using UnityEngine;
using UnityEngine.UI;

namespace Obsolete
{
    /// <summary>
    ///     This class is used to generate sounds when the client clicks on a button etc.
    /// </summary>
    [Obsolete("This class is currently not in use.")]
    public class ClickSound : MonoBehaviour
    {
        public AudioClip clickSound;

        private Button button => GetComponent<Button>();
        private AudioSource source => GetComponent<AudioSource>();

        /// <summary>
        ///     Start is called before the first frame update.
        /// </summary>
        private void Start()
        {
            gameObject.AddComponent<AudioSource>();
            source.clip = clickSound;
            source.playOnAwake = false;

            button.onClick.AddListener(() => PlaySound());
        }

        /// <summary>
        ///     One shot of a sound is played.
        /// </summary>
        private void PlaySound()
        {
            source.PlayOneShot(clickSound);
        }
    }
}