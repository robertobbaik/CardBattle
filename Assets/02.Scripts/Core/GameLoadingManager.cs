using UnityEngine;
using System.Collections;

public class GameLoadingManager : MonoBehaviour
{
    [SerializeField] private float _gameLoadDelay = 2.5f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_gameLoadDelay);

        SceneManager.Instance.LoadScene(SceneType.GameScene);
    }
}
