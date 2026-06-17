using System;
using UnityEngine;

public class FX : MonoBehaviour
{
    private const string StartTrigger = "Start";

    [SerializeField] private Animator _animator;
    [SerializeField] private float _fallbackDuration = 1f;

    private FXManager _owner;
    private FXKind _kind;
    private Action _onComplete;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void Initialize(FXManager owner, FXKind kind)
    {
        _owner = owner;
        _kind = kind;

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void PlayLocal(Transform parent, Vector3 localPosition, Quaternion localRotation, Action onComplete)
    {
        PlayInternal(parent, localPosition, localRotation, false, onComplete);
    }

    public void PlayWorld(Transform parent, Vector3 worldPosition, Quaternion worldRotation, Action onComplete)
    {
        PlayInternal(parent, worldPosition, worldRotation, true, onComplete);
    }

    private void PlayInternal(Transform parent, Vector3 position, Quaternion rotation, bool useWorldSpace, Action onComplete)
    {
        CancelInvoke(nameof(ReturnToPool));
        _onComplete = onComplete;

        if (parent != null)
        {
            transform.SetParent(parent, false);
        }

        if (useWorldSpace)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
        else
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
        }

        gameObject.SetActive(true);

        if (_animator != null)
        {
            _animator.Rebind();
            _animator.Update(0f);
            _animator.ResetTrigger(StartTrigger);
            _animator.SetTrigger(StartTrigger);
        }

        Invoke(nameof(ReturnToPool), GetPlaybackDuration());
    }

    private float GetPlaybackDuration()
    {
        if (_animator == null)
        {
            return _fallbackDuration;
        }

        RuntimeAnimatorController controller = _animator.runtimeAnimatorController;
        if (controller == null)
        {
            return _fallbackDuration;
        }

        AnimationClip[] clips = controller.animationClips;
        float longestClipLength = 0f;

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null)
            {
                continue;
            }

            if (clip.length > longestClipLength)
            {
                longestClipLength = clip.length;
            }
        }

        if (longestClipLength <= 0f)
        {
            return _fallbackDuration;
        }

        float speed = Mathf.Abs(_animator.speed);
        if (speed <= 0.0001f)
        {
            speed = 1f;
        }

        return longestClipLength / speed;
    }

    private void ReturnToPool()
    {
        Action onComplete = _onComplete;
        _onComplete = null;

        if (_owner == null)
        {
            onComplete?.Invoke();
            return;
        }

        _owner.Recycle(this, _kind);
        onComplete?.Invoke();
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ReturnToPool));
    }
}
