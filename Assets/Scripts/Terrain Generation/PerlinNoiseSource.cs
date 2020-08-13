using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PerlinNoiseSource : NoiseSource {

	/// <summary>
	/// Does a perlin noise lookup at x and y. Return is (mostly) in [-1, 1]
	/// </summary>
	public override float Evaluate (float x, float y) {
		TransformCoords(ref x, ref y);
		float sample = Mathf.PerlinNoise(x, y);
		return settings.strength * ((2f * sample) - 1f);
	}

}
