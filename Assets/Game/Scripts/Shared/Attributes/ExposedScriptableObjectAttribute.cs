using UnityEngine;

namespace Game.Shared
{
    public class ExposedScriptableObjectAttribute : PropertyAttribute
    {
        public bool Readonly { get; private set; }

        public ExposedScriptableObjectAttribute(bool @readonly = true)
        {
            Readonly = @readonly;
        }
    }
}
