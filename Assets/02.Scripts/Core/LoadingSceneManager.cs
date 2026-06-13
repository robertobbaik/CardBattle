using System.Collections;
using UnityEngine;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private float _lobbyLoadDelay = 2.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_lobbyLoadDelay);

        SceneManager.Instance.LoadScene(SceneType.Lobby);
    }
}
