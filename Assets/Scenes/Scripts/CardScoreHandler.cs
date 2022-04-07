using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CardScoreHandler : MonoBehaviour
{

	[Header("Score Sprite Renderers")]
	[SerializeField]
	private SpriteRenderer _singleScoreSr;
	[SerializeField]
	private SpriteRenderer _doubleScore1Sr;
	[SerializeField]
	private SpriteRenderer _doubleScore2Sr;

	[Header("Score Sprites")]
	[SerializeField]
	private Sprite[] _spriteScores;

	public void UpdateScore(int score)
	{
		var scoreText = score.ToString();

		switch (scoreText.Length)
		{
			case 1:
				{
					if (int.TryParse(scoreText, out int spriteScoreIndex))
					{
						_singleScoreSr.gameObject.SetActive(true);
						_singleScoreSr.sprite = _spriteScores[spriteScoreIndex];
					}
					break;
				}
			case 2:
				{
					var parentGo = _doubleScore1Sr.transform.parent.gameObject;
					parentGo.SetActive(true);

					if (score == 11)
					{
						parentGo.transform.Translate(new(0.1F, 0.0F, 0.0F));
					}

					if (int.TryParse(scoreText[0].ToString(), out int spriteScore1Index))
					{
						_doubleScore1Sr.sprite = _spriteScores[spriteScore1Index];
					}
					if (int.TryParse(scoreText[1].ToString(), out int spriteScore2Index))
					{
						_doubleScore2Sr.sprite = _spriteScores[spriteScore2Index];
					}
					break;
				}
			default:
				return;
		}
	}
}
