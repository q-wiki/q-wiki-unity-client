using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingDots : MonoBehaviour
{
    //the total time of the animation
    public float repeatTime = 1;

    //the time for a dot to bounce up and come back down
    public float bounceTime = 0.25f;

    //how far does each dot move
    public float bounceHeight = 10;

    public Transform[] dots;


    private void Update()
    {
        for (int i = 0; i < this.dots.Length; i++)
        {
            var p = this.dots[i].localPosition;
            var t = Time.time * this.repeatTime * Mathf.PI + p.x;
            var y = (Mathf.Cos(t) - this.bounceTime) / (1f -  this.bounceTime);
            p.y = Mathf.Max(0, y * this.bounceHeight);
            this.dots[i].localPosition = p;
        }
    }
}
