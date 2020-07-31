using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour
{
    public GameObject loadingScreen;

    public void LoadScene(int index)
    {
        if(loadingScreen != null) loadingScreen.SetActive(true);
        SceneManager.LoadScene(index);
    }

    public void VK()
    {
        Application.OpenURL("https://vk.com/zhabinmax");
    }

}
