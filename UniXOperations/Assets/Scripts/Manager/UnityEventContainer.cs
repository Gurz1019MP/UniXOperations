using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class UnityEventContainer : MonoBehaviour
{
    public UnityEvent OnClick;

    private void Start()
    {
        OnClick ??= new UnityEvent();
    }
}
