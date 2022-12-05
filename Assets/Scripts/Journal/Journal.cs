using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI body;
    [SerializeField] private Image journalImage;
    [SerializeField] private Sprite undiscoveredJournalSprite;
    [SerializeField] private GameObject journalArchiveUI;
    [SerializeField] private GameObject nextEntryButton;
    [SerializeField] private GameObject previousEntryButton;
    [SerializeField] private TextMeshProUGUI areaName;
    [SerializeField] private GameObject nextAreaButton;
    [SerializeField] private GameObject previousAreaButton;
    
    private JournalEntry journalEntry = null;
    private JournalEntry nextEntry = null;
    private JournalEntry previousEntry = null;
    private PauseMenuController pauseMenuController = null;

    private bool inputInitialized = false;
    private bool archiveInputInitialized = false;
    
    private void OnDisable()
    {
        if (pauseMenuController == null)
        {
            GameManager.instance.UnPauseGame();
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        }
        
        AudioManager.instance.PlayTurnPage();
        GameManager.instance.SwitchInputToPrevious();

        if (pauseMenuController != null)
        {
            pauseMenuController.SwitchToPausePage();
        }
    }

    public void OpenJournal(JournalEntry entry)
    {
        pauseMenuController = null;
        
        PopulateEntry(entry);
        
        gameObject.SetActive(true);
        journalArchiveUI.SetActive(false);
        GameManager.instance.PauseGame(true);
        GameManager.instance.SwitchInput(GameManager.instance.controls.Journal.Get());
        
        if (!inputInitialized)
        {
            GameManager.instance.controls.Journal.Close.performed += context => gameObject.SetActive(false);
            GameManager.instance.controls.Journal.ToggleBackground.performed += context => ToggleBackground();
            inputInitialized = true;
        }
    }

    public void OpenJournalArchive(PauseMenuController pauseMenu = null)
    {
        pauseMenuController = pauseMenu;

        if (journalEntry == null)
        {
            if (GameManager.instance.Player.JournalEntries.Count == 0)
            {
                journalEntry = GameManager.instance.allJournalEntries.Find(journal => journal.area == GameArea.Alleyway && journal.areaOrder == 1);
            }
            else
            {
                // Default to whatever journal entry was last shown, but if none was, find the first found one.
                GameArea earliestArea = GameManager.instance.Player.JournalEntries
                    .Min(journal => journal.area);
                journalEntry = GameManager.instance.Player.JournalEntries
                    .Where(journal => journal.area == earliestArea)
                    .OrderBy(journal => journal.areaOrder)
                    .First();
            }
        }

        PopulateEntry(journalEntry, true);
        
        gameObject.SetActive(true);
        journalArchiveUI.SetActive(true);

        if (pauseMenuController == null)
        {
            GameManager.instance.PauseGame();
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }

        AudioManager.instance.PlayOpenJournal();
    }

    public void ToggleBackground()
    {
        //journalImage.gameObject.SetActive(!journalImage.gameObject.activeSelf);
    }

    public void NextEntry()
    {
        if (nextEntry != null)
        {
            AudioManager.instance.PlaySelectUI();
            //AudioManager.instance.PlayTurnPage();
            PopulateEntry(nextEntry, true);
        }
    }

    public void PreviousEntry()
    {
        if (previousEntry != null)
        {
            AudioManager.instance.PlaySelectUI();
            //AudioManager.instance.PlayTurnPage();
            PopulateEntry(previousEntry, true);
        }
    }

    private void SwitchToArea(GameArea area)
    {
        journalEntry = GameManager.instance.Player.JournalEntries
            .Where(journal => journal.area == area)
            .OrderBy(journal => journal.areaOrder)
            .FirstOrDefault();

        if (journalEntry == null)
        {
            journalEntry = GameManager.instance.allJournalEntries.Find(j => j.area == area && j.areaOrder == 1);
        }

        AudioManager.instance.PlaySelectUI();
        //AudioManager.instance.PlayTurnPage();
        PopulateEntry(journalEntry, true);
    }
    
    public void NextArea()
    {
        if (journalEntry.area != GameArea.Forest)
        {
            SwitchToArea(journalEntry.area + 1);
        }
    }

    public void PreviousArea()
    {
        if (journalEntry.area != GameArea.Alleyway)
        {
            SwitchToArea(journalEntry.area - 1);
        }
    }
    
    private void PopulateEntry(JournalEntry entry, bool archiveMode = false)
    {
        Debug.Log("Populating journal entry: " + entry.entryName);
        journalEntry = entry;
        string count = "";

        if (archiveMode)
        {
            int areaCount = GameManager.instance.allJournalEntries.Where(journal => journal.area == entry.area).Count();
            count = $" - {entry.areaOrder} / {areaCount}";
        }

        journalImage.gameObject.SetActive(false);

        if (GameManager.instance.Player.JournalEntries.Contains(entry))
        {
            title.text = entry.entryName;
            body.text = entry.entryText;
            if (entry.background != null) journalImage.sprite = entry.background;
        }
        else
        {
            title.text = "???";
            body.text = "Not yet discovered.";
            journalImage.sprite = undiscoveredJournalSprite;
        }

        int areaDiscoveredCount = GameManager.instance.Player.JournalEntries.Where(j => j.area == entry.area).Count();

        if (areaDiscoveredCount > 0 || entry.area == GameArea.Alleyway)
        {
            areaName.text = entry.GetGameAreaName() + count;
        }
        else
        {
            areaName.text = "???" + count;
        }
        
        previousEntry = GameManager.instance.allJournalEntries.Find(journal => journal.area == entry.area && journal.areaOrder == entry.areaOrder - 1);
        nextEntry = GameManager.instance.allJournalEntries.Find(journal => journal.area == entry.area && journal.areaOrder == entry.areaOrder + 1);
        
        if (previousEntry == null && entry.area != GameArea.Alleyway)
        {
            int lastEntryInPreviousArea = GameManager.instance.allJournalEntries
                .Where(journal => journal.area == entry.area - 1)
                .Max(journal => journal.areaOrder);
            
            previousEntry = GameManager.instance.allJournalEntries.Find(journal => journal.area == entry.area - 1 && journal.areaOrder == lastEntryInPreviousArea);
        }
        
        if (nextEntry == null && entry.area != GameArea.Forest)
        {
            nextEntry = GameManager.instance.allJournalEntries.Find(journal => journal.area == entry.area + 1 && journal.areaOrder == 1);
        }

        previousEntryButton.SetActive(previousEntry != null);
        nextEntryButton.SetActive(nextEntry != null);
        nextAreaButton.SetActive(entry.area != GameArea.Forest);
        previousAreaButton.SetActive(entry.area != GameArea.Alleyway);
    }
}
