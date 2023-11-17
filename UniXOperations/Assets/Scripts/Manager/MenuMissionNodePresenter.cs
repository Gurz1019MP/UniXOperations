using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UniRx.Triggers;
using UniRx;

public class MenuMissionNodePresenter : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler
{
    public Color DefaultTextColor;
    public Color MouseOnTextColor;

    [ReadOnly]
    public Text MissionName;
    [ReadOnly]
    public Button Button;

    private Subject<MissionInformation> _selectedSubject = new Subject<MissionInformation>();
    public System.IObservable<MissionInformation> Selected => _selectedSubject;

    public void SetMissionInformation(MissionInformation information)
    {
        MissionName = transform.GetDescendantsWithParent().Single(c => c.name.Equals("Text")).GetComponent<Text>();
        Button = transform.GetDescendantsWithParent().Single(c => c.name.Equals("Button")).GetComponent<Button>();
        _information = information;

        MissionName.text = _information.DisplayName;
        MissionName.color = DefaultTextColor;
        Button.onClick.AddListener(() =>
        {
            _selectedSubject.OnNext(_information);
            _selectedSubject.OnCompleted();
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MissionName.color = MouseOnTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MissionName.color = DefaultTextColor;
    }

    private MissionInformation _information;
}
