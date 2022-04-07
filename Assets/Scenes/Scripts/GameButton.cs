using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameButton : MonoBehaviour
{

	[Header("Setup")]
	[SerializeField]
	private TextMeshPro _titleText;
	[SerializeField]
	private Renderer _fillRenderer;
	[SerializeField]
	private AudioSource _hoverAs;
	[SerializeField]
	private AudioSource _clickAs;
	[SerializeField]
	private string _text;

	[Header("Hover Animation")]
	[SerializeField]
	private float _hoverPunchDuration;
	[SerializeField]
	private Vector3 _hoverPunchRotation;
	[SerializeField]
	private int _hoverPunchVibrato;
	[SerializeField]
	private float _hoverPunchElasticity;
	[SerializeField]
	private Color _fillColorTo;

	private Tween _hoverRotationTween;
	private Tween _hoverFillMaterialTween;
	private bool _isHovered;
	private bool _isExited;
	private bool _isInteractive;

	public event Action OnClickEvent;

	private void Awake()
	{
		_titleText.text = _text;
	}

	private void Start()
	{
		_hoverFillMaterialTween = _fillRenderer.material.DOColor(_fillColorTo, _hoverPunchDuration * 0.75F)
			.SetAutoKill(false);
		_hoverFillMaterialTween.Rewind();
	}

	private void OnMouseEnter()
	{
		if (!_isInteractive)
		{
			return;
		}

		_hoverFillMaterialTween.PlayForward();

		if (_isExited && !_hoverRotationTween.IsActive())
		{
			_isHovered = false;
		}
		if (_isHovered)
		{
			return;
		}

		_hoverRotationTween = transform.DOPunchRotation(
			_hoverPunchRotation,
			_hoverPunchDuration,
			_hoverPunchVibrato,
			_hoverPunchElasticity
		);
		_hoverAs.Play();

		_isHovered = true;
		_isExited = false;
	}

	private void OnMouseOver()
	{
		if (!_isInteractive)
		{
			return;
		}

		if (Input.GetMouseButtonDown(0))
		{
			InvokeOnClick();
		}
	}

	private void OnMouseExit()
	{
		if (!_isInteractive)
		{
			return;
		}

		_hoverFillMaterialTween.PlayBackwards();

		_isExited = true;

		if (_hoverRotationTween.IsActive())
		{
			return;
		}

		_isHovered = false;
	}

	public void SetInteractive(bool isInteractive) => _isInteractive = isInteractive;

	public bool IsInteractive() => _isInteractive;

	public void InvokeOnClick()
	{
		_clickAs.Play();

		OnClickEvent?.Invoke();
	}
}
