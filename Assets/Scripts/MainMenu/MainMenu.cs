using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainMenu;
    [SerializeField]
    private GameObject _controlsMenu;


    public void LoadGame()
    {
        SceneManager.LoadScene(1); //GalaxyShooterRedux Scene
    }

    public void ControlMenu()
    {
        _mainMenu.SetActive(false);
        _controlsMenu.SetActive(true);
    }

}
