using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//L: More Boomo Scripts Yay.
public class OnTriggerEnter : MonoBehaviour
{
    public UnityEvent onEnter;
    public UnityEvent onExit;

    [SerializeField] private bool onlyTriggerOnce;
    
    private bool _hasEntered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (onlyTriggerOnce && _hasEntered) return;
            onEnter.Invoke();
            _hasEntered = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            onExit.Invoke();
        }
    }
}
