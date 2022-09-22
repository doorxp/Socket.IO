using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var io = IO.IO.Connection("ws://192.168.1.2:443");

        io.On("connect", ret =>
        {
            Debug.Log("connected");
        });

        io.On("disconnect", ret =>
        {
            Debug.Log("disconnect");
        });

        io.On("error", ret =>
        {
            Debug.Log("error");
        });

        io.On("reconnect", ret =>
        {
            Debug.Log("reconnect");
        });

        io.Open();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
