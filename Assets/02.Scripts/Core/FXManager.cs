using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    [SerializeField] private FX _damageFxPrefab;
    [SerializeField] private FX _healFxPrefab;

    private readonly Queue<FX> _damagePool = new Queue<FX>();
    private readonly Queue<FX> _healPool = new Queue<FX>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public FX PlayDamageFX(Transform parent, Vector3 localPosition)
    {
        return Play(_damageFxPrefab, _damagePool, false, parent, localPosition);
    }

    public FX PlayHealFX(Transform parent, Vector3 localPosition)
    {
        return Play(_healFxPrefab, _healPool, true, parent, localPosition);
    }

    public void Recycle(FX fx, bool isHeal)
    {
        if (fx == null)
        {
            return;
        }

        fx.transform.SetParent(transform, false);
        fx.gameObject.SetActive(false);

        if (isHeal)
        {
            _healPool.Enqueue(fx);
            return;
        }

        _damagePool.Enqueue(fx);
    }

    private FX Play(FX prefab, Queue<FX> pool, bool isHeal, Transform parent, Vector3 localPosition)
    {
        if (prefab == null)
        {
            return null;
        }

        FX fx = GetInstance(prefab, pool, isHeal);
        if (fx == null)
        {
            return null;
        }

        fx.Play(parent == null ? transform : parent, localPosition);
        return fx;
    }

    private FX GetInstance(FX prefab, Queue<FX> pool, bool isHeal)
    {
        FX fx;

        if (pool.Count > 0)
        {
            fx = pool.Dequeue();
        }
        else
        {
            fx = Instantiate(prefab, transform);
            fx.gameObject.SetActive(false);
        }

        fx.Initialize(this, isHeal);

        return fx;
    }
}
