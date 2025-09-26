using System;

public interface INextPath
{
    public IObservable<short> OnChangeNextPath { get; }
}
