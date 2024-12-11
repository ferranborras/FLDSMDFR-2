using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //ACLARACIONES

    //0 == Pan
    //1 == Bando Rojo
    //2 == Bando Azul
    //3 == Edificio Rojo
    //4 == Edificio Azul

    public enum SelectedState {
        unselected,
        attack,
        move,
    }


    private Tilemap tilemap;                // El tilemap donde estan los tiles
    private Tilemap hoverTilemap;

    private bool p1Turn;
    private bool AiMoving;
    private int moneyP1;
    private int moneyP2;

    private int p1TilesCount = 1;
    private int p2TilesCount = 1;

    public Team carbonaraTeam;
    public Team supremeTeam;

    [HideInInspector] public GameObject unidadSelected;
    [HideInInspector] public Vertex vertexSelected;
    private SelectedState selState;

    [SerializeField] private GeneratorAI generatorAi;

    public MapGenerator mapGenerator;
    [SerializeField] private CanvasController canvasController;
    [SerializeField] private Camera cam;
    [SerializeField] private float camSpeed;
    [SerializeField] private float zoomSpeed;
    private bool movingCam;

    private float winPercent = .9f;

    private List<TropaAI> AiTropas = new List<TropaAI>();

    private void Start()
    {
        movingCam = false;

        cam = Camera.main;
        cam.transform.position = new Vector3(Globals.size/2f, Globals.size/2f, -10);
        cam.orthographicSize = Globals.size/2f + 1;

        selState = SelectedState.unselected;

        mapGenerator.MakeMap();
        tilemap = mapGenerator.tilemap;
        hoverTilemap = mapGenerator.hoverTilemap;

        p1Turn = false;
        
        moneyP1 = 5 - (p1Turn ? 1 : 0);
        moneyP2 = 5 - (!p1Turn ? 1 : 0);

        StartNewTurn();
    }

    void Update()
    {
        // Obtener la posicion del raton en el mundo
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cell = tilemap.WorldToCell(mouseWorldPos);
        if (Input.GetButtonUp("LeftClick"))
        {
            // ----------- TURNO DEL JUGADOR ----------- //
            if (!Globals.vsAi || Globals.vsAi && p1Turn)
                MakeAction(cell);

            // ----------- TURNO DE LA IA ----------- //
            if (Globals.vsAi && !p1Turn) {
                print("TURNO DE IA");
                Action[] actions = null;
                if (!AiMoving) {
                    WorldModel worldModel = new WorldModel();
                    worldModel.GenerateFromMap(mapGenerator.map, moneyP2);
                    actions = generatorAi.PlanActions(worldModel, 5);

                    AiMoving = true;
                }

                // Hacer las acciones
                print("Check GeneratorAI actions");
                if (actions != null) {
                    print("actions.Length" + actions.Length);
                    foreach (Action action in actions) {
                        print(action);
                        action.ApplyInGame(this);
                    }
                }

                // Para cada tropa hacer acciones
                print("Check TropaAI actions");
                foreach (TropaAI tropaAi in AiTropas) {
                    TropaWorldModel worldModel = new TropaWorldModel(tropaAi.gameObject.GetComponent<Unidad>());
                    Debug.Log("unidad is null? " + (worldModel.unidad == null));
                    worldModel.GenerateFromMap(mapGenerator.map, moneyP2);
                    actions = tropaAi.PlanActions((WorldModel) worldModel, 2);

                    if (actions != null) {
                        foreach (Action action in actions) {
                            action.ApplyInGame(this);
                            print(action);
                        }
                    }
                }

                print("FIN DE TURNO DE IA");
                if (vertexSelected != null) {
                    Debug.Log("22vertexSelected.unidad == null: " + (vertexSelected.unidad == null));
                    Debug.Log("22vertexSelected: " + vertexSelected.coord);
                }
                StartNewTurn();
            }

        }
        ManageCameraMovement();
    }

    public void MakeAction(Vector3Int currentCell) {
        Vector2Int currentCoord = new Vector2Int(currentCell.x, currentCell.y);

        mapGenerator.UnselectTiles();
        mapGenerator.UpdateSelectedTiles();
        movingCam = false;
        if (currentCell.x >= 0 && currentCell.x < Globals.size && currentCell.y >= 0 && currentCell.y < Globals.size) {
            if (mapGenerator.map[currentCell.x, currentCell.y] != null) { // Tile valida
                //------------------ GESTION DE LA COMPRA DE UNIDADES -------------------//
                if (ActionBuy(currentCoord))
                    print("Done: ActionBuy");

                //---------------------- AL PULSAR SOBRE UNA UNIDAD -----------------------//
                else if (ActionClickUnidad(currentCoord))
                    print("Done: ActionClickUnidad");

                //---------------------- AL PULSAR SOBRE EL SUELO -----------------------//
                else if (ActionClickEmpty(currentCoord))
                    print("Done: ActionClickEmpty");
            } 
        }
    }

    public bool ActionBuy(Vector2Int currentCoord) {
        if (unidadSelected == null) { // Comprueba si ha seleccionado una unidad para comprar y colocar
            print("unidaSelected == null : ActionBuy");
            return false;
        }
        
        if (mapGenerator.map[currentCoord.x, currentCoord.y].suelo != (p1Turn ? 1 : 2)) { // Comprueba si el suelo es propio del jugador que tiene el turno
            print("mapGenerator.map[currentCoord.x, currentCoord.y].suelo != (p1Turn ? 1 : 2) : ActionBuy");
            return false;
        }

        if (mapGenerator.map[currentCoord.x, currentCoord.y].unidad != null) { // Comprueba que no haya una undad en esa casilla
            print("mapGenerator.map[currentCoord.x, currentCoord.y].unidad != null : ActionBuy");
            return false;
        }
        
        if ((p1Turn ? moneyP1 : moneyP2) < unidadSelected.GetComponent<Unidad>().Cost()) { // Comprueba que el jugador tenga suficiente dinero para comprar la unidad
            print("(p1Turn ? moneyP1 : moneyP2) >= unidadSelected.GetComponent<Unidad>().Cost() : ActionBuy");
            return false;
        }

        moneyP1 -= p1Turn ? BuyUnidad(currentCoord) : 0;
        moneyP2 -= !p1Turn ? BuyUnidad(currentCoord) : 0;
        canvasController.UpdateMoney(moneyP1, moneyP2);

        return true;
    }

    private bool ActionClickUnidad(Vector2Int currentCoord) {
        if (mapGenerator.map[currentCoord.x, currentCoord.y].unidad == null) { // Comprueba si hay una unidad desplegada en la casilla
            print("mapGenerator.map[currentCoord.x, currentCoord.y].unidad == null : ActionClickUnidad");
            return false;
        }

        Unidad unidad = mapGenerator.map[currentCoord.x, currentCoord.y].unidad.GetComponent<Unidad>();

        //-------------------------------- MAKE ATTACK ------------------------------------//
        if (CanAttackEnemy(unidad)) {
            AttackUnidad(unidad.coord);
        }
        
        //--------------------------- DISPLAY ATTACK OPTIONS -------------------------------//
        else if (currentCoord == unidad.coord && !(unidad is Edificio)
        && (unidad.coord != unidad.origin || selState == SelectedState.move && vertexSelected.coord == unidad.origin)) {
            selState = SelectedState.attack;
            hoverTilemap.color = mapGenerator.attackColor;
            vertexSelected = mapGenerator.map[unidad.coord.x, unidad.coord.y];
        }

        //---------------------------- DISPLAY MOVE OPTIONS --------------------------------//
        else if (currentCoord == unidad.origin) {
            if (vertexSelected != null && currentCoord == vertexSelected.coord && unidad.isP1 == p1Turn && !(unidad is Edificio)) // Comprueba si ha clickado en el origen
                MoveUnidad(currentCoord);
            selState = SelectedState.move;
            hoverTilemap.color = mapGenerator.moveColor;
            vertexSelected = mapGenerator.map[unidad.origin.x, unidad.origin.y];
        }

        unidadSelected = null;
        canvasController.EnableButtons();

        SelectTiles(vertexSelected.coord);
        mapGenerator.UpdateSelectedTiles();

        return true;
    }

    public bool CanAttackEnemy(Unidad enemy) {
        if (vertexSelected == null) {
            print("vertexSelected == null : CanAttackEnemy");
            return false;
        }

        if (vertexSelected.unidad == null) {
            print("vertexSelected.unidad == null : CanAttackEnemy");
            return false;
        }

        Unidad ally = vertexSelected.unidad.GetComponent<Unidad>();
        if (ally.isP1 != p1Turn) {
            print("ally.isP1 != p1Turn : CanAttackEnemy");
            return false;
        }
            
        if (enemy.isP1 == p1Turn) {
            print("enemy.isP1 == p1Turn : CanAttackEnemy");
            return false;
        }

        if (selState != SelectedState.attack) {
            print("selState != SelectedState.attack : CanAttackEnemy");
            return false;
        }

        if (ally is Edificio) {
            print("ally is Edificio : CanAttackEnemy");
            return false;
        }

        Vector2Int[] selTiles = ally.GetRangeCoords(ally.coord);
        Vertex vert;
        for (int i = 0; i < selTiles.Length; i++) {
            if (selTiles[i] == enemy.coord)
                return true;
        }
        
        print("enemy not in attack area : CanAttackEnemy");
        return false;
    }

    private bool ActionClickEmpty(Vector2Int currentCoord) {
        if (vertexSelected == null) { // Comprueba si se ha guardado una casilla previamente (solo se guarda la ultima casilla clickada con unidad)
            print("vertexSelected == null : ActionClickEmpty");
            return false;
        }

        Unidad unidadScript = vertexSelected.unidad.GetComponent<Unidad>();
        if (unidadScript == null) { // Comprueba que la unidad seleccionada tenga un script activo
            print("unidadScript == null : ActionClickEmpty");
            return false;
        }
        if (unidadScript.isP1 != p1Turn) { // Comprueba si la unidad seleccionada es del jugador con el turno
            print("unidadScript.isP1 != p1Turn : ActionClickEmpty");
            return false;
        }

        unidadSelected = null;
        canvasController.EnableButtons();

        //----------------------------- DISPLAY TILES ---------------------------------//
        Vector2Int[] selTiles = vertexSelected.unidad.GetComponent<Unidad>().GetRangeCoords(vertexSelected.coord);
        Vertex vert;
        bool selArea = false;
        for (int i = 0; i < selTiles.Length; i++) {
            vert = mapGenerator.map[selTiles[i].x, selTiles[i].y];
            if (vert != null) {
                vert.selected = true;
                if (vert.coord == currentCoord) {
                    selArea = true;
                }

            }
        }
        //----------------------------- MOVE CONDITION ---------------------------------//
        if (selArea && selState == SelectedState.move && !(unidadScript is Edificio)) { // Comprueba si se ha clickado una casilla vacia dentro del rango de movimiento para desplazarse
            MoveUnidad(currentCoord);
        }
        //---------------------------- ATTACK CONDITION --------------------------------//
        else { // Comprueba si se ha clickado fuera del rango o si no esta desplegado el rango de movimiento
            selState = SelectedState.unselected;
            hoverTilemap.color = mapGenerator.moveColor;
            vertexSelected = null;
            mapGenerator.UnselectTiles();
        }
        mapGenerator.UpdateSelectedTiles();

        return true;
    }

    private void SelectTiles(Vector2Int currentCoord) {
        Unidad unidad = mapGenerator.map[currentCoord.x, currentCoord.y].unidad.GetComponent<Unidad>();
        Vector2Int[] selTiles = vertexSelected.unidad.GetComponent<Unidad>().GetRangeCoords(currentCoord == unidad.coord ? unidad.coord : unidad.origin);
        Vertex vert;
        for (int i = 0; i < selTiles.Length; i++) {
            vert = mapGenerator.map[selTiles[i].x, selTiles[i].y];
            if (vert != null) {
                vert.selected = true;
            }
        }
    }

    public void ManageCameraMovement() {
        if (Input.GetButtonDown("WheelClick"))
            movingCam = true;
        else if (Input.GetButtonUp("WheelClick"))
            movingCam = false;

        if (movingCam) {
            float mouseX = Input.GetAxis("MouseX");
            float mouseY = Input.GetAxis("MouseY");

            float adjustedSpeed = camSpeed / (Globals.size/2+1) * cam.orthographicSize;
            Vector3 camMovement = new Vector3(-mouseX * adjustedSpeed, -mouseY * adjustedSpeed, 0);
            
            cam.transform.position = cam.transform.position + camMovement;

            cam.transform.position = new Vector3(Mathf.Clamp(cam.transform.position.x, 0, 15), Mathf.Clamp(cam.transform.position.y, 0, 15), -10);
        }

        float scroll = Input.GetAxis("ScrollWheel");
        if (scroll != 0) {
            cam.orthographicSize -= scroll * zoomSpeed;

            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 3.5f, Globals.size/2+1);
        }
    }

    public void StartNewTurn()
    {
        selState = SelectedState.unselected;
        hoverTilemap.color = mapGenerator.moveColor;

        p1TilesCount = 0;
        p2TilesCount = 0;

        int removedTiles = 0;
        int enemyAddedTiles = 0;

        Vertex current;
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                current = mapGenerator.map[c, r];

                if (current != null) {
                    if (current.unidad != null) { // Actualizar la ubicacion de la unidad para que su origen sea igual a su posicion al terminar el turno
                        Unidad unidad = current.unidad.GetComponent<Unidad>();

                        /*if (unidad.origin != unidad.coord) {
                            mapGenerator.map[unidad.origin.x, unidad.origin.y].unidad = null;
                            mapGenerator.map[unidad.coord.x, unidad.coord.y].unidad = current.unidad;
                        }*/
                        unidad.origin = unidad.coord;
                    }

                    if (current.attacking != null) {
                        Unidad enemy = current.attacking.GetComponent<Unidad>();
                        if (enemy != null) {
                            if (mapGenerator.map[enemy.coord.x, enemy.coord.y].unidad != null) {
                                print("Current: " + current.coord);
                                print("current.unidad is null: " + (current.unidad == null));
                                print("vertexSelected: " + vertexSelected.coord);
                                Debug.Log("vertexSelected.unidad == null: " + (vertexSelected.unidad == null));
                                Unidad ally = current.unidad.GetComponent<Unidad>();
                                bool isDead = enemy.GetDamage(ally.Damage());
                                if (isDead) {
                                    moneyP1 += p1Turn ? enemy.DeathEarnings() : 0;
                                    moneyP2 += !p1Turn ? enemy.DeathEarnings() : 0;

                                    if (enemy is Edificio) {
                                        Vector2Int newTiles = mapGenerator.RemoveStructure((Edificio) enemy);
                                        removedTiles += newTiles.x;
                                        enemyAddedTiles += newTiles.y;
                                    }
                                    mapGenerator.map[enemy.coord.x, enemy.coord.y].unidad = null;
                                    mapGenerator.map[enemy.origin.x, enemy.origin.y].unidad = null;

                                    if (p1Turn && !(enemy is Edificio) && Globals.vsAi) {
                                        AiTropas.Remove(enemy.gameObject.GetComponent<TropaAI>());
                                    }
                                    Destroy(current.attacking.gameObject);
                                }
                            }
                        }
                        current.attacking = null;
                    }

                    if (current.ghost != null) { // Se eliminan los duplicados fantasma y se resetea la informacion de las casillas
                        Destroy(current.ghost);
                        current.ghost = null;

                        if (current.unidad != null) {
                            Unidad unidad = current.unidad.GetComponent<Unidad>();
                            if (unidad.coord != current.coord)
                                current.unidad = null;
                        }
                    }

                    p1TilesCount += current.suelo == 1 ? 1 : 0;
                    p2TilesCount += current.suelo == 2 ? 1 : 0;
                }
            }
        }

        p1TilesCount -= !p1Turn ? removedTiles : 0;
        p2TilesCount -= p1Turn ? removedTiles : 0;

        p1TilesCount += p1Turn ? enemyAddedTiles : 0;
        p2TilesCount += !p1Turn ? enemyAddedTiles : 0;

        moneyP1 += p1Turn ? p1TilesCount : 0;
        moneyP2 += !p1Turn ? p2TilesCount : 0;

        p1Turn = !p1Turn;

        AiMoving = p1Turn;
        
        canvasController.UpdateTurn(p1Turn);
        canvasController.UpdateMoney(moneyP1, moneyP2);
        canvasController.UpdateSliders(p1TilesCount/mapGenerator.tilesCount, p2TilesCount/mapGenerator.tilesCount);
        canvasController.EnableButtons();
        unidadSelected = null;
        vertexSelected = null;

        if (p1TilesCount/mapGenerator.tilesCount >= winPercent || p2TilesCount/mapGenerator.tilesCount >= winPercent
        || p1TilesCount <= 0 || p2TilesCount <= 0) {
            Globals.winner = p1TilesCount > p2TilesCount ? "PLAYER 1" : "PLAYER 2";
            SceneManager.LoadScene("EndScene");
        }
    }

    private int BuyUnidad(Vector2Int coord) {
        GameObject go = Instantiate(unidadSelected, unidadSelected.transform.position, Quaternion.identity) as GameObject;
        Unidad goScript = go.GetComponent<Unidad>();
        goScript.Initialize(coord, p1Turn);

        if (goScript.GetType() == typeof(Edificio)) {
            int newTiles = mapGenerator.SetStructure(goScript as Edificio, p1Turn ? 1 : 2);
            p1TilesCount += p1Turn ? newTiles : 0;
            p2TilesCount += !p1Turn ? newTiles : 0;

            canvasController.UpdateSliders(p1TilesCount/mapGenerator.tilesCount, p2TilesCount/mapGenerator.tilesCount);
        }

        mapGenerator.map[coord.x, coord.y].unidad = go;
        if (!p1Turn && !(goScript is Edificio))
            AiTropas.Add(go.GetComponent<TropaAI>());

        return goScript.Cost();
    }

    public void MoveUnidad(Vector2Int target) {
        Unidad unidad = vertexSelected.unidad.GetComponent<Unidad>();

        Stack<Vector3> path = FindPath(unidad.coord, target);

        if (unidad.coord == vertexSelected.coord && vertexSelected.ghost == null) {

            print("target != vertexSelected.coord: " + (target != vertexSelected.coord) + "\n"
            + "vertexSelected.ghost == null: " + (vertexSelected.ghost == null));

            GameObject ghostGo = Instantiate(vertexSelected.unidad, vertexSelected.pos, Quaternion.identity) as GameObject;
            ghostGo.GetComponent<Unidad>().Initialize(vertexSelected.coord, p1Turn);

            Renderer renderer = ghostGo.GetComponent<Renderer>();
            if (renderer != null) {
                Material material = renderer.material;

                Color color = material.color;
                color.a = .5f;
                material.color = color;
            }
            //mapGenerator.map[target.x, target.y].unidad = vertexSelected.unidad;
            mapGenerator.map[unidad.origin.x, unidad.origin.y].ghost = ghostGo;
        }
        //else if (target == vertexSelected.coord) {
        //    Destroy(vertexSelected.ghost);
        //}

        mapGenerator.map[target.x, target.y].unidad = unidad.gameObject;
        if (target != unidad.coord) {
            //mapGenerator.map[target.x, target.y].unidad = vertexSelected.unidad;

            if (unidad.coord != vertexSelected.coord && (Globals.vsAi && p1Turn || !Globals.vsAi))
                mapGenerator.map[unidad.coord.x, unidad.coord.y].unidad = null;
        }

        if (path != null) {
            foreach (Vector3 e in path)
                unidad.path.Enqueue(e);
        }

        if (unidad.coord != target) {
            vertexSelected.attacking = null;
            print("Attacking removed");
        }
        
        unidad.coord = target;
    }

    public void AttackUnidad(Vector2Int target) {
        print("Enemy attacked");

        vertexSelected.attacking = mapGenerator.map[target.x, target.y].unidad;
    }

    public void BaconSelected() {
        unidadSelected = unidadSelected == carbonaraTeam.soldierPrefab ? null : carbonaraTeam.soldierPrefab;
    }

    public void ChampiSelected() {
        unidadSelected = unidadSelected == carbonaraTeam.archerPrefab ? null : carbonaraTeam.archerPrefab;
    }

    public void CebollaSelected() {
        unidadSelected = unidadSelected == carbonaraTeam.tankPrefab ? null : carbonaraTeam.tankPrefab;
    }

    public void CocineroSelected() {
        unidadSelected = unidadSelected == carbonaraTeam.structurePrefab ? null : carbonaraTeam.structurePrefab;
    }

    public void PeperoniSelected() {
        unidadSelected = unidadSelected == supremeTeam.soldierPrefab ? null : supremeTeam.soldierPrefab;
    }

    public void AceitunaSelected() {
        unidadSelected = unidadSelected == supremeTeam.archerPrefab ? null : supremeTeam.archerPrefab;
    }

    public void PimientoSelected() {
        unidadSelected = unidadSelected == supremeTeam.tankPrefab ? null : supremeTeam.tankPrefab;
    }

    public void PizzeroSelected() {
        unidadSelected = unidadSelected == supremeTeam.structurePrefab ? null : supremeTeam.structurePrefab;
    }

    private Stack<Vector3> FindPath(Vector2Int origin, Vector2Int target) {

        float[,] straightDistance = new float[Globals.size, Globals.size];
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (mapGenerator.map[c, r] != null)
                    straightDistance[c, r] = Vector3.Distance(mapGenerator.map[c, r].pos, mapGenerator.map[target.x, target.y].pos);
            }
        }
                                
        PriorityMatrixQueue queue = new PriorityMatrixQueue(Globals.size, Globals.size);

        float[,] optimalPathWeight = new float[Globals.size, Globals.size];
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                optimalPathWeight[c, r] = float.PositiveInfinity;
            }
        }

        Vector2Int[,] parent = new Vector2Int[Globals.size, Globals.size];
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                parent[c, r] = new Vector2Int(-1, -1);
            }
        }

        optimalPathWeight[origin.x, origin.y] = straightDistance[origin.x, origin.y];

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                queue.add(new Vector2Int(c, r), optimalPathWeight[c, r]);
            }
        }

        while (!queue.isEmpty()) {
            Vector2Int currentVertex = queue.popMin();

            //cout << "vertice elegido: " << verticeElegido << "\tpeso: " << pesoCaminoOptimo[verticeElegido] << endl;

            if (currentVertex == target || optimalPathWeight[currentVertex.x, currentVertex.y] == float.PositiveInfinity)
                break;

            for (Edge edge = mapGenerator.map[currentVertex.x, currentVertex.y].firstEdge; edge != null; edge = edge.next) {
                Vector2Int neighborVertex = edge.neighbour.coord;
                float newWeight = optimalPathWeight[currentVertex.x, currentVertex.y] - straightDistance[currentVertex.x, currentVertex.y] + edge.weight + straightDistance[neighborVertex.x, neighborVertex.y];
                if (newWeight < optimalPathWeight[neighborVertex.x, neighborVertex.y]) {
                    optimalPathWeight[neighborVertex.x, neighborVertex.y] = newWeight;
                    parent[neighborVertex.x, neighborVertex.y] = currentVertex;
                    queue.updatePriority(neighborVertex, newWeight);
                }
            }
        }

        if (parent[target.x, target.y] == new Vector2Int(-1, -1))
            return null;
        
        Vector2Int vert = target;
        Stack<Vector3> path = new Stack<Vector3>();
        path.Push(mapGenerator.map[vert.x, vert.y].pos);

        while (vert != origin) {
            vert = parent[vert.x, vert.y];
            path.Push(mapGenerator.map[vert.x, vert.y].pos);
        }
        //Debug.Log("Actions Count: " + counter);

        //Debug.Log("Path is null: " + (path == null));
        return path;
    }
}

[System.Serializable]
public class Team
{
    public GameObject soldierPrefab;
    public GameObject archerPrefab;
    public GameObject tankPrefab;
    public GameObject structurePrefab;
}

public class PriorityMatrixQueue {
    public float[,] queue;
    public int size;
    
    public PriorityMatrixQueue(int rows, int cols) {
        this.queue = new float[rows, cols];

        for (int r = 0; r < rows; r++) {
            for (int c = 0; c < cols; c++) {
                this.queue[c, r] = -1f;
            }
        }
    }
    
    public void add(Vector2Int id, float priority) {
        if (id.x < 0 || id.y < 0 || id.x >= queue.GetLength(1) || id.y >= queue.GetLength(0) || priority < 0) {
            //Debug.Log("Couldn't add item to the PriorityQueue, please check that 0 <= id < queue.length && initialPriority >= 0 is true.");
            return;
        }
        if (queue[id.x, id.y] != -1) {
            //Debug.Log("There is an element already in this location.");
            return;
        }
    
        queue[id.x, id.y] = priority;
        size++;
    }

    public void updatePriority(Vector2Int id, float newPriority) {
        if (id.x < 0 || id.y < 0 || id.x >= queue.GetLength(1) || id.y >= queue.GetLength(0) || newPriority < 0) {
            //Debug.Log("Couldn't add item to the PriorityQueue, please check that 0 <= id < queue.length && initialPriority >= 0 is true.");
            return;
        }
        if (queue[id.x, id.y] == -1) {
            //Debug.Log("There is no element in queue to update.");
            return;
        }
            
        queue[id.x, id.y] = newPriority;
    }
    
    public Vector2Int popMin() {
        if (queue.Length == 0)
            throw new InvalidOperationException("Couldn't run popMin() function, please check that the queue is not empty.");
        
        Vector2Int minId = new Vector2Int(-1, -1);
        for (int r = 0; r < queue.GetLength(0); r++) {
            for (int c = 0; c < queue.GetLength(1); c++) {
                if (queue[c, r] != -1 && (minId == new Vector2Int(-1, -1) || queue[c, r] < queue[minId.x, minId.y]))
                    minId = new Vector2Int(c, r);
            }
        } 

        queue[minId.x, minId.y] = -1;
        size--;
        return minId;       
    }
    
    public bool isEmpty() {
        return size == 0;
    }

}
