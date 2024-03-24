using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LambaSahnesi : MonoBehaviour
{
    //sahne ebati ayalama icin
    public int genislik = 6;
    public int yukseklik = 8;

    //boslukları ayarla
    public float boslukX;
    public float boslukY;

    //lambalarimin resimlerinin referansini al
    public GameObject[] lambaPrefabs;

    //Lamba sahnesinden referans al yani lambaSahne
    public Node[,] lambaSahnesi;
    public GameObject lambaSahnesiGO;

    public List<GameObject> yokedilecekLambalar = new();
    public GameObject lambaParent;

    [SerializeField]
    private Lamba selectedLamba;

    [SerializeField]
    private bool isProcessingMove;

    [SerializeField]
    List<Lamba> kaldirilacakLambalar = new();

    public ArrayLayout arrayLayout;
    

    public static LambaSahnesi instance;

    public void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SahneyiBaslat();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<Lamba>())
            {
                if (isProcessingMove)
                    return;

                Lamba lamba = hit.collider.gameObject.GetComponent<Lamba>();
                Debug.Log("Lambaya tikladim ve bu: " + lamba.gameObject);

                SelectedLamba(lamba);
            }
        }
    }
    void SahneyiBaslat()
    {
        DestroyLambalar();
        lambaSahnesi = new Node[genislik, yukseklik];

        boslukX = (float)((genislik - 1) / 2);
        boslukY = (float)((yukseklik - 1) / 2) + 1;

        for (int y = 0; y < yukseklik; y++)
        {
            for (int x = 0; x < genislik; x++)
            {
                Vector2 position = new Vector2(x - boslukX, y - boslukY);
                if (arrayLayout.rows[y].row[x])
                {
                    lambaSahnesi[x, y] = new Node(false, null);
                }

                else
                {
                    int rastgeleDeger = Random.Range(0, lambaPrefabs.Length);

                    GameObject lamba = Instantiate(lambaPrefabs[rastgeleDeger], position, Quaternion.identity);
                    lamba.transform.SetParent(lambaParent.transform);
                    lamba.GetComponent<Lamba>().SetIndicies(x, y);
                    lambaSahnesi[x, y] = new Node(true, lamba);
                    yokedilecekLambalar.Add(lamba);
                }
            }
        }
        if (CheckSahne())
        {
            Debug.Log("Eslesmelerimiz var ve sahnemizi yeniden olusturalım");
            SahneyiBaslat();
        }
        else
        {
            Debug.Log("Herhangi bir eslesme yok, oyunu baslatma zamani geldide geciyorrrrr.");
        }
    }

    private void DestroyLambalar()
    {
        if (yokedilecekLambalar != null)
        {
            foreach (GameObject lamba in yokedilecekLambalar)
            {
                Destroy(lamba);
            }
            yokedilecekLambalar.Clear();
        }
    }

    public bool CheckSahne()
    {
        if (GameManager.Instance.oyunBittimi)
            return false;
        Debug.Log("Sahne kontrol ediliyor");
        bool hasMatched = false;

        kaldirilacakLambalar.Clear();

        foreach(Node nodeLamba in lambaSahnesi) 
        {
            if(nodeLamba.lamba != null)
            {
                nodeLamba.lamba.GetComponent<Lamba>().isMatched = false;
            }
        }

        for (int x = 0; x < genislik; x++)
        {
            for(int y = 0; y < yukseklik; y++)
            {
                if (lambaSahnesi[x, y].isUsable)
                {
                    Lamba lamba = lambaSahnesi[x,y].lamba.GetComponent<Lamba>();

                    if (!lamba.isMatched)
                    {
                        MatchResult matchedLambalar = IsConnected(lamba);

                        if (matchedLambalar.connectedLambalar.Count >= 3)
                        {
                            MatchResult superMatchedLambalar = SuperMatch(matchedLambalar);

                            kaldirilacakLambalar.AddRange(superMatchedLambalar.connectedLambalar);

                            foreach(Lamba lam in superMatchedLambalar.connectedLambalar)
                                lam.isMatched = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }

        return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _hamleAzalt)
    {
        foreach (Lamba kaldirilacakLamba in kaldirilacakLambalar)
        {
            kaldirilacakLamba.isMatched = false;
        }
        RemoveAndRefill(kaldirilacakLambalar);
        GameManager.Instance.ProcessTurn(kaldirilacakLambalar.Count, _hamleAzalt);
        yield return new WaitForSeconds(0.4f);

        if (CheckSahne())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }
    private void RemoveAndRefill(List<Lamba> _kaldirilacakLambalar)
    {
        //Lambayi sahneden kaldirmak ve temizlemek icin
        foreach (Lamba lamba in _kaldirilacakLambalar)
        {
            //x ve y yi alıp depolayalim
            int _xIndex = lamba.xIndex;
            int _yIndex = lamba.yIndex;

            //lambayi yok et
            Destroy(lamba.gameObject);

            //bosluk yarat
            lambaSahnesi[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x=0; x < genislik; x++)
        {
            for(int y=0; y < yukseklik; y++)
            {
                if (lambaSahnesi[x,y].lamba == null)
                {
                    Debug.Log("X lokasyonu:" + x + "Y: " + y + "bos. Doldurululmaya calisiliyor.");
                    RefillLamba(x, y);
                }
                
            }
        }
    }

    private void RefillLamba(int x, int y)
    {
        int yOffset = 1;

        while (y + yOffset < yukseklik && lambaSahnesi[x, y + yOffset].lamba == null) 
        {
            Debug.Log("Altimda ki lamba bos, ama sahnenin ustunde de henuz degilim yani benim yoffsetime ekle ve tekrar dene. Su anki offset: " + yOffset + " birinci olarak");
            yOffset++;
        }

        //ne hikmetse sahnenin en ustu vurmadik ya da lamba bulduk

        if(y + yOffset < yukseklik && lambaSahnesi[x, y + yOffset].lamba != null)
        {
            //lamba bulduk

            Lamba lambaAbove = lambaSahnesi[x,y + yOffset].lamba.GetComponent<Lamba>();

            //doru yere yerlestir.
            Vector3 targetPos =new Vector3(x - boslukX, y - boslukY,lambaAbove.transform.position.z);
            Debug.Log("Sahneyi doldururken bir lamba buldum ve bunun lokasyonu: [" + x + "," + (y + yOffset) + "ve bunu bu pozisyona tasidik: [" + x + "," + y + "]");
            //Lokasyona yonlendir.
            lambaAbove.MoveToTarget(targetPos);
            //guncelle 
            lambaAbove.SetIndicies(x, y);
            //lambalari guncelle.
            lambaSahnesi[x,y] = lambaSahnesi[x,y + yOffset];
            //lambalarin geldigi lokasyonu ayarla.
            lambaSahnesi[x, y + yOffset] = new Node(true, null);
        }

        //eger sahnede hic lamba yoksa en uste gore
        if(y + yOffset == yukseklik)
        {
            Debug.Log("Sahnede en uste lamba bulmadan ulastim");
            SpawnLambaAtTop(x);
        }

    }

    private void SpawnLambaAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int locationToMoveTo = 8 - index;
        Debug.Log("lamba olustururken indeksede eklemek isterim:" + index);

        //rastgele bir lamba al
        int randomIndex = Random.Range(0, lambaPrefabs.Length);
        GameObject newLamba = Instantiate(lambaPrefabs[randomIndex], new Vector2(x - boslukX, yukseklik - boslukY), Quaternion.identity);
        newLamba.transform.SetParent(lambaParent.transform);

        newLamba.GetComponent<Lamba>().SetIndicies(x, index);

        lambaSahnesi[x,index] = new Node(true, newLamba);

        Vector3 targetPosition = new Vector3(newLamba.transform.position.x, newLamba.transform.position.y - locationToMoveTo, newLamba.transform.position.z);
        newLamba.GetComponent<Lamba>().MoveToTarget(targetPosition);

    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for (int y = 7; y >= 0; y--)
        {
            if (lambaSahnesi[x,y].lamba == null)
            {
                lowestNull = y;
            }
            
        }
        return lowestNull;
    }


    #region Cascading Lambalar



    //SpawnLambaAtTop()

    //FindIndexOfLowestNull

    #endregion

    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        //yatay yada uzun yatay eslesme
        if(_matchedResults.direction == MatchDirection.Yatay || _matchedResults.direction == MatchDirection.UzunYatay)
        {
            foreach(Lamba lam in _matchedResults.connectedLambalar)
            {
                List<Lamba> extraConnectedLambalar = new();

                CheckDirection(lam, new Vector2Int(0, 1), extraConnectedLambalar);

                CheckDirection(lam, new Vector2Int(0, -1), extraConnectedLambalar);

                if (extraConnectedLambalar.Count >= 2) 
                {
                    Debug.Log("super yatay eslesmem var");
                    extraConnectedLambalar.AddRange(_matchedResults.connectedLambalar);

                    return new MatchResult
                    {
                        connectedLambalar = extraConnectedLambalar,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedLambalar = _matchedResults.connectedLambalar,
                direction = _matchedResults.direction
            };
        }

        else if (_matchedResults.direction == MatchDirection.Dikey || _matchedResults.direction == MatchDirection.UzunDikey)
        {
            foreach (Lamba lam in _matchedResults.connectedLambalar)
            {
                List<Lamba> extraConnectedLambalar = new();

                CheckDirection(lam, new Vector2Int(1, 0), extraConnectedLambalar);

                CheckDirection(lam, new Vector2Int(-1, 0), extraConnectedLambalar);

                if (extraConnectedLambalar.Count >= 2)
                {
                    Debug.Log("super dikey eslesmem var");
                    extraConnectedLambalar.AddRange(_matchedResults.connectedLambalar);

                    return new MatchResult
                    {
                        connectedLambalar = extraConnectedLambalar,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedLambalar = _matchedResults.connectedLambalar,
                direction = _matchedResults.direction
            };
        }
        return null;
        

    }

    MatchResult IsConnected(Lamba lamba)
    {
        List<Lamba> connectedLambalar  = new();
        LambaTipi lambaTipi = lamba.lambaTipi;

        connectedLambalar.Add(lamba);

        //sagi kontrol et
        CheckDirection(lamba, new Vector2Int(1, 0), connectedLambalar);
        //solu kontrol et
        CheckDirection(lamba, new Vector2Int(-1, 0), connectedLambalar);
        //3 lu eslesme yaptik mi? yatay icin
        if (connectedLambalar.Count == 3)
        {
            Debug.Log("Normal yatay eslesme ve renkleri budur:" + connectedLambalar[0].lambaTipi);
            return new MatchResult
            {
                connectedLambalar = connectedLambalar,
                direction = MatchDirection.Yatay
            };
        }
        //3 ten fazla eslesme yaptik mi? uzunyatay icin
        else if(connectedLambalar.Count > 3)
        {
            Debug.Log("Uzun yatay eslesme ve renkleri budur:" + connectedLambalar[0].lambaTipi);
            return new MatchResult
            {
                connectedLambalar = connectedLambalar,
                direction = MatchDirection.UzunYatay
            };
        }
        //temizleeeee
        connectedLambalar.Clear();
        //hepsini oku
        connectedLambalar.Add(lamba);
        //yukari kontrol et
        CheckDirection(lamba, new Vector2Int(0, 1), connectedLambalar);
        //asagi kontrol et
        CheckDirection(lamba, new Vector2Int(0, -1), connectedLambalar);

        //3 lu eslesme yaptik mi? dikey icin
        if (connectedLambalar.Count == 3)
        {
            Debug.Log("Normal dikey eslesme ve renkleri budur:" + connectedLambalar[0].lambaTipi);
            return new MatchResult
            {
                connectedLambalar = connectedLambalar,
                direction = MatchDirection.Dikey
            };
        }
        //3 ten fazla eslesme yaptik mi? uzundikey icin
        else if (connectedLambalar.Count > 3)
        {
            Debug.Log("Uzun dikey eslesme ve renkleri budur:" + connectedLambalar[0].lambaTipi);
            return new MatchResult
            {
                connectedLambalar = connectedLambalar,
                direction = MatchDirection.UzunDikey
            };
        }
        else
        {
            return new MatchResult
            {
                connectedLambalar = connectedLambalar,
                direction = MatchDirection.None
            };
        }
    }

    void CheckDirection(Lamba lam, Vector2Int direction, List<Lamba> connectedLambalar)
    {
        LambaTipi lambaTipi = lam.lambaTipi;
        int x = lam.xIndex + direction.x;
        int y = lam.yIndex + direction.y;

        while (x >= 0 && x < genislik && y >= 0 && y < yukseklik)
        {
            if (lambaSahnesi[x, y].isUsable)
            {
                Lamba yanLamba = lambaSahnesi[x, y].lamba.GetComponent<Lamba>();

                if(!yanLamba.isMatched && yanLamba.lambaTipi == lambaTipi)
                {
                    connectedLambalar.Add(yanLamba);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }

            else
            {
                break;
            }
        }
    }

    #region Swapping Lambalar

    //lamba secme
    public void SelectedLamba(Lamba _lamba)
    {
        //eger secilmis bi lamba yoksa tikladigim secili olan lamba olsun.
        if(selectedLamba == null)
        {
            Debug.Log(_lamba);
            selectedLamba = _lamba;
        }
        //eger 2 kere secersek bi lambayi onu bosa brrrrr
        else if (selectedLamba == _lamba)
        {
            selectedLamba = null;
        }
        //eger secili lamba secili degilse ve suan ki pozisyonunda degilse degistir
        //secili lamba bosa go brrr
        else if(selectedLamba != _lamba) 
        {
            SwapLamba(selectedLamba, _lamba);
            selectedLamba = null;
        }
    }
    //lamba yer degistime mantigi
    private void SwapLamba(Lamba _currentLamba, Lamba _targetLamba)
    {
        if (!IsAdjacent(_currentLamba, _targetLamba))
        {
            return;
        }

        DoSwap(_currentLamba, _targetLamba);

        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentLamba, _targetLamba));
    }
    //yer degistir
    private void DoSwap(Lamba _currentLamba, Lamba _targetLamba)
    {
        GameObject temp = lambaSahnesi[_currentLamba.xIndex, _currentLamba.yIndex].lamba;

        lambaSahnesi[_currentLamba.xIndex, _currentLamba.yIndex].lamba = lambaSahnesi[_targetLamba.xIndex, _targetLamba.yIndex].lamba;
        lambaSahnesi[_targetLamba.xIndex, _targetLamba.yIndex].lamba = temp;

        //guncel
        int tempXIndex = _currentLamba.xIndex;
        int tempYIndex = _currentLamba.yIndex;
        _currentLamba.xIndex = _targetLamba.xIndex;
        _currentLamba.yIndex = _targetLamba.yIndex;
        _targetLamba.xIndex = tempXIndex;
        _targetLamba.yIndex = tempYIndex;

        _currentLamba.MoveToTarget(lambaSahnesi[_targetLamba.xIndex,_targetLamba.yIndex].lamba.transform.position);

        _targetLamba.MoveToTarget(lambaSahnesi[_currentLamba.xIndex, _currentLamba.yIndex].lamba.transform.position);
    }

    private IEnumerator ProcessMatches(Lamba _currentLamba, Lamba _targetLamba)
    {
        yield return new WaitForSeconds(0.2f);

        if(CheckSahne())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            DoSwap(_currentLamba, _targetLamba);
        }

        isProcessingMove = false;

    }
    //IsAdjacent ya da bitisik mi?
    private bool IsAdjacent(Lamba _currentLamba, Lamba _targetLamba)
    {
        return Mathf.Abs(_currentLamba.xIndex - _targetLamba.xIndex) + Mathf.Abs(_currentLamba.yIndex - _targetLamba.yIndex) == 1;
    }
    //eslesme prosesi


    #endregion 

}



public class MatchResult
{
    public List<Lamba> connectedLambalar;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Yatay,
    Dikey,
    UzunDikey,
    UzunYatay,
    Super,
    None,
}