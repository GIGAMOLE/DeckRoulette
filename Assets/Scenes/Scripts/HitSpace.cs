using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HitSpace : MonoBehaviour
{

	[Header("Sprites")]
	[SerializeField]
	private Sprite _defaultSprite;
	[SerializeField]
	private Sprite _hitSprite;

	[Header("Opening/Closing Animation")]
	[SerializeField]
	private float _openingAnimationDuration;
	[SerializeField]
	private float _openingAnimationScaleTo;

	[Header("Hit Animation")]
	[SerializeField]
	private float _hitAnimationPunchDuration;
	[SerializeField]
	private float _hitAnimationPunchScale;
	[SerializeField]
	private float _hitAnimationSwitchDuration;
	[SerializeField]
	private float _hitAnimationSwitchRotation;
	[SerializeField]
	private float _hitAnimationWhenFiredDelay;

	private SpriteRenderer _sr;
	private AudioSource _as;
	private Vector3 _scaleFrom;
	private Vector3 _scaleTo;
	private Vector3 _punchScale;
	private bool isFirstTimeFired;
	private Sequence _hitAnimationSequence;
	private Tweener _openingAnimation;

	private void Awake()
	{
		_sr = GetComponent<SpriteRenderer>();
		_as = GetComponent<AudioSource>();

		_scaleFrom = Vector3.zero;
		_scaleTo = new(_openingAnimationScaleTo, _openingAnimationScaleTo, _openingAnimationScaleTo);

		_punchScale = new(_hitAnimationPunchScale, _hitAnimationPunchScale, _hitAnimationPunchScale);

		transform.localScale = _scaleFrom;
		gameObject.SetActive(false);
	}

	public void PlayOpeningAnimation()
	{
		_hitAnimationSequence?.Kill();
		_openingAnimation.Kill();

		_sr.sprite = _defaultSprite;

		_openingAnimation = transform.DOScale(_scaleTo, _openingAnimationDuration)
			.SetEase(Ease.OutBack)
			.SetDelay(isFirstTimeFired ? _hitAnimationWhenFiredDelay : 0)
			.OnStart(() => gameObject.SetActive(true))
			.OnComplete(PlayHitAnimation);

		isFirstTimeFired = true;
	}

	public void PlayClosingAnimation()
	{
		_hitAnimationSequence?.Kill();

		if (!gameObject.activeInHierarchy)
		{
			_openingAnimation.Kill();
			return;
		}

		transform.DOScale(_scaleFrom, _openingAnimationDuration)
			.SetEase(Ease.InBack)
			.OnComplete(() => gameObject.SetActive(false));
	}

	private void PlayHitAnimation()
	{
		_openingAnimation?.Kill();

		_hitAnimationSequence = DOTween.Sequence()
			.AppendCallback(() =>
			{
				_sr.sprite = _hitSprite;

				if (gameObject.activeInHierarchy)
				{
					_as.Play();
				}
				transform.DOPunchScale(_punchScale, _hitAnimationPunchDuration, 5, 1);
			})
			.AppendInterval(_hitAnimationSwitchDuration)
			.AppendCallback(() => _sr.sprite = _defaultSprite)
			.AppendInterval(_hitAnimationSwitchDuration)
			.SetLoops(-1, LoopType.Restart);
	}
}
