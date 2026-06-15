using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject LoadGameObject(string resourcePath, string resourceName)
    {
        string fullPath = string.IsNullOrEmpty(resourcePath) ? resourceName : string.Concat(resourcePath, GlobalString.Slash, resourceName);
        GameObject resource = Resources.Load<GameObject>(fullPath);

        if (resource == null)
        {
            Debug.LogError(string.Format(GlobalString.ResourceNotFoundMessage, fullPath));
            return null;
        }

        return resource;
    }
}
