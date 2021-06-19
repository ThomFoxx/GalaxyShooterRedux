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
    private Image _livesDisplay;
    [SerializeField]
    private TMP_Text _gameOverText;
    [SerializeField]
    [Range(0f, 5f)]
    private float _gameOverFlickerSpeed;
    private Player _player;
    [SerializeField]
    private Sprite[] _ammoTypes;
    [SerializeField]
    private Image _ammoDisplay;
    [SerializeField]
    private TMP_Text _ammoCountDisplay;
    [SerializeField]
    private TMP_Text _waveDisplayText;
    [SerializeField]
    private Sprite[] _missileSprites;
    [SerializeField]
    private Image _missileDisplay;
    [SerializeField]
    private Image _magnetDisplay;
    [SerializeField]
    private TMP_Text _magnetText;


    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("UI Manager is Null!!!");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        Enemy.OnEnemyDeath += EnemyDeath;
        Teleporter.OnEnemyDeath += EnemyDeath;
        Aggressive_Enemy.OnEnemyDeath += EnemyDeath;
        Mine_Layer.OnEnemyDeath += EnemyDeath;
        Mine.OnMineDestroy += EnemyDeath;
        ShieldUnit.OnEnemyDeath += EnemyDeath;
        Player.OnPlayerDamaged += PlayerDamaged;
        Player.OnPlayerDeath += PlayerDeath;
        Player.OnAmmoTypeChange += FireType;
        Player.OnReloadAmmo += Reload;
    }

    void Start()
    {
        _scoreText.text = "Score: " + 0;
        _gameOverText.gameObject.SetActive(false);
        StartCoroutine(UpdateMissiles(0));
        _magnetDisplay.gameObject.SetActive(false);
    }

    private IEnumerator UpdateScore()
    {
        yield return new WaitForEndOfFrame();
        _scoreText.text = "Score: " + _player.GetScore();
    }

    public IEnumerator UpdateLives(int lives)
    {
        yield return new WaitForEndOfFrame();
        if(lives < _livesSprites.Length && lives >=0)
        _livesDisplay.sprite = _livesSprites[lives];
    }

    public IEnumerator UpdateMissiles(int missiles)
    {
        yield return new WaitForEndOfFrame();
        if (missiles > 0)
        {
            _missileDisplay.enabled = true;
            _missileDisplay.sprite = _missileSprites[missiles - 1];
        }
        else if (missiles == 0)
            _missileDisplay.enabled = false;
    }

    private IEnumerator UpdateAmmoCount(int current, int max)
    {
        yield return new WaitForEndOfFrame();
        _ammoCountDisplay.text = current + " / " + max;
    }

    private IEnumerator UpdateAmmoType(int type)
    {
        yield return new WaitForEndOfFrame();
        _ammoDisplay.sprite = _ammoTypes[type];
    }

    public IEnumerator DisplayGameOver()
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

    private IEnumerator DisplayWave()
    {
        _waveDisplayText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        _waveDisplayText.gameObject.SetActive(false);
    }

    private IEnumerator MagnetDisplay(int time)
    {
        yield return new WaitForEndOfFrame();
        if (time == 0)
            _magnetDisplay.gameObject.SetActive(false);
        else
            _magnetDisplay.gameObject.SetActive(true);

        if (_magnetText.text != time.ToString())
            _magnetText.text = time.ToString();
    }

    public void MagnetUpdate(int time)
    {
        StartCoroutine(MagnetDisplay(time));
    }

    public void MissileUpdate(int missiles)
    {
        StartCoroutine(UpdateMissiles(missiles));
    }

    private void EnemyDeath(int notUsed, Transform notUsed2)
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

    private void Reload(int current, int max)
    {
        StartCoroutine(UpdateAmmoCount(current, max));
    }

    private void FireType(int type)
    {
        StartCoroutine(UpdateAmmoType(type));
    }

    public void UpdateWaveDisplay(int Wave)
    {
        _waveDisplayText.text = "Wave " + Wave;
        StartCoroutine(DisplayWave());
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= EnemyDeath;
        Teleporter.OnEnemyDeath -= EnemyDeath;
        Aggressive_Enemy.OnEnemyDeath -= EnemyDeath;
        Mine_Layer.OnEnemyDeath -= EnemyDeath;
        ShieldUnit.OnEnemyDeath -= EnemyDeath;
        Mine.OnMineDestroy -= EnemyDeath;
        Player.OnPlayerDamaged -= PlayerDamaged;
        Player.OnPlayerDeath -= PlayerDeath;
        Player.OnAmmoTypeChange -= FireType;
        Player.OnReloadAmmo -= Reload;
    }
}
