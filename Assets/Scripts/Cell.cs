using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public enum CellType
    {
        Empty,
        Room,
        Corridor
    }

    public CellType type;
    public Vector2 pos;
    public List<Generator.Directions> availableDirections = new List<Generator.Directions>();

    private void Start()
    {
        type = CellType.Empty;
    }
}
