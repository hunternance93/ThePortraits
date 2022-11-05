
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Toggle = UnityEngine.UI.Toggle;

public class DropDownKeyboardHelper: MonoBehaviour, ISubmitHandler
{
    public void OnSubmit(BaseEventData eventData)
    {
        GetComponentInChildren<Toggle>().Select();
    }
}
