using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    [SerializeField] GameObject UIControls;
    [SerializeField] AudioSource buttonSelect;
    [SerializeField] GameObject fadeOut;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIControls.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RestartGame()
    {
        StartCoroutine(RestartButton());
    }

    public void MainMenu()
    {
        StartCoroutine(BackMainMenuButton());
    }

    IEnumerator RestartButton()
    {
        buttonSelect.Play();
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
    }

    IEnumerator BackMainMenuButton()
    {
        buttonSelect.Play();
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);
    }
}
