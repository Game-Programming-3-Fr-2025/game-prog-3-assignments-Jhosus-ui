using UnityEngine;
using System.Collections.Generic;

public class CF : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public Vector3 center;    // Centro de la habitación
        public Vector2 size;      // Tamaño de la habitación
    }

    public Transform player;
    public List<Room> rooms = new List<Room>();
    public Camera cam;            // Referencia a la cámara

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
        // Mover cámara al centro de la habitación
        transform.position = new Vector3(room.center.x, room.center.y, -10);

        // Adaptar el tamaño de la cámara para que encaje con la habitación
        AdaptCameraToRoom(room);
    }

    void AdaptCameraToRoom(Room room)
    {
        if (cam == null) return;

        // Calcular el aspect ratio de la habitación
        float roomAspect = room.size.x / room.size.y;
        float screenAspect = (float)Screen.width / Screen.height;

        // Ajustar el tamaño de la cámara para que la habitación quede perfectamente en vista
        if (roomAspect > screenAspect)
        {
            // La habitación es más ancha que la pantalla
            cam.orthographicSize = room.size.x / (2f * screenAspect);
        }
        else
        {
            // La habitación es más alta que la pantalla
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
                // Dibujar el rectángulo de la habitación
                Gizmos.DrawWireCube(room.center, new Vector3(room.size.x, room.size.y, 0.1f));

                // Dibujar el área visible de la cámara (solo en Play mode)
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