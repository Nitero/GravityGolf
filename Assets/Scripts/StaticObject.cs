using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : MonoBehaviour
{
    //private static StaticObject[] staticObjs = new StaticObject[2];
    private static StaticObject staticObj;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);


        /*if (staticObjs[0] == null)
        {
            staticObjs[0] = this;
        }
        else if (staticObjs[1] == null)
        {
            staticObjs[1] = this;
        }*/
        if(staticObj == null)
        {
            staticObj = this;
        }
        else
        {
            DestroyObject(gameObject);
        }
    }

}
