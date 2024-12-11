using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    public static bool vsAi { get; set; }
    public static bool p1IsCarbonara { get; set; }
    public static int size { get; set; }
    public static string winner { get; set; }

    public static void ResetData() {
        vsAi = true;
        p1IsCarbonara = true;
        size = 15;
        winner = "";
    }
}
