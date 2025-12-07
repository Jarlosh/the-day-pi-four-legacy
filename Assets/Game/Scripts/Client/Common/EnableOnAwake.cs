using UnityEngine;

namespace Game.Client
{
    public class EnableOnAwake: MonoBehaviour
    {
        [SerializeField] private GameObject[] _targets;

        private void Awake()
        {
            foreach (var target in _targets)
            {
                target.SetActive(true);
            }
        }
    }
}