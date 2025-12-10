using System;
using System.Collections.Generic;
using System.Linq;
using Game.Shared;
using UnityEngine;

namespace Game.Client
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class CombineMesh: MonoBehaviour
	{
		[SerializeField, ReadOnlyField] private MeshFilter _meshFilter;

		[SerializeField, ReadOnlyField] private List<MeshFilter> _meshFilters;

		private CombineInstance[] _combine;
		private void OnValidate()
		{
			_meshFilter = GetComponent<MeshFilter>();
			_meshFilters = new();

			var filters = GetComponentsInChildren<MeshFilter>();
			foreach (var filter in filters)
			{
				if (filter == _meshFilter)
				{
					continue;
				}
				
				_meshFilters.Add(filter);
			}
		}

		private void Start()
		{
			Optimize();
		}

		public void Optimize()
		{
			_combine = new CombineInstance[_meshFilters.Count];

			var inverseMatrix = Matrix4x4.TRS(transform.position, Quaternion.identity, Vector3.one).inverse;
			var i = 0;
			while (i < _meshFilters.Count)
			{
				_combine[i].mesh = _meshFilters[i].sharedMesh;
				var localToWorld = _meshFilters[i].transform.localToWorldMatrix;
				_combine[i].transform = inverseMatrix * localToWorld;
				_meshFilters[i].gameObject.SetActive(false);

				i++;
			}

			var mesh = new Mesh();
			mesh.CombineMeshes(_combine);
			mesh.RecalculateBounds();

			_meshFilter.sharedMesh = mesh;
			transform.gameObject.SetActive(true);
		}
	}
}