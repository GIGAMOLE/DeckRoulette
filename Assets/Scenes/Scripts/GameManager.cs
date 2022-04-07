using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/**
* - Provide sounds/effect for all events.
*/
public class GameManager : MonoBehaviour
{
	public delegate void OnSetTurnStateAnimationEnded();

	[Header("Game Objects")]
	[SerializeField]
	private DeckManager _deckManager;
	[SerializeField]
	private Player _player1;
	[SerializeField]
	private Player _player2;
	[SerializeField]
	private HitSpace _hitSpace;
	[SerializeField]
	private WinnerMenu _winnerMenu;
	[SerializeField]
	private AudioSource _angryEmoteAs;

	[Header("Shake Angry")]
	[SerializeField]
	private GameCamera _camera;
	[SerializeField]
	private GameObject _lightHolder;
	[SerializeField]
	private float _shakeCameraDuration;
	[SerializeField]
	private float _shakeCameraStrength;
	[SerializeField]
	private float _shakeLightDelay;
	[SerializeField]
	private float _shakeLightDuration;
	[SerializeField]
	private float _shakeLightStrength;

	private TurnState _turnState;
	private bool _isDrawing;
	private bool _isTheEnd;

	private void Awake()
	{
		_turnState = TurnState.None;

		_player1.OnAngryEmoteTriggeredCallback = OnAngryEmoteTriggered;
		_player2.OnAngryEmoteTriggeredCallback = OnAngryEmoteTriggered;
	}

	private void OnEnable()
	{
		_winnerMenu.OnRestartEvent += OnRestartTriggered;
	}

	private void OnDisable()
	{
		_winnerMenu.OnRestartEvent -= OnRestartTriggered;
	}

	public void PlayOpeningAnimation() => _deckManager.PlayOpeningAnimation(OnOpeningAnimationEnded);

	private void OnOpeningAnimationEnded() => SetTurnState(TurnState.Player1);

	private void OnRestartTriggered() => Restart();

	private void Update()
	{
		if (_isTheEnd)
		{
			return;
		}
		if (_isDrawing)
		{
			return;
		}

		if (_turnState == TurnState.Player1 && Input.GetKeyDown(KeyCode.Space))
		{
			_isDrawing = true;
			_deckManager.DrawCard(OnCardDrawn);
		}
	}

	private void OnCardDrawn(Card card, bool isLast)
	{
		if (isLast)
		{
			_isTheEnd = true;
		}

		switch (_turnState)
		{
			case TurnState.Player1:
				{
					_player1.AddScore(card.Score(), OnPlayer1ScoreAdded);

					_player1.PlayEmote();
					_player2.PlayRevertEmote(_player1.LastCardScore());
				}
				break;
			case TurnState.Player2:
				{
					_player2.AddScore(card.Score(), OnPlayer2ScoreAdded);

					_player2.PlayEmote();
					_player1.PlayRevertEmote(_player2.LastCardScore());
				}
				break;
			default:
				break;
		}
	}

	private void OnPlayer1ScoreAdded()
	{
		if (_isTheEnd)
		{
			OnTheEnd();
			return;
		}

		SetTurnState(
			TurnState.Player2,
			() =>
			{
				_isDrawing = true;

				_deckManager.DrawCard(OnCardDrawn, true);
			}
		);
	}

	private void OnPlayer2ScoreAdded()
	{
		if (_isTheEnd)
		{
			OnTheEnd();
			return;
		}

		_isDrawing = false;

		SetTurnState(TurnState.Player1);
	}

#nullable enable
	private void SetTurnState(
		TurnState turnState,
		OnSetTurnStateAnimationEnded? onSetTurnStateAnimationEnded = null
	)
	{
		void OnSetDeckStateAnimationEnded()
		{
			_turnState = turnState;
			onSetTurnStateAnimationEnded?.Invoke();
		}

		switch (turnState)
		{
			case TurnState.None:
				_player1.SetScoreState(Player.ScoreState.Default);
				_player2.SetScoreState(Player.ScoreState.Default);

				_deckManager.SetDeckState(DeckManager.DeckState.None, OnSetDeckStateAnimationEnded);
				break;
			case TurnState.Player1:
				_player1.SetScoreState(Player.ScoreState.Active);
				_player2.SetScoreState(Player.ScoreState.Unactive);

				_deckManager.SetDeckState(DeckManager.DeckState.Player1, OnSetDeckStateAnimationEnded);
				break;
			case TurnState.Player2:
				_player1.SetScoreState(Player.ScoreState.Unactive);
				_player2.SetScoreState(Player.ScoreState.Active);

				_deckManager.SetDeckState(DeckManager.DeckState.Player2, OnSetDeckStateAnimationEnded);
				break;
			default:
				break;
		}

		if (turnState == TurnState.Player1)
		{
			_player1.PlayDrawCardEnterAnimation();
			_hitSpace.PlayOpeningAnimation();
		}
		else
		{
			_player1.PlayDrawCardExitAnimation();
			_hitSpace.PlayClosingAnimation();
		}
	}

	private void OnTheEnd()
	{
		int player1Score = _player1.Score();
		int player2Score = _player2.Score();

		if (player1Score == player2Score)
		{
			OnDraw();
		}
		else if (player1Score > player2Score)
		{
			OnPlayer1Win();
		}
		else
		{
			OnPlayer2Win();
		}

		void OnClosingAnimationEnded() => _winnerMenu.PlayMenuAnimation();

		_deckManager.PlayClosingAnimation(OnClosingAnimationEnded);
		_deckManager.SetDeckState(DeckManager.DeckState.None);
	}

	private void OnPlayer1Win()
	{
		_player1.EnterWinnerScoreState();
		_player2.SetScoreState(Player.ScoreState.Unactive);

		_winnerMenu.PlayWinnerAnimation(WinnerMenu.WinnerState.Player);
	}

	private void OnPlayer2Win()
	{
		_player2.EnterWinnerScoreState();
		_player1.SetScoreState(Player.ScoreState.Unactive);

		_winnerMenu.PlayWinnerAnimation(WinnerMenu.WinnerState.CPU);
	}

	private void OnDraw()
	{
		_player1.EnterWinnerScoreState();
		_player2.EnterWinnerScoreState();

		_winnerMenu.PlayWinnerAnimation(WinnerMenu.WinnerState.Draw);
	}

	private void Restart()
	{
		_player1.ExitWinnerScoreState();
		_player2.ExitWinnerScoreState();

		_turnState = TurnState.None;
		_isTheEnd = false;
		_isDrawing = false;

		PlayOpeningAnimation();
	}

	private async void OnAngryEmoteTriggered()
	{
		await Task.Delay(100);

		_angryEmoteAs.Play();

		DOTween.Sequence()
			.Append(
				_camera.gameObject.transform.DOShakePosition(
					_shakeCameraDuration,
					_shakeCameraStrength,
					6
				).OnStart(_camera.DisableTracking)
				.OnComplete(_camera.EnableTracking)
			).Join(
				_lightHolder.transform.DOShakePosition(
					_shakeLightDuration,
					_shakeLightStrength,
					6
				).SetDelay(_shakeLightDelay)
			);
	}

	public enum TurnState
	{
		None,
		Player1,
		Player2
	}
}
