using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiedCharacter : MonoBehaviour
{
    public GameObject _raycastRoot;
    public float fallSpeed;
    public float ArmRotateSpeed;
    public Transform ArmBase;

    private bool isFalled;
    private float fallDirection;
    private float TargetAngle;
    private float _currentArmAngle;

    public void Initialize(Mesh mesh, Material material, Texture2D texture, bool isFrontFall)
    {
        fallDirection = isFrontFall ? 1 : -1;
        SetCharacterVisual(mesh, material, texture);
        TargetAngle = Random.Range(0, 3) == 0 ? -90 : 90;
        _currentArmAngle = ArmBase.localEulerAngles.x;
        if (_currentArmAngle > 180)
        {
            _currentArmAngle -= 360;
        }
    }

    private void Update()
    {
        if (!isFalled && _raycastRoot != null)
        {
            transform.rotation *= Quaternion.Euler(fallSpeed * fallDirection * Time.deltaTime, 0, 0);

            RaycastHit[] hits = Physics.RaycastAll(_raycastRoot.transform.position, _raycastRoot.transform.forward * fallDirection, 0.1f, LayerMask.GetMask("Stage"));
            if (hits.Any())
            {
                isFalled = true;
            }
        }

        if (Mathf.Abs(_currentArmAngle - TargetAngle) > 1f)
        {
            float direction = _currentArmAngle < TargetAngle ? 1 : -1;
            float rotateSpeed = ArmRotateSpeed * direction * Time.deltaTime;
            if (rotateSpeed > Mathf.Abs(TargetAngle - _currentArmAngle))
            {
                rotateSpeed = TargetAngle - _currentArmAngle;
            }
            _currentArmAngle += rotateSpeed;
            ArmBase.localEulerAngles = new Vector3(_currentArmAngle, 0, 0);
        }
    }

    private void SetCharacterVisual(Mesh mesh, Material material, Texture2D texture)
    {
        try
        {
            // ビジュアル用オブジェクトを取得
            var arm = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.CompareTag(ConstantsManager.TagArmHolding)).Select(t => t.gameObject).Single();
            var up = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.CompareTag(ConstantsManager.TagUp)).Select(t => t.gameObject).Single();
            var leg = gameObject.GetComponentsInChildren<Transform>(true).Where(t => t.CompareTag(ConstantsManager.TagLeg)).Select(t => t.gameObject).Single();

            // メッシュの読み込み
            VisualChanger.ChangeMesh(up, mesh);

            // マテリアルの読み込み
            foreach(var target in new GameObject[] { arm, up, leg })
            {
                VisualChanger.ChangeMaterial(target, material, texture);
            }
        }
        catch
        {
            Debug.LogError("死亡キャラクターのビジュアル設定で例外が発生");
        }
    }
}
