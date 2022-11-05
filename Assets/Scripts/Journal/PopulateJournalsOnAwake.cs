using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateJournalsOnAwake : MonoBehaviour
{
    public JournalEntry[] journalsToAdd;

    void Start()
    {
        StartCoroutine(WaitAFrameAndPopulate());
    }

    private IEnumerator WaitAFrameAndPopulate()
    {
        yield return null;
        foreach (JournalEntry j in journalsToAdd)
        {
            GameManager.instance.AddJournalEntry(j, true);
        }
    }
}
