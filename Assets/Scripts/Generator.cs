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
    [SerializeField] private float drawingInterval = 0.5f;

    [SerializeField] private Cell baseCell;
    [SerializeField] private Sprite room;
    [SerializeField] private Sprite corridor;

    private int dimension, createdRooms = 0;
    private float timer = 0.5f;
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
                cells[i][j].pos = new Vector2(j, i);
                // metemos las direcciones disponibles de cada casilla
                if (i != 0) cells[i][j].availableDirections.Add(Directions.Up);
                if (i != dimension - 1) cells[i][j].availableDirections.Add(Directions.Down);
                if (j != 0) cells[i][j].availableDirections.Add(Directions.Left);
                if (j != dimension - 1) cells[i][j].availableDirections.Add(Directions.Right);
            }
        }
        Vector2 ini = new Vector2(Random.Range(0, dimension - 1), Random.Range(0, dimension - 1));
        cells[(int)ini.y][(int)ini.x].GetComponent<Image>().sprite = room;
        cells[(int)ini.y][(int)ini.x].type = Cell.CellType.Room;
        cells[(int)ini.y][(int)ini.x].Number = 0;
        RemoveNeighbourConnections(cells[(int)ini.y][(int)ini.x]);
        roomsQueue.Enqueue(cells[(int)ini.y][(int)ini.x]);
    }

    void Update()
    {
        if (createdRooms < numRooms && roomsQueue.Count > 0)
        {
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                Cell cell = roomsQueue.Dequeue();
                CreateRoom(cell);
                timer = drawingInterval;
            }
        }
    }
    /// <summary>
    /// Crea habitaciones respecto a la celda pasada por parámetro.
    /// </summary>
    /// <param name="cell"></param>
    void CreateRoom(Cell cell)
    {
        /* Genera la cantidad de salas conectadas que va a tener según el número de lados
         * disponibles para conexión que tenga
         */
        int dirAv = cell.availableDirections.Count;
        int nSalasConectadas = Random.Range(1, dirAv);
        Debug.Log("Número salas conectadas: " + nSalasConectadas);
        for (int i = 0; i < nSalasConectadas && createdRooms < numRooms; ++i)
        {
            /* Se generan todas las salas, una a una. Se establece por qué lado va a ser la conexión
             * (de forma aleatoria) y se bloquea esa dirección, para que no se pueda hacer ninguna más
             * por ese lado de la sala. Si se llega al mínimo de salas necesarias, se termina la generación
             */
            int dirConection = Random.Range(0, dirAv - 1);
            Debug.Log("Dirección seleccionada para la primera sala: " + dirConection);
            Directions dir = cell.availableDirections[dirConection];
            cell.availableDirections.Remove(dir);
            Debug.Log("Dirección seleccionada para la primera sala: " + dir.ToString());


            /* Se genera la distacia a la que se coloca la nueva habitación. Teniendo en cuenta hasta cuál 
             * es la distancia máxima posible.
             */
            int dist, maxDist;
            int newY = 0, newX = 0;
            maxDist = GetMaxDistanceFromCellInDirection(new Vector2(cell.pos.x, cell.pos.y), dir);
            dist = Random.Range(1, maxDist);
            switch (dir)
            {
                case Directions.Left:
                    newY = (int)cell.pos.y;
                    newX = (int)cell.pos.x - dist;
                    break;
                case Directions.Down:
                    newY = (int)cell.pos.y + dist;
                    newX = (int)cell.pos.x;
                    break;
                case Directions.Right:
                    newY = (int)cell.pos.y;
                    newX = (int)cell.pos.x + dist;
                    break;
                case Directions.Up:
                    newY = (int)cell.pos.y - dist;
                    newX = (int)cell.pos.x;
                    break;
            }
            Debug.Log("Distancia respecto a la primera sala: " + dist);


            // Pinta y establece la habitación, y la mete en la cola de habitaciones.
            // También elimina las conexiones de las salas adyacentes.
            cells[newY][newX].GetComponent<Image>().sprite = room;
            cells[newY][newX].type = Cell.CellType.Room;
            createdRooms++;
            cells[newY][newX].Number = createdRooms;
            RemoveNeighbourConnections(cells[newY][newX]);
            roomsQueue.Enqueue(cells[newY][newX]);

            // Pinta y establece pasillos además de bloquear todas sus conexiones.
            // También elimina las conexiones de las salas adyacentes.
            if (cell.pos.y == newY) //Pasillos en horizontal
            {
                for (int j = 1; j < dist; ++j)
                {
                    int increase = j;
                    if (dir == Directions.Left)
                        increase *= -1;

                    cells[(int)cell.pos.y][(int)cell.pos.x + increase].GetComponent<Image>().sprite = corridor;
                    cells[(int)cell.pos.y][(int)cell.pos.x + increase].transform.Rotate(0, 0, 90);
                    cells[(int)cell.pos.y][(int)cell.pos.x + increase].type = Cell.CellType.Corridor;
                    cells[(int)cell.pos.y][(int)cell.pos.x + increase].availableDirections.Clear();
                    RemoveNeighbourConnections(cells[(int)cell.pos.y][(int)cell.pos.x + increase]);
                }
            }
            else if (cell.pos.x == newX) //Pasillos en vertical
            {
                for (int j = 1; j < dist; ++j)
                {
                    int increase = j;
                    if (dir == Directions.Up)
                        increase *= -1;
                    cells[(int)cell.pos.y + increase][(int)cell.pos.x].GetComponent<Image>().sprite = corridor;
                    cells[(int)cell.pos.y + increase][(int)cell.pos.x].type = Cell.CellType.Corridor;
                    cells[(int)cell.pos.y + increase][(int)cell.pos.x].availableDirections.Clear();
                    RemoveNeighbourConnections(cells[(int)cell.pos.y + increase][(int)cell.pos.x]);
                }
            }
        }
    }

    //Habitación aleatoriamente asignada a otra
    void CreateRandomRoom()
    {

    }

    /// <summary>
    /// Obtiene la distancia máxima desde una posición en una dirección dada.
    /// Se tienen en cuenta tanto los límites del tablero como casillas ocupadas
    /// por pasillos o salas.
    /// </summary>
    /// <param name="pos">Posición desde la que comprobar casillas</param>
    /// <param name="dir">Dirección en la que comprobar casillas</param>
    /// <returns>Número de casillas libres</returns>
    private int GetMaxDistanceFromCellInDirection(Vector2 pos, Directions dir)
    {
        int distance = 0;
        Vector2 increment = Vector2.zero, auxPos = pos;
        switch (dir)
        {
            case Directions.Up:
                increment.y = -1;
                break;
            case Directions.Right:
                increment.x = 1;
                break;
            case Directions.Down:
                increment.y = 1;
                break;
            case Directions.Left:
                increment.x = -1;
                break;
        }
        auxPos += increment;
        while (auxPos.x >= 0 && auxPos.x < dimension && 
            auxPos.y >= 0 && auxPos.y < dimension &&
            cells[(int)auxPos.y][(int)auxPos.x].type == Cell.CellType.Empty)
        {
            auxPos += increment;
            distance++;
        }
        return distance;
    }

    /// <summary>
    /// Recorre las casillas adyacentes y elimina la posibilidad de conexión con la casilla parámetro
    /// </summary>
    /// <param name="cell">Casilla desde la que se comprueban las adyacentes</param>
    private void RemoveNeighbourConnections(Cell cell)
    {
        if(cell.pos.x > 0)
        {
            cells[(int)cell.pos.y][(int)cell.pos.x - 1].availableDirections.Remove(Directions.Right);
        }
        if(cell.pos.x < dimension - 1)
        {
            cells[(int)cell.pos.y][(int)cell.pos.x + 1].availableDirections.Remove(Directions.Left);
        }
        if (cell.pos.y > 0)
        {
            cells[(int)cell.pos.y - 1][(int)cell.pos.x].availableDirections.Remove(Directions.Down);
        }
        if (cell.pos.y < dimension - 1)
        {
            cells[(int)cell.pos.y + 1][(int)cell.pos.x].availableDirections.Remove(Directions.Up);
        }
    }
}
