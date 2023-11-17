using System.Collections;
using System.Linq;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WeaponState Weapon;
    public float Gravity;
    public LayerMask Mask;
    public float UnlockPickupDelay = 0.5f;
    public float PickCheckInterval;
    public float PickCheckRadius;

    private Vector3 _lastPosition;
    private Vector3 _moveDelta;
    private bool isGround;

    void Update()
    {
        if (!isGround)
        {
            _lastPosition = transform.position;
            _moveDelta -= Vector3.up * Gravity * Time.deltaTime;
            transform.position += _moveDelta;

            Vector3 displacementSinceLastFrame = transform.position - _lastPosition;
            RaycastHit[] hits = Physics.RaycastAll(_lastPosition, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, Mask);

            if (hits.Any())
            {
                isGround = true;
                _moveDelta = Vector3.zero;
                transform.position = hits[0].point;
            }
        }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize(WeaponState weapon)
    {
        Weapon.ChangeWeapon(weapon);

        if (!string.IsNullOrEmpty(Weapon.Spec.ModelName))
        {
            GameObject model = AssetLoader.LoadAsset<GameObject>(ConstantsManager.GetResoucePathWeapon(Weapon.Spec.ModelName));
            if (model == null)
            {
                Debug.Log($"ピックアップに使用する武器モデルが見つかりませんでした ModelName:{Weapon.Spec.ModelName}");
            }
            else
            {
                Instantiate(model, transform);
            }
        }

        StartCoroutine(UnlockPick());
    }

    private IEnumerator UnlockPick()
    {
        yield return new WaitForSeconds(UnlockPickupDelay);

        StartCoroutine(PickupCheck());
    }

    private IEnumerator PickupCheck()
    {
        while (true)
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, PickCheckRadius, transform.up, 0, LayerMask.GetMask("Character_Root"));
            if (hits.Any())
            {
                var characterState = hits.First().collider.GetComponent<CharacterState>();
                if (characterState != null)
                {
                    bool isPicked = characterState.PickupWeapon(Weapon);
                    if (isPicked)
                    {
                        Destroy(gameObject);
                        yield break;
                    }
                }
            }

            yield return new WaitForSeconds(PickCheckInterval);
        }
    }
}
