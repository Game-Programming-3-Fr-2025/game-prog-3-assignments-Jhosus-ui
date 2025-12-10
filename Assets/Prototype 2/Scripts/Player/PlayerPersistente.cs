//using UnityEngine;
//using UnityEngine.SceneManagement;
//using System.Collections;

//public class PlayerPersistente : MonoBehaviour
//{
//    private static PlayerPersistente instance;
//    private Vector3 spawnPosition;
//    private string targetScene = "";

//    [Header("Configuración UI")]
//    [SerializeField] private GameObject playerUICanvas; // Arrastra tu Canvas de UI aquí

//    void Awake()
//    {
//        if (instance == null)
//        {
//            instance = this;
//            DontDestroyOnLoad(gameObject);

//            // Hacer persistente la UI del jugador
//            if (playerUICanvas != null)
//            {
//                DontDestroyOnLoad(playerUICanvas);
//            }

//            SceneManager.sceneLoaded += OnSceneLoaded;
//        }
//        else
//        {
//            // Si ya existe una instancia, destruir este objeto y su UI
//            if (playerUICanvas != null)
//            {
//                Destroy(playerUICanvas);
//            }
//            Destroy(gameObject);
//        }
//    }

//    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
//    {
//        // Buscar un spawn point en la escena actual
//        GameObject spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint");
//        if (spawnPoint != null)
//        {
//            transform.position = spawnPoint.transform.position;

//            // Hacer fade in
//            StartCoroutine(FadeInAlEntrar());
//        }

//        // Reconfigurar la UI si es necesario
//        ReconfigurarUIEnNuevaEscena();
//    }

//    // Método para que Exit.cs establezca posición específica
//    public void SetTargetPosition(Vector3 position, string sceneName)
//    {
//        spawnPosition = position;
//        targetScene = sceneName;
//    }

//    private IEnumerator FadeInAlEntrar()
//    {
//        GameObject fadeObject = GameObject.FindGameObjectWithTag("Fade");
//        if (fadeObject != null)
//        {
//            UnityEngine.UI.Image fadeImage = fadeObject.GetComponent<UnityEngine.UI.Image>();
//            if (fadeImage != null)
//            {
//                float duracion = 1.5f;
//                float tiempo = 0f;
//                Color color = fadeImage.color;
//                color.a = 1f; // Comenzar completamente negro
//                fadeImage.color = color;

//                while (tiempo < duracion)
//                {
//                    tiempo += Time.deltaTime;
//                    color.a = Mathf.Lerp(1f, 0f, tiempo / duracion);
//                    fadeImage.color = color;
//                    yield return null;
//                }

//                color.a = 0f;
//                fadeImage.color = color;
//            }
//        }
//    }

//    private void ReconfigurarUIEnNuevaEscena()
//    {
//        if (playerUICanvas != null)
//        {
//            // Asegurar que la UI esté en la capa correcta
//            playerUICanvas.transform.SetAsLastSibling();
//        }
//    }

//    void OnDestroy()
//    {
//        SceneManager.sceneLoaded -= OnSceneLoaded;
//    }

//    // Método público para acceder a la UI desde otros scripts
//    public GameObject GetPlayerUI()
//    {
//        return playerUICanvas;
//    }
//}