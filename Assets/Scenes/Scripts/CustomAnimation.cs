using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

[Serializable]
public class CustomAnimation
{
	public delegate void OnStartedCallback();
	public delegate void OnProgressCallback();
	public delegate void OnEndedCallback();

	public AnimationCurve? AnimationCurve;
	public float Duration;

	public bool IsStarted { get; private set; }
	public bool IsProgress { get; private set; }
	public bool IsEnded { get; private set; }
	public float Fraction { get; private set; }
	public float CurveFraction { get; private set; }
	public float Timer { get; private set; }

	public OnStartedCallback? OnStarted;
	public OnProgressCallback? OnProgress;
	public OnEndedCallback? OnEnded;

	public void Start()
	{
		IsStarted = true;
		IsProgress = false;
		IsEnded = false;
		Timer = 0.0F;

		OnStarted?.Invoke();
	}

	public void Update()
	{
		if (IsStarted)
		{
			var isEnded = false;

			Timer += Time.deltaTime;
			if (Timer >= Duration)
			{
				Timer = Duration;
				isEnded = true;
			}

			Fraction = Mathf.Clamp(Timer / Duration, 0.0F, 1.0F);
			CurveFraction = AnimationCurve?.Evaluate(Fraction) ?? Fraction;
			IsProgress = true;

			OnProgress?.Invoke();

			if (isEnded)
			{
				IsStarted = false;
				IsProgress = false;
				IsEnded = true;
				Timer = 0.0F;

				OnEnded?.Invoke();
			}
		}
	}
}
