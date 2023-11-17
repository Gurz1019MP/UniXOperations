using UnityEngine;

[ExecuteInEditMode()]
public class Targetting : MonoBehaviour
{
	public GameObject target;

	void Update()
	{
		gameObject.transform.LookAt(target.transform);
	}
}