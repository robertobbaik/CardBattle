using UnityEngine;

public class FX : MonoBehaviour
{
    private const string StartTrigger = "Start";

    [SerializeField] private Animator _animator;
    [SerializeField] private float _fallbackDuration = 1f;

    private FXManager _owner;
    private bool _isHeal;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void Initialize(FXManager owner, bool isHeal)
    {
        _owner = owner;
        _isHeal = isHeal;

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
    }

    public void Play(Transform parent, Vector3 localPosition)
    {
        CancelInvoke(nameof(ReturnToPool));

        if (parent != null)
        {
            transform.SetParent(parent, false);
        }

        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.identity;
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
        if (_owner == null)
        {
            return;
        }

        _owner.Recycle(this, _isHeal);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ReturnToPool));
    }
}
