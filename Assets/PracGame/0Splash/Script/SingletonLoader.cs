using UnityEngine;

public class SingletonLoader : MonoBehaviour
{
	public GameObject googlesheetManagerPrefab;
	void Awake()
	{
		GoogleSheetManager.Load(googlesheetManagerPrefab);
	}
}
