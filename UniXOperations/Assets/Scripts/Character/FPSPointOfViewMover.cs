using UnityEngine;

[RequireComponent(typeof(CharacterInputterContainer))]
public class FPSPointOfViewMover : MonoBehaviour
{
    public float mouseSensitivity = 150;
    public float maxAngle = 90;
    public GameObject HorizontalObject;

    private CharacterInputterContainer _characterInputterContainer;
    private float currentXAngle;

    public ICharacterInputter Inputter => _characterInputterContainer.Inputter;

    public void Initialize(CharacterState characterState)
    {
        _characterInputterContainer = characterState.InputterContainer;
        currentXAngle = HorizontalObject.transform.localEulerAngles.x;
    }

    private void Update()
    {
        if (HorizontalObject != null && Inputter != null)
        {
            Vector3 rotate = new Vector3(Inputter.MouseY, Inputter.MouseX, 0) * mouseSensitivity * Time.deltaTime;
            gameObject.transform.Rotate(0, rotate.y, 0);

            currentXAngle = Mathf.Clamp(currentXAngle + rotate.x, -maxAngle, maxAngle);
            HorizontalObject.transform.localEulerAngles = new Vector3(currentXAngle, 0, 0);
        }
    }

    public void MuzzleJump(float jumpMagnitude)
    {
        if (HorizontalObject != null)
        {
            Vector3 rotate = new Vector3(-0.2f, Random.Range(-0.1f, 0.1f), 0) * jumpMagnitude;
            gameObject.transform.Rotate(0, rotate.y, 0);

            currentXAngle = Mathf.Clamp(currentXAngle + rotate.x, -maxAngle, maxAngle);
            HorizontalObject.transform.localEulerAngles = new Vector3(currentXAngle, 0, 0);
        }
    }
}
