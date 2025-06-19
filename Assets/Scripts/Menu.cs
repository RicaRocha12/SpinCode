using UnityEngine;
using UnityEngine.SceneManagement;
public class Menu : MonoBehaviour
{
    public GameObject obrigado;
    public AudioSource musicaFundo;
    public void ExitApplication()
    {
        obrigado.SetActive(true);
        musicaFundo.Stop();    
    }
    public void ComecarApplication()
    {
        SceneManager.LoadScene("Nivel1");
    }
}
