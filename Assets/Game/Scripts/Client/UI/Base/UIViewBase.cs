using UnityEngine;

namespace Game.Client.UI
{
    public class UIViewBase: MonoBehaviour
    {
        private void Awake()
        {   
            Init();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        protected virtual void Init()
        {
        }

        protected virtual void Subscribe()
        {
        }

        protected virtual void Unsubscribe()
        {
        }
    }
}