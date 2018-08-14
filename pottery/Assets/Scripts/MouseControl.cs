using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MouseControl : MonoBehaviour
{


    public GameObject pottery;

    public float Move_x;

    public float Move_y;

    void Update()
    {
        GetMouseMove();


    }

    void GetMouseMove()
    {
        Move_x = Input.mousePosition.x - Camera.main.WorldToScreenPoint(pottery.transform.position).x;

        Move_y = Input.mousePosition.y - Camera.main.WorldToScreenPoint(pottery.transform.position).y;



    }

}