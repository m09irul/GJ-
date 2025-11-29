using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class loadingBar : MonoBehaviour {

	public GameObject loadingSceneCanvas;
	public Slider slider;

	/* Load Asyschronously. 
	 * works well for heavy scene..
	 * 
	 void Start()
	{
		loadLevel(SceneManager.GetActiveScene().buildIndex + 1);
	}
	public void loadLevel(int sceneIndex)
	{
		StartCoroutine(LoadAsynchronously(sceneIndex));
	}
	IEnumerator LoadAsynchronously(int sceneIndex)
	{
		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

		loadingSceneCanvas.SetActive(true);
		while(!operation.isDone)
		{
			float progress = Mathf.Clamp01(operation.progress / .9f);

			slider.value = progress;
			yield return null;
		}
		
	}*/

	/* Load fake. 
	 * works well for light scene..
	 */
	 void Start()
	{
		AudioManager.instance.play("CatMew01SFX");
		loadLevel(SceneManager.GetActiveScene().buildIndex + 1);
	}
	public void loadLevel(int sceneIndex)
	{
		StartCoroutine(LoadFake(sceneIndex));
	}
	IEnumerator LoadFake(int sceneIndex)
	{
		//AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
		float currenttime = 0f;
		loadingSceneCanvas.SetActive(true);
		while(currenttime <4f)
		{
			currenttime += Time.deltaTime; 
			slider.value = currenttime;
			yield return null;
		}
		LevelLoader.instance.loadNextLevel();
		
	}
}
