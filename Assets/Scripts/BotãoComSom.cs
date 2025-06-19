using UnityEngine;
using UnityEngine.UI;

public class BotaoComSom : MonoBehaviour
{
    public AudioClip somClique;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(TocarSom);
    }

    void TocarSom()
    {
        audioSource.PlayOneShot(somClique);
    }
}
