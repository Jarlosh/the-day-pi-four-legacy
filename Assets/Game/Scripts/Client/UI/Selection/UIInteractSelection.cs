using TMPro;
using UnityEngine;
using Game.Shared;

namespace Game.Client.UI
{
    public class UIInteractSelection: UIViewBase
    {
        [SerializeField] private GameObject _selectionView;
        [SerializeField] private TMP_Text _selectionLabel;
        
        private IInteractable _lastInteractable;

        protected override void Init()
        {
            base.Init();
            _selectionView.SetActive(false);
        }

        protected override void Subscribe()
        {
            EventBus.Instance.Subscribe<OnSelectionChangedEvent>(OnSelectionChanged);
        }

        protected override void Unsubscribe()
        {
            EventBus.Instance.Unsubscribe<OnSelectionChangedEvent>(OnSelectionChanged);
        }

        private void OnSelectionChanged(OnSelectionChangedEvent selectionChangedEvent)
        {
            var interactable = selectionChangedEvent.Interactable;
            if (_lastInteractable != interactable)
            {
                _lastInteractable = interactable;
                if (_lastInteractable != null)
                {
                    UpdateText();
                }    
                _selectionView.SetActive(_lastInteractable != null);
            }
        }

        private void UpdateText()
        {
            var text = _lastInteractable.GetDescriptionKey();
            var interactKey = "[E]"; // todo: faked for now
            _selectionLabel.text = $"{interactKey} {text}";
        }
    }
}