using UnityEngine;

public class PortraitManager : MonoBehaviour
{
    public StaringAtPortrait Staring = null;

    [HideInInspector] public static PortraitManager instance = null;

    private int phase = 0;
    private int portraitInteracts = 0;

    [HideInInspector] public bool TerrorGazeEnabled = false;


    private void Awake()
    {
        instance = this;
    }

    public void SetPhase(int newPhase)
    {
        phase = newPhase;
        if (newPhase == 1) TerrorGazeEnabled = true;
        else if (newPhase == 2)
        {
            portraitInteracts = 0;
            TerrorGazeEnabled = false;
        }
    }

    public void HandlePortraitInteract()
    {
        Staring.SetStaring(!Staring.LookingAtPortrait);

        return;

        switch (phase)
        {
            case 0:
                switch (portraitInteracts)
                {
                    case 0:
                        GameManager.instance.DisplayMessage("What the hell is this portrait? Why would they put up something so freaky and then have guests here...");
                        break;
                    case 1:
                        GameManager.instance.DisplayMessage("Looking at this is making me feel ill...");
                        break;
                    case 2:
                        GameManager.instance.DisplayMessage("It may be just because I am so tired and delirious, but this portrait is really making me uncomfortable.");
                        break;
                    case 3:
                        //TODO: Maybe some sort of flash? or noise
                        GameManager.instance.DisplayMessage("No matter where I go, it feels like I am being watched.");
                        break;
                    case 4:
                        GameManager.instance.DisplayMessage("That's it, I can't keep looking.");
                        break;
                    default:
                        GameManager.instance.DisplayMessage("...");
                        SetPhase(1);
                        break;
                }
                break;
            case 1:
                //GameManager.instance.DisplayMessage("I can't keep looking at this!");
                break;
            case 2:
                if (portraitInteracts == 0)
                {
                    //TODO: Sound effect
                    GameManager.instance.DisplayMessage("These were windows...");
                }
                else
                {
                    GameManager.instance.DisplayMessage("I need to get the fuck out of here.");
                }
                break;
        }
        portraitInteracts++;
    }
}
