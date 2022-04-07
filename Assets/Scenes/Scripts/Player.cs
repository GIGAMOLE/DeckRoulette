using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
	public delegate void OnAddScoreAnimationEnded();
	public delegate void OnAngryEmoteTriggered();

	[Header("Game Objects")]
	[SerializeField]
	private TextMeshPro _scoreText;
	[SerializeField]
	private SpriteRenderer _scoreBgSr;
	[SerializeField]
	private GameObject _scoreStateGo;
	[SerializeField]
	private AudioClip _preparedAc;

	[Header("Emotes")]
	[SerializeField]
	private ParticleSystem _angryParticleSystem;
	[SerializeField]
	private ParticleSystem _naniParticleSystem;
	[SerializeField]
	private ParticleSystem _cryParticleSystem;
	[SerializeField]
	private ParticleSystem _shitParticleSystem;
	[SerializeField]
	private ParticleSystem _lolParticleSystem;
	[SerializeField]
	private ParticleSystem _mehParticleSystem;
	[SerializeField]
	private ParticleSystem _starsParticleSystem;
	[SerializeField]
	private ParticleSystem _energyParticleSystem;
	[SerializeField]
	private ParticleSystem _evilParticleSystem;
	[SerializeField]
	private ParticleSystem _waveParticleSystem;

	[Header("Score State Animation")]
	[SerializeField]
	private float _scoreStateActiveAnimationScale;
	[SerializeField]
	private float _scoreStateAnimationAlpha;
	[SerializeField]
	private float _scoreStateAnimationDuration;

	[Header("Score State Winner Animation")]
	[SerializeField]
	private float _scoreStateWinnerAnimationDuration;
	[SerializeField]
	private float _scoreStateWinnerAnimationHeight;
	[SerializeField]
	private float _scoreStateWinnerAnimationRandomRotation;

	[Header("Score Text Animation")]
	[SerializeField]
	private float _scoreTextAnimationCharDuration;
	[SerializeField]
	private float _scoreTextAnimationCharDelay;
	[SerializeField]
	private float _scoreTextAnimationCharOffset;

	private int _score;
	private int _lastCardScore;
	private int _lastOtherPlayerCardScore;
	private bool _isRevertEmote;
	private ScoreState _scoreState;
	private DOTweenTMPAnimator _scoreTextAnimation;
	private GameObject _scoreStateCenterChildGo;
	private AudioSource _scoreAs;
	private Tween _scoreStateWinnerAnimationLoopMove;
	private Tween _scoreStateWinnerAnimationLoopRotate;
	private Animator _playerAnimator;
	private AudioSource _as;


	private AudioSource _angryAs;
	private AudioSource _naniAs;
	private AudioSource _cryAs;
	private AudioSource _shitAs;
	private AudioSource _lolAs;
	private AudioSource _mehAs;
	private AudioSource _starsAs;
	private AudioSource _energyAs;
	private AudioSource _evilAs;
	private AudioSource _waveAs;

#nullable enable
	public OnAngryEmoteTriggered? OnAngryEmoteTriggeredCallback;

	private void Awake()
	{
		_scoreTextAnimation = new(_scoreText);
		_scoreStateCenterChildGo = _scoreStateGo.transform.GetChild(0).gameObject;
		_scoreAs = _scoreText.GetComponent<AudioSource>();
		_playerAnimator = GetComponent<Animator>();
		_as = GetComponent<AudioSource>();

		_angryAs = _angryParticleSystem.GetComponent<AudioSource>();
		_naniAs = _naniParticleSystem.GetComponent<AudioSource>();
		_cryAs = _cryParticleSystem.GetComponent<AudioSource>();
		_shitAs = _shitParticleSystem.GetComponent<AudioSource>();
		_lolAs = _lolParticleSystem.GetComponent<AudioSource>();
		_mehAs = _mehParticleSystem.GetComponent<AudioSource>();
		_starsAs = _starsParticleSystem.GetComponent<AudioSource>();
		_energyAs = _energyParticleSystem.GetComponent<AudioSource>();
		_evilAs = _evilParticleSystem.GetComponent<AudioSource>();
		_waveAs = _waveParticleSystem.GetComponent<AudioSource>();
	}

	private void Start()
	{
		AddScore(0);
		SetScoreState(Player.ScoreState.Default);
	}

#nullable enable
	public void AddScore(int score, OnAddScoreAnimationEnded? onAddScoreAnimationEnded = null)
	{
		_lastCardScore = score;
		_score += score;
		_scoreText.text = _score.ToString();

		if (onAddScoreAnimationEnded == null)
		{
			return;
		}

		_scoreTextAnimation.Refresh();
		var sequence = DOTween.Sequence();

		for (int i = 0; i < _scoreTextAnimation.textInfo.characterCount; i++)
		{
			if (_scoreTextAnimation.textInfo.characterInfo[i].isVisible)
			{
				sequence.Join(
					_scoreTextAnimation.DOPunchCharScale(
						i,
						_scoreTextAnimationCharOffset,
						_scoreTextAnimationCharDuration,
						2,
						1
					).SetDelay(_scoreTextAnimationCharDelay * i)
					.OnStart(() => _scoreAs.Play())
				);
			}
		}

		sequence.AppendCallback(() => onAddScoreAnimationEnded?.Invoke());
	}

	public int Score() => _score;

	public int LastCardScore() => _lastCardScore;

	public void SetScoreState(ScoreState scoreState)
	{
		_scoreState = scoreState;

		switch (scoreState)
		{
			case ScoreState.Default:
				PlayScoreStateDefaultAnimation();
				break;
			case ScoreState.Active:
				PlayScoreStateActiveAnimation();
				break;
			case ScoreState.Unactive:
				PlayScoreStateUnactiveAnimation();
				break;
			default:
				break;
		}
	}

	public void EnterWinnerScoreState()
	{
		_scoreState = ScoreState.Winner;

		PlayScoreStateWinnerAnimation();
	}

	public void ExitWinnerScoreState()
	{
		var shortScoreStateWinnerAnimationDuration = _scoreStateWinnerAnimationDuration / 3;

		_scoreText.DOCounter(_score, 0, shortScoreStateWinnerAnimationDuration, false)
			.OnComplete(() =>
			{
				_lastCardScore = 0;
				_lastOtherPlayerCardScore = 0;
				_score = 0;
			});

		if (_scoreStateWinnerAnimationLoopMove is null && _scoreStateWinnerAnimationLoopRotate is null)
		{
			SetScoreState(ScoreState.Default);
			return;
		}

		_scoreStateWinnerAnimationLoopMove.Kill();
		_scoreStateWinnerAnimationLoopRotate.Kill();

		DOTween.Sequence()
			.Append(
				_scoreStateCenterChildGo.transform.DOMoveY(
					_scoreStateGo.transform.position.y, shortScoreStateWinnerAnimationDuration
				)
			).Join(
				_scoreStateCenterChildGo.transform.DORotate(
					Vector3.zero, shortScoreStateWinnerAnimationDuration / 2
				)
			).OnComplete(() => SetScoreState(ScoreState.Default));
	}

	private Sequence PlayScoreStateActiveAnimation() => DOTween.Sequence()
			.Append(
				_scoreStateGo.transform.DOScale(
					new Vector3(
						_scoreStateActiveAnimationScale,
						_scoreStateActiveAnimationScale,
						_scoreStateActiveAnimationScale
					),
					_scoreStateAnimationDuration
				).SetEase(Ease.OutBack)
			).Join(
				_scoreText.DOFade(1.0F, _scoreStateAnimationDuration)
			).Join(
				_scoreBgSr.DOFade(1.0F, _scoreStateAnimationDuration)
			);

	private void PlayScoreStateUnactiveAnimation()
	{
		DOTween.Sequence()
			.Append(
				_scoreStateGo.transform.DOScale(Vector3.one, _scoreStateAnimationDuration)
					.SetEase(Ease.OutBack)
			).Join(
				_scoreText.DOFade(_scoreStateAnimationAlpha, _scoreStateAnimationDuration)
			).Join(
				_scoreBgSr.DOFade(_scoreStateAnimationAlpha, _scoreStateAnimationDuration)
			);
	}

	private void PlayScoreStateDefaultAnimation()
	{
		DOTween.Sequence()
			.Append(
				_scoreStateGo.transform.DOScale(Vector3.one, _scoreStateAnimationDuration)
					.SetEase(Ease.OutBack)
			).Join(
				_scoreText.DOFade(1.0F, _scoreStateAnimationDuration)
			).Join(
				_scoreBgSr.DOFade(1.0F, _scoreStateAnimationDuration)
			);
	}

	private void PlayScoreStateWinnerAnimation()
	{
		var shortScoreStateWinnerAnimationDuration = _scoreStateWinnerAnimationDuration / 3;
		var shortScoreStateWinnerAnimationHeight = _scoreStateWinnerAnimationHeight / 3;

		DOTween.Sequence()
			.Append(
				_scoreStateCenterChildGo.transform.DOMoveY(
					_scoreStateWinnerAnimationHeight, shortScoreStateWinnerAnimationDuration
				).SetEase(Ease.OutBack)
			)
			.Join(PlayScoreStateActiveAnimation())
			.OnComplete(() =>
			{
				_scoreStateWinnerAnimationLoopMove = _scoreStateCenterChildGo.transform.DOMoveY(
					-shortScoreStateWinnerAnimationHeight, _scoreStateWinnerAnimationDuration
				).SetRelative(true)
				.SetEase(Ease.InOutSine)
				.SetLoops(-1, LoopType.Yoyo);

				var shortScoreStateWinnerAnimationRandomRotation = _scoreStateWinnerAnimationRandomRotation / 2;

				float MinRandomRotation()
				{
					var randomRotation = shortScoreStateWinnerAnimationRandomRotation +
						Random.Range(0, shortScoreStateWinnerAnimationRandomRotation);

					return Random.Range(0, 2) switch
					{
						0 => -randomRotation,
						1 => randomRotation,
						_ => throw new System.NotImplementedException(),
					};
				}

				void Rotate()
				{
					_scoreStateWinnerAnimationLoopRotate = _scoreStateCenterChildGo.transform.DOLocalRotate(
						new(MinRandomRotation(), MinRandomRotation(), MinRandomRotation()),
						_scoreStateWinnerAnimationDuration * 2
					).SetEase(Ease.InOutCubic)
					.OnComplete(() => Rotate());
				}

				Rotate();
			});
	}

	public void PlayPrepareAnimation()
	{
		_as.volume = 0.15F;
		_as.PlayOneShot(_preparedAc);

		int prepareStateType = Random.Range(1, 3);

		_playerAnimator.SetInteger("PrepareStateType", prepareStateType);
		_playerAnimator.SetTrigger("PlayPrepare");
	}

	public void SetPrepared() => _playerAnimator.SetBool("IsPrepared", true);

	public void PlayDrawCardEnterAnimation()
	{
		_playerAnimator.SetBool("IsDrawn", false);
		_playerAnimator.SetTrigger("PlayDraw");
	}

	public void PlayDrawCardExitAnimation() => _playerAnimator.SetBool("IsDrawn", true);

	public void PlayEmote()
	{
		var playEmote = Random.value > 0.5F;
		if (playEmote)
		{
			_isRevertEmote = false;
			_lastOtherPlayerCardScore = 0;

			_playerAnimator.SetTrigger("PlayEmote");
		}
	}

	public async void PlayRevertEmote(int otherPlayerLastCardScore)
	{
		var playEmote = Random.value > 0.5F;
		if (playEmote)
		{
			await Task.Delay(250);

			_isRevertEmote = true;
			_lastOtherPlayerCardScore = otherPlayerLastCardScore;

			_playerAnimator.SetTrigger("PlayEmote");
		}
	}

	public void PlayEmotePs()
	{
		var randomType = Random.value > 0.5F;
		int score = _isRevertEmote switch
		{
			true => getRevertScore(),
			_ => _lastCardScore
		};

		switch (score)
		{
			case <= 3:
				{
					if (randomType)
					{
						_naniAs.Play();
						_naniParticleSystem.Play();
					}
					else
					{
						_angryAs.Play();
						_angryParticleSystem.Play();
					}

					OnAngryEmoteTriggeredCallback?.Invoke();
				}
				break;
			case <= 6:
				{
					if (randomType)
					{
						_shitAs.Play();
						_shitParticleSystem.Play();
					}
					else
					{
						_cryAs.Play();
						_cryParticleSystem.Play();
					}
				}
				break;
			case <= 9:
				{
					if (randomType)
					{
						_lolAs.Play();
						_lolParticleSystem.Play();
					}
					else
					{
						_mehAs.Play();
						_mehParticleSystem.Play();
					}
				}
				break;
			case <= 12:
				{
					if (randomType)
					{
						_starsAs.Play();
						_starsParticleSystem.Play();
					}
					else
					{
						_energyAs.Play();
						_energyParticleSystem.Play();
					}
				}
				break;
			case <= 15:
				{
					if (randomType)
					{
						_evilAs.Play();
						_evilParticleSystem.Play();
					}
					else
					{
						_waveAs.Play();
						_waveParticleSystem.Play();
					}
				}
				break;
			default:
				break;
		}

		_isRevertEmote = false;
		_lastOtherPlayerCardScore = 0;
	}

	private int getRevertScore() => _lastOtherPlayerCardScore switch
	{
		<= 3 => 15,
		<= 6 => 12,
		<= 9 => 9,
		<= 12 => 6,
		<= 15 => 3,
		_ => throw new System.NotImplementedException($"{_lastOtherPlayerCardScore}"),
	};

	public enum ScoreState
	{
		Default,
		Active,
		Unactive,
		Winner
	}
}
