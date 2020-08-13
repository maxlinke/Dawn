using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseSource {

	[System.Serializable]
	public struct NoiseSettings {
		public float scale;
		[Range(0f, 1f)] public float strength;
	}

	public NoiseSettings settings;
	public bool randomOffset;
	public bool randomRotation;
	[HideInInspector] public Vector2 offset;
	[HideInInspector] public float rotation;

	public abstract float Evaluate (float x, float y);

	protected void TransformCoords (ref float x, ref float y) {
		if(randomOffset){
			x = x + offset.x;
			y = y + offset.y;
		}
		if(randomRotation){
			float sin = Mathf.Sin(rotation);
			float cos = Mathf.Cos(rotation);
			float ox = x;
			float oy = y;
			x = (cos * ox) + (sin * oy);
			y = (cos * oy) - (sin * ox);
		}
		x = x / settings.scale;
		y = y / settings.scale;
	}

}
