using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DeckCounter : MonoBehaviour
{
	[Header("Game Objects")]
	[SerializeField]
	private TextMeshPro _counterText;

	[Header("Opening/Closing Animation")]
	[SerializeField]
	private float _animationDuration;
	[SerializeField]
	private float _animationScaleFrom;
	[SerializeField]
	private float _animationScaleTo;

	private int _maxCount;
	private Vector3 _scaleFrom;
	private Vector3 _scaleTo;
	private Vector3 _punchScale;

	private void Awake()
	{
		_scaleFrom = new(_animationScaleFrom, _animationScaleFrom, _animationScaleFrom);
		_scaleTo = new(_animationScaleTo, _animationScaleTo, _animationScaleTo);

		var punchScaleFactor = 0.5F;
		_punchScale = new(punchScaleFactor, punchScaleFactor, punchScaleFactor);

		transform.localScale = _scaleFrom;
		gameObject.SetActive(false);
	}

	public void PlayOpeningAnimation()
	{
		transform.DOScale(_scaleTo, _animationDuration)
			.SetEase(Ease.OutBack)
			.OnStart(() => gameObject.SetActive(true));
	}

	public void PlayClosingAnimation()
	{
		transform.DOScale(_scaleFrom, _animationDuration)
			.SetEase(Ease.InBack)
			.OnComplete(() => gameObject.SetActive(false));
	}

	public void SetupMaxCount(int maxCount)
	{
		_maxCount = maxCount;

		UpdateCount(maxCount);
	}

	public void UpdateCount(int count)
	{
		_counterText.text = $"{count}  /  {_maxCount}";

		transform.DOPunchScale(_punchScale, 0.2F, 5, 1);
	}
}
