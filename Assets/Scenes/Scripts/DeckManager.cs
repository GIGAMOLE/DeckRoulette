using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
	public delegate void OnOpeningAnimationEnded();
	public delegate void OnSetDeckStateAnimationEnded();
	public delegate void OnClosingAnimationEnded();
	public delegate void OnCardDrawn(Card card, bool isLast);

	[Header("Game Object")]
	[SerializeField]
	private GameObject _cardPf;
	[SerializeField]
	private GameObject _zoneBgGo;
	[SerializeField]
	private GameObject _pileGo;
	[SerializeField]
	private DeckCounter _deckCounter;

	[Header("Deck Setup")]
	[SerializeField]
	private int _cardScoreRangeFrom;
	[SerializeField]
	private int _cardScoreRangeTo;
	[SerializeField]
	private int _cardDuplicates;

	[Header("Opeing Animation")]
	[SerializeField]
	private float _openingAnimationDuration;
	[SerializeField]
	private float _openingAnimationHeight;
	[SerializeField]
	private float _cardRandomPositionRange;
	[SerializeField]
	private float _cardRandomRotationYRange;
	[SerializeField]
	private float _cardStackOffset;

	[Header("Deck State Animation")]
	[SerializeField]
	private float _deckStateAnimationDuration;
	[SerializeField]
	private float _deckStateAnimationNoneZ;
	[SerializeField]
	private float _deckStateAnimationPlayer1Z;
	[SerializeField]
	private float _deckStateAnimationPlayer2Z;

	[Header("Closing Animation")]
	[SerializeField]
	private float _closingAnimationDuration;
	[SerializeField]
	private float _closingAnimationRandomPositionRange;
	[SerializeField]
	private float _closingAnimationExplosionForce;

	private readonly List<Card> _cards = new();
	private readonly List<Card> _drawnCards = new();
	private int _openingAnimationPrevCardIndex;
	private int _closingAnimationPrevCardIndex;
	private float _pileZoneBgHeight;
	private DeckState _deckState;

	private void Awake()
	{
		_pileZoneBgHeight = _pileGo.GetComponent<BoxCollider>().bounds.size.y;
	}

	private void CreateDeck()
	{
		GenerateDeck();
		ShuffleDeck();
		PositionDeck();
	}

	private void GenerateDeck()
	{
		_cards.Clear();
		_drawnCards.Clear();

		foreach (var cardScore in Enumerable.Range(_cardScoreRangeFrom, _cardScoreRangeTo - _cardScoreRangeFrom))
		{
			foreach (var _ in Enumerable.Range(0, _cardDuplicates))
			{
				var cardGo = Instantiate(
					_cardPf,
					transform.position,
					_cardPf.transform.rotation,
					transform
				);
				var card = cardGo.GetComponent<Card>();

				card.SetScore(cardScore);

				_cards.Add(card);
			}
		}

		_deckCounter.SetupMaxCount(_cards.Count());
	}

	private void ShuffleDeck()
	{
		var cards = new List<Card>(_cards);

		_cards.Clear();
		_cards.AddRange(cards.OrderBy(_ => System.Guid.NewGuid()).ToList());
	}

	private void PositionDeck()
	{
		var zoneBgHeight = _zoneBgGo.GetComponent<BoxCollider>().bounds.size.y;

		for (int index = 0; index < _cards.Count; index++)
		{
			var card = _cards[index];
			// Random rotation.
			card.transform.rotation *= card.FlipRotation;
			card.transform.rotation *= Quaternion.Euler(
				0.0F,
				Random.Range(-_cardRandomRotationYRange, _cardRandomRotationYRange),
				0.0F
			);

			// Stacking position.
			var currentCardPosition = card.transform.position;
			card.transform.position = new(
				currentCardPosition.x + Random.Range(-_cardRandomPositionRange, _cardRandomPositionRange),
				zoneBgHeight + (card.Height + _cardStackOffset) + index * card.Height + _openingAnimationHeight,
				currentCardPosition.z + Random.Range(-_cardRandomPositionRange, _cardRandomPositionRange)
			);
		}
	}

	public void PlayOpeningAnimation(OnOpeningAnimationEnded onOpeningAnimationEnded)
	{
		CreateDeck();

		void OnOpeningAnimationEnded()
		{
			_deckCounter.PlayOpeningAnimation();

			onOpeningAnimationEnded();
		}

		_openingAnimationPrevCardIndex = 0;

		int maxIndex = _cards.Count() - 1;
		DOVirtual.Int(
			0,
			maxIndex,
			_openingAnimationDuration,
			(index) =>
			{
				for (var i = _openingAnimationPrevCardIndex; i <= index; i++)
				{
					_cards[i].PlayOpeningAnimation(
						_openingAnimationHeight,
						i == maxIndex ? OnOpeningAnimationEnded : null
					);
				}

				_openingAnimationPrevCardIndex = index;
			}
		).SetEase(Ease.InCirc);
	}

	public void DrawCard(
		OnCardDrawn onCardDrawn,
		bool isCpuDraw = false
	)
	{
		var card = _cards.Last();
		var drawPileHeight = _drawnCards.Count * (card.Height + _cardStackOffset) + card.Height + _pileZoneBgHeight;
		var drawPilePosition = _pileGo.transform.position;

		_cards.Remove(card);
		_drawnCards.Add(card);

		void onDrawAnimationEnded()
		{
			card.transform.parent = _pileGo.transform.parent;
			onCardDrawn(card, _cards.Count == 0);
		}

		card.StartDrawAnimation(
			new(
				drawPilePosition.x,
				drawPileHeight,
				drawPilePosition.z
			),
			isCpuDraw,
			onDrawAnimationEnded
		);

		_deckCounter.UpdateCount(_cards.Count());
	}

#nullable enable
	public void SetDeckState(
		DeckState deckState,
		OnSetDeckStateAnimationEnded? onSetDeckStateAnimationEnded = null
	)
	{
		_deckState = deckState;

		var deckStateZ = deckState switch
		{
			DeckState.None => _deckStateAnimationNoneZ,
			DeckState.Player1 => _deckStateAnimationPlayer1Z,
			DeckState.Player2 => _deckStateAnimationPlayer2Z,
			_ => throw new System.NotImplementedException(),
		};

		transform.DOMoveZ(deckStateZ, _deckStateAnimationDuration)
			.SetEase(Ease.InOutBack)
			.OnComplete(() => onSetDeckStateAnimationEnded?.Invoke());
	}

	public void PlayClosingAnimation(OnClosingAnimationEnded onClosingAnimationEnded)
	{
		var maxIndex = _drawnCards.Count - 1;

		_closingAnimationPrevCardIndex = maxIndex;

		DOVirtual.Int(
			0,
			maxIndex,
			_closingAnimationDuration,
			(index) =>
			{
				for (var i = _closingAnimationPrevCardIndex; i <= index; i++)
				{
					int revertIndex = maxIndex - i;
					var drawnCard = _drawnCards[revertIndex];

					if (!drawnCard.IsFlyGravityDefined())
					{
						var randomPosition = new Vector3(
							Random.Range(_closingAnimationRandomPositionRange, _closingAnimationRandomPositionRange),
							0.0F,
							Random.Range(-_closingAnimationRandomPositionRange, _closingAnimationRandomPositionRange)
						);

						drawnCard.DefineFlyGravity(_pileGo.transform.position, _closingAnimationExplosionForce);
					}
				}

				_closingAnimationPrevCardIndex = index;
			}
		).SetEase(Ease.InQuad)
		.SetDelay(0.25F)
		.OnComplete(() =>
		{
			_cards.Clear();
			_drawnCards.Clear();

			onClosingAnimationEnded?.Invoke();
		});

		_deckCounter.PlayClosingAnimation();
	}

	public enum DeckState
	{
		None,
		Player1,
		Player2
	}
}
