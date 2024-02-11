using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroDialouge : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float textSpeed;
    public string paragraph;

    private bool textFinished = false;
    public FadeOut fadeScreen;


    void Start()
    {
        textComponent.text = string.Empty;
        StartDialogue();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            textSpeed = textSpeed / 100;
        }
        if (textFinished && Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(LoadSceneWithFade(1f));
        }
    }
    void StartDialogue()
    {
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in paragraph.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        textFinished = true;
    }

    public IEnumerator LoadSceneWithFade(float _delay)
    {
        fadeScreen.Fade();

        yield return new WaitForSeconds(_delay);
        SceneManager.LoadScene("Game");

    }
}
