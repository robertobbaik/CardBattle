using System.Collections.Generic;
using System;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance { get; private set; }

    private const string DamageFxName = "FX - Damage";
    private const string HealFxName = "Fx - Heal";
    private const string LiningFxName = "FX - Lining";
    private const string DoubleSlashFxName = "FX - DoubleSlash";
    private const string DestroyFxName = "FX - Destroy";
    private const string ShieldFxName = "FX - Shield";
    private const string DebuffFxName = "FX - Debuff";

    private GameObject _damageFxPrefab;
    private GameObject _healFxPrefab;
    private GameObject _liningFxPrefab;
    private GameObject _doubleSlashFxPrefab;
    private GameObject _destroyFxPrefab;
    private GameObject _shieldFxPrefab;
    private GameObject _debuffFxPrefab;

    private readonly Queue<FX> _damagePool = new Queue<FX>();
    private readonly Queue<FX> _healPool = new Queue<FX>();
    private readonly Queue<FX> _liningPool = new Queue<FX>();
    private readonly Queue<FX> _doubleSlashPool = new Queue<FX>();
    private readonly Queue<FX> _destroyPool = new Queue<FX>();
    private readonly Queue<FX> _shieldPool = new Queue<FX>();
    private readonly Queue<FX> _debuffPool = new Queue<FX>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadPrefabs();
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

    public FX PlayDestroyFX(Transform parent, Vector3 localPosition, Action onComplete)
    {
        return PlayLocal(_destroyFxPrefab, _destroyPool, FXKind.Destroy, parent, localPosition, Quaternion.identity, onComplete);
    }

    public FX PlayShieldFX(Transform parent, Vector3 localPosition)
    {
        return PlayLocal(_shieldFxPrefab, _shieldPool, FXKind.Shield, parent, localPosition, Quaternion.identity, null);
    }

    public FX PlayDebuffFX(Transform parent, Vector3 localPosition)
    {
        return PlayLocal(_debuffFxPrefab, _debuffPool, FXKind.Debuff, parent, localPosition, Quaternion.identity, null);
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
            case FXKind.Destroy:
                _destroyPool.Enqueue(fx);
                return;
            case FXKind.Shield:
                _shieldPool.Enqueue(fx);
                return;
            case FXKind.Debuff:
                _debuffPool.Enqueue(fx);
                return;
        }
    }

    private void LoadPrefabs()
    {
        _damageFxPrefab = LoadPrefab(DamageFxName);
        _healFxPrefab = LoadPrefab(HealFxName);
        _liningFxPrefab = LoadPrefab(LiningFxName);
        _doubleSlashFxPrefab = LoadPrefab(DoubleSlashFxName);
        _destroyFxPrefab = LoadPrefab(DestroyFxName);
        _shieldFxPrefab = LoadPrefab(ShieldFxName);
        _debuffFxPrefab = LoadPrefab(DebuffFxName);
    }

    private GameObject LoadPrefab(string resourceName)
    {
        string resourcePath = string.Concat(GlobalString.ResourceFxPath, GlobalString.Slash, resourceName);
        GameObject prefab = Resources.Load<GameObject>(resourcePath);

        if (prefab == null)
        {
            Debug.LogError(string.Format(GlobalString.ResourceNotFoundMessage, resourcePath));
        }

        return prefab;
    }

    private FX PlayLocal(GameObject prefab, Queue<FX> pool, FXKind kind, Transform parent, Vector3 localPosition, Quaternion localRotation, Action onComplete)
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

    private FX PlayWorld(GameObject prefab, Queue<FX> pool, FXKind kind, Transform parent, Vector3 worldPosition, Quaternion worldRotation, Action onComplete)
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

    private FX GetInstance(GameObject prefab, Queue<FX> pool, FXKind kind)
    {
        FX fx;

        if (pool.Count > 0)
        {
            fx = pool.Dequeue();
        }
        else
        {
            GameObject instance = Instantiate(prefab, transform);
            fx = instance.GetComponent<FX>();
            if (fx == null)
            {
                Destroy(instance);
                return null;
            }

            fx.gameObject.SetActive(false);
        }

        fx.Initialize(this, kind);

        return fx;
    }
}
