using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PrefabInstance : MonoBehaviour
{
    public GameObject prefab;

    public struct Things
    {
        public Mesh mesh;
        public Matrix4x4 matrix;
        public List<Material> materials;
    }
    [System.NonSerialized] public List<Things> things = new List<Things>();

    private void OnValidate()
    {
        things.Clear();
        if (enabled)
        {
            Rebuild(prefab, Matrix4x4.identity);
        }
    }
    private void OnEnable()
    {
        things.Clear();
        if (enabled)
        {
            Rebuild(prefab, Matrix4x4.identity);
        }
    }
    void Rebuild(GameObject source, Matrix4x4 inMatrix)
    {
        if (!source)
        {
            return;
        }
        Matrix4x4 baseMat = inMatrix * Matrix4x4.TRS(-source.transform.position, Quaternion.identity, Vector3.one); // 重构Matrix
        foreach (MeshRenderer mr in source.GetComponentsInChildren(typeof(Renderer), true))  // 依次获取新加组分的Renderer，包含了mesh、材质
        {
            things.Add(new Things()
            {
                mesh = mr.GetComponent<MeshFilter>().sharedMesh,
                matrix = baseMat * mr.transform.localToWorldMatrix,
                materials = new List<Material>(mr.sharedMaterials)  // 这个只是创建材质
            });
        }
        foreach (PrefabInstance pi in source.GetComponentsInChildren(typeof(PrefabInstance), true)) // 依次获取原有Prefab的组分
        {
            if (pi.enabled && pi.gameObject.activeSelf)
            {
                Rebuild(pi.prefab, baseMat * pi.transform.localToWorldMatrix);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EditorApplication.isPlaying) return;
        Matrix4x4 mat = transform.localToWorldMatrix;
        foreach (Things t in things)
        {
            for (int i = 0; i < t.materials.Count; i++)
            {
                Graphics.DrawMesh(t.mesh, mat * t.matrix, t.materials[i], gameObject.layer, null, i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        DrawGizmos(new Color(0, 0, 0, 0));
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmos(new Color(0, 0, 1, .2f));
    }
    void DrawGizmos(Color col)
    {
        if (EditorApplication.isPlaying) return;
        Gizmos.color = col;
        Matrix4x4 mat = transform.localToWorldMatrix;
        foreach(Things t in things)
        {
            Gizmos.DrawCube(t.mesh.bounds.center, t.mesh.bounds.size);
        }
    }
    [PostProcessScene(-2)]
    public static void OnPostprecessScene()
    {
        foreach(PrefabInstance pi in FindObjectsOfType(typeof(PrefabInstance)))
        {
            BakeInstance(pi);
        }
    }
    public static void BakeInstance(PrefabInstance pi)
    {
        if (!pi.prefab || !pi.enabled) return;
        pi.enabled = false;
        GameObject go = PrefabUtility.InstantiatePrefab(pi.prefab) as GameObject;
        Quaternion rot = go.transform.localRotation;
        Vector3 scale = go.transform.localScale;
        go.transform.parent = pi.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = scale;
        go.transform.localRotation = rot;
        pi.prefab = null;
        foreach(PrefabInstance childPi in go.GetComponentsInChildren<PrefabInstance>())
        {
            BakeInstance(childPi);
        }
    }
}
