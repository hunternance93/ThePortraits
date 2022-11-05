using UnityEngine;
using UnityEngine.EventSystems;

public class DropDownMenuItemScroll: MonoBehaviour, ISelectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        GetComponentInParent<EventSensitiveScrollRect>().OnUpdateSelected(eventData);
    }
}
