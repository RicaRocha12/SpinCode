using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class SlotMachine3 : MonoBehaviour
{
    [Header("Botões")]
    public Button[] botoesAposta;          // Botões de escolha de números (1 a 7)
    public Button botaoApagarUltima;       // Botão para apagar a última aposta
    public Button botaoVerificar;          // Botão para verificar a aposta
    public Button botaoReiniciar;          // Botão para reiniciar o jogo
    public Button botãoSair;               // Botão para sair do jogo
    public Button botaoAjuda;              // Botão para mostrar o ecrã de ajuda
    public Button botaoAjudaVoltar;        // Botão "Voltar" no ecrã de ajuda

    [Header("Interface")]
    public Text pontuacaoText;             // UI Text para mostrar a pontuação
    public Text Vidas;                     // UI Text para mostrar as vidas
    public Text textoPergunta;             // UI Text para mostrar a pergunta
    public GameObject ecraDeDerrota;       // Ecrã de derrota (GameObject a mostrar quando o jogador perde)
    public GameObject ecraAjuda;           // Ecrã de ajuda (GameObject a mostrar quando o jogador clica no botão de ajuda)


    [Header("Slot Machine")]
    public GameObject[] reels;             // Roletas (imagens com os sprites)
    public Sprite[] simbolos;              // Sprites disponíveis (índices 0 a 6)
    public Sprite[] simbolosDourados;      // Mesmo número e ordem que 'simbolos'


    [Header("Som")]
    public AudioSource audioSource;        // Referência ao AudioSource
    public AudioClip SomRoleta;            // O som que toca a cada mudança
    public AudioClip SomDourado;           // Som a tocar quando um símbolo se transforma em dourado
    public AudioClip SomDerrota;           // Som a tocar quando o jogador erra
    public GameObject SomParaRemover;      // Som a tocar quando o jogador acerta tudo
    public AudioClip SomVitoria;           // Som a tocar quando o jogador acerta tudo
    public AudioClip SomErro;              // Som a tocar quando o jogador perde


    [Header("Perguntas e Jogo")]
    private int numeroDaQuestao = 1;        // Número da questão padrão
    private int[] solution = new int[3];    // Solução correta
    private List<int> apostaDoJogador = new List<int>(); // Lista para guardar os números apostados pelo jogador
    private bool isSpinning = false;        // Flag para controlar se a roleta está a girar
    private int vidas = 1;                  // Variável para controlar as vidas, tendo 3 vidas no início
    private Dictionary<int, (string pergunta, int[] solucao)> perguntasData; // Dicionário para armazenar as perguntas e soluções
    private List<int> perguntasRespondidas = new List<int>(); // Lista de perguntas já respondidas

    void Start()
    {
        vidas = 2; // Inicializa as vidas
        Vidas.text = vidas.ToString(); // Atualiza o texto das vidas
        if (ecraDeDerrota != null)
            ecraDeDerrota.SetActive(false);
        for (int i = 0; i < botoesAposta.Length; i++)
        {
            int valor = i; // valor entre 0 e 6
            botoesAposta[i].onClick.AddListener(() => AdicionarNumero(valor));
        }
        numeroDaQuestao = Random.Range(1, 5); // <-- Número da questão (1 a 7)
        CarregarPerguntas();
        AplicarPergunta(numeroDaQuestao);
        botaoAjuda.onClick.AddListener(() =>
        {
            ecraAjuda.SetActive(true);
            Time.timeScale = 0; // Pausa tudo
        });
        botaoAjudaVoltar.onClick.AddListener(() =>
        {
            ecraAjuda.SetActive(false);
            Time.timeScale = 1; // Retoma o jogo
        });
        botaoVerificar.onClick.AddListener(VerificarAposta);
        botaoReiniciar.onClick.AddListener(ReiniciarJogoTotalmente);
        botaoApagarUltima.onClick.AddListener(ApagarUltimaAposta);
        botãoSair.onClick.AddListener(() => SceneManager.LoadScene("PaginaInicial")); // Botão para sair do jogo
        ReiniciarJogo(); // começa limpo
    }
    void CarregarPerguntas()
    {
        perguntasData = new Dictionary<int, (string, int[])>();

        TextAsset ficheiro = Resources.Load<TextAsset>("perguntas3");
        Debug.Log("Conteúdo do ficheiro:\n" + ficheiro.text);
        if (ficheiro == null)
        {
            Debug.LogError("Ficheiro perguntas3.txt não encontrado em Resources!");
            return;
        }

        string[] linhas = ficheiro.text.Split('\n');
        for (int i = 0; i < linhas.Length; i++)
        {
            string linha = linhas[i].Trim();
            if (string.IsNullOrEmpty(linha)) continue;

            string[] partes = linha.Split('|');
            if (partes.Length != 2) continue;

            string pergunta = partes[0];
            int[] solucao = partes[1].Split(',').Select(s => int.Parse(s.Trim())).ToArray();

            perguntasData[i + 1] = (pergunta, solucao);
        }
    }

    void AplicarPergunta(int numero)
    {
        if (!perguntasData.ContainsKey(numero))
        {
            Debug.LogError($"Questão {numero} não encontrada!");
            return;
        }

        var dados = perguntasData[numero];
        StopAllCoroutines(); // Cancela qualquer animação anterior, caso haja
        StartCoroutine(MostrarPerguntaGradualmente(dados.pergunta));

        solution = dados.solucao;
        Debug.Log($"Questão {numero}: {dados.pergunta} | Solução: {string.Join(", ", dados.solucao)}");
    }

    void AdicionarNumero(int numero)
    {
        if (apostaDoJogador.Count >= 3 || isSpinning) return;

        apostaDoJogador.Add(numero);

        int indice = apostaDoJogador.Count - 1;

        // Mostra imediatamente a aposta na respetiva roleta
        reels[indice].SetActive(true);
        reels[indice].GetComponent<Image>().sprite = simbolos[numero];
    }


    void VerificarAposta()
    {
        if (apostaDoJogador.Count != 3 || isSpinning) return;

        // Iniciar a sequência de spin
        StartCoroutine(IniciarSpin());
        Debug.Log("Começou a rodar");
    }

    IEnumerator IniciarSpin()
    {
        isSpinning = true; // Bloqueia inputs
        yield return new WaitForSeconds(0.5f);

        // Ativa os reels para a animação
        foreach (GameObject reel in reels)
        {
            reel.SetActive(true);
        }

        // Inicia a coroutine que faz o spin e define o resultado
        StartCoroutine(SpinReels());
    }

    IEnumerator SpinReels()
    {
        float visualSpinDuration = 10f;
        float elapsedTime = 0f;

        // Primeiro, preparar os sprites finais com base na aposta
        Sprite[] spritesFinais = new Sprite[reels.Length];
        bool acertouTudo = true;


        // Verifica se o jogador acertou tudo ou não
        // Se acertou tudo, mostra os símbolos corretos
        // Se não acertou tudo, escolhe símbolos aleatórios diferentes da aposta e da solução
        // (exceto se não houver opções válidas, nesse caso mostra a solução)
        // 1) Prepara todos os finais como normais
        for (int i = 0; i < reels.Length; i++)
        {
            int aposta = apostaDoJogador[i];
            int solucao = solution[i];

            if (aposta == solucao)
            {
                // primeiro mete o normal
                spritesFinais[i] = simbolos[solucao];
            }
            else
            {
                acertouTudo = false; // Se não acertou tudo, muda a flag
                List<int> opcoesValidas = new List<int>();
                for (int j = 0; j < simbolos.Length; j++)
                    if (j != solucao && j != aposta)
                        opcoesValidas.Add(j);

                int idx = (opcoesValidas.Count > 0)
                    ? opcoesValidas[Random.Range(0, opcoesValidas.Count)]
                    : solucao;

                spritesFinais[i] = simbolos[idx];
            }
        }

        // Parte 1: animação visual com easing (acelera e depois desacelera)
        float timeSinceLastChange = 0f;

        while (elapsedTime < visualSpinDuration)
        {
            // Usamos uma curva de easing para determinar a velocidade (tempo entre mudanças)
            float progress = elapsedTime / visualSpinDuration; // de 0 a 1
            float speed = Mathf.Lerp(0.2f, 0.08f, Mathf.Sin(progress * Mathf.PI));
            // Começa lento (0.2s), acelera a meio (~0.03s), e volta a abrandar

            timeSinceLastChange += Time.deltaTime;
            // Se o tempo desde a última mudança for maior que a velocidade calculada, muda o símbolo
            if (timeSinceLastChange >= speed)
            {
                for (int i = 0; i < reels.Length; i++)
                {
                    reels[i].GetComponent<Image>().sprite = simbolos[Random.Range(0, simbolos.Length)];
                }

                // TOCA O SOM AQUI
                if (audioSource != null && SomRoleta != null)
                {
                    audioSource.PlayOneShot(SomRoleta);
                }

                timeSinceLastChange = 0f;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // 2) Aplica os sprites apostados pelo utilizador de uma só vez (em vez dos finais aleatórios)
        for (int i = 0; i < reels.Length; i++)
        {
            reels[i].GetComponent<Image>().sprite = simbolos[apostaDoJogador[i]];
            // Garante que a cor volta ao normal ao mostrar a aposta
            reels[i].GetComponent<Image>().color = Color.white;
        }

        // 3) Agora, progressivamente, substitui pelos dourados os que estão certos
        for (int i = 0; i < reels.Length; i++)
        {
            yield return new WaitForSeconds(1f);
            if (apostaDoJogador[i] == solution[i])
            {
                reels[i].GetComponent<Image>().sprite = simbolosDourados[solution[i]];

                // TOCAR O SOM DOURADO AQUI
                if (audioSource != null && SomDourado != null)
                {
                    audioSource.PlayOneShot(SomDourado);
                }
            }
            else
            {
                if (audioSource != null && SomDerrota != null)
                {
                    audioSource.PlayOneShot(SomDerrota);
                }
                // Se errou, escurece o sprite da aposta
                Image img = reels[i].GetComponent<Image>();
                img.sprite = simbolos[apostaDoJogador[i]];
                img.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Escurece o sprite
            }
            yield return new WaitForSeconds(0.5f);
        }


        // Efeito bónus se acertou tudo
        if (acertouTudo)
        {
            // Adiciona a pergunta atual à lista de respondidas
            if (!perguntasRespondidas.Contains(numeroDaQuestao))
                perguntasRespondidas.Add(numeroDaQuestao);

            yield return new WaitForSeconds(1f); // Espera antes de mostrar o "efeito de vitória"
            // Toca o som de vitória
            if (audioSource != null && SomVitoria != null)
            {
                audioSource.PlayOneShot(SomVitoria);
            }
            textoPergunta.fontSize = 60;
            textoPergunta.text = "Parabéns, acertaste!";

            yield return new WaitForSeconds(3f); // Espera antes de mudar de pergunta
            textoPergunta.fontSize = 35; // Restaura o tamanho da fonte

            // Se acertou 4 perguntas, guarda progresso e muda para a cena "nivel4"
            if (perguntasRespondidas.Count >= 4)
            {
                // Salva progresso usando PlayerPrefs no WebGL
                string perguntasCorretas = string.Join(",", perguntasRespondidas);
                PlayerPrefs.SetString("Nivel1_PerguntasCorretas", perguntasCorretas);
                PlayerPrefs.SetInt("Nivel1_Vidas", vidas);
                PlayerPrefs.Save();
                Debug.Log("Progresso guardado em PlayerPrefs (WebGL)");
                SceneManager.LoadScene("Nivel2"); // Muda para a cena "nivel4"
                yield break; // Para a coroutine aqui
            }

            // Seleciona apenas perguntas ainda não respondidas
            List<int> perguntasDisponiveis = perguntasData.Keys.Except(perguntasRespondidas).ToList();
            if (perguntasDisponiveis.Count > 0)
            {
                // Escolhe aleatoriamente uma das perguntas ainda não respondidas
                numeroDaQuestao = perguntasDisponiveis[Random.Range(0, perguntasDisponiveis.Count)];
                pontuacaoText.text = perguntasRespondidas.Count + "/4"; // Atualiza a pontuação

                AplicarPergunta(numeroDaQuestao);
                ReiniciarJogo();
            }
            else
            {
                Debug.Log("Não há mais perguntas disponíveis.");
                textoPergunta.text = "Fim das perguntas!";
                // Aqui podes mostrar uma mensagem final ou reiniciar desde a primeira
            }
        }
        else
        {
            string respostaErrada = textoPergunta.text;
            
            yield return new WaitForSeconds(1f);
            // Se o jogador errou, toca o som de erro
            if (audioSource != null && SomErro != null)
            {
                audioSource.PlayOneShot(SomErro);
            }
            textoPergunta.fontSize = 60;
            textoPergunta.text = "Perdeste! Tenta novamente.";
            yield return new WaitForSeconds(3f);
            vidas--;
            Vidas.text = vidas.ToString(); // Atualiza o texto das vidas
            if (vidas <= 0)
            {
                SomParaRemover.SetActive(false); // Toca o som de derrota
                // Mostra o ecrã de derrota
                if (ecraDeDerrota != null)
                    ecraDeDerrota.SetActive(true);
                isSpinning = false;
                yield break; // Para a coroutine
            }
            textoPergunta.fontSize = 35;
            ReiniciarJogo();
            textoPergunta.text = respostaErrada; // Mostra a pergunta errada
        }

        isSpinning = false;
    }
    void ReiniciarJogo()
    {
        apostaDoJogador.Clear();
        SomParaRemover.SetActive(true); // Toca o som de derrota    

        foreach (GameObject reel in reels)
        {
            reel.SetActive(false); // Esconde os reels até nova aposta
            Image img = reel.GetComponent<Image>();
            if (img != null)
                img.color = Color.white;
        }
        ecraDeDerrota.SetActive(false); // Esconde o ecrã de derrota
        isSpinning = false;
    }
    void ReiniciarJogoTotalmente()
    {
        // Recarrega a cena atual, reiniciando tudo para o padrão
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    void ApagarUltimaAposta()
    {
        if (apostaDoJogador.Count > 0 && !isSpinning)
        {
            int ultimaIndex = apostaDoJogador.Count - 1;
            apostaDoJogador.RemoveAt(ultimaIndex);

            // Desativa a reel correspondente (desaparece do ecrã)
            reels[ultimaIndex].SetActive(false);
        }
    }
    IEnumerator MostrarPerguntaGradualmente(string perguntaCompleta)
    {
        textoPergunta.text = "";
        string[] palavras = perguntaCompleta.Split(' ');

        foreach (string palavra in palavras)
        {
            textoPergunta.text += palavra + " ";
            yield return new WaitForSeconds(0.08f); // Delay entre palavras (ajustável)
        }
    }

}