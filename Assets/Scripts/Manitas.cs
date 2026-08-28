using UnityEngine;
using TMPro;
using System.Collections;

public class Manitas3D : MonoBehaviour
{
    [Header("Interfaz Minimalista")]
    public TextMeshProUGUI textoEstado;
    public GameObject panelInstrucciones; 
    public float velocidadFade = 1.5f;

    [Header("Cámara Cinematográfica")]
    public Animator animCamara; 

    [Header("Animadores (Los Cerebros)")]
    public Animator animJ1; 
    public Animator animJ2;

    [Header("Personajes (Ragdoll)")]
    public ControladorRagdoll ragdollJ1;
    public ControladorRagdoll ragdollJ2;

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
        
        // Iniciamos la cuenta regresiva para quitar el panel
        StartCoroutine(RutinaInstrucciones());
    }

    // --- NUEVA RUTINA PARA EL PANEL ---
    IEnumerator RutinaInstrucciones()
    {
        // Esperamos exactamente 2 segundos
        yield return new WaitForSeconds(2f);
        
        // Apagamos el panel y arrancamos el juego
        if (panelInstrucciones != null) panelInstrucciones.SetActive(false);
        juegoEmpezado = true;
        StartCoroutine(SecuenciaDeInicio());
    }

    void Update()
    {
        // Si el juego no ha empezado (estamos en los 3 segundos del panel), ignoramos el teclado
        if (!juegoEmpezado) return; 

        if (juegoTerminado)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !secuenciaEnCurso)
            {
                StartCoroutine(SecuenciaDeInicio());
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
                int ganador = (teclaPresionada == 1) ? 2 : 1;
                
                if (teclaPresionada == 1) animJ1.SetTrigger("FalsoInicio");
                else animJ2.SetTrigger("FalsoInicio");

                // Texto sin la "Ó"
                StartCoroutine(RutinaImpacto(ganador, "¡J" + teclaPresionada + " SE ADELANTO!\nGANA JUGADOR " + ganador));
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
                if (teclaPresionada == 1) animJ1.SetTrigger("Ataque");
                else animJ2.SetTrigger("Ataque");
                
                StartCoroutine(RutinaImpacto(teclaPresionada, "¡GANA JUGADOR " + teclaPresionada + "!"));
            }
            else
            {
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

        if (animCamara != null) 
        {
            animCamara.Play("PaneoInicio", -1, 0f); 
        }

        yield return new WaitForSeconds(1.5f);

        esperandoAlerta = true;
        tiempoEspera = Random.Range(3f, 6f); 
        textoEstado.text = "Preparados...";
        
        Color colorInicial = textoEstado.color;
        colorInicial.a = 1f; 
        textoEstado.color = colorInicial;

        secuenciaEnCurso = false;
    }

    IEnumerator RutinaImpacto(int ganador, string mensaje)
    {
        juegoTerminado = true;
        esperandoAlerta = false;
        secuenciaEnCurso = true; 
        
        textoEstado.text = mensaje + "\n<size=40%>(Presiona ESPACIO para reiniciar)</size>";
        Color colorFinal = textoEstado.color;
        colorFinal.a = 1f;
        textoEstado.color = colorFinal;

        yield return new WaitForSeconds(0.25f);

        if (ganador == 1) ragdollJ2.VolarPorLosAires(-ragdollJ2.transform.forward);
        else if (ganador == 2) ragdollJ1.VolarPorLosAires(-ragdollJ1.transform.forward);

        secuenciaEnCurso = false; 
    }

    IEnumerator RutinaEsquivar()
    {
        juegoTerminado = true;
        esperandoAlerta = false;
        secuenciaEnCurso = true; 
        
        // Texto sin la "Ó"
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
}