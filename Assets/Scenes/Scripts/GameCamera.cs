using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
	[Header("Setup")]
	[SerializeField]
	private GameObject _trackerGo;
	[SerializeField]
	private float _enableTrackingAnimationDuration;

	private bool _isTrackingEnabled;

	private void Update()
	{
		if (_isTrackingEnabled)
		{
			transform.LookAt(_trackerGo.transform);
		}
	}

	public void EnableTracking()
	{
		var currentRotation = transform.rotation;

		DOVirtual.Float(0.0F, 1.0F, _enableTrackingAnimationDuration, (fraction) =>
		{
			transform.LookAt(_trackerGo.transform);
			transform.rotation = Quaternion.Lerp(currentRotation, transform.rotation, fraction);
		}).OnComplete(() => _isTrackingEnabled = true);
	}

	public void DisableTracking() => _isTrackingEnabled = false;
}
