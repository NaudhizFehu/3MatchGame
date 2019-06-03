using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    public float delayTime;
    private float playTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, delayTime);
    }

    // Update is called once per frame
    void Update()
    {
        //playTime += Time.deltaTime;
        //if(playTime > delayTime)
        //{
        //    Destroy(this.gameObject);
        //}
    }
}
