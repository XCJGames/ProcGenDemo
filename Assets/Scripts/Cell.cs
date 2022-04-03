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

    [SerializeField] private List<Image> doors;

    private int number;
    private Text text;

    public int Number { get => number; 
        set
        {
            number = value;
            text.text = value.ToString();
        }
    }

    public void ShowDoor(Generator.Directions dir)
    {
        switch (dir)
        {
            case Generator.Directions.Up:
                doors[0].gameObject.SetActive(true);
                break;
            case Generator.Directions.Right:
                doors[1].gameObject.SetActive(true);
                break;
            case Generator.Directions.Down:
                doors[2].gameObject.SetActive(true);
                break;
            case Generator.Directions.Left:
                doors[3].gameObject.SetActive(true);
                break;
        }
    }

    private void Awake()
    {
        text = GetComponentInChildren<Text>();
    }

    private void Start()
    {
        type = CellType.Empty;
    }
}
