using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI
{
	public class StyleView: UIViewBase
	{

		[Header("UI Elements")]
		[SerializeField]
		private TextMeshProUGUI _rankText;

		[SerializeField] private Slider _meterSlider;
		[SerializeField] private TextMeshProUGUI _multiplierText;
		[SerializeField] private TextMeshProUGUI _scoreText;

		[Header("Settings")] 
		[SerializeField] private string _multiplierFormat = "x{0:F2}";
		[SerializeField] private string _scoreFormat = "Score: {0:0}";
		[SerializeField] private Color[] _rankColors;

		private StyleSystem _styleSystem;
		
		protected override void Init()
		{
			base.Init();

			if (_styleSystem == null)
			{
				_styleSystem = ServiceLocator.Get<StyleSystem>();
			}

			UpdateStyleDisplay();
			
		}

		protected override void Subscribe()
		{
			base.Subscribe();

			EventBus.Instance.Subscribe<StyleRankChangedEvent>(OnRankChanged);
			EventBus.Instance.Subscribe<StyleMeterChangedEvent>(OnMeterChanged);
			EventBus.Instance.Subscribe<StyleScoreChangedEvent>(OnScoreChanged);
		}

		protected override void Unsubscribe()
		{
			base.Unsubscribe();

			EventBus.Instance.Unsubscribe<StyleRankChangedEvent>(OnRankChanged);
			EventBus.Instance.Unsubscribe<StyleMeterChangedEvent>(OnMeterChanged);
			EventBus.Instance.Unsubscribe<StyleScoreChangedEvent>(OnScoreChanged);
		}

		private void OnRankChanged(StyleRankChangedEvent rankEvent)
		{
			UpdateRankDisplay(rankEvent.RankName, rankEvent.Multiplier, rankEvent.RankIndex);
		}

		private void OnMeterChanged(StyleMeterChangedEvent meterEvent)
		{
			UpdateMeterDisplay(meterEvent.MeterValue);
		}
		
		private void OnScoreChanged(StyleScoreChangedEvent scoreEvent)
		{
			UpdateScoreDisplay(scoreEvent.TotalScore);
		}

		private void UpdateStyleDisplay()
		{
			if (_styleSystem == null)
				return;

			var rank = _styleSystem.CurrentRank;
			if (rank != null)
			{
				UpdateRankDisplay(rank.RankName, rank.Multiplier, _styleSystem.CurrentRankIndex);
			}

			UpdateMeterDisplay(_styleSystem.CurrentMeter);
			UpdateScoreDisplay(_styleSystem.TotalScore);
		}

		private void UpdateRankDisplay(string rankName, float multiplier, int rankIndex)
		{
			if (_rankText != null)
			{
				_rankText.text = rankName;

				if (_rankColors != null && rankIndex < _rankColors.Length)
				{
					_rankText.color = _rankColors[rankIndex];
					_meterSlider.fillRect.GetComponent<Image>().color = _rankColors[rankIndex];
				}
			}

			if (_multiplierText != null)
			{
				_multiplierText.text = string.Format(_multiplierFormat, multiplier);
			}
		}

		private void UpdateMeterDisplay(float meterValue)
		{
			if (_meterSlider != null)
			{
				_meterSlider.value = meterValue;
			}
		}
		
		private void UpdateScoreDisplay(float totalScore)
		{
			if (_scoreText != null)
			{
				_scoreText.text = string.Format(_scoreFormat, totalScore);
			}
		}
	}
}