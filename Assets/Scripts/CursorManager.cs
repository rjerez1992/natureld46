using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class CursorManager : MonoBehaviour
{
    public GameObject CursorImageTree;
    public GameObject CursorImageRock;
    public GameObject CursorImageSheep;
    public PlayerController Player;

    void Start()
    {

    }

    void Update()
    {
        if (Player.CurrentIntent != PlayerAction.None)
        {
            Vector3 CursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            switch (Player.CurrentIntent)
            {
                case PlayerAction.GenerateTrees:
                    CursorImageTree.SetActive(true);
                    CursorImageRock.SetActive(false);
                    CursorImageSheep.SetActive(false);
                    CursorImageTree.transform.position = new Vector3(CursorWorldPosition.x, CursorWorldPosition.y, 1);
                    break;
                case PlayerAction.GenerateRocks:
                    CursorImageTree.SetActive(false);
                    CursorImageRock.SetActive(true);
                    CursorImageSheep.SetActive(false);
                    CursorImageRock.transform.position = new Vector3(CursorWorldPosition.x, CursorWorldPosition.y, 1);
                    break;
                case PlayerAction.GenerateAnimals:
                    CursorImageTree.SetActive(false);
                    CursorImageRock.SetActive(false);
                    CursorImageSheep.SetActive(true);
                    CursorImageSheep.transform.position = new Vector3(CursorWorldPosition.x, CursorWorldPosition.y, 1);
                    break;
                default:
                    break;
            }
        }
        else {
            CursorImageTree.SetActive(false);
            CursorImageRock.SetActive(false);
            CursorImageSheep.SetActive(false);
        }
    }
}
