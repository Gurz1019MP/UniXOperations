using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BrokenArticle : MonoBehaviour
{
    public void Initialize(Mesh mesh, Material material, Texture2D texture, float jumpPower)
    {
        SetVisual(mesh, material, texture);
        Jump(jumpPower);
        Destroy(gameObject, 3);
    }

    private void SetVisual(Mesh mesh, Material material, Texture2D texture)
    {
        // メッシュの読み込み
        VisualChanger.ChangeMesh(gameObject, mesh);

        // マテリアルの読み込み
        VisualChanger.ChangeMaterial(gameObject, material, texture);
    }

    private void Jump(float jumpPower)
    {
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.AddTorque(new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-5, 5)) * jumpPower, ForceMode.Impulse);
        rigidBody.AddForce(new Vector3(Random.Range(-1, 1), 3, Random.Range(-1, 1)) * jumpPower, ForceMode.Impulse);
    }
}
