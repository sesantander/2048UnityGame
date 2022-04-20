using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading;
using System;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] private Slider loadbar;
    [SerializeField] private GameObject loadpanel;
    private double valuebar = 0;

    public void SceneLoad(int sceneIndex)
    {
        loadpanel.SetActive(true);
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneIndex);

        while (valuebar != 0.9)
        {
            loadbar.value = (float)(valuebar / 0.9f);
            valuebar = valuebar + 0.1;
            WaitSomeTime(1000);
            yield return null;

        }
    }
    private void WaitSomeTime(object state)
    {
        Thread.Sleep((int)state);

    }
    public void SoloModeSelect()
    {
        PlayerPrefs.SetString("Mode", "solo");
    }
    public void CoopModeSelect()
    {
        PlayerPrefs.SetString("Mode", "coop");
    }

    public void VersusModeSelect()
    {
        PlayerPrefs.SetString("Mode", "versus");
    }

    public void QuitGameClick()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }

}
