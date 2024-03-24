using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnaMenu : MonoBehaviour
{
   
    public void OyunuOyna()
    {
        SceneManager.LoadSceneAsync(1);
    }

    public void OyundanCik()
    {
        Application.Quit();
    }

}
