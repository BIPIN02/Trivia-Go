using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScript : MonoBehaviour
{
    public Slider slider;
    public Text progressText;
    void Start()
    {
        StartCoroutine(LoadGameSceneAsync());
    }


    public IEnumerator LoadGameSceneAsync()
    {
        float progress = 0f;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MenuScene");
        asyncLoad.allowSceneActivation = false; //scene will not load while this value is false

        while (progress <= 1f)
        {
            slider.value = progress;

            progressText.text = Mathf.Round(progress * 100f) + "%";

            progress += .01f;

            yield return new WaitForSeconds(.01f);
        }
        while (!asyncLoad.isDone && progress >= 1f)
        {
            asyncLoad.allowSceneActivation = true;
            yield return null;
            }

        }
    }
