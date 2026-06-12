using System.Collections;
using UnityEngine;

public sealed class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private float _lobbyLoadDelay = 2.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_lobbyLoadDelay);

        if (SceneManager.Instance == null)
        {
            Debug.LogError("SceneManager instance is missing.");
            yield break;
        }

        SceneManager.Instance.LoadScene(SceneType.Lobby);
    }
}
