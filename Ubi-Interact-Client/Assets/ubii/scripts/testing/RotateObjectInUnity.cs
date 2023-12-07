using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectInUnity : MonoBehaviour
{
    public int degreesPerSecond = 20;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(new Vector3(), Vector3.forward, degreesPerSecond * Time.deltaTime);
    }
}
