using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
  
    public static UIManager instance;

    public GameObject keyUp;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetKeyUp(bool state, Vector2 position = new Vector2())
    {
        if (state)
        {
            if (keyUp.activeInHierarchy)
                return;
        }
        keyUp.transform.position = position + new Vector2(1, 0);
        keyUp.SetActive(state);
    }
}
