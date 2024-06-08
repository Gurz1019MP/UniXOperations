using System;
using UnityEngine;

public static class VisualChanger
{
    public static void ChangeMesh(GameObject target, Mesh mesh)
    {
        if (target == null) return;

        var meshFilterComponent = target.GetComponent<MeshFilter>();
        var meshColliderComponent = target.GetComponent<MeshCollider>();
        if (meshFilterComponent == null)
        {
            Debug.Log("メッシュの読み込みに失敗");
            return;
        }

        meshFilterComponent.mesh = mesh;
        
        if (meshColliderComponent != null)
        {
            meshColliderComponent.sharedMesh = mesh;
        }
    }

    public static void ChangeMaterial(GameObject target, Material material, Texture2D texture)
    {
        if (target == null) return;

        var component = target.GetComponent<Renderer>();
        if (component == null)
        {
            Debug.Log("マテリアルの読み込みに失敗");
            return;
        }

        component.material = material;
        component.material.mainTexture = texture;
    }
}
