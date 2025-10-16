using UnityEngine;
using System.Collections.Generic;

public class CF : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public Vector3 center;    // Centro de la habitaci�n
        public Vector2 size;      // Tama�o de la habitaci�n
    }

    public Transform player;
    public List<Room> rooms = new List<Room>();
    public Camera cam;            // Referencia a la c�mara

    private int currentRoomIndex = -1;

    void Start()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player == null || cam == null) return;

        int newRoomIndex = GetCurrentRoomIndex();

        if (newRoomIndex != currentRoomIndex && newRoomIndex != -1)
        {
            currentRoomIndex = newRoomIndex;
            MoveToRoom(rooms[currentRoomIndex]);
        }
    }

    int GetCurrentRoomIndex()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (IsPlayerInRoom(rooms[i]))
            {
                return i;
            }
        }
        return -1;
    }

    bool IsPlayerInRoom(Room room)
    {
        Vector3 playerPos = player.position;
        Vector3 roomMin = room.center - new Vector3(room.size.x / 2f, room.size.y / 2f, 0);
        Vector3 roomMax = room.center + new Vector3(room.size.x / 2f, room.size.y / 2f, 0);

        return playerPos.x >= roomMin.x && playerPos.x <= roomMax.x &&
               playerPos.y >= roomMin.y && playerPos.y <= roomMax.y;
    }

    void MoveToRoom(Room room)
    {
        // Mover c�mara al centro de la habitaci�n
        transform.position = new Vector3(room.center.x, room.center.y, -10);

        // Adaptar el tama�o de la c�mara para que encaje con la habitaci�n
        AdaptCameraToRoom(room);
    }

    void AdaptCameraToRoom(Room room)
    {
        if (cam == null) return;

        // Calcular el aspect ratio de la habitaci�n
        float roomAspect = room.size.x / room.size.y;
        float screenAspect = (float)Screen.width / Screen.height;

        // Ajustar el tama�o de la c�mara para que la habitaci�n quede perfectamente en vista
        if (roomAspect > screenAspect)
        {
            // La habitaci�n es m�s ancha que la pantalla
            cam.orthographicSize = room.size.x / (2f * screenAspect);
        }
        else
        {
            // La habitaci�n es m�s alta que la pantalla
            cam.orthographicSize = room.size.y / 2f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        foreach (Room room in rooms)
        {
            if (room.size.x > 0 && room.size.y > 0)
            {
                // Dibujar el rect�ngulo de la habitaci�n
                Gizmos.DrawWireCube(room.center, new Vector3(room.size.x, room.size.y, 0.1f));

                // Dibujar el �rea visible de la c�mara (solo en Play mode)
                if (Application.isPlaying && currentRoomIndex >= 0 && rooms[currentRoomIndex].Equals(room))
                {
                    Gizmos.color = Color.green;
                    DrawCameraView(room);
                }
            }
        }
    }

    void DrawCameraView(Room room)
    {
        if (cam == null) return;

        float verticalSize = cam.orthographicSize;
        float horizontalSize = verticalSize * cam.aspect;

        Vector3 cameraViewMin = new Vector3(transform.position.x - horizontalSize, transform.position.y - verticalSize, 0);
        Vector3 cameraViewMax = new Vector3(transform.position.x + horizontalSize, transform.position.y + verticalSize, 0);

        Gizmos.DrawWireCube(transform.position, new Vector3(horizontalSize * 2, verticalSize * 2, 0.1f));
    }
}