using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _scoreText;
    [SerializeField]
    private Sprite[] _livesSprites;
    [SerializeField]
    private Image _livesDispaly;
    [SerializeField]
    private TMP_Text _gameOverText;
    [SerializeField]
    [Range(0f, 5f)]
    private float _gameOverFlickerSpeed;
    private Player _player;


    private void OnEnable()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        Enemy.OnEnemyDeath += EnemyDeath;
        Player.OnPlayerDamaged += PlayerDamaged;
        Player.OnPlayerDeath += PlayerDeath;
    }

    void Start()
    {
        _scoreText.text = "Score: " + 0;
        _gameOverText.gameObject.SetActive(false);
    }    

    private IEnumerator UpdateScore()
    {
        yield return new WaitForEndOfFrame();
        _scoreText.text = "Score: " + _player.GetScore();
    }

    private IEnumerator UpdateLives(int lives)
    {
        yield return new WaitForEndOfFrame();
        _livesDispaly.sprite = _livesSprites[lives];
    }

    private IEnumerator DisplayGameOver()
    {
        yield return new WaitForEndOfFrame();
        _gameOverText.gameObject.SetActive(true);
        byte alpha = 0;
        _gameOverText.color = new Color32(255, 0, 0, 0);
        while (GameManager.Instance.IsGameOver())
        {
           
            while (alpha < 255)
            {
                yield return new WaitForEndOfFrame();
                alpha += (byte)(Time.deltaTime * 100 * _gameOverFlickerSpeed);
                _gameOverText.color = new Color32(255, 0, 0, alpha);
            }

            _gameOverText.color = new Color32(255, 0, 0, 255);

            while (alpha > 0)
            {
                yield return new WaitForEndOfFrame();
                alpha -= (byte)(Time.deltaTime * 100 * _gameOverFlickerSpeed);
                _gameOverText.color = new Color32(255, 0, 0, alpha);
            }

            _gameOverText.color = new Color32(255, 0, 0, 0);
            yield return new WaitForEndOfFrame();
        }
    }

    private void EnemyDeath(int notUsed)
    {
        StartCoroutine(UpdateScore());
    }

    private void PlayerDamaged(int lives)
    {
        StartCoroutine(UpdateLives(lives));
    }

    private void PlayerDeath()
    {
        GameManager.Instance.GameOver(true);
        StartCoroutine(DisplayGameOver());
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= EnemyDeath;
        Player.OnPlayerDamaged -= PlayerDamaged;
        Player.OnPlayerDeath -= PlayerDeath;
    }
}
