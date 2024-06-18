using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BrokenArticle : MonoBehaviour
{
    public void Initialize(VarialbeVisual visual, float jumpPower)
    {
        SetVisual(visual);
        Jump(jumpPower);
        Destroy(gameObject, 3);
    }

    private void SetVisual(VarialbeVisual visual)
    {
        visual.ChangeMesh(gameObject);
        visual.ChangeMaterial(gameObject);
    }

    private void Jump(float jumpPower)
    {
        var rigidBody = GetComponent<Rigidbody>();

        rigidBody.AddTorque(new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-5, 5)) * jumpPower, ForceMode.Impulse);
        rigidBody.AddForce(new Vector3(Random.Range(-1, 1), 3, Random.Range(-1, 1)) * jumpPower, ForceMode.Impulse);
    }
}
