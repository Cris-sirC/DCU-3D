using UnityEngine;
using TMPro;
using System.Collections;

public class Manitas3D : MonoBehaviour
{
    [Header("Interfaz Minimalista")]
    public TextMeshProUGUI textoEstado;
    public GameObject panelInstrucciones; 
    public float velocidadFade = 1.5f;

    [Header("Sistema de Vidas")]
    public GameObject[] corazonesJ1; 
    public GameObject[] corazonesJ2; 
    private int vidasJ1 = 3;
    private int vidasJ2 = 3;
    private bool partidaDefinitivaTerminada = false;

    [Header("Cámara Cinematográfica")]
    public Animator animCamara; 

    [Header("Animadores (Los Cerebros)")]
    public Animator animJ1; 
    public Animator animJ2;

    [Header("Personajes (Ragdoll)")]
    public ControladorRagdoll ragdollJ1;
    public ControladorRagdoll ragdollJ2;

    [Header("Efectos Visuales")]
    public ParticleSystem particulasGolpeJ1; 
    public ParticleSystem particulasGolpeJ2; 

    [Header("Audio")]
    public AudioSource fuenteMusica; 
    public AudioSource fuenteEfectos; 
    
    public AudioClip musicaFondo;
    public AudioClip musicaVictoria;
    public AudioClip sonidoGolpe;
    public AudioClip sonidoFalsoInicio;
    public AudioClip sonidoEsquivar;

    private float tiempoEspera;
    private bool esperandoAlerta;
    private bool juegoTerminado;
    private bool secuenciaEnCurso; 
    private int atacanteActual = 1; 
    private bool pendienteCambioRoles = false;
    private bool juegoEmpezado = false; 

    void Start()
    {
        textoEstado.text = ""; 
        if (panelInstrucciones != null) panelInstrucciones.SetActive(true);
        OcultarCorazones(true);
        StartCoroutine(RutinaInstrucciones());
    }

    void OcultarCorazones(bool ocultar)
    {
        foreach (GameObject corazon in corazonesJ1) if (corazon != null) corazon.SetActive(!ocultar);
        foreach (GameObject corazon in corazonesJ2) if (corazon != null) corazon.SetActive(!ocultar);
    }

    IEnumerator RutinaInstrucciones()
    {
        yield return new WaitForSeconds(3f);
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
        OcultarCorazones(false);
        juegoEmpezado = true;
        
        if (fuenteMusica != null && musicaFondo != null)
        {
            fuenteMusica.clip = musicaFondo;
            fuenteMusica.loop = true; 
            fuenteMusica.Play();
        }

        StartCoroutine(SecuenciaDeInicio());
    }

    void Update()
    {
        if (!juegoEmpezado) return; 

        if (juegoTerminado)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !secuenciaEnCurso)
            {
                if (partidaDefinitivaTerminada) ReiniciarPartidaCompleta();
                else StartCoroutine(SecuenciaDeInicio());
            }
            return;
        }

        int teclaPresionada = 0;
        if (Input.GetKeyDown(KeyCode.W)) teclaPresionada = 1;
        else if (Input.GetKeyDown(KeyCode.UpArrow)) teclaPresionada = 2;

        if (esperandoAlerta)
        {
            tiempoEspera -= Time.deltaTime;

            if (textoEstado.color.a > 0)
            {
                Color colorActual = textoEstado.color;
                colorActual.a -= Time.deltaTime * velocidadFade;
                textoEstado.color = colorActual;
            }

            if (teclaPresionada != 0)
            {
                int perdedor = teclaPresionada;
                int ganador = (teclaPresionada == 1) ? 2 : 1;
                
                if (fuenteEfectos != null && sonidoFalsoInicio != null) fuenteEfectos.PlayOneShot(sonidoFalsoInicio);

                if (teclaPresionada == 1) animJ1.SetTrigger("FalsoInicio");
                else animJ2.SetTrigger("FalsoInicio");

                StartCoroutine(RutinaImpacto(ganador, perdedor, "¡J" + perdedor + " SE ADELANTO!", true));
            }

            if (tiempoEspera <= 0f && esperandoAlerta)
            {
                esperandoAlerta = false;
                textoEstado.text = "¡AHORA!";
                Color colorFuerte = textoEstado.color;
                colorFuerte.a = 1f;
                textoEstado.color = colorFuerte;
            }
        }
        else if (!secuenciaEnCurso && teclaPresionada != 0)
        {
            if (teclaPresionada == atacanteActual)
            {
                int perdedor = (atacanteActual == 1) ? 2 : 1;

                if (teclaPresionada == 1) animJ1.SetTrigger("Ataque");
                else animJ2.SetTrigger("Ataque");
                
                StartCoroutine(RutinaImpacto(atacanteActual, perdedor, "¡PUNTO PARA JUGADOR " + atacanteActual + "!", false));
            }
            else
            {
                if (fuenteEfectos != null && sonidoEsquivar != null) fuenteEfectos.PlayOneShot(sonidoEsquivar);

                if (teclaPresionada == 1) animJ1.SetTrigger("Esquive");
                else animJ2.SetTrigger("Esquive");
                
                StartCoroutine(RutinaEsquivar());
            }
        }
    }

    IEnumerator SecuenciaDeInicio()
    {
        secuenciaEnCurso = true;
        juegoTerminado = false;
        textoEstado.text = ""; 

        if (pendienteCambioRoles)
        {
            RuntimeAnimatorController cerebroTemporal = animJ1.runtimeAnimatorController;
            animJ1.runtimeAnimatorController = animJ2.runtimeAnimatorController;
            animJ2.runtimeAnimatorController = cerebroTemporal;
            pendienteCambioRoles = false; 
        }

        ragdollJ1.ApagarRagdoll();
        ragdollJ2.ApagarRagdoll();

        if (animCamara != null) animCamara.Play("PaneoInicio", -1, 0f); 

        yield return new WaitForSeconds(1.5f);

        esperandoAlerta = true;
        tiempoEspera = Random.Range(3f, 6f); 
        textoEstado.text = "Preparados...";
        
        Color colorInicial = textoEstado.color;
        colorInicial.a = 1f; 
        textoEstado.color = colorInicial;

        secuenciaEnCurso = false;
    }

    IEnumerator RutinaImpacto(int ganador, int perdedor, string mensajeRonda, bool esFalsoInicio)
    {
        juegoTerminado = true;
        esperandoAlerta = false;
        secuenciaEnCurso = true; 
        
        RestarVida(perdedor);

        yield return new WaitForSeconds(0.25f);

        if (!esFalsoInicio && fuenteEfectos != null && sonidoGolpe != null) 
        {
            fuenteEfectos.PlayOneShot(sonidoGolpe);
        }

        if (partidaDefinitivaTerminada)
        {
            textoEstado.text = "¡JUGADOR " + ganador + " GANA EL JUEGO!\n<size=40%>(Presiona ESPACIO para nueva partida)</size>";
            
            if (fuenteMusica != null) fuenteMusica.Stop();
            if (fuenteEfectos != null && musicaVictoria != null) fuenteEfectos.PlayOneShot(musicaVictoria);
        }
        else
        {
            textoEstado.text = mensajeRonda + "\n<size=40%>(Presiona ESPACIO para siguiente ronda)</size>";
        }

        Color colorFinal = textoEstado.color;
        colorFinal.a = 1f;
        textoEstado.color = colorFinal;

        if (ganador == 1) 
        {
            if (particulasGolpeJ2 != null) particulasGolpeJ2.Play();
            ragdollJ2.VolarPorLosAires(-ragdollJ2.transform.forward);
        }
        else if (ganador == 2) 
        {
            if (particulasGolpeJ1 != null) particulasGolpeJ1.Play();
            ragdollJ1.VolarPorLosAires(-ragdollJ1.transform.forward);
        }

        secuenciaEnCurso = false; 
    }

    IEnumerator RutinaEsquivar()
    {
        juegoTerminado = true;
        esperandoAlerta = false;
        secuenciaEnCurso = true; 
        
        textoEstado.text = "¡ESQUIVO!\n<size=60%>Cambio de roles</size>";
        Color colorFinal = textoEstado.color;
        colorFinal.a = 1f;
        textoEstado.color = colorFinal;

        yield return new WaitForSeconds(1.5f);

        atacanteActual = (atacanteActual == 1) ? 2 : 1;
        pendienteCambioRoles = true;

        textoEstado.text += "\n<size=40%>(Presiona ESPACIO para siguiente ronda)</size>";
        secuenciaEnCurso = false; 
    }

    void RestarVida(int perdedor)
    {
        if (perdedor == 1)
        {
            vidasJ1--;
            if (vidasJ1 >= 0 && corazonesJ1[vidasJ1] != null) corazonesJ1[vidasJ1].SetActive(false); 
            if (vidasJ1 <= 0) partidaDefinitivaTerminada = true;
        }
        else
        {
            vidasJ2--;
            if (vidasJ2 >= 0 && corazonesJ2[vidasJ2] != null) corazonesJ2[vidasJ2].SetActive(false);
            if (vidasJ2 <= 0) partidaDefinitivaTerminada = true;
        }
    }

    void ReiniciarPartidaCompleta()
    {
        vidasJ1 = 3;
        vidasJ2 = 3;
        OcultarCorazones(false); 
        partidaDefinitivaTerminada = false;
        pendienteCambioRoles = false;
        
        // ¡LA SOLUCIÓN ESTÁ AQUÍ! Callamos el parlante de efectos (canción de victoria)
        if (fuenteEfectos != null)
        {
            fuenteEfectos.Stop();
        }
        
        if (fuenteMusica != null && musicaFondo != null)
        {
            fuenteMusica.clip = musicaFondo;
            fuenteMusica.Play();
        }

        StartCoroutine(SecuenciaDeInicio());
    }
}