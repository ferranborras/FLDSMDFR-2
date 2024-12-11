using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Vertex[,] map;
    public float tilesCount = 0;

    public GameObject vertexPrefab;
    public Tilemap tilemap;
    public Tilemap hoverTilemap;
    public Tile hoverTile;
    public Color moveColor;
    public Color attackColor;

    public Dictionary<string, Tile> tilesCarbonara;
    public Dictionary<string, Tile> tilesSupreme;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector3.zero;
        tilesCarbonara = new Dictionary<string, Tile>();
        tilesSupreme = new Dictionary<string, Tile>();

        Tile[] tilesArray = Resources.LoadAll<Tile>("Tiles");

        foreach (Tile tile in tilesArray)
        {
            if (tile != null)
            {
                if (tile.name[0] == 'c')
                    tilesCarbonara[tile.name.Substring(tile.name.Length - 4)] = tile; // Usa el nombre del tile como clave
                else if (tile.name[0] == 's')
                    tilesSupreme[tile.name.Substring(tile.name.Length - 4)] = tile;
            }
        }
    }

    public void MakeMap() {
        // Se trata de una rejilla -> las distancias entre nodos son: 10 recto / 14 diagonal
        // Asi no se necesitan floats ni raices cuadradas
        tilesCount = 0;

        //transform.GetChild(0).transform.localPosition = new Vector3(Globals.size/2f, Globals.size/2f, 0);

        map = new Vertex[Globals.size, Globals.size];
        Vector3 mapCenter = transform.position+new Vector3(Globals.size/2f, Globals.size/2f, 0);
        float mapRadius = Globals.size/2f;

        Vertex current;
        Vector3 currentPos;
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                currentPos = new Vector3(c+.5f, r+.5f, 0);
                current = new Vertex(currentPos);

                if (Vector3.Distance(mapCenter, currentPos) <= mapRadius) {
                    // Down-Right
                    if ( c < Globals.size-1 && r > 0) {
                        if (map[c+1, r-1] != null) {
                            Vertex neighbour = map[c+1, r-1];

                            //Arista del actual hacia el vecino
                            current.firstEdge = new Edge(neighbour, 14);

                            // Arista del vecino hacia el actual
                            if (neighbour.firstEdge == null)
                                neighbour.firstEdge = new Edge(current, 14);
                            else {
                                Edge neighbourEdge = neighbour.firstEdge;
                                while (neighbourEdge.next != null)
                                    neighbourEdge = neighbourEdge.next;
                                neighbourEdge.next = new Edge(current, 14);
                            }
                        }
                    }

                    // Down
                    if (r > 0) {
                        if (map[c, r-1] != null) {
                            Vertex neighbour = map[c, r-1];

                            //Arista del actual hacia el vecino
                            if (current.firstEdge == null)
                                current.firstEdge = new Edge(neighbour, 10);
                            else {
                                Edge currentEdge = current.firstEdge;
                                while (currentEdge.next != null)
                                    currentEdge = currentEdge.next;
                                currentEdge.next = new Edge(neighbour, 10);
                            }

                            // Arista del vecino hacia el actual
                            if (neighbour.firstEdge == null)
                                neighbour.firstEdge = new Edge(current, 10);
                            else {
                                Edge neighbourEdge = neighbour.firstEdge;
                                while (neighbourEdge.next != null)
                                    neighbourEdge = neighbourEdge.next;
                                neighbourEdge.next = new Edge(current, 10);
                            }
                        }
                    }

                    // Down-Left
                    if (c > 0 && r > 0) {
                        if (map[c-1, r-1] != null) {
                            Vertex neighbour = map[c-1, r-1];

                            //Arista del actual hacia el vecino
                            if (current.firstEdge == null)
                                current.firstEdge = new Edge(neighbour, 14);
                            else {
                                Edge currentEdge = current.firstEdge;
                                while (currentEdge.next != null)
                                    currentEdge = currentEdge.next;
                                currentEdge.next = new Edge(neighbour, 14);
                            }

                            // Arista del vecino hacia el actual
                            if (neighbour.firstEdge == null)
                                neighbour.firstEdge = new Edge(current, 14);
                            else {
                                Edge neighbourEdge = neighbour.firstEdge;
                                while (neighbourEdge.next != null)
                                    neighbourEdge = neighbourEdge.next;
                                neighbourEdge.next = new Edge(current, 14);
                            }
                        }
                    }

                    // Left
                    if (c > 0) {
                        if (map[c-1, r] != null) {
                            Vertex neighbour = map[c-1, r];

                            //Arista del actual hacia el vecino
                            if (current.firstEdge == null)
                                current.firstEdge = new Edge(neighbour, 10);
                            else {
                                Edge currentEdge = current.firstEdge;
                                while (currentEdge.next != null)
                                    currentEdge = currentEdge.next;
                                currentEdge.next = new Edge(neighbour, 10);
                            }

                            // Arista del vecino hacia el actual
                            if (neighbour.firstEdge == null)
                                neighbour.firstEdge = new Edge(current, 10);
                            else {
                                Edge neighbourEdge = neighbour.firstEdge;
                                while (neighbourEdge.next != null)
                                    neighbourEdge = neighbourEdge.next;
                                neighbourEdge.next = new Edge(current, 10);
                            }
                        }
                    }

                    map[c, r] = current;
                    tilesCount++;
                }
                else
                    map[c, r] = null;
            }
        }

        // Update OriginTiles
        map[0, (int) Mathf.Floor(Globals.size/2)].suelo = 1;
        map[Globals.size-1, (int) Mathf.Floor(Globals.size/2)].suelo = 2;
        if (Globals.size % 2 == 0) {
            map[0, (int) Mathf.Floor(Globals.size/2)-1].suelo = 1;
            map[Globals.size-1, (int) Mathf.Floor(Globals.size/2)-1].suelo = 2;
        }
            
        UpdateMapSprites();
        VisualizeMap();
    }

    public int SetStructure(Edificio edificio, int suelo)
    {
        int newTiles = 0;

        Vertex current;
        Vector2Int[] rangeCoords = edificio.GetRangeCoords(edificio.coord);
        foreach (Vector2Int rangeCoord in rangeCoords) {
            current = map[rangeCoord.x, rangeCoord.y];
            if (current != null) {
                newTiles += current.suelo == 0 ? 1 : 0;
                if (suelo == 1) {
                    current.suelo = current.suelo != 2 ? 1 : 2;
                }
                else {
                    current.suelo = current.suelo != 1 ? 2 : 1;
                }
            }
        }

        UpdateMapSprites();

        return newTiles;
    }

    public Vector2Int RemoveStructure(Edificio edificio) {
        int removedTiles = 0;
        int enemyAddedTiles = 0;

        Vector2Int[] rangeCoords = edificio.GetRangeCoords(edificio.coord);
        Vector2Int[] nearCoords;
        bool shared;
        bool enemyShared;
        int enemySuelo = map[edificio.coord.x, edificio.coord.y].suelo == 1 ? 2 : 1;

        foreach (Vector2Int rangeCoord in rangeCoords) {
            shared = false;
            enemyShared = false;
            if (map[rangeCoord.x, rangeCoord.y] != null) {
                nearCoords = edificio.GetRangeCoords(rangeCoord);

                foreach (Vector2Int nearCoord in nearCoords) {
                    if (map[nearCoord.x, nearCoord.y] != null) {
                        if (IsEdificioNear(nearCoord, edificio)) {
                            if (map[nearCoord.x, nearCoord.y].suelo == map[rangeCoord.x, rangeCoord.y].suelo) {
                                shared = true;
                                break;
                            }
                            else {
                                enemyShared = true;
                            }
                        }
                    }
                }
                if (!shared) {
                    map[rangeCoord.x, rangeCoord.y].suelo = enemyShared ? enemySuelo : 0;
                    enemyAddedTiles += enemyShared ? 1 : 0;
                    removedTiles++;
                }
            }
        }

        UpdateMapSprites();
        print("RemoveTiles: " + removedTiles);
        return new Vector2Int(removedTiles, enemyAddedTiles);
    }

    private bool IsEdificioNear(Vector2Int coord, Edificio edificio) {
        Vertex current = map[coord.x, coord.y];
        if (edificio.coord == coord) {
            print("edificio.coord == coord : IsEdificioNear");
            return false;
        }

        if (current == null) {
            print("current == null : IsEdificioNear");
            return false;
        }
        
        if (current.unidad == null) {
            print("current.unidad == null : IsEdificioNear");
            return false;
        }

        if (!(current.unidad.GetComponent<Unidad>() is Edificio)) {
            print("!(current.unidad.GetComponent<Unidad>() is Edificio) : IsEdificioNear");
            return false;
        }

        return true;
    }

    private void VisualizeMap() {
        GameObject go;
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (map[c, r] != null) {
                    go = Instantiate(vertexPrefab, map[c, r].pos, Quaternion.identity) as GameObject;
                    //go.GetComponent<DebugPosition>().vert = map[c, r];
                }
            }
        }
        //transform.GetChild(0).transform.localScale = new Vector3(Globals.size, Globals.size, 1);
    }

    private void PrintMapaPrueba() {
        int[,] prueba = new int[Globals.size, Globals.size];
        int i = 0;
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                prueba[r, c] = i++;
            }
        }

        string text = "";
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                text += "|  " +prueba[r, c] + "   ";
            }
            text += "|\n";
        }
        //Debug.Log(text);
    }

    public int GetTile(Vector2Int coord) {
        return map[coord.x, coord.y].suelo;
    }

    public void SetTile(Vector2Int coord, int suelo) {
        map[coord.x, coord.y].suelo = suelo;
        UpdateMapSprites();
    }

    public void UpdateMapSprites() {
        Tile currentTile;
        for (int row = 0; row < Globals.size; row++) {
            for (int col = 0; col < Globals.size; col++) {
                if (map[col, row] != null) {
                    currentTile = GetTileSprite(new Vector2Int(col, row));
                    tilemap.SetTile(new Vector3Int((int) map[col, row].pos.x, (int) map[col, row].pos.y, 0), currentTile);
                }
            }
        }

        UpdateSelectedTiles();
    }

    public void UpdateSelectedTiles() {
        hoverTilemap.ClearAllTiles();

        Vertex vert;
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                vert = map[c, r];
                if (vert != null) {
                    if (vert.selected)
                        hoverTilemap.SetTile(new Vector3Int((int) map[c, r].pos.x, (int) map[c, r].pos.y, 0), hoverTile);
                }
            }
        }
    }

    public void UnselectTiles() {
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (map[c, r] != null) {
                    map[c, r].selected = false;
                }
            }
        }
    }

    public Tile GetTileSprite(Vector2Int coord) {
        if (map[coord.x, coord.y] == null)
            return null;
        
        int p = map[coord.x, coord.y].suelo;
        if (p == 0)
            return null;

        string around = "";
        Vector2Int[] directions = {
            new Vector2Int(0, 1),  // Up
            new Vector2Int(1, 0),  // Right
            new Vector2Int(0, -1), // Down
            new Vector2Int(-1, 0)  // Left
        };

        foreach (var dir in directions) {
            int newX = coord.x + dir.x;
            int newY = coord.y + dir.y;

            if (newX >= 0 && newX < Globals.size && newY >= 0 && newY < Globals.size) {
                Vertex current = map[newX, newY];
                around += current != null && current.suelo == p ? "1" : "0";
            }
            else
                around += "0";
        }

        if (around == "0000") {
            if (p == 1 && Globals.p1IsCarbonara)
                return tilesCarbonara["0001"];
            else if (p == 1 && !Globals.p1IsCarbonara)
                return tilesSupreme["0001"];
            else if (p == 2 && Globals.p1IsCarbonara)
                return tilesSupreme["0100"];
            else
                return tilesCarbonara["0100"];
        }

        if ((p == 1) == Globals.p1IsCarbonara)
            return tilesCarbonara[around];
        else
            return tilesSupreme[around];

        return null;
    }
}

public class Vertex
{
    public Vector3 pos;
    public Vector2Int coord;
    public Edge firstEdge;
    public GameObject unidad;
    public GameObject ghost;
    public int suelo;
    public bool selected;
    public GameObject attacking;

    public Vertex(Vector3 p, Edge fEdge = null, GameObject u = null, int s = 0, bool sel = false) {
        this.pos = p;
        this.coord = new Vector2Int((int) (p.x-.5f), (int) (p.y-.5f));
        this.firstEdge = fEdge;
        this.unidad = u;
        this.ghost = null;
        this.suelo = s;
        this.selected = sel;
        attacking = null;
    }
}

public class Edge
{
    public Vertex neighbour;
    public float weight;
    public Edge next;

    public Edge(Vertex n, float w, Edge nxt = null) {
        this.neighbour = n;
        this.weight = w;
        this.next = nxt;
    }
}