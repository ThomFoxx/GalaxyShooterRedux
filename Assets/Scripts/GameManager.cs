using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private bool _isGameOver = false;
    [SerializeField]
    private bool _isHorizontalFlight = false;

    private static GameManager _instance;
    public static GameManager Instance
    { 
        get
        {
            if (_instance == null)
                Debug.LogError("Game Manager is Null!!!");

            return _instance;
        } 
    }
    
    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += PlayerDeath;
    }

    private void Start()
    {
        AudioManager.Instance.PlayMusic(0, .5f);   
    }

    private void Update()
    {
        if(_isGameOver && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    public bool IsHorizontalFlight()
    {
        return _isHorizontalFlight;
    }

    public void GameOver(bool flag)
    {
        _isGameOver = flag;
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }

    private void PlayerDeath()
    {
        StartCoroutine(TitleCountDown());
    }

    private IEnumerator TitleCountDown()
    {
        yield return new WaitForSecondsRealtime(60);
        SceneManager.LoadScene(0);
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= PlayerDeath;
    }
}
