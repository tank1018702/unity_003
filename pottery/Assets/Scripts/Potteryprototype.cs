using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Potteryprototype : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    Mesh mesh;

    public int details = 40;
    public int layer = 10;
    public float Height = 0.2f;

    public float OuterRadius = 1.0f;
    public float InnerRadius = 0.5f;

    List<Vector3> vertices;
    List<Vector2> UV;
    List<int> triangles;

    float EachAngle;
    int SideCount;



    public List<Material> materials;

    bool iscreate=true;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        
        
    }

    public void Create()
    {
        GeneratePrototype();
        meshRenderer.material = materials[0];
        GetComponent<Rotate>().speed = 720;
        iscreate = true;
        transform.localScale = Vector3.one;
    }

    public void Bake()
    {
        NormalsReBuild();
        meshRenderer.material = materials[1];
        GetComponent<Rotate>().speed = 10;
        iscreate = false;
    }

    [ContextMenu("GeneratePottery")]
    void GeneratePrototype()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
        UV = new List<Vector2>();

        EachAngle = Mathf.PI * 2 / details;
        for (int i = 0; i < layer; i++)
        {
            GenerateCircle(i);
        }
        Capping();

        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = UV.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    void GenerateCircle(int _layer)
    {
        //外顶点与内顶点分开存储,方便变化操作时的计算
        List<Vector3> vertices_outside = new List<Vector3>();
        List<Vector3> vertices_inside = new List<Vector3>();


        List<Vector2> UV_outside = new List<Vector2>();
        List<Vector2> UV_inside = new List<Vector2>();


        //外侧和内侧顶点计算
        //注意这里让每一圈的首尾重合了,也就是开始和结尾的顶点坐标一致
        //目的是计算UV坐标时不会出现空缺
        for (float i = 0; i <= Mathf.PI * 2 + EachAngle; i += EachAngle)
        {
            Vector3 v1 = new Vector3(OuterRadius * Mathf.Sin(i), _layer * Height, OuterRadius * Mathf.Cos(i));
            Vector3 v2 = new Vector3(OuterRadius * Mathf.Sin(i), (_layer + 1) * Height, OuterRadius * Mathf.Cos(i));
            Vector3 v3 = new Vector3(InnerRadius * Mathf.Sin(i), _layer * Height, InnerRadius * Mathf.Cos(i));
            Vector3 v4 = new Vector3(InnerRadius * Mathf.Sin(i), (_layer + 1) * Height, InnerRadius * Mathf.Cos(i));
            vertices_outside.Add(v1); vertices_outside.Add(v2);
            vertices_inside.Add(v3); vertices_inside.Add(v4);

            Vector2 uv1 = new Vector2(i / Mathf.PI * 2, _layer * 1.0f / layer * 1.0f);
            Vector2 uv2 = new Vector2(i / Mathf.PI * 2, (_layer + 1) * 1.0f / layer * 1.0f);
            Vector2 uv3 = new Vector2(i / Mathf.PI * 2, _layer * 1.0f / layer * 1.0f);
            Vector2 uv4 = new Vector2(i / Mathf.PI * 2, (_layer + 1) * 1.0f / layer * 1.0f);
            UV_outside.Add(uv1); UV_outside.Add(uv2);
            UV_inside.Add(uv3); UV_inside.Add(uv4);
        }
        vertices.AddRange(vertices_outside);
        vertices.AddRange(vertices_inside);

        UV.AddRange(UV_outside);
        UV.AddRange(UV_inside);

        SideCount = vertices_outside.Count;
        int j = vertices_outside.Count * _layer * 2;
        int n = vertices_outside.Count;
        for (int i = j; i < j + vertices_outside.Count - 2; i += 2)
        {

            triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 1);
            triangles.Add(i + 2); triangles.Add(i + 3); triangles.Add(i + 1);

            triangles.Add(i + n); triangles.Add(i + n + 1); triangles.Add(i + n + 2);
            triangles.Add(i + n + 2); triangles.Add(i + n + 1); triangles.Add(i + n + 3);
        }
    }
    //封顶,底面由于看不见就不用管了
    void Capping()
    {

        for (float i = 0; i <= Mathf.PI * 2 + EachAngle; i += EachAngle)
        {
            Vector3 outer = new Vector3(OuterRadius * Mathf.Sin(i), layer * Height, OuterRadius * Mathf.Cos(i));
            Vector3 inner = new Vector3(InnerRadius * Mathf.Sin(i), layer * Height, InnerRadius * Mathf.Cos(i));

            vertices.Add(outer); vertices.Add(inner);

            Vector2 uv1 = new Vector2(i / Mathf.PI * 2, 0); Vector2 uv2 = new Vector2(i / Mathf.PI * 2, 1);

            UV.Add(uv1); UV.Add(uv2);
        }
        int j = SideCount * layer * 2;
        for (int i = j; i < vertices.Count - 2; i += 2)
        {
            triangles.Add(i); triangles.Add(i + 3); triangles.Add(i + 1);
            triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
        }
        triangles.Add(vertices.Count - 2); triangles.Add(j + 1); triangles.Add(vertices.Count - 1);
        triangles.Add(vertices.Count - 2); triangles.Add(j); triangles.Add(j + 1);

    }

    void GetMouseControlTransform()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit info;
        if (Physics.Raycast(ray.origin, ray.direction, out info)&&iscreate)
        {

            Mesh mesh = meshFilter.mesh;
            Vector3[] _vertices = mesh.vertices;

            for (int i = 0; i < _vertices.Length; i++)
            {

                //x,z平面变换
                if (Mathf.Abs(info.point.y - transform.TransformPoint(_vertices[i]).y) < (5 * Height))
                {

                    Quaternion q;
                    Vector3 axis;
                    float angle;
                    transform.rotation.ToAngleAxis(out angle, out axis);
                    q = Quaternion.AngleAxis(-angle, axis);
                    //因为物体本身在旋转,顶点相对中心点的方向也会发生改变,相对方向需要乘以一个反向旋转的四元数抵消物体自身旋转的影响
                    //方法1
                    //Vector3 v_xz = q* (transform.TransformPoint(_vertices[i]) - new Vector3(transform.position.x, transform.TransformPoint(_vertices[i]).y, transform.position.z)).normalized;
                    //方法2
                    Vector3 v_xz = transform.TransformDirection(transform.InverseTransformPoint(_vertices[i]) - transform.InverseTransformPoint(new Vector3(0, _vertices[i].y, 0)));



                    //Debug.DrawLine(transform.TransformPoint(_vertices[i]), new Vector3(transform.position.x, transform.TransformPoint(_vertices[i]).y, transform.position.z), Color.green, 0.1f);
                    //Debug.DrawRay(transform.TransformPoint(_vertices[i]), v_xz, Color.red, 0.01f);

                    //Debug.DrawRay(transform.TransformPoint(_vertices[i]), new Vector3(0, transform.TransformPoint(_vertices[i]).y, 0), Color.red, 0.1f);
                    int n = i / SideCount;
                    bool side = n % 2 == 0;

                    bool caps = (i - (SideCount * layer * 2)) % 2 == 0;

                    float max;
                    float min;
                    if (i < SideCount * layer * 2)
                    {
                        max = side ? 2f * OuterRadius : 2f * OuterRadius - (OuterRadius - InnerRadius);

                        min = side ? 0.5f * OuterRadius : 0.5f * OuterRadius - (OuterRadius - InnerRadius);
                    }
                    else
                    {
                        max = caps ? 2f * OuterRadius : 2f * OuterRadius - (OuterRadius - InnerRadius); ;
                        min = caps ? 0.5f * OuterRadius : 0.5f * OuterRadius - (OuterRadius - InnerRadius);
                    }

                    float dif = Mathf.Abs(info.point.y - transform.TransformPoint(_vertices[i]).y);
                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        float outer = max - v_xz.magnitude;
                        _vertices[i] += v_xz.normalized * Mathf.Min(0.01f * Mathf.Cos(((dif / (5 * Height)) * Mathf.PI) / 2), outer);
                    }
                    else if (Input.GetKey(KeyCode.LeftArrow))
                    {
                        float inner = v_xz.magnitude - min;
                        _vertices[i] -= v_xz.normalized * Mathf.Min(0.01f * Mathf.Cos(((dif / (5 * Height)) * Mathf.PI) / 2), inner);
                    }

                }
                //Y轴变换

                //int cur_layer = i / (2 * SideCount)+1;

                //int max_layer = cur_layer;

                //int min_layer = cur_layer- (layer+1)>0?cur_layer-(layer+1):0;
                //bool cut = i % 2 == 0;

                //float max_y;
                //float min_y;

                //if (i < SideCount * layer * 2)
                //{
                //    max_y = cut ? (max_layer-1) * Height : max_layer * Height;
                //    min_y =  min_layer * Height ;
                //}
                //else
                //{
                //    max_y = (max_layer-1) * Height;
                //    min_y = min_layer * Height;
                //}
                float scale_y = transform.localScale.y;
                if (Input.GetKey(KeyCode.UpArrow))
                {

                    //float Amount_up = max_y - transform.TransformPoint(_vertices[i]).y > 0 ? max_y - transform.TransformPoint(_vertices[i]).y : 0;
                    //_vertices[i] += transform.up * Mathf.Min(0.01f, max_y - transform.TransformPoint(_vertices[i]).y);

                    scale_y = Mathf.Min(transform.localScale.y + 0.000001f, 2.0f);

                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    //float Amount_down = transform.TransformPoint(_vertices[i]).y - min_y > 0 ? transform.TransformPoint(_vertices[i]).y - min_y : 0;
                    //_vertices[i] += -transform.up * Mathf.Min(0.01f, Amount_down);

                    scale_y = Mathf.Max(transform.localScale.y - 0.000001f, 0.3f);
                }
                transform.localScale = new Vector3(transform.localScale.x, scale_y, transform.localScale.z);
            }


            mesh.vertices = _vertices;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

    }
    [ContextMenu("ReBuildNormals")]
    void NormalsReBuild()
    {
        Mesh mesh = meshFilter.mesh;
        Vector3[] normals = mesh.normals;

        //for (int i = 0; i < mesh.normals.Length; i++)
        //{
        //    bool flag = (i / SideCount) % 2 == 0;
        //    if (i < SideCount * layer * 2)
        //    {
        //        Vector3 tempnormals = transform.TransformDirection(transform.InverseTransformPoint(mesh.vertices[i]) - transform.InverseTransformPoint(new Vector3(0, mesh.vertices[i].y, 0)));
        //        normals[i] = flag ? tempnormals : -tempnormals;
        //        if (i >= SideCount * 2 && i <= mesh.vertices.Length - (mesh.vertices.Length - (layer - 1) * SideCount * 2))
        //        {

        //            Vector3 aixs = transform.TransformDirection(Vector3.Cross(normals[i], mesh.vertices[i - SideCount * 2] - mesh.vertices[i])).normalized;

        //            Quaternion q = Quaternion.FromToRotation(transform.InverseTransformDirection(mesh.vertices[i - SideCount * 2] - mesh.vertices[i]), transform.InverseTransformDirection(mesh.vertices[i + SideCount * 2] - mesh.vertices[i]));
        //            Quaternion q = Quaternion.FromToRotation(Vector3.up, -Vector3.up);
        //            float angle;
        //            Vector3 aixs2;
        //            q.ToAngleAxis(out angle, out aixs2);


        //            Debug.Log(angle);

        //            Debug.Log(aixs2);
        //            Debug.DrawRay(transform.TransformPoint(mesh.vertices[i]), transform.TransformDirection(mesh.vertices[i - SideCount * 2] - mesh.vertices[i]), Color.red, 10f);
        //            Debug.DrawRay(transform.TransformPoint(mesh.vertices[i]), transform.TransformDirection(mesh.vertices[i + SideCount * 2] - mesh.vertices[i]), Color.blue, 10f);
        //            q = Quaternion.AngleAxis(angle / 2, aixs2);

        //            normals[i] = q * normals[i];


        //        }


        //    }
        //}
        //for (int i = 1; i < layer - 1; i++)
        //{
        //    for (int j = 0; j < SideCount * 2; j++)
        //    {
        //        int index = i * SideCount * 2 + j;
        //        bool mark = index % 2 == 0;
        //        Vector3 tempnormals = transform.TransformDirection(transform.InverseTransformPoint(mesh.vertices[index]) - transform.InverseTransformPoint(new Vector3(0, mesh.vertices[index].y, 0)));
        //        normals[index] = j < SideCount ? tempnormals : -tempnormals;


        //        Vector3 aixs = transform.TransformDirection(Vector3.Cross(normals[index], mesh.vertices[index - SideCount * 2] - mesh.vertices[index])).normalized;

        //        Quaternion q = Quaternion.FromToRotation(transform.InverseTransformDirection(mesh.vertices[index + SideCount * 2] - mesh.vertices[index]),
        //               transform.InverseTransformDirection(mesh.vertices[index - SideCount * 2]) - mesh.vertices[index]);
        //        Vector3 aixs2;
        //        float angle;
        //        q.ToAngleAxis(out angle, out aixs2);

        //        Quaternion current;
        //        if (mark)
        //        {
        //            current = Quaternion.AngleAxis(Mathf.Abs(angle - 180f) / 2, aixs2);



        //            //Vector3 angle = q.eulerAngles;
        //            //Debug.Log(angle);

        //        }
        //        else
        //        {
        //            current = Quaternion.AngleAxis(Mathf.Abs(angle - 180f) / 2, aixs2);


        //        }
        //        Debug.Log(angle);
        //        normals[index] = current * normals[index];

        //        //Debug.DrawRay(transform.TransformPoint(mesh.vertices[index]), transform.TransformDirection(mesh.vertices[index - SideCount * 2] - mesh.vertices[index]), Color.red, 1000f);
        //        //Debug.DrawRay(transform.TransformPoint(mesh.vertices[index]), transform.TransformDirection(mesh.vertices[index + SideCount * 2] - mesh.vertices[index]), Color.blue, 1000f);
        //        //Debug.DrawRay(transform.TransformPoint(mesh.vertices[index]), aixs*0.1f, Color.red, 10f);
        //    }


        //}

        //上下法线平均
        for (int i = SideCount * 2; i < SideCount * 2 * (layer - 1); i++)
        {
            bool mark = i % 2 == 0;
            if (mark)
            {
                normals[i] = (normals[i] + normals[(i + 1) - SideCount * 2]).normalized;
            }
        }

        for (int i = SideCount * 2; i < SideCount * 2 * (layer - 1); i++)
        {
            bool mark = i % 2 == 0;
            if (!mark)
            {
                normals[i] = normals[i - 1 + SideCount * 2];
            }
        }
        //第一层
        for (int i = 0; i < SideCount * 2; i++)
        {
            bool mark = i % 2 == 0;
            if (i < SideCount)
            {
                if (!mark)
                {
                    normals[i] = normals[i - 1 + SideCount * 2];
                }
                else
                {
                    normals[i] = transform.TransformDirection(transform.InverseTransformPoint(mesh.vertices[i]) - transform.InverseTransformPoint(new Vector3(0, mesh.vertices[i].y, 0)));
                }
            }
            else
            {
                if (!mark)
                {
                    normals[i] = normals[i - 1 + SideCount * 2];
                }
                else
                {
                    normals[i] = transform.TransformDirection(transform.InverseTransformPoint(new Vector3(0, mesh.vertices[i].y, 0)) - transform.InverseTransformPoint(mesh.vertices[i]));
                }
            }

        }

        for (int i = 0; i < layer; i++)
        {
            //外侧首尾相接法线平均
            normals[SideCount * i * 2] = (normals[SideCount * i * 2] + normals[SideCount * (i * 2 + 1) - 2]).normalized;
            normals[SideCount * (i * 2 + 1) - 2] = normals[SideCount * i * 2];
            normals[SideCount * i * 2 + 1] = (normals[SideCount * i * 2 + 1] + normals[SideCount * (i * 2 + 1) - 1]).normalized;
            normals[SideCount * (i * 2 + 1) - 1] = normals[SideCount * i * 2 + 1];

            //内侧
            normals[SideCount * (i * 2 + 1)] = (normals[SideCount * (i * 2 + 1)] + normals[SideCount * (i + 1) * 2 - 2]).normalized;
            normals[SideCount * (i + 1) * 2 - 2] = normals[SideCount * (i * 2 + 1)];
            normals[SideCount * (i * 2 + 1) + 1] = (normals[SideCount * (i * 2 + 1) + 1] + normals[SideCount * (i + 1) * 2 - 1]).normalized;
            normals[SideCount * (i + 1) * 2 - 1] = normals[SideCount * (i * 2 + 1) + 1];
        }
        //for(int i = SideCount * 2 * (layer - 1); i < SideCount * 2 * layer; i++)
        //{
        //    bool mark = i % 2 == 0;
        //    if(mark)
        //    {
        //        normals[i] = (normals[i] + normals[i + 1]).normalized;
        //    }
        //    else
        //    {
        //        normals[i] = normals[i - 1];
        //    }
        //}

        //for (int i = SideCount * 2 * (layer - 1); i < SideCount * 2 * layer; i++)
        //{
        //    bool mark = i % 2 == 0;
        //    if (mark)
        //    {
        //        normals[i] = (normals[i] + normals[(i + 1) - SideCount * 2]).normalized;
        //    }
        //    else
        //    {
        //        if (i < SideCount * (2 * layer - 1))
        //        {
        //            normals[i] = (normals[i] + normals[SideCount * 2 * layer + (i - (SideCount * (2 * layer - 1)) / 2) * 2]).normalized;
        //        }
        //        else
        //        {
        //            normals[i] = (normals[i] + normals[SideCount * 2 * layer + (i - (SideCount * (2 * layer - 1)) / 2) * 2 + 1]).normalized;
        //        }
        //    }

        //}

        //最上层
        for (int i = SideCount * 2 * (layer - 1); i < normals.Length; i++)
        {
            bool mark = i % 2 == 0;

            if (i < SideCount * (2 * layer - 1))
            {
                if (!mark)
                {
                    normals[i] = (normals[i] + normals[i + SideCount * 2 - 1]).normalized;
                }
                //else
                //{
                //    normals[i] = (normals[i] + normals[i + SideCount]).normalized;
                //}
            }
            else if (i < SideCount * layer * 2)
            {
                if (!mark)
                {
                    normals[i] = (normals[i] + normals[i + SideCount]).normalized;
                }
            }
            else
            {
                if (mark)
                {
                    normals[i] = normals[i - SideCount * 2 + 1];
                }
                else
                {
                    normals[i] = normals[i - SideCount];
                }
            }


        }

        mesh.normals = normals;
        meshFilter.mesh = mesh;




    }

    [ContextMenu("details_info")]
    void test()
    {
        StartCoroutine(Print_Normals());
        //StartCoroutine(print_vertices());
        //StartCoroutine(PrintUV());

    }
    IEnumerator print_vertices()
    {
        for (int i = 0; i < meshFilter.mesh.triangles.Length; i += 3)
        {
            Debug.DrawLine(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i]] * 5, meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 1]] * 5, Color.red, 100f);
            yield return new WaitForSeconds(Time.deltaTime);
            Debug.DrawLine(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 1]] * 5, meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 2]] * 5, Color.yellow, 100f);
            yield return new WaitForSeconds(Time.deltaTime);
            Debug.DrawLine(meshFilter.mesh.vertices[meshFilter.mesh.triangles[i + 2]] * 5, meshFilter.mesh.vertices[meshFilter.mesh.triangles[i]] * 5, Color.blue, 100f);
            yield return new WaitForSeconds(Time.deltaTime);
        }
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

    IEnumerator Print_Normals()
    {
     
        for (int i = 0; i < meshFilter.mesh.vertices.Length; i++)
        {    
            if (i % 2 == 0)
            {
                Debug.DrawRay(transform.TransformPoint(meshFilter.mesh.vertices[i]), transform.TransformDirection(meshFilter.mesh.normals[i] * 0.3f), Color.green, 1000f);
            }
            else
            {
                Debug.DrawRay(transform.TransformPoint(meshFilter.mesh.vertices[i]), transform.TransformDirection(meshFilter.mesh.normals[i] * 0.3f), Color.blue, 1000f);
            }

            yield return new WaitForSeconds(Time.deltaTime);

        }
    }

void Update()
{
    GetMouseControlTransform();
}
}
