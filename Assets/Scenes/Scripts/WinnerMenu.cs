using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WinnerMenu : MonoBehaviour
{

	[Header("Game Objects")]
	[SerializeField]
	private SpriteRenderer _winnerSr;
	[SerializeField]
	private Sprite _drawSprite;
	[SerializeField]
	private Sprite _playerSprite;
	[SerializeField]
	private Sprite _cpuSprite;
	[SerializeField]
	private GameButton _restartButton;
	[SerializeField]
	private AudioSource _cheeringAs;
	[SerializeField]
	private AudioSource _fireworksAs;
	[SerializeField]
	private ParticleSystem _leftFireworkPs;
	[SerializeField]
	private ParticleSystem _rightFireworkPs;

	[Header("Animation")]
	[SerializeField]
	private float _winnerDuration;
	[SerializeField]
	private float _showWinerDelay;
	[SerializeField]
	private float _menuDuration;
	[SerializeField]
	private Vector3 _positionTo;
	[SerializeField]
	private Quaternion _rotationTo;
	[SerializeField]
	private Vector3 _scaleTo;

	private Vector3 _positionFrom;
	private Vector3 _scaleMin;
	private Vector3 _scaleMax;
	private Quaternion _rotationFrom;
	private AudioSource _as;
	private AudioSource _restartButtonAs;
	private float _cheeringAsVolume;
	private float _fireworksAsVolume;

	public event Action OnRestartEvent;

	private void Awake()
	{
		_positionFrom = transform.position;
		_rotationFrom = transform.rotation;

		_scaleMin = Vector3.zero;
		_scaleMax = Vector3.one;

		_as = GetComponent<AudioSource>();
		_restartButtonAs = _restartButton.GetComponent<AudioSource>();
		_fireworksAsVolume = _fireworksAs.volume;
		_cheeringAsVolume = _cheeringAs.volume;

		gameObject.SetActive(false);
		_restartButton.gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		_restartButton.OnClickEvent += OnRestartClicked;
	}

	private void OnDisable()
	{
		_restartButton.OnClickEvent -= OnRestartClicked;
	}

	private void OnRestartClicked()
	{
		_restartButton.SetInteractive(false);

		OnRestartEvent?.Invoke();

		_cheeringAs.DOFade(0.0F, _menuDuration * 2)
			.OnComplete(() =>
			{
				_cheeringAs.volume = _cheeringAsVolume;
				_cheeringAs.Stop();
			});
		_fireworksAs.DOFade(0.0F, _menuDuration)
			.OnComplete(() =>
			{
				_fireworksAs.volume = _fireworksAsVolume;
				_fireworksAs.Stop();
			});

		DOTween.Sequence()
			.Append(
				transform.DOScale(_scaleMin, _menuDuration)
					.SetEase(Ease.InBack)
			).Append(
				_restartButton.gameObject.transform.DOScale(_scaleMin, _menuDuration / 2)
					.SetEase(Ease.InBack)
			).AppendCallback(() =>
			{
				gameObject.SetActive(false);
				_restartButton.gameObject.SetActive(false);
			});

		_leftFireworkPs.Stop();
		_rightFireworkPs.Stop();
	}

	private void Update()
	{
		if (!_restartButton.IsInteractive())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			_restartButton.InvokeOnClick();
		}
	}

	public void PlayWinnerAnimation(WinnerState winnerState)
	{
		transform.transform.position = _positionFrom;
		transform.transform.rotation = _rotationFrom;
		transform.localScale = _scaleMin;

		_winnerSr.sprite = winnerState switch
		{
			WinnerState.Draw => _drawSprite,
			WinnerState.Player => _playerSprite,
			WinnerState.CPU => _cpuSprite,
			_ => throw new NotImplementedException(),
		};

		gameObject.SetActive(true);

		_as.Play();
		_cheeringAs.DOFade(0.0F, _winnerDuration * 2)
			.From()
			.OnStart(_cheeringAs.Play);
		_fireworksAs.DOFade(0.0F, _winnerDuration)
			.From()
			.OnStart(_fireworksAs.Play);

		transform.DOScale(_scaleMax, _winnerDuration)
			.SetEase(Ease.OutBack);

		_leftFireworkPs.Play();
		_rightFireworkPs.Play();
	}

	public void PlayMenuAnimation()
	{
		_restartButton.transform.localScale = _scaleMin;
		_restartButton.gameObject.SetActive(true);

		DOTween.Sequence()
			.Append(
				DOVirtual.DelayedCall(_showWinerDelay, () => { })
			).Append(
				transform.DOJump(_positionTo, 4, 1, _menuDuration)
			).Join(
				transform.DORotateQuaternion(_rotationTo, _menuDuration)
			).Join(
				transform.DOScale(_scaleTo, _menuDuration)
			).Append(
				_restartButton.gameObject.transform.DOScale(_scaleMax, _menuDuration / 2)
					.SetEase(Ease.OutBack)
					.OnStart(_restartButtonAs.Play)
			).AppendCallback(() =>
				_restartButton.SetInteractive(true)
			);
	}

	public enum WinnerState
	{
		Draw,
		Player,
		CPU
	}
}
