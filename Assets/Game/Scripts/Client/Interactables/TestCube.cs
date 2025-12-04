using UnityEngine;

namespace Game.Client
{
    public class TestCube: MonoBehaviour, IInteractable
    {
        public bool CanInteract()
        {
            return true;
        }

        public void Interact()
        {
            Debug.Log($"Touched {name}");
        }

        public string GetDescriptionKey()
        {
            return "Interact";
        }
    }
}