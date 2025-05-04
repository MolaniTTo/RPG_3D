using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Action onOpen;
    public Action onClose;

    private Animator animator;
    private bool isOpen = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        onOpen += PlayOpenAnimation;
        onClose += PlayCloseAnimation;
    }

    public void Open()
    {
        onOpen?.Invoke();
        isOpen = true;
    }

    public void Close()
    {
        onClose?.Invoke();
        isOpen = false;
    }

    private void PlayOpenAnimation()
    {
        if (animator != null && !isOpen)
        {
            animator.SetTrigger("Open");
        }
    }

    private void PlayCloseAnimation()
    {
        if (animator != null && isOpen)
        {
            animator.SetTrigger("Close");
        }
    }
    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }
}
