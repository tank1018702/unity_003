using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public int speed;

	
	void Update ()
    {
        transform.Rotate(transform.up, speed * Time.deltaTime);
	}
}
