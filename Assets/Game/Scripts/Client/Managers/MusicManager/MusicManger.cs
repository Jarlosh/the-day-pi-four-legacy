using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Client
{
    public enum GameMusicState
    {
        StateMenu,
        StateCalm,
        StateBattle,
    }

    public class MusicManager: MonoBehaviour
    {
        [Header("Audio Sources")] [SerializeField]
        private AudioSource _audioSource1;

        [SerializeField] private AudioSource _audioSource2;

        [Header("Music Lists")]
        [SerializeField] private List<AudioClip> _stateMenuMusicList = new List<AudioClip>();
        [SerializeField] private List<AudioClip> _stateCalmMusicList = new List<AudioClip>();
        [SerializeField] private List<AudioClip> _stateBattleMusicList = new List<AudioClip>();

        [Header("Settings")] [SerializeField] private float _fadeDuration = 2f;
        [SerializeField] private float _maxVolume = 1f;
        [SerializeField] private GameMusicState _initialState = GameMusicState.StateMenu;
        [SerializeField] private bool _preloadAudio = true; // Предзагрузка аудио данных

        private GameMusicState _currentState;
        private AudioClip _currentClip;
        private AudioSource _activeSource;
        private AudioSource _inactiveSource;
        private CancellationTokenSource _musicCts;
        private bool _isFading = false;
        private bool _stateChanged = false;
        private bool _isDestroyed = false;

        public GameMusicState CurrentState => _currentState;

        private void Awake()
        {
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

            // Предзагружаем все аудио клипы
            if (_preloadAudio)
            {
                PreloadAllAudioClips();
            }
        }

        private void Start()
        {
            _musicCts = new CancellationTokenSource();
            StartMusicLoop(_musicCts.Token).Forget();
        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<GameMusicStateChangedEvent>(OnStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Instance.Unsubscribe<GameMusicStateChangedEvent>(OnStateChanged);
            StopMusic();
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
            StopMusic();
        }

        private void StopMusic()
        {
            if (_musicCts != null)
            {
                _musicCts.Cancel();
                _musicCts.Dispose();
                _musicCts = null;
            }

            if (_activeSource != null)
            {
                _activeSource.Stop();
            }

            if (_inactiveSource != null)
            {
                _inactiveSource.Stop();
            }
        }

        private void PreloadAllAudioClips()
        {
            // Предзагружаем все аудио клипы из обоих списков
            var allClips = new List<AudioClip>();
            allClips.AddRange(_stateMenuMusicList);
            allClips.AddRange(_stateCalmMusicList);
            allClips.AddRange(_stateBattleMusicList);

            foreach (var clip in allClips)
            {
                if (clip != null && !clip.preloadAudioData)
                {
                    clip.LoadAudioData();
                }
            }
        }

        private void OnStateChanged(GameMusicStateChangedEvent stateEvent)
        {
            if (_isDestroyed)
                return;

            if (_currentState != stateEvent.NewState)
            {
                _currentState = stateEvent.NewState;
                _stateChanged = true;
                
                StartMusicLoop(default).Forget();
            }
        }

        private async UniTaskVoid StartMusicLoop(CancellationToken token)
        {
            try
            {
                // Запускаем первый трек
                await PlayNextTrack(token, false);

                while (!token.IsCancellationRequested && !_isDestroyed)
                {
                    // Ждём, пока трек закончится
                    while (_activeSource != null && _activeSource.isPlaying && !token.IsCancellationRequested && !_isDestroyed)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                    }

                    if (token.IsCancellationRequested || _isDestroyed)
                        break;

                    // Проверяем, изменился ли стейт
                    bool needFade = _stateChanged;
                    _stateChanged = false;

                    // Выбираем и воспроизводим следующий трек
                    await PlayNextTrack(token, needFade);
                }
            }
            catch (OperationCanceledException)
            {
                // Отменено - это нормально
            }
        }

        private async UniTask PlayNextTrack(CancellationToken token, bool useFade)
        {
            if (_isDestroyed || token.IsCancellationRequested)
                return;

            var musicList = GetCurrentMusicList();

            if (musicList == null || musicList.Count == 0)
            {
                Debug.LogWarning($"Music list for state {_currentState} is empty!");
                return;
            }

            // Выбираем случайный трек
            AudioClip nextClip = SelectNextClip(musicList);

            if (nextClip == null)
                return;

            // Предзагружаем аудио данные, если ещё не загружены
            if (!nextClip.preloadAudioData)
            {
                nextClip.LoadAudioData();
                // Даём кадр на загрузку
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

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

        private AudioClip SelectNextClip(List<AudioClip> musicList)
        {
            if (musicList.Count == 0)
                return null;

            if (musicList.Count == 1)
            {
                return musicList[0];
            }

            // Выбираем случайный трек, отличный от текущего
            var availableClips = new List<AudioClip>(musicList);
            if (_currentClip != null && availableClips.Contains(_currentClip))
            {
                availableClips.Remove(_currentClip);
            }

            if (availableClips.Count > 0)
            {
                return availableClips[UnityEngine.Random.Range(0, availableClips.Count)];
            }

            return musicList[UnityEngine.Random.Range(0, musicList.Count)];
        }

        private async UniTask SwitchToNextTrack(AudioClip nextClip, CancellationToken token)
        {
            if (_isDestroyed || _activeSource == null || token.IsCancellationRequested)
                return;

            // Просто переключаемся на новый трек без fade
            _activeSource.clip = nextClip;
            _activeSource.volume = _maxVolume;
            _activeSource.Play();

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        private async UniTask FadeToNextTrack(AudioClip nextClip, CancellationToken token)
        {
            if (_isFading || _isDestroyed || token.IsCancellationRequested)
                return;

            if (_activeSource == null || _inactiveSource == null)
                return;

            _isFading = true;

            try
            {
                // Меняем местами активный и неактивный источники
                var temp = _activeSource;
                _activeSource = _inactiveSource;
                _inactiveSource = temp;

                // Настраиваем новый активный источник
                _activeSource.clip = nextClip;
                _activeSource.volume = 0f;
                _activeSource.Play();

                // Плавно переключаем громкость между источниками
                float elapsed = 0f;
                float inactiveStartVolume = _inactiveSource.volume;

                while (elapsed < _fadeDuration && !token.IsCancellationRequested && !_isDestroyed)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / _fadeDuration;

                    if (_activeSource != null)
                    {
                        _activeSource.volume = Mathf.Lerp(0f, _maxVolume, t);
                    }

                    if (_inactiveSource != null)
                    {
                        _inactiveSource.volume = Mathf.Lerp(inactiveStartVolume, 0f, t);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                if (!token.IsCancellationRequested && !_isDestroyed)
                {
                    if (_activeSource != null)
                    {
                        _activeSource.volume = _maxVolume;
                    }

                    if (_inactiveSource != null)
                    {
                        _inactiveSource.volume = 0f;
                        _inactiveSource.Stop();
                    }
                }
            }
            finally
            {
                _isFading = false;
            }
        }

        private List<AudioClip> GetCurrentMusicList()
        {
            return _currentState switch
            {
                GameMusicState.StateMenu=> _stateMenuMusicList,
                GameMusicState.StateCalm => _stateCalmMusicList,
                GameMusicState.StateBattle => _stateBattleMusicList,
                _ => _stateCalmMusicList
            };
        }

        public void ChangeState(GameMusicState newState)
        {
            if (_isDestroyed)
                return;

            if (_currentState != newState)
            {
                _currentState = newState;
                _stateChanged = true;
                EventBus.Instance.Publish(new GameMusicStateChangedEvent(newState));
            }
        }
    }
}