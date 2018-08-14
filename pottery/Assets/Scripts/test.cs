using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour {

    MeshFilter meshFilter;
	// Use this for initialization
	void Start ()
    {
        meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = meshFilter.mesh;
        Vector3[] normals = mesh.normals;
        for(int i=0;i<normals.Length;i++)
        {
            normals[i] = Vector3.right;
        }
        mesh.normals = normals;
        meshFilter.mesh = mesh;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
    [ContextMenu("1111")]
    void test_1()
    {
        StartCoroutine(PrintUV());
    }

    IEnumerator PrintUV()
    {
        for (int i = 0; i < meshFilter.mesh.triangles.Length; i += 3)
        {
            Debug.DrawLine(meshFilter.mesh.uv[meshFilter.mesh.triangles[i]] * 5, meshFilter.mesh.uv[meshFilter.mesh.triangles[i + 1]] * 5, Color.red, 100f);
            yield return new WaitForSeconds(Time.deltaTime);
            Debug.DrawLine(meshFilter.mesh.uv[meshFilter.mesh.triangles[i + 1]] * 5, meshFilter.mesh.uv[meshFilter.mesh.triangles[i + 2]] * 5, Color.yellow, 100f);
            yield return new WaitForSeconds(Time.deltaTime);
            Debug.DrawLine(meshFilter.mesh.uv[meshFilter.mesh.triangles[i + 2]] * 5, meshFilter.mesh.uv[meshFilter.mesh.triangles[i]] * 5, Color.blue, 100f);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}
