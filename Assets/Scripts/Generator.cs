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
    [SerializeField] private float adjacentRoomsConnectionPercentage = 0.5f;

    [SerializeField] private Cell baseCell;
    [SerializeField] private Sprite room;
    [SerializeField] private Sprite corridor;

    private bool finished = false;
    private int dimension, createdRooms = 0;
    private float timer = 0.5f;
    private List<List<Cell>> cells;
    [SerializeField] private List<Cell> roomsWithAvailableDirections = new List<Cell>();
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
        roomsWithAvailableDirections.Add(cells[(int)ini.y][(int)ini.x]);
        RemoveNeighbourConnections(cells[(int)ini.y][(int)ini.x]);
        roomsQueue.Enqueue(cells[(int)ini.y][(int)ini.x]);
    }

    void Update()
    {
        if (createdRooms < numRooms)
        {
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                // Si la cola está vacía pero quedan salas por poner, cogemos una aleatoria
                // con al menos una conexión posible y forzamos a que escoja una dirección
                if(roomsQueue.Count == 0)
                {
                    var randomRoom = Random.Range(0, roomsWithAvailableDirections.Count);
                    roomsQueue.Enqueue(
                        roomsWithAvailableDirections[randomRoom]);
                    Cell cell = roomsQueue.Dequeue();
                    CreateRoom(cell, 1);
                }
                // Si es la primera generación a partir de la sala inicial, forzamos 2 caminos
                else if(createdRooms == 0)
                {
                    Cell cell = roomsQueue.Dequeue();
                    CreateRoom(cell, 2);
                }
                // Si no, ponemos como mínimo 0 para que pueda hacer caminos sin ampliar
                else
                {
                    Cell cell = roomsQueue.Dequeue();
                    CreateRoom(cell, 0);
                }
                timer = drawingInterval;
            }
        }
        else if(!finished)
        {
            // Una vez colocadas todas las salas, recorremos las que tengan direcciones
            // disponibles y comprobamos si se pueden unir con otras mediante pasillos
            finished = true;
            Cell[] rooms = new Cell[roomsWithAvailableDirections.Count];
            roomsWithAvailableDirections.CopyTo(rooms);
            foreach(var room in rooms)
            {
                if (roomsWithAvailableDirections.Contains(room))
                {
                    CheckForAdditionalConnections(room);
                }
            }
        }
    }
    /// <summary>
    /// Crea habitaciones respecto a la celda pasada por parámetro.
    /// </summary>
    /// <param name="cell"></param>
    void CreateRoom(Cell cell, int minConnections)
    {
        /* Genera la cantidad de salas conectadas que va a tener según el número de lados
         * disponibles para conexión que tenga
         */
        int nSalasConectadas = Random.Range(minConnections, cell.availableDirections.Count);
        for (int i = 0; i < nSalasConectadas && createdRooms < numRooms; ++i)
        {
            /* Se generan todas las salas, una a una. Se establece por qué lado va a ser la conexión
             * (de forma aleatoria) y se bloquea esa dirección, para que no se pueda hacer ninguna más
             * por ese lado de la sala. Si se llega al mínimo de salas necesarias, se termina la generación
             */
            int dirConection = Random.Range(0, cell.availableDirections.Count - 1);
            Directions dir = cell.availableDirections[dirConection];
            cell.ShowDoor(dir);
            cell.availableDirections.Remove(dir);
            CheckAvailableDirections(cell);

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
                    cells[newY][newX].ShowDoor(Directions.Right);
                    break;
                case Directions.Down:
                    newY = (int)cell.pos.y + dist;
                    newX = (int)cell.pos.x;
                    cells[newY][newX].ShowDoor(Directions.Up);
                    break;
                case Directions.Right:
                    newY = (int)cell.pos.y;
                    newX = (int)cell.pos.x + dist;
                    cells[newY][newX].ShowDoor(Directions.Left);
                    break;
                case Directions.Up:
                    newY = (int)cell.pos.y - dist;
                    newX = (int)cell.pos.x;
                    cells[newY][newX].ShowDoor(Directions.Down);
                    break;
            }


            // Pinta y establece la habitación, y la mete en la cola de habitaciones.
            // También elimina las conexiones de las salas adyacentes.
            cells[newY][newX].GetComponent<Image>().sprite = room;
            cells[newY][newX].type = Cell.CellType.Room;
            createdRooms++;
            cells[newY][newX].Number = createdRooms;
            if(cells[newY][newX].availableDirections.Count > 0)
            {
                roomsWithAvailableDirections.Add(cells[newY][newX]);
            }
            CheckNeighbourRoomsForConnections(cells[newY][newX]);
            RemoveNeighbourConnections(cells[newY][newX]);
            roomsQueue.Enqueue(cells[newY][newX]);

            // Pinta y establece pasillos además de bloquear todas sus conexiones.
            BuildCorridor(cell.pos, dist, dir);
        }
    }

    /// <summary>
    /// Pinta y establece pasillos además de bloquear todas sus conexiones.
    /// Actualiza las conexiones disponibles de las casillas adyacentes.
    /// </summary>
    /// <param name="origin">Casilla de origen</param>
    /// <param name="distance">Número de pasillos</param>
    /// <param name="dir">Dirección en la que construir los pasillos</param>
    private void BuildCorridor(Vector2 origin, float distance, Directions dir)
    {
        for (int j = 1; j < distance; ++j)
        {
            int increase = j;
            Vector2 newPos;
            if (dir == Directions.Left || dir == Directions.Up)
                increase *= -1;
            if(dir == Directions.Up || dir == Directions.Down)
            {
                newPos = new Vector2(origin.x, origin.y + increase);
            }
            else
            {
                newPos = new Vector2(origin.x + increase, origin.y);
                cells[(int)newPos.y][(int)newPos.x].transform.Rotate(0, 0, 90);
            }
            cells[(int)newPos.y][(int)newPos.x].GetComponent<Image>().sprite = corridor;
            cells[(int)newPos.y][(int)newPos.x].type = Cell.CellType.Corridor;
            cells[(int)newPos.y][(int)newPos.x].availableDirections.Clear();
            RemoveNeighbourConnections(cells[(int)newPos.y][(int)newPos.x]);
        }
    }

    /// <summary>
    /// Comprueba las casillas adyacentes y, si son salas, decide aleatoriamente
    /// si las une o no
    /// </summary>
    /// <param name="cell">Casilla desde la que se comprueban las adyacentes</param>
    private void CheckNeighbourRoomsForConnections(Cell cell)
    {
        if (cell.pos.x > 0 &&
            cells[(int)cell.pos.y][(int)cell.pos.x - 1].type == Cell.CellType.Room)
        {
            if(Random.Range(0f, 1f) < adjacentRoomsConnectionPercentage)
            {
                cell.ShowDoor(Directions.Left);
                cells[(int)cell.pos.y][(int)cell.pos.x - 1].ShowDoor(Directions.Right);
            }
        }
        if (cell.pos.x < dimension - 1 &&
            cells[(int)cell.pos.y][(int)cell.pos.x + 1].type == Cell.CellType.Room)
        {
            if (Random.Range(0f, 1f) < adjacentRoomsConnectionPercentage)
            {
                cell.ShowDoor(Directions.Right);
                cells[(int)cell.pos.y][(int)cell.pos.x + 1].ShowDoor(Directions.Left);
            }
        }
        if (cell.pos.y > 0 &&
            cells[(int)cell.pos.y - 1][(int)cell.pos.x].type == Cell.CellType.Room)
        {
            if (Random.Range(0f, 1f) < adjacentRoomsConnectionPercentage)
            {
                cell.ShowDoor(Directions.Up);
                cells[(int)cell.pos.y - 1][(int)cell.pos.x].ShowDoor(Directions.Down);
            }
        }
        if (cell.pos.y < dimension - 1 &&
            cells[(int)cell.pos.y + 1][(int)cell.pos.x].type == Cell.CellType.Room)
        {
            if (Random.Range(0f, 1f) < adjacentRoomsConnectionPercentage)
            {
                cell.ShowDoor(Directions.Down);
                cells[(int)cell.pos.y + 1][(int)cell.pos.x].ShowDoor(Directions.Up);
            }
        }
    }

    /// <summary>
    /// Comprueba en todas las direcciones disponibles si la sala se puede unir
    /// con otra mediante pasillos adicionales.
    /// </summary>
    /// <param name="cell">Casilla desde la que se comprueba</param>
    private void CheckForAdditionalConnections(Cell cell)
    {
        Directions[] dirs = new Directions[cell.availableDirections.Count];
        cell.availableDirections.CopyTo(dirs);
        //List<Directions> availableDirections = cell.availableDirections;
        foreach(var dir in dirs)
        {
            if (cell.availableDirections.Contains(dir))
            {
                int maxDist = GetMaxDistanceFromCellInDirection(cell.pos, dir) + 1;
                if(maxDist > 0)
                {
                    Vector2 newPos = Vector2.zero;
                    switch (dir)
                    {
                        case Directions.Up:
                            newPos.x = cell.pos.x;
                            newPos.y = cell.pos.y - maxDist;
                            break;
                        case Directions.Right:
                            newPos.x = cell.pos.x + maxDist;
                            newPos.y = cell.pos.y;
                            break;
                        case Directions.Down:
                            newPos.x = cell.pos.x;
                            newPos.y = cell.pos.y + maxDist;
                            break;
                        case Directions.Left:
                            newPos.x = cell.pos.x - maxDist;
                            newPos.y = cell.pos.y;
                            break;
                    }
                    if(ValidPosition(newPos) && 
                        cells[(int)newPos.y][(int)newPos.x].type == Cell.CellType.Room &&
                        Random.Range(0f, 1f) < adjacentRoomsConnectionPercentage)
                    {
                        BuildCorridor(cell.pos, maxDist, dir);
                        cell.ShowDoor(dir);
                        cells[(int)newPos.y][(int)newPos.x].ShowOppositeDoor(dir);
                    }

                }
            }
        }
    }

    /// <summary>
    /// Comprueba si la posición es válida dentro de las dimensiones del tablero
    /// </summary>
    /// <param name="pos">Posición a comprobar</param>
    /// <returns>True si es una posición válida, false en caso contrario</returns>
    private bool ValidPosition(Vector2 pos)
    {
        return pos.x >= 0 && pos.x < dimension &&
            pos.y >= 0 && pos.y < dimension;
    }

    /// <summary>
    /// Comprueba si la sala tiene conexiones disponibles y, si no las tiene,
    /// elimina la sala de la lista de salas con conexiones disponibles.
    /// </summary>
    /// <param name="cell"></param>
    private void CheckAvailableDirections(Cell cell)
    {
        if (cell.availableDirections.Count == 0 &&
                roomsWithAvailableDirections.Contains(cell))
        {
            roomsWithAvailableDirections.Remove(cell);
        }
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
        while (ValidPosition(auxPos) &&
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
            CheckAvailableDirections(cells[(int)cell.pos.y][(int)cell.pos.x - 1]);
        }
        if(cell.pos.x < dimension - 1)
        {
            cells[(int)cell.pos.y][(int)cell.pos.x + 1].availableDirections.Remove(Directions.Left);
            CheckAvailableDirections(cells[(int)cell.pos.y][(int)cell.pos.x + 1]);
        }
        if (cell.pos.y > 0)
        {
            cells[(int)cell.pos.y - 1][(int)cell.pos.x].availableDirections.Remove(Directions.Down);
            CheckAvailableDirections(cells[(int)cell.pos.y - 1][(int)cell.pos.x]);
        }
        if (cell.pos.y < dimension - 1)
        {
            cells[(int)cell.pos.y + 1][(int)cell.pos.x].availableDirections.Remove(Directions.Up);
            CheckAvailableDirections(cells[(int)cell.pos.y + 1][(int)cell.pos.x]);
        }
    }
}
