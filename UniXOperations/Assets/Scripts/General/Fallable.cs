using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;

public class Fallable : MonoBehaviour
{
    public Transform BottomTransfrom;
    public float Gravity;
    public float FallDeltaTime;
    [ReadOnly]
    public bool IsGround;

    private Vector3 _lastPosition;
    private Vector3 _moveDelta;

    // Start is called before the first frame update
    void Start()
    {
        Observable.FromCoroutine(Fall).Subscribe().AddTo(gameObject);
    }

    private IEnumerator Fall()
    {
        while (true)
        {
            if (IsGround)
            {
                yield break;
            }

            _lastPosition = BottomTransfrom.position;
            _moveDelta -= Vector3.up * Gravity * FallDeltaTime;
            transform.position += _moveDelta;

            Vector3 displacementSinceLastFrame = BottomTransfrom.position - _lastPosition;
            RaycastHit[] hits = Physics.SphereCastAll(_lastPosition, 0.01f, displacementSinceLastFrame.normalized, displacementSinceLastFrame.magnitude, LayerMask.GetMask("Stage"));

            if (hits.Any())
            {
                IsGround = true;
                _moveDelta = Vector3.zero;
                transform.position = hits[0].point + BottomTransfrom.localPosition * -1;

                yield break;
            }

            yield return new WaitForSeconds(FallDeltaTime);
        }
    }
}
