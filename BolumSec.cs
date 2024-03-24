using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BolumSec : MonoBehaviour
{
    public void BolumuAc(int levelId)
    {
        string bolumAdi = "Bolum " + levelId;
        SceneManager.LoadScene(bolumAdi);
    }
}
