using System;
using UnityEngine;

namespace Game.Client.Fracture
{
    public class FractureManager: MonoBehaviour
    {
        [SerializeField] private LayerMask _fractureLayer;
        [SerializeField] private float _damage;

        private void Awake()
        {
            Fragmenter.OnFragmentCreated += SetSuckable;
        }

        private void OnDestroy()
        {
            Fragmenter.OnFragmentCreated -= SetSuckable;
        }

        private void SetSuckable(GameObject obj)
        {
            var vacuumedObject = obj.AddComponent<VacuumedObject>();
            vacuumedObject.Init(_fractureLayer);
        }
    }
}