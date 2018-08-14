using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class AutoRotate: MonoBehaviour {
 
 public float xSpeed = 15.0f;
 public float ySpeed = 10.0f;
 public float zSpeed = 5.0f;
  
   // Update is called once per frame
   void Update () {
      // Rotation on x, y and z axis with different speed values 
      transform.Rotate( xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, zSpeed * Time.deltaTime);
   }
}