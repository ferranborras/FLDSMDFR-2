using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using UnityEngine.UI;

public class VisualizeInfluenceMap : MonoBehaviour
{
    [SerializeField] private InfluenceMap influenceMap;

    [SerializeField] private Color minInfluenceColor = Color.blue;
    [SerializeField] private Color maxInfluenceColor = Color.red;

    [SerializeField] private Tilemap bgTilemap;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tile tile;

    private Camera cam;
    [SerializeField] private GameObject info;
    [SerializeField] private TextMeshProUGUI influenceText;

    [SerializeField] private int maxIterations = 1;
    private int currentIteration = 1;
    private bool isManual = false;
    [SerializeField] private GameObject buttonStart;
    private List<Vector3Int> sources = new List<Vector3Int>();

    // Start is called before the first frame update
    void Start()
    {
        Globals.ResetData();

        cam = Camera.main;
        cam.transform.position = new Vector3(Globals.size/2+.5f, Globals.size/2+.5f, -10);
        cam.orthographicSize = Globals.size/2f + 1;

        influenceMap = new InfluenceMap();

        float influence = 0;
        //Color tileColor = Color.Lerp(minInfluenceColor, maxInfluenceColor, normalizedValue);
        Color tileColor = influence > 0 ? maxInfluenceColor : minInfluenceColor;
        tileColor.a = Mathf.Abs(influence);

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                //tilemap.SetTileFlags(new Vector3Int(c, r, 0), TileFlags.None);
                if (influenceMap.vertexMap[c, r] != null) {
                    tilemap.SetTile(new Vector3Int(c, r, 0), tile);
                    tilemap.SetColor(new Vector3Int(c, r, 0), tileColor);

                    bgTilemap.SetTile(new Vector3Int(c, r, 0), tile);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        info.transform.position = mouseWorldPos + new Vector3(-1.5f, .5f, 0);

        Vector3Int cell = tilemap.WorldToCell(mouseWorldPos);
        Vector2Int coord = new Vector2Int(cell.x, cell.y);
        if (Input.GetButton("LeftClick"))
        {
            if (CoordInBounds(coord)) {
                influenceMap.SetInfluence(coord, 1);
                //print(influenceMap.MapToString());
            }
        }
        else if (Input.GetButton("RightClick")) {
            if (CoordInBounds(coord)) {
                influenceMap.SetInfluence(coord, -1);
                //print(influenceMap.MapToString());
            }
        }

        //print(influenceMap.MapToString());
        //print("Propagate");
        if (currentIteration < maxIterations || !isManual) {

            influenceMap.PropagateInfluence(true);

            currentIteration++;
        }
           
        //print(influenceMap.MapToString());

        float influence;
        float normalizedValue;
        Color tileColor;
        Tile newTile;
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                influence = influenceMap.map[c, r];
                tileColor = influence > 0 ? maxInfluenceColor : minInfluenceColor;
                tileColor.a = Mathf.Abs(influence);
                print(influence);

                tilemap.SetColor(new Vector3Int(c, r, 0), tileColor);
            }
        }

        if (CoordInBounds(coord)) {
            if (influenceMap.vertexMap[coord.x, coord.y] != null) {
                info.SetActive(true);
                influenceText.text = (influenceMap.map[coord.x, coord.y] >= 0 ? "+" : "") + influenceMap.map[coord.x, coord.y].ToString("F2");
            }
            else
                info.SetActive(false);
        }
        else
            info.SetActive(false);

    }

    private List<Vector3Int> KeepSources() {
        List<Vector3Int> sources = new List<Vector3Int>();
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (influenceMap.map[c, r] == 1)
                    sources.Add(new Vector3Int(c, r, 1));
                else if (influenceMap.map[c, r] == -1)
                    sources.Add(new Vector3Int(c, r, -1));
            }
        }

        return sources;
    }

    private bool CoordInBounds(Vector2Int coord) {
        return coord.x >= 0 && coord.x < Globals.size && coord.y >= 0 && coord.y < Globals.size;
    }

    public void ResetMap() {
        float saveDecay = influenceMap.decayRate;
        float saveMomentum = influenceMap.momentum;

        influenceMap = new InfluenceMap();
        influenceMap.decayRate = saveDecay;
        influenceMap.momentum = saveMomentum;

        currentIteration = maxIterations;
        sources = new List<Vector3Int>();
    }

    public void StartPropagation() {
        currentIteration = 0;
    }

    public void ChangeManual(Toggle toggle) {
        currentIteration = maxIterations;
        isManual = toggle.isOn;
        buttonStart.SetActive(isManual);
        sources = new List<Vector3Int>();
    }
    
}
