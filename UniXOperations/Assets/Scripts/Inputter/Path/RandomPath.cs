using System;
using UniRx;

public class RandomPath : IPath, INextPath
{
    public IObservable<short> OnChangeNextPath => _onChangeNextPathSubject;

    public void InputUpdate()
    {
        _onChangeNextPathSubject.OnNext(UnityEngine.Random.Range(0, 1) == 0 ? _pointData.Data3 : _pointData.Data2);
    }

    private readonly PointData _pointData;
    private readonly Subject<short> _onChangeNextPathSubject = new Subject<short>();
}
