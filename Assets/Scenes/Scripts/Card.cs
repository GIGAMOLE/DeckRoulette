using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{

	public delegate void OnOpeningAnimationEnded();
	public delegate void OnDrawAnimationEnded();

	[Header("Audios")]
	[SerializeField]
	private AudioSource _flipAs;
	[SerializeField]
	private AudioSource _splatAs;

	[Header("Particles")]
	[SerializeField]
	private ParticleSystem _splashParticleSystem;
	[SerializeField]
	private ParticleSystem _splatParticleSystem;

	[Header("Opening Animation")]
	[SerializeField]
	private float _openingAnimationDuration;
	[SerializeField]
	private float _openingAnimationScale;
	[SerializeField]
	private float _openingStackAnimationDuration;

	[SerializeField]
	private float _openingStackAnimationScale;

	[Header("Draw Animation")]
	[SerializeField]
	private float _drawAnimationDuration;
	[SerializeField]
	private float _drawAnimationDelay;
	[SerializeField]
	private float _drawAnimationYHeight;
	[SerializeField]
	private float _drawAnimationRandomPositionRange;
	[SerializeField]
	private float _drawAnimationRandomRotationYRange;

	[Header("Life Bounds")]
	[SerializeField]
	private float _lifeBoundsY;

	private int _score;
	private BoxCollider _bc;
	private Rigidbody _rb;
	private CardScoreHandler _cardScoreHanler;
	private bool _isOpeningAnimationFired;
	private bool _isFlyGravityDefined;
	private AudioSource _stackAs;

	public readonly Quaternion FlipRotation = Quaternion.Euler(0, 0, 180.0F);
	public float Height { get; private set; }

	private void Awake()
	{
		_bc = GetComponent<BoxCollider>();
		_rb = GetComponent<Rigidbody>();
		_cardScoreHanler = GetComponent<CardScoreHandler>();
		_stackAs = GetComponent<AudioSource>();

		Height = _bc.bounds.size.y;
	}

#nullable enable
	public void PlayOpeningAnimation(
		float openingAnimationHeight,
		OnOpeningAnimationEnded? onOpeningAnimationEnded
	)
	{
		if (_isOpeningAnimationFired)
		{
			return;
		}

		_isOpeningAnimationFired = true;

		DOTween.Sequence()
			.Append(
				transform.DOMoveY(-openingAnimationHeight, _openingAnimationDuration)
					.SetEase(Ease.InExpo)
					.SetRelative(true)
			).Join(
				transform.DOScale(
						new Vector3(_openingAnimationScale, _openingAnimationScale, _openingAnimationScale),
						_openingAnimationDuration
					).From()
			).AppendCallback(_stackAs.Play)
			.Append(
				transform.DOPunchScale(
						new Vector3(_openingStackAnimationScale, _openingStackAnimationScale, _openingStackAnimationScale),
						_openingStackAnimationDuration,
						4,
						1
					).SetRelative(true)
			).AppendCallback(() => onOpeningAnimationEnded?.Invoke());
	}

	public void SetScore(int score)
	{
		_score = score;
		_cardScoreHanler.UpdateScore(score);
	}

	public int Score() => _score;

	public void StartDrawAnimation(
		Vector3 drawToPosition,
		bool useDelay,
		OnDrawAnimationEnded onDrawAnimationEnded
	)
	{
		DOTween.Sequence()
			.AppendInterval(useDelay ? _drawAnimationDelay : 0.0F)
			.AppendCallback(_flipAs.Play)
			.Append(
				transform.DOJump(
					drawToPosition + new Vector3(
						Random.Range(-_drawAnimationRandomPositionRange, _drawAnimationRandomPositionRange),
						0.0F,
						Random.Range(-_drawAnimationRandomPositionRange, _drawAnimationRandomPositionRange)
					),
					_drawAnimationYHeight,
					1,
					_drawAnimationDuration
				).SetEase(Ease.InQuart)
			).Join(
				transform.DOLocalRotateQuaternion(
					Quaternion.Euler(
						0.0F,
						Random.Range(-_drawAnimationRandomRotationYRange, _drawAnimationRandomRotationYRange),
						-0.0F
					),
					_drawAnimationDuration
				).SetEase(Ease.InQuart)
			).AppendCallback(onDrawAnimationEnded.Invoke)
			.AppendCallback(OnDrawAnimationStack)
			.Append(
				transform.DOPunchScale(
						new Vector3(_openingStackAnimationScale, _openingStackAnimationScale, _openingStackAnimationScale) * 2F,
						_openingStackAnimationDuration * 1.5F,
						5,
						1
					).SetRelative(true)
			);
	}

	private void OnDrawAnimationStack()
	{
		_splatAs.Play();

		_splashParticleSystem.Play();
		_splatParticleSystem.Play();
	}

	public void DisableRbKinematic()
	{
		_rb.mass = 0.000001F;
		_rb.useGravity = false;
		_rb.isKinematic = false;
	}

	public bool IsFlyGravityDefined() => _isFlyGravityDefined;

	public void DefineFlyGravity(Vector3 explosionPosition, float explosionForce)
	{
		if (_isFlyGravityDefined)
		{
			return;
		}

		_isFlyGravityDefined = true;

		_rb.mass = 0.01F;
		_rb.useGravity = false;
		_rb.isKinematic = false;

		_rb.AddExplosionForce(explosionForce, explosionPosition, explosionForce);
	}

	private void Update()
	{
		if (!_isFlyGravityDefined)
		{
			return;
		}
		if (transform.position.y >= _lifeBoundsY)
		{
			Destroy(gameObject);
		}
	}
}
