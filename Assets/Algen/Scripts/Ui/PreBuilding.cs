using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (Input.GetMouseButton(0))
        {
            if (gameObj != null)
            {
                GameObject obj = Instantiate(gameObj);
                obj.transform.position = this.transform.position;
                //건설 시 인벤토리 아이템 사용
                //건설 중 모드 생성해야함
            }
        }
        else if (Input.GetMouseButton(1))
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

    void ReSetImage()
    {
        spriteRenderer.sprite = null;
        isSelect = false;
        gameObj = null;
        gameObject.SetActive(false);
    }
}
