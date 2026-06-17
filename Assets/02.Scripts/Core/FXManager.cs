using System.Collections.Generic;
using System;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    [SerializeField] private FX _damageFxPrefab;
    [SerializeField] private FX _healFxPrefab;
    [SerializeField] private FX _liningFxPrefab;
    [SerializeField] private FX _doubleSlashFxPrefab;

    private readonly Queue<FX> _damagePool = new Queue<FX>();
    private readonly Queue<FX> _healPool = new Queue<FX>();
    private readonly Queue<FX> _liningPool = new Queue<FX>();
    private readonly Queue<FX> _doubleSlashPool = new Queue<FX>();

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
        return PlayLocal(_damageFxPrefab, _damagePool, FXKind.Damage, parent, localPosition, Quaternion.identity, null);
    }

    public FX PlayHealFX(Transform parent, Vector3 localPosition)
    {
        return PlayLocal(_healFxPrefab, _healPool, FXKind.Heal, parent, localPosition, Quaternion.identity, null);
    }

    public FX PlayLiningFX(Transform parent, Vector3 worldPosition, Quaternion worldRotation, Action onComplete)
    {
        return PlayWorld(_liningFxPrefab, _liningPool, FXKind.Lining, parent, worldPosition, worldRotation, onComplete);
    }

    public FX PlayDoubleSlashFX(Transform parent, Vector3 worldPosition, Quaternion worldRotation, Action onComplete)
    {
        return PlayWorld(_doubleSlashFxPrefab, _doubleSlashPool, FXKind.DoubleSlash, parent, worldPosition, worldRotation, onComplete);
    }

    public void Recycle(FX fx, FXKind kind)
    {
        if (fx == null)
        {
            return;
        }

        fx.transform.SetParent(transform, false);
        fx.gameObject.SetActive(false);

        switch (kind)
        {
            case FXKind.Damage:
                _damagePool.Enqueue(fx);
                return;
            case FXKind.Heal:
                _healPool.Enqueue(fx);
                return;
            case FXKind.Lining:
                _liningPool.Enqueue(fx);
                return;
            case FXKind.DoubleSlash:
                _doubleSlashPool.Enqueue(fx);
                return;
        }
    }

    private FX PlayLocal(FX prefab, Queue<FX> pool, FXKind kind, Transform parent, Vector3 localPosition, Quaternion localRotation, Action onComplete)
    {
        if (prefab == null)
        {
            return null;
        }

        FX fx = GetInstance(prefab, pool, kind);
        if (fx == null)
        {
            return null;
        }

        fx.PlayLocal(parent == null ? transform : parent, localPosition, localRotation, onComplete);
        return fx;
    }

    private FX PlayWorld(FX prefab, Queue<FX> pool, FXKind kind, Transform parent, Vector3 worldPosition, Quaternion worldRotation, Action onComplete)
    {
        if (prefab == null)
        {
            return null;
        }

        FX fx = GetInstance(prefab, pool, kind);
        if (fx == null)
        {
            return null;
        }

        fx.PlayWorld(parent == null ? transform : parent, worldPosition, worldRotation, onComplete);
        return fx;
    }

    private FX GetInstance(FX prefab, Queue<FX> pool, FXKind kind)
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

        fx.Initialize(this, kind);

        return fx;
    }
}
