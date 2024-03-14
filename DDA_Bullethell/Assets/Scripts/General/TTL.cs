using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTL : MonoBehaviour
{
    void Start()
    {
        var particle = gameObject.GetComponent<ParticleSystem>();
        Destroy(gameObject, particle.main.duration);
    }
}
