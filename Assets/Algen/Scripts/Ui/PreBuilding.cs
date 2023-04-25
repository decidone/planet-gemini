using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PreBuilding : MonoBehaviour
{
    public static PreBuilding instance;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    GameObject gameObj = null; 
    public bool isSelect = false;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of drag slot found!");
            return;
        }

        instance = this;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        InputCheck();

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);        
    }

    protected virtual void InputCheck()
    {
        if (!EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonUp(0))
        {
            if (gameObj != null)
            {
                GameObject obj = Instantiate(gameObj);
                obj.transform.position = this.transform.position;

                BuildingInfo.instance.BuildingEnd();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ReSetImage();
        }
    }

    public void SetImage(Sprite getSprite, GameObject game)
    {
        gameObj = game;
        spriteRenderer.sprite = getSprite;
        isSelect = true;
    }

    public void ReSetImage()
    {
        spriteRenderer.sprite = null;
        isSelect = false;
        gameObj = null;
        gameObject.SetActive(false);
    }
}
