using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Client.App;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI
{
	public class LoadingScreen: MonoBehaviour
	{
		[field: SerializeField] public Canvas Canvas { get; private set; }

		[field: SerializeField] public Slider ProgressFill { get; private set; }

		[field: SerializeField] public TextMeshProUGUI LoadingInfo { get; private set; }

		[field: SerializeField] public float BarSpeed { get; private set; } = 2;

		public float TargetProcess { get; private set; }

		[field: SerializeField] public Image FadeOverlay { get; private set; }

		[field: SerializeField] public float FadeDuration { get; private set; } = 1f;

		[field: SerializeField] public float WaitAfterAllLoadComplete { get; private set; } = 1.5f;

		public async UniTask Load(Queue<ILoadingOperation> loadingOperations)
		{
			UnityEngine.Application.backgroundLoadingPriority = ThreadPriority.High;

			Canvas.enabled = true;
			FadeOverlay.gameObject.SetActive(true);

			await FadeInAsync(FadeDuration);

			StartCoroutine(UpdateProgressBar());

			try
			{
				foreach (var operation in loadingOperations)
				{
					ResetFill();
					//LoadingInfo.text = operation.Description;

					await operation.Load(OnProgress);

					await WaitForBarFill(0.15f);
				}
			}
			catch (Exception exception) when (!(exception is OperationCanceledException))
			{
				Console.WriteLine(exception);
			}

			await WaitForLoadComplete(WaitAfterAllLoadComplete);

			await FadeOutAsync(FadeDuration);

			FadeOverlay.gameObject.SetActive(false);
			Canvas.enabled = false;
		}

		private void ResetFill()
		{
			ProgressFill.value = 0;
			TargetProcess = 0;
		}

		private void OnProgress(float progress)
		{
			TargetProcess = progress;
		}

		private async UniTask WaitForBarFill(float value)
		{
			while (ProgressFill.value < TargetProcess)
			{
				await UniTask.Delay(1);
			}

			await UniTask.Delay(TimeSpan.FromSeconds(value));
		}

		private async UniTask WaitForLoadComplete(float value)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(value));
		}

		private IEnumerator UpdateProgressBar()
		{
			while (Canvas.enabled)
			{
				if (ProgressFill.value < TargetProcess)
				{
					ProgressFill.value += Time.deltaTime * BarSpeed;
				}

				yield return null;
			}
		}

		private async UniTask FadeInAsync(float duration)
		{
			FadeOverlay.CrossFadeAlpha(0, duration, true);
			await UniTask.Delay(TimeSpan.FromSeconds(duration), true);
		}

		private async UniTask FadeOutAsync(float duration)
		{
			FadeOverlay.CrossFadeAlpha(1, duration, true);
			await UniTask.Delay(TimeSpan.FromSeconds(duration), true);
		}
	}
}