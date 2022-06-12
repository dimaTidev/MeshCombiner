using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeshBake_DoneEvent : MonoBehaviour
{
    [SerializeField] UnityEvent onBakeDone = null;

    public void OnBakeDone() 
    {
        onBakeDone?.Invoke();
        //Debug.Log("Invoke bake end!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }
}
