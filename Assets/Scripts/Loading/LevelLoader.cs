using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LevelLoader : MonoBehaviour
{
    public Sprite ManorImage = null;
    public Sprite UnderneathManorImage = null;
    public Sprite EscapeTownImage = null;
    public Sprite EscapeForestImage = null;
    public Image Picture = null;
    public Slider progressSlider = null;
    public GameObject loadingScreen = null;
    public AudioClip onLoadSound = null;
    
    private void Start()
    {
        LoadScene(LoadingData.sceneToLoad, LoadingData.loadSaveDataAfterLoad);

        switch (LoadingData.sceneToLoad)
        {
            case "Manor":
                Picture.sprite = ManorImage;
                break;
            case "UnderneathManor":
                Picture.sprite = UnderneathManorImage;
                break;
            case "EscapeTown":
                Picture.sprite = EscapeTownImage;
                break;
            case "EscapeForest":
                Picture.sprite = EscapeForestImage;
                break;
            default:
                break;
        }
    }

    public void LoadScene(string sceneName, bool loadSaveData = false)
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadSceneAsync(sceneName, loadSaveData));
    }

    private IEnumerator LoadSceneAsync(string sceneName, bool loadSaveData = false)
    {
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(sceneName);

        bool onLoadSoundPlayed = false;
        while (loadingOperation.progress < 1)
        {
            //progressSlider.value = loadingOperation.progress;

            // Play a sound when we're about to finish loading, to notify the player.
            if (loadingOperation.progress >= .8 && !onLoadSoundPlayed)
            {
                AudioSource.PlayClipAtPoint(onLoadSound, new Vector3());
                onLoadSoundPlayed = true;
            }

            yield return null;
        }
        while(!SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }

        if (loadSaveData)
        {
            SaveGameManager.Instance.LoadAllData();
            LoadingData.loadSaveDataAfterLoad = false;
        }

        Destroy(gameObject);

        
    }

    /* Tell the scene manager to load the LoadingScreen scene and indicate the next level to load.
     */
    public static void LoadLevel(string sceneName, bool loadSaveData = false)
    {
        Debug.Log("Loading Level: " + sceneName);
        LoadingData.sceneToLoad = sceneName;
        LoadingData.loadSaveDataAfterLoad = loadSaveData;
        SceneManager.LoadSceneAsync("LoadingScreen"); // TODO add this to a Constants file somewhere.
    }
}
