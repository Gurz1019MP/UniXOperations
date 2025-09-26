using System.Collections;
using UnityEngine;

public class FireTrigger : IInputter
{
    #region IInputter

    public float Horizontal { get; private set; }

    public float Vertical { get; private set; }

    public bool Fire { get; private set; }

    public bool Jump { get; private set; }

    public float MouseX { get; private set; }

    public float MouseY { get; private set; }

    public bool Walk { get; private set; }

    public bool Reload { get; private set; }

    public bool DropWeapon { get; private set; }

    public bool Zoom { get; private set; }

    public bool FireMode { get; private set; }

    public bool SwitchWeapon { get; private set; }

    #endregion

    public IEnumerator FireFull()
    {
        Fire = true;
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
        Fire = false;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
    }

    public IEnumerator FireSemi()
    {
        Fire = true;
        yield return new WaitForSeconds(Random.Range(0.2f, 1.0f));
        Fire = false;
        yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
    }
}
