using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeKaedeSayTrigger : MonoBehaviour
{
    public string WhatToSay = "";
    public bool interrupt = true;
    public bool fadeOut = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            switch (WhatToSay) {
                case "SighOfRelief":
                    KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.SighOfRelief, interrupt, fadeOut);
                    break;
                case "ChasedByMonster1":
                    KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.ChasedByMonster1, interrupt, fadeOut);
                    break;
                case "ChasedByMonster2":
                    KaedeVoiceManager.instance.MakeKaedeSay(KaedeVoiceManager.instance.ChasedByMonster2, interrupt, fadeOut);
                    break;
            }
        }
    }
}
