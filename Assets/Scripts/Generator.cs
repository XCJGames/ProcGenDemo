using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Generator : MonoBehaviour
{
    public enum Directions
    {
        Up, Right, Down, Left
    }

    [SerializeField] private int numRooms = 10;

    [SerializeField] private Cell baseCell;
    [SerializeField] private Sprite room;
    [SerializeField] private Sprite corridor;
    int dimension;

    private List<List<Cell>> cells;
    private List<Cell> roomsWithAvailableDirections = new List<Cell>();
    private Queue<Cell> roomsQueue = new Queue<Cell>();

    void Start()
    {
        // dimension = numRooms / 2 + 1 (de momento)
        dimension = Mathf.RoundToInt(numRooms / 2) + 1;
        cells = new List<List<Cell>>(dimension);
        var rectTransform = GetComponent<RectTransform>();
        // casillas de 100x100, por lo que ponemos el tamaño del panel a 100xdimension
        rectTransform.sizeDelta = new Vector2(dimension * 100, dimension * 100);
        for (int i = 0; i < dimension; i++)
        {
            cells.Add(new List<Cell>(dimension));
            for (int j = 0; j < dimension; j++)
            {
                cells[i].Add(Instantiate(baseCell, transform));
                // metemos las direcciones disponibles de cada casilla
                if (i != 0) cells[i][j].availableDirections.Add(Directions.Up);
                if (i < dimension - 1) cells[i][j].availableDirections.Add(Directions.Down);
                if (j != 0) cells[i][j].availableDirections.Add(Directions.Left);
                if (j < dimension - 1) cells[i][j].availableDirections.Add(Directions.Right);
            }
        }
        Vector2 ini = new Vector2(Random.Range(0, dimension - 1), Random.Range(0, dimension - 1));
        cells[(int)ini.x][(int)ini.y].GetComponent<Image>().sprite = room;
        cells[(int)ini.x][(int)ini.y].type = Cell.CellType.Room;
        roomsQueue.Enqueue(cells[(int)ini.x][(int)ini.y]);
        CreateRoom(cells[(int)ini.x][(int)ini.y], (int)ini.x, (int)ini.y);
    }

    void Update()
    {

    }
    //Crea habitaciones respecto a la primera en la cola.
    void CreateRoom(Cell cell, int posX, int posY)
    {
        int dirAv = roomsQueue.Peek().availableDirections.Count;
        int nSalasConectadas = Random.Range(1, dirAv - 1);
        Debug.Log("Número salas conectadas: " + nSalasConectadas);
        for (int i = 0; i < nSalasConectadas; ++i)
        {
            int dirConection = Random.Range(0, dirAv - 1);
            Directions dir = cell.availableDirections[dirConection];
            int dist = 0, maxDist = 0;
            int newX = 0, newY = 0;
            switch (dir)
            {
                case Directions.Up:
                    maxDist = posY;
                    dist = Random.Range(1, maxDist);
                    newX = posX;
                    newY = posY - dist;
                    break;
                case Directions.Right:
                    maxDist = (dimension - 1) - posX;
                    dist = Random.Range(1, maxDist);
                    newX = posX + dist;
                    newY = posY;
                    break;
                case Directions.Down:
                    maxDist = (dimension - 1) - posY;
                    dist = Random.Range(1, maxDist);
                    newX = posX;
                    newY = posY + dist;
                    break;
                case Directions.Left:
                    maxDist = posX;
                    dist = Random.Range(1, maxDist);
                    newX = posX - dist;
                    newY = posY;
                    break;
            }
            //Pintado habitación
            cells[newX][newY].GetComponent<Image>().sprite = room;
            cells[newX][newY].type = Cell.CellType.Room;
            roomsQueue.Enqueue(cells[newX][newY]);

            //Pintado pasillos
            if (posX == newX) //Los pasillos son en vertical
            {
                for (int j = 1; j < dist; ++j)
                {
                    int increase = j;
                    if (dir == Directions.Up)
                        increase *= -1;
                    cells[posX][posY + increase].GetComponent<Image>().sprite = corridor;
                    cells[posX][posY + increase].transform.Rotate(0, 0, 90);
                    cells[posX][posY + increase].type = Cell.CellType.Corridor;
                }
            }
            else if (posY == newY) //Pasillos en horizontal
            {
                for (int j = 1; j < dist; ++j)
                {
                    int increase = j;
                    if (dir == Directions.Left)
                        increase *= -1;
                    cells[posX + increase][posY].GetComponent<Image>().sprite = corridor;
                    cells[posX + increase][posY].type = Cell.CellType.Corridor;
                }
            }
        }
    }

    //Habitación aleatoriamente asignada a otra
    void CreateRandomRoom()
    {

    }

}
