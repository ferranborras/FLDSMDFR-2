using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InfluenceMap
{
    [HideInInspector] public Vertex[,] vertexMap;
    [HideInInspector] public float[,] map;
    private float[] sectionBlurFilter = {1, 2, 1};

    public float decayRate = 0.5f;
    public float momentum = 0.9f;

    public InfluenceMap() {
        MakeMap();
    }

    public void MakeMap() {
        // Se trata de una rejilla -> las distancias entre nodos son: 10 recto / 14 diagonal
        // Asi no se necesitan floats ni raices cuadradas

        map = new float[Globals.size, Globals.size];

        vertexMap = new Vertex[Globals.size, Globals.size];
        Vector3 vertexMapCenter = new Vector3(Globals.size/2f, Globals.size/2f, 0);
        float vertexMapRadius = Globals.size/2f;

        Vertex current;
        Vector3 currentPos;
        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                currentPos = new Vector3(c+.5f, r+.5f, 0);
                current = new Vertex(currentPos);

                if (Vector3.Distance(vertexMapCenter, currentPos) <= vertexMapRadius) {
                    // Down-Right
                    if ( c < Globals.size-1 && r > 0) {
                        if (vertexMap[c+1, r-1] != null) {
                            Vertex neighbour = vertexMap[c+1, r-1];

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
                        if (vertexMap[c, r-1] != null) {
                            Vertex neighbour = vertexMap[c, r-1];

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
                        if (vertexMap[c-1, r-1] != null) {
                            Vertex neighbour = vertexMap[c-1, r-1];

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
                        if (vertexMap[c-1, r] != null) {
                            Vertex neighbour = vertexMap[c-1, r];

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

                    vertexMap[c, r] = current;
                }
                else
                    vertexMap[c, r] = null;
            }
        }
    }

    public void SetInfluence(Vector2Int coord, float influence) {
        map[coord.x, coord.y] = influence;
    }
    
    public void PropagateInfluence(bool keepSources) {
        //map = CalculateVertical();
        //map = CalculateHorizontal();
        List<Vector3Int> sources = new List<Vector3Int>();
        if (keepSources) {
            for (int r = 0; r < Globals.size; r++) {
                for (int c = 0; c < Globals.size; c++) {
                    if (map[c, r] == 1)
                        sources.Add(new Vector3Int(c, r, 1));
                    else if (map[c, r] == -1)
                        sources.Add(new Vector3Int(c, r, -1));
                }
            }
        }

        Edge edge;
        float accumulatedInfluence = 0f;
        float totalWeight = 0f;

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (vertexMap[c, r] != null) {
                    edge = vertexMap[c, r].firstEdge;
                    accumulatedInfluence = 0f;
                    totalWeight = 0f;

                    while (edge != null) {
                        Vertex neighbour = edge.neighbour;
                        float neighbourInfluence = map[neighbour.coord.x, neighbour.coord.y];
                        float weightedInfluence = neighbourInfluence * Mathf.Exp(-edge.weight * decayRate);

                        accumulatedInfluence += weightedInfluence; // Sumar influencia
                        totalWeight += Mathf.Exp(-edge.weight * decayRate); // Acumular pesos para normalizar

                        edge = edge.next;
                    }

                    // Normalizar la influencia acumulada si hay vecinos
                    float averagedInfluence = (totalWeight > 0) ? (accumulatedInfluence / totalWeight) : 0f;

                    // Combinar la influencia calculada con la actual del nodo usando momentum
                    map[c, r] = Mathf.Lerp(map[c, r], averagedInfluence, momentum);
                }
            }
        }

        foreach (Vector3Int source in sources)
            SetInfluence(new Vector2Int(source.x, source.y), source.z);
    }

    private bool IsFrontierTop(Vector2Int coord) {
        return coord.y == Globals.size-1;
    }

    private bool IsFrontierBottom(Vector2Int coord) {
        return coord.y == 0;
    }

    private bool IsFrontierLeft(Vector2Int coord) {
        return coord.x == 0;
    }

    private bool IsFrontierRight(Vector2Int coord) {
        return coord.x == Globals.size-1;
    }

    private float[,] CalculateVertical() {
        float[,] verticalResult = new float[Globals.size, Globals.size];

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (IsFrontierTop(new Vector2Int(c, r))) {
                    verticalResult[c, r] = (1/3f) * (sectionBlurFilter[1]*map[c, r]
                                                + sectionBlurFilter[2]*map[c, r-1]);
                }
                else if (IsFrontierBottom(new Vector2Int(c, r))) {
                    verticalResult[c, r] = (1/3f) * (sectionBlurFilter[0]*map[c, r+1]
                                                + sectionBlurFilter[1]*map[c, r]);
                }
                else {
                    verticalResult[c, r] = (1/4f) * (sectionBlurFilter[0]*map[c, r-1]
                                                + sectionBlurFilter[1]*map[c, r]
                                                + sectionBlurFilter[2]*map[c, r+1]);
                }
            }
        }

        return verticalResult;
    }

    private float[,] CalculateHorizontal() {
        float[,] horizontalResult = new float[Globals.size, Globals.size];

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                if (IsFrontierRight(new Vector2Int(c, r))) {
                    horizontalResult[c, r] = (1/3f) * (sectionBlurFilter[0]*map[c-1, r]
                                                    + sectionBlurFilter[1]*map[c, r]);
                }
                else if (IsFrontierLeft(new Vector2Int(c, r))) {
                    horizontalResult[c, r] = (1/3f) * (sectionBlurFilter[1]*map[c, r]
                                                    + sectionBlurFilter[2]*map[c+1, r]);
                }
                else {
                    horizontalResult[c, r] = (1/4f) * (sectionBlurFilter[0]*map[c-1, r]
                                                    + sectionBlurFilter[1]*map[c, r]
                                                    + sectionBlurFilter[2]*map[c+1, r]);
                }
            }
        }

        return horizontalResult;
    }

    public void Decay(Vector2Int coord) {
        map[coord.x, coord.y] *= Mathf.Exp(-decayRate);
        //map[coord.x, coord.y] *= (1 - decayRate);
        if (Mathf.Abs(map[coord.x, coord.y]) < 0.01f) {
            map[coord.x, coord.y] = 0;
        }
    }

    public string MapToString() {
        string result = "";

        for (int r = 0; r < Globals.size; r++) {
            for (int c = 0; c < Globals.size; c++) {
                result += map[c, r].ToString("F2") + " ";
            }
            result += "\n";
        }

        return result;
    }
}
