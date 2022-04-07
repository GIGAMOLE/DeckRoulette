using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EntranceManager : MonoBehaviour
{

	[Header("Game Objects")]
	[SerializeField]
	private Player _player1;
	[SerializeField]
	private GameObject _player1Go;
	[SerializeField]
	private Player _player2;
	[SerializeField]
	private GameObject _player2Go;
	[SerializeField]
	private GameObject _player1ScoreGo;
	[SerializeField]
	private GameObject _player2ScoreGo;
	[SerializeField]
	private GameObject _pileGo;
	[SerializeField]
	private SpriteRenderer _pileBgSr;
	[SerializeField]
	private GameObject _deckGo;
	[SerializeField]
	private GameObject _lightHolderGo;
	[SerializeField]
	private GameCamera _camera;
	[SerializeField]
	private Image _splashPanel;

	[Header("Welcome Game Objects")]
	[SerializeField]
	private GameObject _welcomeMenuGo;
	[SerializeField]
	private TextMeshPro _welcomeText;
	[SerializeField]
	private GameObject _menuTitleGo;
	[SerializeField]
	private SpriteRenderer _deckTitleSprite;
	[SerializeField]
	private SpriteRenderer _rouletteTitleSprite;
	[SerializeField]
	private SpriteRenderer _splashTitleSprite;
	[SerializeField]
	private GameButton _playButton;

	[Header("Menu Animation")]
	[SerializeField]
	private Vector3 _player1HolderPositionFrom;
	[SerializeField]
	private Vector3 _player1HolderPositionTo;
	[SerializeField]
	private Vector3 _player2HolderPositionFrom;
	[SerializeField]
	private Vector3 _player2HolderPositionTo;
	[SerializeField]
	private Vector3 _player1ScorePositionFrom;
	[SerializeField]
	private Vector3 _player1ScorePositionTo;
	[SerializeField]
	private Vector3 _player2ScorePositionFrom;
	[SerializeField]
	private Vector3 _player2ScorePositionTo;
	[SerializeField]
	private Vector3 _deckPositionFrom;
	[SerializeField]
	private Vector3 _deckPositionTo;
	[SerializeField]
	private float _lightRadiusFrom;
	[SerializeField]
	private float _lightRadiusTo;
	[SerializeField]
	private float _lightIntensityFrom;
	[SerializeField]
	private float _lightIntensityTo;

	[SerializeField]
	private Color _pileColorFrom;
	[SerializeField]
	private Color _pileColorTo;
	[SerializeField]
	private Vector3 _cameraPositionTo;
	[SerializeField]
	private Vector3 _cameraRotationFrom;
	[SerializeField]
	private Vector3 _cameraRotationTo;
	[SerializeField]
	private float _entranceAnimationDuration;

	[Header("Opening Animation")]
	[SerializeField]
	private float _openingAnimationDuration;
	[SerializeField]
	private Vector3 _cameraOpeningPositionFrom;
	[SerializeField]
	private Vector3 _cameraOpeningPositionTo;

	private GameManager _gameManager;
	private Light _light;
	private EntranceState _entranceState;
	private bool _isOpeningAnimationFired;
	private AudioSource _gameAs;
	private AudioSource _menuAs;

	private void Awake()
	{
		_gameManager = GetComponent<GameManager>();
		_gameAs = GetComponent<AudioSource>();
		_menuAs = _welcomeMenuGo.GetComponent<AudioSource>();
		_light = _lightHolderGo.GetComponentInChildren<Light>();

		_player2.transform.position = _player2HolderPositionFrom;
		_deckGo.transform.position = _deckPositionFrom;
		_player1.transform.position = _player1HolderPositionFrom;
		_player1ScoreGo.transform.position = _player1ScorePositionFrom;
		_player2ScoreGo.transform.position = _player2ScorePositionFrom;

		_camera.gameObject.transform.position = _cameraOpeningPositionFrom;
		_camera.gameObject.transform.DOLocalRotate(_cameraRotationFrom, 0);

		_light.spotAngle = _lightRadiusFrom;
		_light.intensity = _lightIntensityFrom;

		_pileBgSr.color = _pileColorFrom;
		_welcomeText.text = "";
		_deckTitleSprite.DOFade(0, 0);
		_rouletteTitleSprite.DOFade(0, 0);
		_splashTitleSprite.DOFade(0, 0);
		_playButton.gameObject.SetActive(false);

		_splashPanel.gameObject.SetActive(true);
		_welcomeMenuGo.SetActive(true);
	}

	private void OnEnable()
	{
		_playButton.OnClickEvent += OnStartButtonClicked;
	}

	private void OnDisable()
	{
		_playButton.OnClickEvent -= OnStartButtonClicked;
	}

	private void Start()
	{
		PlayerOpeningAnimation();
	}

	private void Update()
	{
		if (_entranceState != EntranceState.Opening)
		{
			return;
		}
		if (_isOpeningAnimationFired && Input.GetKeyDown(KeyCode.Space))
		{
			_playButton.InvokeOnClick();
		}
	}

	private void PlayerOpeningAnimation()
	{
		_entranceState = EntranceState.Opening;

		DOTween.Sequence()
			.Append(
				_splashPanel.DOFade(0, _openingAnimationDuration)
					.OnComplete(() => _splashPanel.gameObject.SetActive(false))
			)
			.Join(
				_camera.gameObject.transform.DOMove(_cameraOpeningPositionTo, _openingAnimationDuration * 1.2F)
			);

		var welcomeDuration = _openingAnimationDuration / 3;
		var pileMenuDuration = welcomeDuration / 2.6F;
		var pileMenuDelay = pileMenuDuration / 2.6F;

		_menuAs.DOFade(0.0F, _openingAnimationDuration)
			.From()
			.OnStart(_menuAs.Play);

		DOTween.Sequence()
			.Append(
				_welcomeText.DOText("Welcome to", welcomeDuration, false, ScrambleMode.All)
					.OnStart(() =>
					{
						var welcomeTextAs = _welcomeText.GetComponent<AudioSource>();
						welcomeTextAs.DOFade(0.0F, welcomeDuration)
							.From()
							.OnStart(welcomeTextAs.Play);
					})
			).Join(
				_deckTitleSprite.gameObject.transform.DOMoveZ(-2.0F, pileMenuDuration)
					.From(true)
					.SetEase(Ease.OutBack)
					.SetDelay(pileMenuDelay)
					.OnStart(_deckTitleSprite.GetComponent<AudioSource>().Play)
			).Join(
				_deckTitleSprite.DOFade(1.0F, pileMenuDuration)
					.SetDelay(pileMenuDelay)
			).Join(
				_splashTitleSprite.gameObject.transform.DOMoveZ(-2.0F, pileMenuDuration)
					.From(true)
					.SetEase(Ease.OutBack)
					.SetDelay(pileMenuDelay)
					.OnStart(_splashTitleSprite.GetComponent<AudioSource>().Play)
			).Join(
				_splashTitleSprite.DOFade(1.0F, pileMenuDuration)
					.SetDelay(pileMenuDelay)
			).Join(
				_rouletteTitleSprite.gameObject.transform.DOMoveZ(-2.0F, pileMenuDuration)
					.From(true)
					.SetEase(Ease.OutBack)
					.SetDelay(pileMenuDelay)
					.OnStart(_rouletteTitleSprite.GetComponent<AudioSource>().Play)
			).Join(
				_rouletteTitleSprite.DOFade(1.0F, pileMenuDuration)
					.SetDelay(pileMenuDelay)
			)
			.AppendCallback(() => _playButton.gameObject.SetActive(true))
			.Join(
				_playButton.transform.DOMoveY(-1.0F, pileMenuDuration)
					.From()
					.SetEase(Ease.OutBounce)
					.OnStart(_playButton.GetComponent<AudioSource>().Play)
			).AppendCallback(
				() =>
				{
					_isOpeningAnimationFired = true;
					_playButton.SetInteractive(true);
				}
			);
	}

	private void OnStartButtonClicked()
	{
		_playButton.SetInteractive(false);

		_entranceState = EntranceState.Game;

		var hideMenuDuration = _entranceAnimationDuration / 8;
		var hideMenuItemDelay = hideMenuDuration / 3;

		_gameAs.DOFade(0.0F, _entranceAnimationDuration)
			.From()
			.OnStart(_gameAs.Play);
		_menuAs.DOFade(0.0F, hideMenuDuration)
			.OnComplete(_menuAs.Stop);

		DOTween.Sequence()
			.Append(
				_playButton.transform.DOScale(Vector3.zero, hideMenuDuration)
					.SetEase(Ease.InBack)
					.SetDelay(hideMenuItemDelay)
			).Join(
				_menuTitleGo.transform.DOScale(Vector3.zero, hideMenuDuration)
					.SetEase(Ease.InBack)
					.SetDelay(hideMenuItemDelay)
			).Join(
				_welcomeText.transform.DOScale(Vector3.zero, hideMenuDuration)
					.SetEase(Ease.InBack)
					.SetDelay(hideMenuItemDelay)
			).AppendCallback(() => _welcomeMenuGo.SetActive(false));

		DOTween.Sequence()
			.Append(
				_camera.gameObject.transform.DOMove(_cameraPositionTo, _entranceAnimationDuration)
			).Join(
				_camera.gameObject.transform.DORotate(_cameraRotationTo, _entranceAnimationDuration)
					.OnComplete(() => _camera.EnableTracking())
			)
			.Join(
				_pileBgSr.DOColor(_pileColorTo, _entranceAnimationDuration)
					.SetEase(Ease.OutCirc)
			).Join(
				_light.DOIntensity(_lightIntensityTo, _entranceAnimationDuration)
			).Join(
				DOVirtual.Float(
					_lightRadiusFrom,
					_lightRadiusTo,
					_entranceAnimationDuration,
					(lightRadius) => _light.spotAngle = lightRadius
				)
			);

		var moveObjectsDuration = _entranceAnimationDuration / 4;
		var moveObjectsDelay = 0.12F;

		DOTween.Sequence()
			.Append(
				_player2.transform.DOMove(_player2HolderPositionTo, moveObjectsDuration)
					.SetDelay(moveObjectsDelay)
					.OnStart(_player2.GetComponent<AudioSource>().Play)
					.OnComplete(_player2.PlayPrepareAnimation)
			)
			.Join(
				_deckGo.transform.DOMove(_deckPositionTo, moveObjectsDuration)
					.SetDelay(moveObjectsDelay)
					.OnStart(_deckGo.GetComponent<AudioSource>().Play)
			).Join(
				_player1.transform.DOMove(_player1HolderPositionTo, moveObjectsDuration)
					.SetDelay(moveObjectsDelay)
					.OnStart(_player1.GetComponent<AudioSource>().Play)
					.OnComplete(_player1.PlayPrepareAnimation)
			)
			.Join(
				_player1ScoreGo.transform.DOMove(_player1ScorePositionTo, moveObjectsDuration)
					.SetDelay(moveObjectsDelay)
					.OnStart(_player1ScoreGo.GetComponent<AudioSource>().Play)
			).Join(
				_player2ScoreGo.transform.DOMove(_player2ScorePositionTo, moveObjectsDuration)
					.SetDelay(moveObjectsDelay)
					.OnStart(_player2ScoreGo.GetComponent<AudioSource>().Play)
			).AppendCallback(_gameManager.PlayOpeningAnimation);
	}

	private enum EntranceState
	{
		None,
		Opening,
		Game
	}
}
