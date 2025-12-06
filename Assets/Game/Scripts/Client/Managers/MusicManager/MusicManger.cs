using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
    public enum GameMusicState
    {
        StateCalm,
        StateBattle
    }

    public class MusicManager: MonoBehaviour
    {
        [Header("Audio Sources")] [SerializeField]
        private AudioSource _audioSource1;

        [SerializeField] private AudioSource _audioSource2;

        [Header("Music Lists")] [SerializeField]
        private List<AudioClip> _state1MusicList = new List<AudioClip>();

        [SerializeField] private List<AudioClip> _state2MusicList = new List<AudioClip>();

        [Header("Settings")] [SerializeField] private float _fadeDuration = 2f; // Длительность плавного перехода при смене стейта
        [SerializeField] private float _maxVolume = 1f; // Максимальная громкость
        [SerializeField] private GameMusicState _initialState = GameMusicState.StateCalm;

        private GameMusicState _currentState;
        private AudioClip _currentClip;
        private AudioSource _activeSource; // Активный источник
        private AudioSource _inactiveSource; // Неактивный источник (для кроссфейда)
        private CancellationTokenSource _musicCts;
        private bool _isFading = false;
        private bool _stateChanged = false; // Флаг смены стейта

        public GameMusicState CurrentState => _currentState;

        private void Awake()
        {
            // Создаём AudioSource, если не назначены
            if (_audioSource1 == null)
            {
                _audioSource1 = gameObject.AddComponent<AudioSource>();
            }

            if (_audioSource2 == null)
            {
                var go = new GameObject("MusicSource2");
                go.transform.SetParent(transform);
                _audioSource2 = go.AddComponent<AudioSource>();
            }

            _audioSource1.loop = false;
            _audioSource1.volume = _maxVolume;
            _audioSource2.loop = false;
            _audioSource2.volume = 0f;

            _activeSource = _audioSource1;
            _inactiveSource = _audioSource2;

            _currentState = _initialState;
        }

        private void Start()
        {
            StartMusicLoop().Forget();
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<GameMusicStateChangedEvent>(OnStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<GameMusicStateChangedEvent>(OnStateChanged);
            _musicCts?.Cancel();
        }

        private void OnDestroy()
        {
            _musicCts?.Cancel();
            _musicCts?.Dispose();
        }

        private void OnStateChanged(GameMusicStateChangedEvent stateEvent)
        {
            if (_currentState != stateEvent.NewState)
            {
                _currentState = stateEvent.NewState;
                _stateChanged = true;

                StartMusicLoop().Forget();
            }
        }

        private async UniTaskVoid StartMusicLoop()
        {
            _musicCts = new CancellationTokenSource();
            var token = _musicCts.Token;

            try
            {
                await PlayNextTrack(token, false);

                while (!token.IsCancellationRequested)
                {
                    while (_activeSource != null && _activeSource.isPlaying && !token.IsCancellationRequested)
                    {
                        await UniTask.Yield(token);
                    }

                    if (token.IsCancellationRequested)
                        break;

                    bool needFade = _stateChanged;
                    _stateChanged = false;

                    await PlayNextTrack(token, needFade);
                }
            }
            catch (OperationCanceledException)
            {
                // Отменено
            }
        }

        private async UniTask PlayNextTrack(CancellationToken token, bool useFade)
        {
            var musicList = GetCurrentMusicList();

            if (musicList == null || musicList.Count == 0)
            {
                Debug.LogWarning($"Music list for state {_currentState} is empty!");
                return;
            }

            AudioClip nextClip = null;

            if (musicList.Count == 1)
            {
                nextClip = musicList[0];
            }
            else
            {
                var availableClips = new List<AudioClip>(musicList);
                if (_currentClip != null && availableClips.Contains(_currentClip))
                {
                    availableClips.Remove(_currentClip);
                }

                if (availableClips.Count > 0)
                {
                    nextClip = availableClips[UnityEngine.Random.Range(0, availableClips.Count)];
                }
                else
                {
                    nextClip = musicList[UnityEngine.Random.Range(0, musicList.Count)];
                }
            }

            if (nextClip == null)
                return;

            _currentClip = nextClip;

            if (useFade)
            {
                await FadeToNextTrack(nextClip, token);
            }
            else
            {
                await SwitchToNextTrack(nextClip, token);
            }
        }

        private async UniTask SwitchToNextTrack(AudioClip nextClip, CancellationToken token)
        {
            if (_activeSource == null)
            {
                return;
            }
            
            _activeSource.clip = nextClip;
            _activeSource.volume = _maxVolume;
            _activeSource.Play();

            await UniTask.Yield(token);
        }

        private async UniTask FadeToNextTrack(AudioClip nextClip, CancellationToken token)
        {
            if (_isFading)
                return;

            _isFading = true;

            (_activeSource, _inactiveSource) = (_inactiveSource, _activeSource);

            if (_activeSource == null)
            {
                return;
            }
            
            _activeSource.clip = nextClip;
            _activeSource.volume = 0f;
            _activeSource.Play();

            float elapsed = 0f;
            float inactiveStartVolume = _inactiveSource.volume;

            while (elapsed < _fadeDuration && !token.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _fadeDuration;

                _activeSource.volume = Mathf.Lerp(0f, _maxVolume, t);

                _inactiveSource.volume = Mathf.Lerp(inactiveStartVolume, 0f, t);

                await UniTask.Yield(token);
            }

            if (!token.IsCancellationRequested)
            {
                _activeSource.volume = _maxVolume;
                _inactiveSource.volume = 0f;
                _inactiveSource.Stop();
            }

            _isFading = false;
        }

        private List<AudioClip> GetCurrentMusicList()
        {
            return _currentState switch
            {
                GameMusicState.StateCalm => _state1MusicList,
                GameMusicState.StateBattle => _state2MusicList,
                _ => _state1MusicList
            };
        }

        public void ChangeState(GameMusicState newState)
        {
            if (_currentState != newState)
            {
                _currentState = newState;
                _stateChanged = true;
                
                EventBus.Instance.Publish(new GameMusicStateChangedEvent(newState));
            }
        }
    }
}