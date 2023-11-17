using System.Collections.Generic;
using UnityEngine;

public class InputTrigger : MonoBehaviour
{
    public List<string> RegisteredInputList;
    private Dictionary<string, bool> _lastInput;
    private Dictionary<string, bool> _isEnterInput;

    void Start()
    {
        _lastInput = new Dictionary<string, bool>();
        _isEnterInput = new Dictionary<string, bool>();
        foreach (string input in RegisteredInputList)
        {
            _lastInput.Add(input, false);
            _isEnterInput.Add(input, false);
        }
    }

    void FixedUpdate()
    {
        foreach (string input in RegisteredInputList)
        {
            _isEnterInput[input] = !_lastInput[input] && Input.GetAxis(input) > 0;
            _lastInput[input] = Input.GetAxis(input) > 0;
        }
    }

    public bool InputEnter(string input)
    {
        if (RegisteredInputList.Contains(input))
        {
            return _isEnterInput[input];
        }
        else
        {
            throw new System.Exception("指定されたInputは登録されていません");
        }
    }
}
