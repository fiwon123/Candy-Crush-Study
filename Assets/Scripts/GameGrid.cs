using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    [Header("Audios")]
    public AudioSource swapAudio;
    public AudioSource clearAudio;

    [Header("Grid Config")]
    public int xSize;
    public int ySize;
    [Tooltip("porcentagem de espaçamento no eixo x ")]
    public float itemWidth = 1f;
    public static int minItemsForMatch = 3;
    public float delayBetweenMatches = 0.35f;

    [Header("Runtime")]
    public GameObject[] _itemsObj;
    public bool canPlay;

    private GridItem[,] _items;
    private GridItem _currentlySelectedItem;

    public static GameGrid instance;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Reseta todo grid com novos items.
    /// </summary>
    public void Reset()
    {
        GridItem.OnMouseOverItemEventHandler -= OnMouseOverItem;
        GridItem.OnMouseSelectedItemEventHandler -= OnMouseSelectedItem;
        canPlay = true;
        DestroyGrid();
        GetItems();
        FillGrid();
        ClearGrid();
        Shuffling();
        GridItem.OnMouseOverItemEventHandler += OnMouseOverItem;
        GridItem.OnMouseSelectedItemEventHandler += OnMouseSelectedItem;
    }

    void OnDisable()
    {
        GridItem.OnMouseOverItemEventHandler -= OnMouseOverItem;
        GridItem.OnMouseSelectedItemEventHandler -= OnMouseSelectedItem;
    }

    /// <summary>
    /// Limpa o grid.
    /// </summary>
    public void DestroyGrid()
    {
        if (_items == null)
            return;

        foreach (GridItem gi in _items)
        {
            if (gi)
                Destroy(gi.gameObject);
        }
        _items = null;
    }

    /// <summary>
    /// Começa o preenchimento do grid.
    /// </summary>
    void FillGrid()
    {

        _items = new GridItem[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                _items[x, y] = InstantiateItem(x, y);
            }
        }
    }

    /// <summary>
    /// Deixa o grid sem nenhum match pronto.
    /// </summary>
    void ClearGrid()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                MatchInfo matchInfo = GetMatchInformation(_items[x, y]);
                if (matchInfo.validMatch)
                {
                    Destroy(_items[x, y].gameObject);
                    _items[x, y] = InstantiateItem(x, y);
                    y--;
                }
            }
        }
    }

    GridItem InstantiateItem(int x, int y)
    {
        GameObject randomItem = _itemsObj[Random.Range(0, _itemsObj.Length)];
        GridItem newItem = ((GameObject)Instantiate(randomItem, new Vector3(x * itemWidth, y), Quaternion.identity)).GetComponent<GridItem>();
        newItem.OnItemPositionChanged(x, y);
        return newItem;
    }

    void OnMouseSelectedItem(GridItem item)
    {
        if (_currentlySelectedItem == item || !canPlay)
            return;

        if (item == null)
        {
            _currentlySelectedItem = null;
            return;
        }

        if (_currentlySelectedItem == null)
        {
            _currentlySelectedItem = item;
        }

    }

    void OnMouseOverItem(GridItem item)
    {
        if (_currentlySelectedItem == item || !canPlay)
            return;

        if (_currentlySelectedItem == null)
        {
            return;
        }
        else
        {
            float xDiff = Mathf.Abs(item.x - _currentlySelectedItem.x);
            float yDiff = Mathf.Abs(item.y - _currentlySelectedItem.y);
            if (xDiff + yDiff == 1)
            {
                StartCoroutine(TryMatch(_currentlySelectedItem, item));
            }
            else
            {
                Debug.Log("Esses itens estão mais de 1 unidade longe um do outro");
            }

            _currentlySelectedItem = null;
        }
    }

    IEnumerator TryMatch(GridItem a, GridItem b)
    {
        canPlay = false;
        swapAudio.Play();
        yield return StartCoroutine(Swap(a, b));
        MatchInfo matchA = GetMatchInformation(a);
        MatchInfo matchB = GetMatchInformation(b);
        if (!matchA.validMatch && !matchB.validMatch)
        {
            yield return StartCoroutine(Swap(a, b));
            canPlay = true;
            yield break;
        }
        if (matchA.validMatch)
        {
            yield return StartCoroutine(DestroyItems(matchA.match));
            yield return new WaitForSeconds(delayBetweenMatches);
            yield return StartCoroutine(UpdateGridAfterMatch(matchA));
        }
        else if (matchB.validMatch)
        {
            yield return StartCoroutine(DestroyItems(matchB.match));
            yield return new WaitForSeconds(delayBetweenMatches);
            yield return StartCoroutine(UpdateGridAfterMatch(matchB));
        }

        Shuffling();

        canPlay = true;
    }

    void Shuffling()
    {
        while (IsDeadLock())
        {
            Debug.Log("Shuffle!");
            bool isValid;
            do
            {
                isValid = Shuffle();
            } while (!isValid); // Enquanto não tiver match no grid

            PanelGame.instance.Shuffle();
        }
    }

    // Embaralha o grid.
    bool Shuffle()
    {
        List<GridItem> listGems = new List<GridItem>();
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                listGems.Add(_items[x, y]);
            }
        }

        while (listGems.Count > 0)
        {
            int idA, idB;

            idA = Random.Range(0, listGems.Count);
            GridItem itemA = listGems[idA];
            listGems.RemoveAt(idA);

            idB = Random.Range(0, listGems.Count);
            GridItem itemB = listGems[idB];
            listGems.RemoveAt(idB);

            SwapIndices(itemA, itemB);
            Vector3 auxPosA = itemA.gameObject.transform.position;
            itemA.gameObject.transform.position = itemB.gameObject.transform.position;
            itemB.gameObject.transform.position = auxPosA;
        }

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                MatchInfo matchInfo = GetMatchInformation(_items[x, y]);
                if (matchInfo.validMatch)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Verifica se o grid não possuí movimentos disponíveis para match.
    bool IsDeadLock()
    {
        // Horizontal
        for (int x = 0; x < xSize - 1; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                SwapIndices(_items[x, y], _items[x + 1, y]);
                MatchInfo matchInfoA = GetMatchInformation(_items[x, y]);
                MatchInfo matchInfoB = GetMatchInformation(_items[x + 1, y]);
                if (matchInfoA.validMatch || matchInfoB.validMatch)
                {
                    SwapIndices(_items[x, y], _items[x + 1, y]);
                    return false;
                }
                SwapIndices(_items[x, y], _items[x + 1, y]);
            }
        }
        // Vertical
        for (int y = 0; y < ySize - 1; y++)
        {
            for (int x = 0; x < xSize; x++)
            {
                SwapIndices(_items[x, y], _items[x, y + 1]);
                MatchInfo matchInfoA = GetMatchInformation(_items[x, y]);
                MatchInfo matchInfoB = GetMatchInformation(_items[x, y + 1]);
                if (matchInfoA.validMatch || matchInfoB.validMatch)
                {
                    SwapIndices(_items[x, y], _items[x, y + 1]);
                    return false;
                }
                SwapIndices(_items[x, y], _items[x, y + 1]);
            }
        }
        return true;
    }
    IEnumerator UpdateGridAfterMatch(MatchInfo match)
    {

        if (match.matchStartingY == match.matchEndingY)
        {
            // match horizontal
            for (int x = match.matchStartingX; x <= match.matchEndingX; x++)
            {
                for (int y = match.matchStartingY; y < ySize - 1; y++)
                {
                    GridItem upperIndex = _items[x, y + 1];
                    GridItem current = _items[x, y];
                    _items[x, y] = upperIndex;
                    _items[x, y + 1] = current;
                    _items[x, y].OnItemPositionChanged(_items[x, y].x, _items[x, y].y - 1);
                }
                _items[x, ySize - 1] = InstantiateItem(x, ySize - 1);
            }
        }
        else if (match.matchEndingX == match.matchStartingX)
        {
            // match vertical
            int matchHeight = 1 + (match.matchEndingY - match.matchStartingY);
            for (int y = match.matchStartingY + matchHeight; y <= ySize - 1; y++)
            {
                GridItem lowerIndex = _items[match.matchStartingX, y - matchHeight];
                GridItem current = _items[match.matchStartingX, y];
                _items[match.matchStartingX, y - matchHeight] = current;
                _items[match.matchStartingX, y] = lowerIndex;
            }

            for (int y = 0; y < ySize - matchHeight; y++)
            {
                _items[match.matchStartingX, y].OnItemPositionChanged(match.matchStartingX, y);
            }

            for (int i = 0; i < match.match.Count; i++)
            {
                _items[match.matchStartingX, (ySize - 1) - i] = InstantiateItem(match.matchStartingX, (ySize - 1) - i);
            }
        }

        // Procura por mais match no grid
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                MatchInfo matchInfo = GetMatchInformation(_items[x, y]);
                if (matchInfo.validMatch)
                {
                    yield return new WaitForSeconds(delayBetweenMatches);
                    yield return StartCoroutine(DestroyItems(matchInfo.match));
                    yield return new WaitForSeconds(delayBetweenMatches);
                    yield return StartCoroutine(UpdateGridAfterMatch(matchInfo));
                }
            }
        }
    }

    IEnumerator DestroyItems(List<GridItem> items)
    {
        clearAudio.Play();
        foreach (GridItem i in items)
        {
            yield return StartCoroutine(i.transform.Scale(Vector3.zero, 0.05f));
            GameManager.instance.UpScore(i.score);
            Destroy(i.gameObject);
        }
    }

    IEnumerator Swap(GridItem a, GridItem b)
    {
        ChangeRigidbodyStatus(false); // Desativar todos os corpos rigidos;
        float movDuration = 0.1f;
        Vector3 aPosition = a.transform.position;
        StartCoroutine(a.transform.Move(b.transform.position, movDuration));
        StartCoroutine(b.transform.Move(aPosition, movDuration));
        yield return new WaitForSeconds(movDuration);
        SwapIndices(a, b);
        ChangeRigidbodyStatus(true);
    }

    void SwapIndices(GridItem a, GridItem b)
    {
        GridItem tempA = _items[a.x, a.y];
        _items[a.x, a.y] = b;
        _items[b.x, b.y] = tempA;
        int bOldX = b.x; int bOldY = b.y;
        b.OnItemPositionChanged(a.x, a.y);
        a.OnItemPositionChanged(bOldX, bOldY);
    }

    #region SearchGrid
    List<GridItem> SearchHorizontally(GridItem item)
    {
        List<GridItem> hItems = new List<GridItem> { item };
        int left = item.x - 1;
        int right = item.x + 1;
        while (left >= 0 && _items[left, item.y].id == item.id)
        {
            hItems.Add(_items[left, item.y]);
            left--;
        }

        while (right < xSize && _items[right, item.y].id == item.id)
        {
            hItems.Add(_items[right, item.y]);
            right++;
        }
        return hItems;
    }

    List<GridItem> SearchVertically(GridItem item)
    {
        List<GridItem> vItems = new List<GridItem> { item };
        int lower = item.y - 1;
        int upper = item.y + 1;
        while (lower >= 0 && _items[item.x, lower].id == item.id)
        {
            vItems.Add(_items[item.x, lower]);
            lower--;
        }
        while (upper < ySize && _items[item.x, upper].id == item.id)
        {
            vItems.Add(_items[item.x, upper]);
            upper++;
        }

        return vItems;
    }
    #endregion

    // Procura por informações do possível match
    MatchInfo GetMatchInformation(GridItem item)
    {
        MatchInfo m = new MatchInfo();
        m.match = null;
        List<GridItem> hMatch = SearchHorizontally(item);
        List<GridItem> vMatch = SearchVertically(item);
        if (hMatch.Count >= minItemsForMatch && hMatch.Count > vMatch.Count)
        {
            m.matchStartingX = GetMinimumX(hMatch);
            m.matchEndingX = GetMaximumX(hMatch);
            m.matchStartingY = m.matchEndingY = hMatch[0].y;
            m.match = hMatch;
        }
        else if (vMatch.Count >= minItemsForMatch)
        {
            m.matchStartingY = GetMinimumY(vMatch);
            m.matchEndingY = GetMaximumY(vMatch);
            m.matchStartingX = m.matchEndingX = hMatch[0].x;
            m.match = vMatch;
        }
        return m;
    }


    #region MatchSizeInformation
    int GetMinimumX(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = items[i].x;
        }
        return (int)Mathf.Min(indices);
    }

    int GetMaximumX(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = items[i].x;
        }
        return (int)Mathf.Max(indices);
    }

    int GetMinimumY(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = items[i].y;
        }
        return (int)Mathf.Min(indices);
    }

    int GetMaximumY(List<GridItem> items)
    {
        float[] indices = new float[items.Count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = items[i].y;
        }
        return (int)Mathf.Max(indices);
    }
    #endregion

    void GetItems()
    {
        _itemsObj = Resources.LoadAll<GameObject>("Prefabs");
        for (int i = 0; i < _itemsObj.Length; i++)
        {
            _itemsObj[i].GetComponent<GridItem>().id = i;
        }
    }

    void ChangeRigidbodyStatus(bool status)
    {
        foreach (GridItem g in _items)
        {
            g.GetComponent<Rigidbody2D>().isKinematic = !status;
        }
    }
}
