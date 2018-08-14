using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class ProceduralMatcapGUI : ShaderGUI
{
    const string version = "1.1";

    MaterialProperty _Matcap;
    MaterialProperty _MainTex;
    MaterialProperty _BumpMap;

    MaterialProperty _OutlineSize;
    MaterialProperty _OutlineColor;

    MaterialEditor m_MaterialEditor;
    Material material;

    //Meta
    bool showHelp;
    GUIContent matcapTexName = new GUIContent("Matcap texture", "");
    GUIContent mainTexName = new GUIContent("Diffuse", "");
    GUIContent normalMapName = new GUIContent("Normal Map", "");

    //Keyword bools
    bool useNormals;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        this.FindProperties(props);

        this.m_MaterialEditor = materialEditor;
        this.material = m_MaterialEditor.target as Material;

        //Style similar to Standard shader
        m_MaterialEditor.SetDefaultGUIWidths();
        m_MaterialEditor.UseDefaultMargins();
        EditorGUIUtility.labelWidth = 0f;

        // see if redify is set, and show a checkbox
        useNormals = Array.IndexOf(material.shaderKeywords, "_NORMALMAP") != -1;

        EditorGUI.BeginChangeCheck();

        //Draw fields
        DoHeader();
        DoMapsArea();
        DoOutlineArea();

#if UNITY_5_5_OR_NEWER
        m_MaterialEditor.RenderQueueField();
#endif

        if (EditorGUI.EndChangeCheck())
        {
            ApplyKeywords();
        }

        DoFooter();

    }

    public void FindProperties(MaterialProperty[] props)
    {

        //Color
        _Matcap = FindProperty("_Matcap", props);
        _MainTex = FindProperty("_MainTex", props);
        _BumpMap = FindProperty("_BumpMap", props);

        //Outline
        _OutlineColor = FindProperty("_OutlineColor", props);
        _OutlineSize = FindProperty("_OutlineSize", props);


    }

    void DoHeader()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Procedural Matcap Shader " + version, new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            fontSize = 12
        });
        EditorGUILayout.EndHorizontal();
    }

    void DoMapsArea()
    {
        GUILayout.Label("Main maps", EditorStyles.boldLabel);
        this.m_MaterialEditor.TexturePropertySingleLine(matcapTexName, this._Matcap);
        this.m_MaterialEditor.TexturePropertySingleLine(mainTexName, this._MainTex);
        this.m_MaterialEditor.TexturePropertySingleLine(normalMapName, this._BumpMap);
        useNormals = (_BumpMap.textureValue) ? true : false;

        EditorGUILayout.Space();
    }

    void DoOutlineArea()
    {
        GUILayout.Label("Outline", EditorStyles.boldLabel);
        m_MaterialEditor.ShaderProperty(_OutlineColor, _OutlineColor.displayName);
        m_MaterialEditor.ShaderProperty(_OutlineSize, _OutlineSize.displayName);

        EditorGUILayout.Space();
    }

    void DoFooter()
    {
        GUILayout.Label("- Staggart Creations -", new GUIStyle(EditorStyles.centeredGreyMiniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            fontSize = 12
        });
    }

    public void ApplyKeywords()
    {
        // enable or disable the keyword based on checkbox
        if (useNormals)
            material.EnableKeyword("_NORMALMAP");
        else
            material.DisableKeyword("_NORMALMAP");
    }




}
