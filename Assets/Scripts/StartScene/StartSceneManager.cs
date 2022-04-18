using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class StartSceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void SoloModeSelect()
    {
        PlayerPrefs.SetString("Mode", "solo");
        SceneManager.LoadScene("SampleScene");
    }
    public void CoopModeSelect()
    {
        PlayerPrefs.SetString("Mode", "coop");
        SceneManager.LoadScene("SampleScene");
    }
}
