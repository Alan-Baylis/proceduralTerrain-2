using UnityEngine;

public static class MapConstants {
	
	public const float hexOuterRadius = 10f;
	public const float hexInnerRadius = hexOuterRadius * 0.866025404f;

	public static Vector3[] hexCorners = {
		new Vector3(0f, 0f, hexOuterRadius),
		new Vector3(hexInnerRadius, 0f, 0.5f * hexOuterRadius),
		new Vector3(hexInnerRadius, 0f, -0.5f * hexOuterRadius),
		new Vector3(0f, 0f, -hexOuterRadius),
		new Vector3(-hexInnerRadius, 0f, -0.5f * hexOuterRadius),
		new Vector3(-hexInnerRadius, 0f, 0.5f * hexOuterRadius)
	};
}

