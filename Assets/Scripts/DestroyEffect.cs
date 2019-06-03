using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    public float delayTime;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, delayTime);
    }

}
