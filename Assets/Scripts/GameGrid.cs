using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGrid : MonoBehaviour
{

    public int xSize, ySize;
    public float candyWidth = 1f;
    public GameObject[] _candies;
    private GridItem[,] _items;
    private GridItem _currentlySelectedItem;
    public static int minItemsForMatch = 3;

    // Start is called before the first frame update
    void Start()
    {
        GetCandies();
        FillGrid();
        GridItem.OnMouseOverItemEventHandler += OnMouseOverItem;
        List<GridItem> matchesForItems = SearchVertically(_items[3,3]);
        if (matchesForItems.Count >= 3)
        {
            Debug.Log("Há um match válido para o indice 3, 3");
        } else {
            Debug.Log("Não há um match válido para o indice 3, 3");
        }
    }

    void OnDisable()
    {
        GridItem.OnMouseOverItemEventHandler -= OnMouseOverItem;
    }

    // Começa a preencher o grid quando o jogo é iniciado
    void FillGrid()
    {

        _items = new GridItem[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                _items[x, y] = InstantiateCandy(x, y);
            }
        }
    }

    // Preenche o grid com candies
    GridItem InstantiateCandy(int x, int y)
    {
        GameObject randomCandy = _candies[Random.Range(0, _candies.Length)];
        GridItem newCandy = ((GameObject)Instantiate(randomCandy, new Vector3(x * candyWidth, y), Quaternion.identity)).GetComponent<GridItem>();
        newCandy.OnItemPositionChanged(x, y);
        return newCandy;
    }

    void OnMouseOverItem(GridItem item)
    {

        if (_currentlySelectedItem == item)
        {
            return;
        }

        if (_currentlySelectedItem == null)
        {
            _currentlySelectedItem = item;
        }
        else
        {
            float xDiff = Mathf.Abs(item.x - _currentlySelectedItem.x);
            float yDiff = Mathf.Abs(item.y - _currentlySelectedItem.y);
            if (xDiff + yDiff == 1)
            {
                StartCoroutine(Swap(_currentlySelectedItem, item));
            }
            else
            {
                Debug.LogError("Esses itens estão mais de 1 unidade longe um do outro");
            }

            _currentlySelectedItem = null;
        }
    }

    IEnumerator TryMatch(GridItem a, GridItem b){

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

    void SwapIndices (GridItem a, GridItem b){
        GridItem tempA = _items [a.x, a.y];
        _items [a.x, a.y] = b;
        _items [b.x, b.y] = tempA;
        int bOldX = b.x; int bOldY = b.y;
        b.OnItemPositionChanged(a.x, a.y);
        a.OnItemPositionChanged(bOldX, bOldY);
    }

    List<GridItem> SearchHorizontally (GridItem item){
        List<GridItem> hItems = new List<GridItem> { item };
        int left = item.x - 1;
        int right = item.x + 1;
        while (left >= 0 && _items [left, item.y].id == item.id){
            hItems.Add (_items [left, item.y]);
            left--;
        }

        while (right < xSize && _items[right, item.y].id == item.id){
            hItems.Add(_items[right, item.y]);
            right++;
        }
        return hItems;
    }

    List<GridItem> SearchVertically(GridItem item){
        List<GridItem> vItems = new List<GridItem>{item};
        int lower = item.y - 1;
        int upper = item.y + 1;
        while (lower >= 0 && _items [item.x, lower].id == item.id){
            vItems.Add(_items[item.x, lower]);
            lower--;
        }
        while (upper < ySize && _items[item.x, upper].id == item.id){
            vItems.Add(_items[item.x, upper]);
            upper++;
        }

        return vItems;
    }

    MatchInfo GetMatchInformation(GridItem item){
        MatchInfo m = new MatchInfo();
        m.match = null;
        List<GridItem> hMatch = SearchHorizontally (item);
        List<GridItem> vMatch = SearchVertically (item);
        if (hMatch.Count >= minItemsForMatch && hMatch.Count > vMatch.Count){
            m.matchStartingX = GetMinimumX(hMatch);
            m.matchEndingX = GetMaximumX(hMatch);
            m.matchStartingY = m.matchEndingY = hMatch[0].y;
            m.match = hMatch;
        } else if (vMatch.Count >= minItemsForMatch){
            m.matchStartingY = GetMinimumY(vMatch);
            m.matchEndingY = GetMaximumY(vMatch);
            m.matchStartingX = m.matchEndingX = hMatch[0].x;
            m.match = vMatch;
        }
        return m;
    }

    int GetMinimumX (List<GridItem> items){
        float[] indices = new float [items.Count];
        for (int i = 0; i < indices.Length; i++){
            indices[i] = items[i].x;
        }
        return (int) Mathf.Min(indices);
    }

    int GetMaximumX (List<GridItem> items){
        float[] indices = new float [items.Count];
        for (int i = 0; i < indices.Length; i++){
            indices[i] = items[i].x;
        }
        return (int) Mathf.Max(indices);
    }

    int GetMinimumY (List<GridItem> items){
        float[] indices = new float [items.Count];
        for (int i = 0; i < indices.Length; i++){
            indices[i] = items[i].y;
        }
        return (int) Mathf.Min(indices);
    }

    int GetMaximumY (List<GridItem> items){
        float[] indices = new float [items.Count];
        for (int i = 0; i < indices.Length; i++){
            indices[i] = items[i].y;
        }
        return (int) Mathf.Max(indices);
    }

    void GetCandies()
    {
        _candies = Resources.LoadAll<GameObject>("Prefabs");
        for (int i = 0; i < _candies.Length; i++)
        {
            _candies[i].GetComponent<GridItem>().id = i;
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
