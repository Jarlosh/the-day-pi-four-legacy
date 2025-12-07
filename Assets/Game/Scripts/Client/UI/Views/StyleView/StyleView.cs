using Game.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI
{
	public class StyleView: UIViewBase
	{
		[System.Serializable]
		public class Stage
		{
			[Header("UI Settings")]
			public Color uiColor = Color.white;         // цвет основного изображения
			public Sprite backgroundSprite;             // фон

			[Header("UI Particle Settings")]
			public Color particleColor = Color.white;   // цвет UI-частиц
			public float particleSpeed = 200f;          // скорость движения частиц
			public float particleRate = 10f;            // частота спавна
		}

		[Header("References")]
		public Image mainImage;                 // основное изображение
		public Image backgroundImage;           // фоновое изображение
		public UISimpleEmitter uiEmitter;       // наш UI-эмиттер

		[Header("Stages")]
		public Stage[] stages;

		private int currentStage = -1;
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
					SetStage(rankIndex);
					
					_meterSlider.fillRect.GetComponent<Image>().color = _rankColors[rankIndex];
				}
			}

			if (_multiplierText != null)
			{
				_multiplierText.text = string.Format(_multiplierFormat, multiplier);
			}
		}
		
		public void SetStage(int index)
		{
			if (index < 0 || index >= stages.Length)
				return;

			currentStage = index;
			Stage stage = stages[index];

			// Главный UI элемент
			if (mainImage != null)
			{
				Color safeColor = stage.uiColor;

				// защита от невидимой альфы
				if (safeColor.a <= 0.01f)
					safeColor.a = 1f;

				mainImage.color = safeColor;
			}

			// Фон
			if (backgroundImage != null)
				backgroundImage.sprite = stage.backgroundSprite;

			// Эмиттер UI-частиц
			if (uiEmitter != null)
			{
				uiEmitter.particleColor = stage.particleColor;
				uiEmitter.speed = stage.particleSpeed;
				uiEmitter.spawnRate = stage.particleRate;
			}

			Debug.Log("Stage switched to: " + (index + 1));
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