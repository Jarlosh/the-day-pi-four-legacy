using UnityEngine;

namespace Game.Client
{
	public class MoveCamera: MonoBehaviour
	{
		[field: SerializeField] private Transform CameraPosition { get; set; }

		private void Update()
		{
			transform.position = CameraPosition.position;
		}
	}
}
