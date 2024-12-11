using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugPosition : MonoBehaviour
{
    public TextMeshProUGUI vertexText;
    public Vertex vert;

    public void DebugPos(Vector2Int coord) {
        //vertexText.text = "("+coord.x+","+coord.y+")";
    }

    void Update() {
        //vertexText.text = vert.suelo + "";
    }
}
