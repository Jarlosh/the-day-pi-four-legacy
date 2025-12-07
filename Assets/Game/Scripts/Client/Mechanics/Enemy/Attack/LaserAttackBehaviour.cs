using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Core;
using UnityEngine;

namespace Game.Client
{
	public class LaserAttackBehaviour: MonoBehaviour, IEnemyAttackBehaviour
	{
		[Header("Laser Settings")] 
		[SerializeField] private float _laserDamage = 5f;

		[SerializeField] private float _damageTickInterval = 0.2f;
		[SerializeField] private float _maxLaserDistance = 50f;
		[SerializeField] private LayerMask _playerLayerMask;
		[SerializeField] private LayerMask _obstacleLayerMask;
		[SerializeField] private float _laserWidth = 0.5f; // Ширина лазера для проверки попадания

		[Header("Audio")]
		[SerializeField] private AudioSource _laserAudioSource;
		[SerializeField] private AudioClip _laserLoopClip;
		[SerializeField] private float _laserVolume = 1f;
		
		[Header("Visual - Main Laser")] 
		[SerializeField] private LineRenderer _laserRenderer;

		[SerializeField] private Material _laserMaterial;
		[SerializeField] private Color _laserColor = Color.red;
		[SerializeField] private float _laserWidthVisual = 0.2f;

		[Header("Visual - Core (внутренний яркий слой)")] 
		[SerializeField] private LineRenderer _coreRenderer;

		[SerializeField] private Material _coreMaterial;
		[SerializeField] private Color _coreColor = Color.white;
		[SerializeField] private float _coreWidth = 0.05f;

		[Header("Visual - Glow (свечение)")] 
		[SerializeField] private LineRenderer _glowRenderer;

		[SerializeField] private Material _glowMaterial;
		[SerializeField] private Color _glowColor = new Color(1f, 0.3f, 0.3f, 0.3f);
		[SerializeField] private float _glowWidth = 0.5f;

		[Header("Visual - Particles")] 
		[SerializeField] private ParticleSystem _startParticles;
		[SerializeField] private ParticleSystem _endParticles;
		[SerializeField] private ParticleSystem _impactParticles;

		[Header("Visual - Animation")] 
		[SerializeField] private float _pulseSpeed = 2f;

		[SerializeField] private float _pulseIntensity = 0.2f;
		[SerializeField] private bool _usePulse = true;

		[Header("Spawn Point")] 
		[SerializeField] private Transform _spawnPoint;

		private Enemy _enemy;
		private Transform _target;
		private bool _isAttacking;
		private CancellationTokenSource _laserCts;
		private float _lastDamageTick;
		private float _pulseTimer;
		private Vector3 _currentEndPoint;

		private void Awake()
		{
			SetupLaserRenderers();
			SetupParticles();
			SetupAudio();

			if (_spawnPoint == null)
			{
				_spawnPoint = transform;
			}
		}
		
		private void SetupAudio()
		{
			if (_laserAudioSource == null)
			{
				var go = new GameObject("LaserAudio");
				go.transform.SetParent(transform);
				_laserAudioSource = go.AddComponent<AudioSource>();
			}

			if (_laserAudioSource != null)
			{
				_laserAudioSource.loop = true;
				_laserAudioSource.volume = _laserVolume;
				_laserAudioSource.spatialBlend = 1f;
				_laserAudioSource.playOnAwake = false;
			}
		}


		public void Initialize(Enemy enemy, Transform target)
		{
			_enemy = enemy;
			_target = target;
		}

		public void StartAttack()
		{
			if (_isAttacking)
				return;

			_isAttacking = true;
			_laserCts = new CancellationTokenSource();

			EnableLaser(true);
			StartLaserDamage().Forget();
		}

		public void StopAttack()
		{
			_isAttacking = false;
			EnableLaser(false);

			_laserCts?.Cancel();
			_laserCts?.Dispose();
			_laserCts = null;
		}

		public void UpdateAttack()
		{
			if (!_isAttacking || _enemy == null || _enemy.IsDead)
			{
				StopAttack();
				return;
			}

			UpdateLaserVisual();
		}

		private void SetupLaserRenderers()
		{
			if (_laserRenderer == null)
			{
				var go = new GameObject("LaserMain");
				go.transform.SetParent(transform);
				_laserRenderer = go.AddComponent<LineRenderer>();
			}

			SetupLineRenderer(_laserRenderer, _laserMaterial, _laserColor, _laserWidthVisual);

			if (_coreRenderer == null)
			{
				var go = new GameObject("LaserCore");
				go.transform.SetParent(transform);
				_coreRenderer = go.AddComponent<LineRenderer>();
			}

			SetupLineRenderer(_coreRenderer, _coreMaterial, _coreColor, _coreWidth);

			if (_glowRenderer == null)
			{
				var go = new GameObject("LaserGlow");
				go.transform.SetParent(transform);
				_glowRenderer = go.AddComponent<LineRenderer>();
			}

			SetupLineRenderer(_glowRenderer, _glowMaterial, _glowColor, _glowWidth);
		}

		private void SetupLineRenderer(LineRenderer lr, Material material, Color color, float width)
		{
			if (lr == null)
				return;

			lr.startWidth = width;
			lr.endWidth = width;
			lr.material = material != null ? material : CreateDefaultMaterial(color);
			lr.startColor = color;
			lr.endColor = color;
			lr.positionCount = 2;
			lr.useWorldSpace = true;
			lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			lr.receiveShadows = false;
			lr.enabled = false;

			lr.textureMode = LineTextureMode.Tile;
			lr.alignment = LineAlignment.View;
		}

		private Material CreateDefaultMaterial(Color color)
		{
			var material = new Material(Shader.Find("Unlit/Color"));
			material.color = color;
			return material;
		}

		private void SetupParticles()
		{
			// Частицы в начале - закрепляем к _spawnPoint
			if (_startParticles == null)
			{
				var go = new GameObject("LaserStartParticles");
				go.transform.SetParent(_spawnPoint != null ? _spawnPoint : transform);
				go.transform.localPosition = Vector3.zero;
				go.transform.localRotation = Quaternion.identity;
				_startParticles = go.AddComponent<ParticleSystem>();
				SetupStartParticles(_startParticles);
			}
			else
			{
				// Убеждаемся, что частицы закреплены к _spawnPoint
				_startParticles.transform.SetParent(_spawnPoint != null ? _spawnPoint : transform);
				_startParticles.transform.localPosition = Vector3.zero;
				_startParticles.transform.localRotation = Quaternion.identity;
			}

			// Частицы в конце - создаём как дочерний объект, но позицию обновляем вручную
			if (_endParticles == null)
			{
				var go = new GameObject("LaserEndParticles");
				go.transform.SetParent(transform); // Родитель - сам противник
				_endParticles = go.AddComponent<ParticleSystem>();
				SetupEndParticles(_endParticles);
			}

			// Частицы при попадании
			if (_impactParticles == null)
			{
				var go = new GameObject("LaserImpactParticles");
				go.transform.SetParent(transform);
				_impactParticles = go.AddComponent<ParticleSystem>();
				SetupImpactParticles(_impactParticles);
			}
		}

		private void SetupStartParticles(ParticleSystem ps)
		{
			var main = ps.main;
			main.startLifetime = 0.5f;
			main.startSpeed = 0.5f;
			main.startSize = 0.1f;
			main.startColor = _laserColor;
			main.maxParticles = 20;

			var emission = ps.emission;
			emission.rateOverTime = 30f;

			var shape = ps.shape;
			shape.shapeType = ParticleSystemShapeType.Circle;
			shape.radius = 0.1f;
		}

		private void SetupEndParticles(ParticleSystem ps)
		{
			var main = ps.main;
			main.startLifetime = 0.3f;
			main.startSpeed = 2f;
			main.startSize = 0.15f;
			main.startColor = _laserColor;
			main.maxParticles = 50;

			var emission = ps.emission;
			emission.rateOverTime = 50f;

			var shape = ps.shape;
			shape.shapeType = ParticleSystemShapeType.Circle;
			shape.radius = 0.2f;
		}

		private void SetupImpactParticles(ParticleSystem ps)
		{
			var main = ps.main;
			main.startLifetime = 0.2f;
			main.startSpeed = 3f;
			main.startSize = 0.2f;
			main.startColor = _laserColor;
			main.maxParticles = 100;

			var emission = ps.emission;
			emission.rateOverTime = 0f;
		}

		private void EnableLaser(bool enable)
		{
			if (_laserRenderer != null)
				_laserRenderer.enabled = enable;

			if (_coreRenderer != null)
				_coreRenderer.enabled = enable;

			if (_glowRenderer != null)
				_glowRenderer.enabled = enable;

			if (_startParticles != null)
			{
				if (enable)
				{
					_startParticles.Play();
				}
				else
				{
					_startParticles.Stop();
				}
				
				_startParticles.gameObject.SetActive(enable);
			}

			if (_endParticles != null)
			{
				if (enable)
				{
					_endParticles.Play();
				}
				else
				{
					_endParticles.Stop();
				}
				
				_endParticles.gameObject.SetActive(enable);

			}

			if (_impactParticles != null && !enable)
			{
				_impactParticles.Stop();
				_impactParticles.gameObject.SetActive(false);
			}
			
			if (_laserAudioSource != null && _laserLoopClip != null)
			{
				if (enable)
				{
					_laserAudioSource.clip = _laserLoopClip;
					_laserAudioSource.Play();
				}
				else
				{
					_laserAudioSource.Stop();
				}
			}
		}

		private void UpdateLaserVisual()
		{
			if (_spawnPoint == null)
				return;

			// Направление всегда вперёд от _spawnPoint (обновляется каждый кадр)
			Vector3 startPos = _spawnPoint.position;
			Vector3 direction = _spawnPoint.forward; // Всегда вперёд от текущего направления
			Vector3 endPos;

			// Проверяем препятствия
			if (Physics.Raycast(startPos, direction, out var hit, _maxLaserDistance, _obstacleLayerMask))
			{
				endPos = hit.point;

				if (_impactParticles != null && !_impactParticles.isPlaying)
				{
					_impactParticles.transform.position = endPos;
					_impactParticles.transform.rotation = Quaternion.LookRotation(hit.normal);
					_impactParticles.Play();
				}
			}
			else
			{
				endPos = startPos + direction * _maxLaserDistance;

				if (_impactParticles != null && _impactParticles.isPlaying)
				{
					_impactParticles.Stop();
				}
			}

			_currentEndPoint = endPos;

			if (_usePulse)
			{
				_pulseTimer += Time.deltaTime * _pulseSpeed;
				float pulse = 1f + Mathf.Sin(_pulseTimer) * _pulseIntensity;

				UpdateLaserLine(_laserRenderer, startPos, endPos, _laserWidthVisual * pulse);
				UpdateLaserLine(_coreRenderer, startPos, endPos, _coreWidth * pulse);
				UpdateLaserLine(_glowRenderer, startPos, endPos, _glowWidth * pulse);
			}
			else
			{
				UpdateLaserLine(_laserRenderer, startPos, endPos, _laserWidthVisual);
				UpdateLaserLine(_coreRenderer, startPos, endPos, _coreWidth);
				UpdateLaserLine(_glowRenderer, startPos, endPos, _glowWidth);
			}

			// Частицы в начале автоматически следуют за _spawnPoint (они дочерние)
			// Только обновляем поворот
			if (_startParticles != null)
			{
				_startParticles.transform.rotation = Quaternion.LookRotation(direction);
			}

			// Частицы в конце обновляем вручную
			if (_endParticles != null)
			{
				_endParticles.transform.position = endPos;
				_endParticles.transform.rotation = Quaternion.LookRotation(-direction);
			}
		}

		private void UpdateLaserLine(LineRenderer lr, Vector3 start, Vector3 end, float width)
		{
			if (lr == null)
				return;

			lr.SetPosition(0, start);
			lr.SetPosition(1, end);
			lr.startWidth = width;
			lr.endWidth = width;
		}

		private async UniTaskVoid StartLaserDamage()
		{
			var token = _laserCts.Token;

			try
			{
				while (!token.IsCancellationRequested && _isAttacking)
				{
					if (Time.time >= _lastDamageTick + _damageTickInterval)
					{
						ApplyLaserDamage();
						_lastDamageTick = Time.time;
					}

					await UniTask.Yield(token);
				}
			}
			catch (OperationCanceledException)
			{
				// Отменено
			}
		}

		private void ApplyLaserDamage()
		{
			if (_spawnPoint == null)
				return;

			Vector3 startPos = _spawnPoint.position;
			Vector3 direction = _spawnPoint.forward;
			float distance = _maxLaserDistance;

			if (Physics.Raycast(startPos, direction, out var obstacleHit, distance, _obstacleLayerMask))
			{
				distance = obstacleHit.distance;
			}

			RaycastHit[] hits = Physics.SphereCastAll(
				startPos,
				_laserWidth * 0.5f,
				direction,
				distance,
				_playerLayerMask
			);

			foreach (var hit in hits)
			{
				var health = hit.collider.GetComponent<IHitHandler>();
				if (health != null && hit.transform.gameObject.layer == LayerManager.Player)
				{
					health.TakeDamage(_laserDamage);
					break;
				}
			}
		}
	}
}