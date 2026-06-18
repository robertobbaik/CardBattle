using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    private const string StartAnimationTrigger = "Start";

    [SerializeField] private List<Image> _myDeckIcons = new List<Image>();
    [SerializeField] private List<Image> _opponentDeckIcons = new List<Image>();
    [SerializeField] private Image _playerFirstTurnImage;
    [SerializeField] private Image _enemyFirstTurnImage;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _animationDelay = 2f;

    private Action _startSequenceCompleteCallback;
    private bool _isPlayingStartSequence;

    public void PlayStartSequence(TurnOwner firstTurnOwner, Action onComplete)
    {
        gameObject.SetActive(true);

        _startSequenceCompleteCallback = onComplete;
        _isPlayingStartSequence = true;

        SetDeckIcons(_myDeckIcons, UserInfoManager.Instance.CurrentDec);
        SetDeckIcons(_opponentDeckIcons, EnemyController.Instance.GetDec());
        SetFirstTurnImage(firstTurnOwner);

        if (_animationDelay > 0f)
        {
            Invoke(nameof(PlayStartAnimation), _animationDelay);
            return;
        }

        PlayStartAnimation();
    }

    public void OnStartAnimationComplete()
    {
        CompleteStartSequence();
    }

    private void PlayStartAnimation()
    {
        if (_animator == null)
        {
            CompleteStartSequence();
            return;
        }

        _animator.ResetTrigger(StartAnimationTrigger);
        _animator.SetTrigger(StartAnimationTrigger);
    }

    private void SetDeckIcons(List<Image> deckIcons, List<int> dec)
    {
        for (int i = 0; i < deckIcons.Count; i++)
        {
            Image iconImage = deckIcons[i];
            if (iconImage == null)
            {
                continue;
            }

            if (i >= dec.Count)
            {
                iconImage.sprite = null;
                iconImage.gameObject.SetActive(false);
                continue;
            }

            iconImage.sprite = DataManager.Instance.GetCardIcon(dec[i]);
            iconImage.gameObject.SetActive(true);
        }
    }

    private void SetFirstTurnImage(TurnOwner firstTurnOwner)
    {
        SetImageActive(_playerFirstTurnImage, firstTurnOwner == TurnOwner.Player);
        SetImageActive(_enemyFirstTurnImage, firstTurnOwner == TurnOwner.Enemy);
    }

    private void SetImageActive(Image image, bool isActive)
    {
        if (image == null)
        {
            return;
        }

        image.gameObject.SetActive(isActive);
    }

    private void CompleteStartSequence()
    {
        if (!_isPlayingStartSequence)
        {
            return;
        }

        _isPlayingStartSequence = false;
        CancelInvoke(nameof(PlayStartAnimation));

        Action completeCallback = _startSequenceCompleteCallback;
        _startSequenceCompleteCallback = null;

        gameObject.SetActive(false);
        SetFirstTurnImage(TurnOwner.None);
        completeCallback?.Invoke();
    }
}
