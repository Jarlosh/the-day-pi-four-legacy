using UnityEngine;

namespace UI.Base
{
    public class UIViewBase: MonoBehaviour
    {
        protected virtual void Awake()
        {   
            Init();
        }

        protected virtual void OnEnable()
        {
            Subscribe();
        }

        protected virtual void OnDisable()
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