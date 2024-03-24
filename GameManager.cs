using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject arkaplanPanel;
    public GameObject kazandinPanel;
    public GameObject kaybettinPanel;

    public int hedef;
    public int hamleSayisi;
    public int puanlar;

    public bool oyunBittimi;

    public TMP_Text puanlarTxt;
    public TMP_Text hamleTxt;
    public TMP_Text hedefTxt;


    private void Awake()
    {
        Instance = this;
    }


    public void Initialize(int _hamleSayisi, int _hedef)
    {
        hamleSayisi = _hamleSayisi;
        hedef = _hedef;
    }

    // Update is called once per frame
    void Update()
    {
        puanlarTxt.text = "Puan: " + puanlar.ToString();
        hamleTxt.text = "Hamle:" + hamleSayisi.ToString();
        hedefTxt.text = "Hedef: " + hedef.ToString();
    }


    public void ProcessTurn(int _alinacakPuan, bool _hamleAzalt)
    {
        puanlar += _alinacakPuan;
        if(_hamleAzalt)
        {
            hamleSayisi --;

            if(puanlar >= hedef) 
            {
                oyunBittimi = true;
                arkaplanPanel.SetActive(true);
                kazandinPanel.SetActive(true);
                LambaSahnesi.instance.lambaParent.SetActive(false);
                return;
            }
            if (hamleSayisi == 0)
            {
                oyunBittimi = true;
                arkaplanPanel.SetActive(true);
                kaybettinPanel.SetActive(true);
                LambaSahnesi.instance.lambaParent.SetActive(false);
                return;
            }
        }
    }

    //kazanirsan bu ekran gelecek
    public void oyunuKazandin()
    {
        SceneManager.LoadScene(0);
    }

    //kaybedersen bu ekran gelecek
    public void oyunuKaybettin()
    {
        SceneManager.LoadScene(0);
    }
}
