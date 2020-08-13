using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TextureNoiseSource : NoiseSource {

	public Texture2D texture;

	/// <summary>
	/// Does a texture lookup at the given uv-coordinates. Output is the texture's luminance denormalized to [-1, 1]
	/// </summary>
	public override float Evaluate (float x, float y) {
		TransformCoords(ref x, ref y);
		x = Mathf.Repeat(x, 1f);
		y = Mathf.Repeat(y, 1f);
		Color col = texture.GetPixelBilinear(x, y);
		float lum = 0.299f * col.r + 0.587f * col.g + 0.115f * col.b;
		return settings.strength * ((2f * lum) - 1f);
	}

}
