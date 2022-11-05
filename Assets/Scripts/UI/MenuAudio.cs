using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuAudio : MonoBehaviour, ISelectHandler, ISubmitHandler, IPointerClickHandler
{
    public AudioSource altSelectSound;
    public AudioSource altClickSound;

    public bool suppressNext = false;
    public bool dontPlayHover = false;
    public bool dontPlaySelect = false;

    public void SuppressNext()
    {
        suppressNext = true;
    }
    
    public void OnSelect(BaseEventData eventData)
    {
        if (dontPlayHover) return;
        if (suppressNext)
        {
            suppressNext = false;
            return;
        }

        Selectable selectable = GetComponent<Selectable>();
        if (!selectable.IsInteractable())
        {
            return;
        }
        
        if (altSelectSound != null)
        {
            altSelectSound.PlayOneShot(altSelectSound.clip);
        }
        else
        {
            AudioManager.instance.PlayHoverOverUI();
        }
    }

    private void OnButtonClicked()
    {
        if (dontPlaySelect) return;
        if (altClickSound != null)
        {
            altClickSound.PlayOneShot(altClickSound.clip);
        }
        else
        {
            AudioManager.instance.PlaySelectUI();
        }
    }

    // Need both of the following to get both keyboard and mouse button activation
    
    public void OnSubmit(BaseEventData eventData)
    {
        OnButtonClicked();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnButtonClicked();
    }
}
