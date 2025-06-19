using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Vitória : MonoBehaviour
{
    // Referências a elementos UI
    public Text textoVidas;
    public Text textoNivel;

    // Novos campos para as perguntas separadas
    public Text textoPerguntas1;
    public Text textoPerguntas2;
    public Text textoPerguntas3;
    public Text textoPerguntas4;

    // Lista de sprites para soluções e array de imagens para exibição
    public List<Sprite> spritesSolucoes;
    public Image[] imagensSolucoes = new Image[9];

    // Lista de imagens para fundo das respostas
    public Image[] imagensFundoRespostas = new Image[3];
    public List<Sprite> spritesFundoRespostas;

    // Controle de nível selecionado
    private int nivelSelecionado = 1;

    // Conteúdo do progresso lido do ficheiro
    private string conteudoProgresso = "";

    // Botões de navegação entre níveis
    public Button botaoProximoNivel;
    public Button botaoNivelAnterior;

    // GameObject para nova música
    public GameObject novoGameObjectMusica; 

    private float tempoParaTrocar = 5f; // Tempo em segundos
    private float tempoDecorrido = 0f;
    private bool trocou = false;

    void Update()
    {
        if (!trocou)
        {
            tempoDecorrido += Time.deltaTime;
            if (tempoDecorrido >= tempoParaTrocar)
            {
                if (novoGameObjectMusica != null)
                {
                    novoGameObjectMusica.SetActive(true);
                }
                trocou = true;
            }
        }
    }

    void Start()
    {
        LerProgressoNivel();
        botaoProximoNivel.onClick.AddListener(ProximoNivel);
        botaoNivelAnterior.onClick.AddListener(NivelAnterior);
    }

    private void LerProgressoNivel()
    {
        int respostas = PlayerPrefs.GetInt($"Nivel{nivelSelecionado}_Respostas", 0);
        string perguntasCorretas = PlayerPrefs.GetString($"Nivel{nivelSelecionado}_PerguntasCorretas", "");
        int vidas = PlayerPrefs.GetInt($"Nivel{nivelSelecionado}_Vidas", 0);

        string conteudo = $"Nivel {nivelSelecionado}\nRespostas: {respostas}\nPerguntasCorretas: {perguntasCorretas}\nVidas: {vidas}";
        conteudoProgresso = conteudo;
        Debug.Log("Progresso lido do PlayerPrefs (WebGL):\n" + conteudoProgresso);
        PreencherCampos(conteudoProgresso);
    }
    private void AtualizarFundosRespostas()
    {
        if (spritesFundoRespostas == null || spritesFundoRespostas.Count < 12) return;

        if (nivelSelecionado == 1)
        {
            imagensFundoRespostas[0].sprite = spritesFundoRespostas[0];
            imagensFundoRespostas[1].sprite = spritesFundoRespostas[3];
            imagensFundoRespostas[2].sprite = spritesFundoRespostas[6];
            imagensFundoRespostas[3].sprite = spritesFundoRespostas[9];

        }
        else if (nivelSelecionado == 2)
        {
            imagensFundoRespostas[0].sprite = spritesFundoRespostas[1];
            imagensFundoRespostas[1].sprite = spritesFundoRespostas[4];
            imagensFundoRespostas[2].sprite = spritesFundoRespostas[7];
            imagensFundoRespostas[3].sprite = spritesFundoRespostas[10];
        }
        else if (nivelSelecionado == 3)
        {
            imagensFundoRespostas[0].sprite = spritesFundoRespostas[2];
            imagensFundoRespostas[1].sprite = spritesFundoRespostas[5];
            imagensFundoRespostas[2].sprite = spritesFundoRespostas[8];
            imagensFundoRespostas[3].sprite = spritesFundoRespostas[11];
        }
    }

    public void ProximoNivel()
    {
        nivelSelecionado++;
        if (nivelSelecionado > 3) nivelSelecionado = 1;
        LerProgressoNivel();
    }

    public void NivelAnterior()
    {
        nivelSelecionado--;
        if (nivelSelecionado < 1) nivelSelecionado = 3;
        LerProgressoNivel();
    }

    void PreencherCampos(string conteudo)
    {
        string nivel = nivelSelecionado.ToString();
        string vidas = "";
        string respostas = "";
        string perguntasCorretasNumeros = "";

        // Novos textos separados
        string perguntasCorretasTexto1 = "";
        string perguntasCorretasTexto2 = "";
        string perguntasCorretasTexto3 = "";
        string perguntasCorretasTexto4 = "";

        string[] linhas = conteudo.Split('\n');
        foreach (string linha in linhas)
        {
            if (linha.StartsWith("Vidas:"))
                vidas = linha.Replace("Vidas:", "").Trim();
            else if (linha.StartsWith("Respostas:"))
                respostas = linha.Replace("Respostas:", "").Trim();
            else if (linha.StartsWith("PerguntasCorretas:"))
                perguntasCorretasNumeros = linha.Replace("PerguntasCorretas:", "").Trim();
        }

        TextAsset perguntasAsset;
        if (nivelSelecionado == 2)
            perguntasAsset = Resources.Load<TextAsset>("perguntas4");
        else if (nivelSelecionado == 3)
            perguntasAsset = Resources.Load<TextAsset>("perguntas5");
        else
            perguntasAsset = Resources.Load<TextAsset>("perguntas3");

        string[] todasPerguntas = perguntasAsset != null ? perguntasAsset.text.Split('\n') : new string[0];

        foreach (var img in imagensSolucoes)
        {
            img.sprite = null;
            img.enabled = false;
        }

        int[] imagensAtivas;
        if (nivelSelecionado == 1)
            imagensAtivas = new int[] { 2, 3, 4, 7, 8, 9, 12, 13, 14, 17, 18, 19 }; // Ativa as imagens que vão receber as soluções
        else if (nivelSelecionado == 2)
            imagensAtivas = new int[] { 1, 2, 3, 4, 6, 7, 8, 9, 11, 12, 13, 14, 16, 17, 18, 19 }; // Ativa as imagens que vão receber as soluções
        else
            imagensAtivas = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 10, 12, 13, 14, 15, 16, 17, 18, 19 };

        foreach (int idx in imagensAtivas)
        {
            if (idx >= 0 && idx < imagensSolucoes.Length)
                imagensSolucoes[idx].enabled = true;
        }

        // Mapeamento dos índices das imagens para cada pergunta e nível
        int[][] mapasImagensPorNivel = null;
        if (nivelSelecionado == 1)
        {
            mapasImagensPorNivel = new int[][] {
            new int[] { 2, 3, 4 },    // 1ª pergunta: imagens 2,3,4 
            new int[] { 7, 8, 9 },    // 2ª pergunta: imagens 7,8,9
            new int[] { 12, 13, 14} , // 3ª pergunta: imagens 12,13,14
            new int[] { 17, 18, 19}     // 4ª pergunta: imagens 1,2,3
        };
        }
        else if (nivelSelecionado == 2)
        {
            mapasImagensPorNivel = new int[][] {
            new int[] { 1, 2, 3, 4 },    // 1ª pergunta: imagens 1,2,3,4
            new int[] { 6, 7, 8, 9 },    // 2ª pergunta: imagens 6,7,8,9
            new int[] { 11, 12, 13, 14 }, // 3ª pergunta: imagens 11,12,13,14
            new int[] { 16, 17, 18, 19 } // 4ª pergunta: imagens 15,16,17,18
        };
        }
        else if (nivelSelecionado == 3)
        {
            mapasImagensPorNivel = new int[][] {
            new int[] { 0, 1, 2, 3, 4 },    // 1ª pergunta: imagens 1,2,3,4
            new int[] { 5, 6, 7, 8, 9 },    // 2ª pergunta: imagens 6,7,8,9
            new int[] { 10, 11, 12, 13, 14 }, // 3ª pergunta: imagens 11,12,13,14
            new int[] { 15, 16, 17, 18, 19, } // 4ª pergunta: imagens 15,16,17,18
        };
        }


        if (!string.IsNullOrEmpty(perguntasCorretasNumeros))
        {
            string[] indices = perguntasCorretasNumeros.Split(',');
            int perguntaCount = 0;
            foreach (string indiceStr in indices)
            {
                if (int.TryParse(indiceStr, out int indice))
                {
                    int idx = indice - 1;
                    if (idx >= 0 && idx < todasPerguntas.Length)
                    {
                        string[] partes = todasPerguntas[idx].Split('|');
                        string pergunta = partes[0].Trim();

                        // Distribui as perguntas entre os três textos
                        if (perguntaCount % 4 == 0)
                            perguntasCorretasTexto1 += $"{pergunta}\n\n";
                        else if (perguntaCount % 4 == 1)
                            perguntasCorretasTexto2 += $"{pergunta}\n\n";
                        else if (perguntaCount % 4 == 2)
                            perguntasCorretasTexto3 += $"{pergunta}\n\n";
                        else
                            perguntasCorretasTexto4 += $"{pergunta}\n\n";

                        string solucao = partes.Length > 1 ? partes[1].Trim() : "";

                        if (!string.IsNullOrEmpty(solucao))
                        {
                            string[] valores = solucao.Split(',');
                            if (mapasImagensPorNivel != null && perguntaCount < mapasImagensPorNivel.Length)
                            {
                                var indicesImagens = mapasImagensPorNivel[perguntaCount];
                                for (int i = 0; i < valores.Length && i < indicesImagens.Length; i++)
                                {
                                    if (int.TryParse(valores[i], out int valor))
                                    {
                                        int spriteIndex = valor;
                                        int imgAtivaIdx = indicesImagens[i];
                                        if (spriteIndex >= 0 && spriteIndex < spritesSolucoes.Count && imgAtivaIdx < imagensSolucoes.Length)
                                        {
                                            imagensSolucoes[imgAtivaIdx].sprite = spritesSolucoes[spriteIndex];
                                            imagensSolucoes[imgAtivaIdx].enabled = true;
                                        }
                                    }
                                }
                            }
                            else if (mapasImagensPorNivel == null)
                            {
                                // Nível 3: comportamento antigo
                                int imgIndex = 0;
                                for (int i = 0; i < valores.Length && imgIndex < imagensAtivas.Length; i++, imgIndex++)
                                {
                                    if (int.TryParse(valores[i], out int valor))
                                    {
                                        int spriteIndex = valor;
                                        int imgAtivaIdx = imagensAtivas[imgIndex];
                                        if (spriteIndex >= 0 && spriteIndex < spritesSolucoes.Count && imgAtivaIdx < imagensSolucoes.Length)
                                        {
                                            imagensSolucoes[imgAtivaIdx].sprite = spritesSolucoes[spriteIndex];
                                            imagensSolucoes[imgAtivaIdx].enabled = true;
                                        }
                                    }
                                }
                            }
                        }
                        perguntaCount++;
                    }
                }
            }
        }
        else
        {
            perguntasCorretasTexto1 = "Nenhuma pergunta correta encontrada.";
            perguntasCorretasTexto2 = "";
            perguntasCorretasTexto3 = "";
        }

        textoNivel.text = $"Nível {nivel}";
        textoVidas.text = $"Vidas: {vidas}";
        textoPerguntas1.text = perguntasCorretasTexto1;
        textoPerguntas2.text = perguntasCorretasTexto2;
        textoPerguntas3.text = perguntasCorretasTexto3;
        textoPerguntas4.text = perguntasCorretasTexto4;

        float largura = 1280f;
        float posX = -180f;
        if (nivelSelecionado == 2)
        {
            largura = 1200f;
            posX = -220f;
        }
        else if (nivelSelecionado == 3)
        {
            largura = 1100f;
            posX = -270f;
        }

        textoPerguntas1.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largura);
        textoPerguntas2.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largura);
        textoPerguntas3.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largura);
        textoPerguntas4.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, largura);

        textoPerguntas1.rectTransform.anchoredPosition = new Vector2(posX, textoPerguntas1.rectTransform.anchoredPosition.y);
        textoPerguntas2.rectTransform.anchoredPosition = new Vector2(posX, textoPerguntas2.rectTransform.anchoredPosition.y);
        textoPerguntas3.rectTransform.anchoredPosition = new Vector2(posX, textoPerguntas3.rectTransform.anchoredPosition.y);
        textoPerguntas4.rectTransform.anchoredPosition = new Vector2(posX, textoPerguntas4.rectTransform.anchoredPosition.y);


        AtualizarFundosRespostas();
    }

    public void VoltarMenu()
    {
        SceneManager.LoadScene("PaginaInicial");
    }
}