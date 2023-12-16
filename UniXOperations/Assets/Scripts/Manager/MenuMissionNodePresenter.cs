using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UniRx.Triggers;
using UniRx;

public class MenuMissionNodePresenter : MonoBehaviour
{
    public Color DefaultTextColor;
    public Color MouseOnTextColor;

    [ReadOnly]
    public Text MissionName;

    private Subject<MissionInformation> _selectedSubject = new Subject<MissionInformation>();
    public System.IObservable<MissionInformation> Selected => _selectedSubject;

    public void SetMissionInformation(MissionInformation information)
    {
        MissionName = transform.GetDescendantsWithParent().Single(c => c.name.Equals("Text")).GetComponent<Text>();
        _information = information;

        MissionName.text = _information.DisplayName;
        MissionName.color = DefaultTextColor;
    }

    public void RaiseSelected()
    {
        _selectedSubject.OnNext(_information);
        _selectedSubject.OnCompleted();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cursor"))
        {
            MissionName.color = MouseOnTextColor;
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Cursor"))
        {
            MissionName.color = DefaultTextColor;
        }
    }

    private MissionInformation _information;
}
