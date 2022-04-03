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
        cells[(int)ini.x][(int)ini.y].pos = ini;
        roomsQueue.Enqueue(cells[(int)ini.x][(int)ini.y]);
        //CreateRoom(cells[(int)ini.x][(int)ini.y], (int)ini.x, (int)ini.y);
        Cell cell = roomsQueue.Dequeue();
        CreateRoom(cell, (int)cell.pos.x, (int)cell.pos.y); 
    }

    void Update()
    {

    }
    //Crea habitaciones respecto a la celda pasada por parámetro.
    void CreateRoom(Cell cell, int _posY, int _posX)
    {
        /* Genera la cantidad de salas conectadas que va a tener según el número de lados
         * disponibles para conexión que tenga
         */
        int dirAv = cell.availableDirections.Count;
        int nSalasConectadas = Random.Range(1, dirAv - 1);
        Debug.Log("Número salas conectadas: " + nSalasConectadas);
        for (int i = 0; i < nSalasConectadas; ++i)
        {
            /* Se generan todas las salas, una a una. Se establece por qué lado va a ser la conexión
             * (de forma aleatoria) y se bloquea esa dirección, para que no se pueda hacer ninguna más
             * por ese lado de la sala.
             */
            int dirConection = Random.Range(0, dirAv - 1);
            Debug.Log("Dirección seleccionada para la primera sala: " + dirConection);
            Directions dir = cell.availableDirections[dirConection];
            cell.availableDirections.Remove(dir);
            Debug.Log("Dirección seleccionada para la primera sala: " + dir.ToString());


            /* Se genera la distacia a la que se coloca la nueva habitación. Teniendo en cuenta hasta cuál 
             * es la distancia máxima posible.
             * Además, se bloquea la dirección hacia la sala inicial, ya que no se va a poder generar 
             * ninguna otra conexión por ahí. 
             */
            int dist = 0, maxDist = 0;
            int newY = 0, newX = 0;
            switch (dir)
            {
                case Directions.Left:
                    maxDist = _posX;
                    dist = Random.Range(1, maxDist);
                    newY = _posY;
                    newX = _posX - dist;

                    cells[newY][newX].availableDirections.Remove(Directions.Right);
                    break;
                case Directions.Down:
                    maxDist = (dimension - 1) - _posY;
                    dist = Random.Range(1, maxDist);
                    newY = _posY + dist;
                    newX = _posX;

                    cells[newY][newX].availableDirections.Remove(Directions.Up);
                    break;
                case Directions.Right:
                    maxDist = (dimension - 1) - _posX;
                    dist = Random.Range(1, maxDist);
                    newY = _posY;
                    newX = _posX + dist;
                    
                    cells[newY][newX].availableDirections.Remove(Directions.Left);
                    break;
                case Directions.Up:
                    maxDist = _posY;
                    dist = Random.Range(1, maxDist);
                    newY = _posY - dist;
                    newX = _posX;

                    cells[newY][newX].availableDirections.Remove(Directions.Down);
                    break;
            }
            Debug.Log("Distancia respecto a la primera sala: " + dist);


            //Pinta y establece la habitación, y la mete en la cola de habitaciones.
            cells[newY][newX].GetComponent<Image>().sprite = room;
            cells[newY][newX].type = Cell.CellType.Room;
            roomsQueue.Enqueue(cells[newY][newX]);

            //Pinta y establece pasillos además de bloquear todas sus conexiones.
            if (_posY == newY) //Pasillos en vertical
            {
                for (int j = 1; j < dist; ++j)
                {
                    int increase = j;
                    if (dir == Directions.Left)
                        increase *= -1;

                    cells[_posY][_posX + increase].GetComponent<Image>().sprite = corridor;
                    cells[_posY][_posX + increase].transform.Rotate(0, 0, 90);
                    cells[_posY][_posX + increase].type = Cell.CellType.Corridor;

                    for (int x = 0; x < cells[_posY + increase][_posX].availableDirections.Count; ++x)
                    {
                        cells[_posY][_posX + increase].availableDirections.RemoveAt(x);
                    }
                }
            }
            else if (_posX == newX) //Pasillos en horizontal
            {
                for (int j = 1; j < dist; ++j)
                {
                    int increase = j;
                    if (dir == Directions.Up)
                        increase *= -1;
                    cells[_posY + increase][_posX].GetComponent<Image>().sprite = corridor;
                    cells[_posY + increase][_posX].type = Cell.CellType.Corridor;

                    for (int x = 0; x < cells[_posY + increase][_posX].availableDirections.Count; ++x)
                    {
                        cells[_posY + increase][_posX].availableDirections.RemoveAt(x);
                    }
                }
            }
        }
    }

    //Habitación aleatoriamente asignada a otra
    void CreateRandomRoom()
    {

    }

}
