using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameLight : MonoBehaviour
{

	[Header("Circular Animation")]
	[SerializeField]
	private float _circularAnimationDuration;
	[SerializeField]
	private float _circularAnimationRotationRange;
	[SerializeField]
	private float _circularAnimationParentRotationDuration;

	private Light _light;
	private GameObject _parentGo;

	private void Awake()
	{
		_light = GetComponent<Light>();
		_parentGo = transform.parent.gameObject;
	}

	void Start()
	{
		transform.DOLocalRotate(
			new Vector3(-_circularAnimationRotationRange, 0.0F, 0.0F),
			0
		).SetRelative(true);

		transform.DOLocalRotate(
			new Vector3(_circularAnimationRotationRange * 2.0F, 0.0F, 0.0F),
			_circularAnimationDuration
		).SetRelative(true)
		.SetEase(Ease.InOutSine)
		.SetLoops(-1, LoopType.Yoyo);

		_parentGo.transform.DOLocalRotate(
			new Vector3(0.0F, 360.0F, 0.0F),
			_circularAnimationParentRotationDuration,
			RotateMode.FastBeyond360
		).SetRelative(true)
		.SetEase(Ease.Linear)
		.SetLoops(-1, LoopType.Incremental);
	}
}
