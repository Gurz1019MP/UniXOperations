using UnityEngine;

public class HandlingWeapon : MonoBehaviour
{
    public GameObject Root;
    public GameObject HandlingPosition;
    public GameObject MuzzlePosition;
    public GameObject ModelContainer;

    private GameObject _model;

    public void Initialize(Weapon weapon)
    {
        if (weapon.Spec.CanGetResourceWeaponModel)
        {
            HandlingPosition.transform.localPosition = weapon.Spec.HandlingPosition;
            MuzzlePosition.transform.localPosition = weapon.Spec.MuzzlePosition;

            var model = AssetLoader.LoadAsset<GameObject>(weapon.Spec.GetResourceWeaponModel);
            if (model == null)
            {
                Debug.Log("ピックアップに使用する武器モデルが見つかりませんでした");
            }
            else
            {
                _model = Instantiate(model, ModelContainer.transform);
                _model.layer = gameObject.layer;
            }

            Root.transform.localPosition = -HandlingPosition.transform.localPosition;
        }
    }

    public void Clear()
    {
        HandlingPosition.transform.localPosition = Vector3.zero;
        MuzzlePosition.transform.localPosition = Vector3.zero;
        Destroy(_model);
        Root.transform.localPosition = Vector3.zero;
    }
}
