using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JournalInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private JournalEntry journalEntry;
    
    public void Interacted()
    {
        GameManager.instance.AddJournalEntry(journalEntry);
        AudioManager.instance.PlayOpenJournal();
        GameManager.instance.SaveGameManager.SaveJournal();
        //Destroy(gameObject);
    }

    public string GetName()
    {
        return journalEntry.entryName;
    }

    public string GetText()
    {
        return journalEntry.entryText;
    }

    public void SetJournalEntry(JournalEntry entry)
    {
        journalEntry = entry;
    }
}
