using System;
using UnityEngine;

namespace Obsolete
{
    [Obsolete("This class is currently not used anymore.", false)]
    public class LoadingDots : MonoBehaviour
    {
        //how far does each dot move
        public float bounceHeight = 10;

        //the time for a dot to bounce up and come back down
        public float bounceTime = 0.25f;

        public Transform[] dots;

        //the total time of the animation
        public float repeatTime = 1;

        private void Update()
        {
            for (var i = 0; i < dots.Length; i++)
            {
                var p = dots[i].localPosition;
                var t = Time.time * repeatTime * Mathf.PI + p.x;
                var y = (Mathf.Cos(t) - bounceTime) / (1f - bounceTime);
                p.y = Mathf.Max(0, y * bounceHeight);
                dots[i].localPosition = p;
            }
        }
    }
}