using UnityEngine;
using System.Collections.Generic;

public class ControladorRagdoll : MonoBehaviour
{
    private Animator anim;
    private Rigidbody[] huesosFisicos;

    // Memoria para los huesos internos
    private class HuesoMemoria 
    {
        public Transform transform;
        public Vector3 posicionOriginal;
        public Quaternion rotacionOriginal;
    }
    private List<HuesoMemoria> memoriaHuesos = new List<HuesoMemoria>();

    // NUEVO: Memoria para el personaje entero en el mapa
    private Vector3 posicionRaizMundo;
    private Quaternion rotacionRaizMundo;

    [Header("Configuración del Vuelo")]
    public float fuerzaDeImpacto = 15f; 
    public float fuerzaHaciaArriba = 5f; 

    void Awake()
    {
        anim = GetComponent<Animator>();
        huesosFisicos = GetComponentsInChildren<Rigidbody>();

        // 1. Memorizamos dónde está parado el personaje en el mundo
        posicionRaizMundo = transform.position;
        rotacionRaizMundo = transform.rotation;

        // 2. Memorizamos la posición de sus huesos
        foreach (Transform hueso in GetComponentsInChildren<Transform>())
        {
            memoriaHuesos.Add(new HuesoMemoria { 
                transform = hueso, 
                posicionOriginal = hueso.localPosition, 
                rotacionOriginal = hueso.localRotation 
            });
        }

        ApagarRagdoll();
    }

    public void ApagarRagdoll()
    {
        foreach (Rigidbody hueso in huesosFisicos)
        {
            hueso.isKinematic = true;
        }

        transform.position = posicionRaizMundo;
        transform.rotation = rotacionRaizMundo;

        foreach (HuesoMemoria hueso in memoriaHuesos)
        {
            hueso.transform.localPosition = hueso.posicionOriginal;
            hueso.transform.localRotation = hueso.rotacionOriginal;
        }

        // --- EL ARREGLO ESTÁ AQUÍ ---
        if (anim != null) 
        {
            anim.enabled = true;
            anim.Rebind(); // Reinicia el cerebro al bloque por defecto (Idle)
            anim.Update(0f); // Fuerza a Unity a dibujar el cambio instantáneamente
        }
    }

    public void VolarPorLosAires(Vector3 direccionDelGolpe)
    {
        if (anim != null) anim.enabled = false;

        foreach (Rigidbody hueso in huesosFisicos)
        {
            hueso.isKinematic = false;
            Vector3 fuerzaTotal = (direccionDelGolpe * fuerzaDeImpacto) + (Vector3.up * fuerzaHaciaArriba);
            hueso.AddForce(fuerzaTotal, ForceMode.Impulse);
        }
    }
}