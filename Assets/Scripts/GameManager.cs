using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;
public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static GameManager Instance { get; private set; }

    private int score;

    [Header("Waves")]
    [SerializeField] private float maxTime;
    private float currentTimeLeft;
    [SerializeField] private GameObject[] waveParents;
    [SerializeField] private int numTargetsPerWave;
    [SerializeField] private int waveIncrementAmount;
    private int currentWave = 0;
    [Header("UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timer;
    [SerializeField] private GameObject endScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Text finalScore;
    [SerializeField] private Text finalTargetsHit;
    [SerializeField] private Text waveText;
    [SerializeField] private GameObject waveObject;
    private WaitForSeconds textWait;
    private WaitForSeconds firstDelay;
    private float waitTime = 0.2f;
    private bool gameIsRunning = true;
    private int numTargetsHit;
    private int totalNumTargetsHit;

    public bool paused = false;
    [SerializeField] StudioEventEmitter Music;
    [SerializeField] StudioEventEmitter Ding;
    [SerializeField] StudioEventEmitter Voice;
    [SerializeField] StudioEventEmitter GameOver;

    public Vector2 sensitivity = new Vector2(0.4f, 0.4f);
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            
            Destroy(this);
        }
        else
        {
            
            Instance = this;
        }
        currentTimeLeft = maxTime;
        textWait = new WaitForSeconds(waitTime);
        firstDelay = new WaitForSeconds(2f);

        FMODUnity.RuntimeManager.GetBus("bus:/").setVolume(1f);
        FMODUnity.RuntimeManager.GetBus("bus:/Music").setVolume(1f);
        FMODUnity.RuntimeManager.GetBus("bus:/SFX").setVolume(1f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;


    }
    private void Start()
    {
        for (int i = 1; i < waveParents.Length; i++)
        {
            
            waveParents[i].SetActive(false);
        }
        StartCoroutine(WaveText());
    }
    // Update is called once per frame
    void Update()
    {

        if (gameIsRunning)
        {
            if (!Music.IsPlaying()) Music.Play();
            currentTimeLeft -= Time.deltaTime;
            currentTimeLeft = Mathf.Clamp(currentTimeLeft, 0, maxTime);
            int minutes = Mathf.FloorToInt(currentTimeLeft / 60F);
            int seconds = Mathf.FloorToInt(currentTimeLeft - minutes * 60);

            string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);

            timer.text = niceTime;

            float OldValue;
            FMODUnity.RuntimeManager.StudioSystem.getParameterByName("Streak", out OldValue);
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Streak", OldValue - Time.deltaTime * 0.2f);

        }
        if(currentTimeLeft <= 0 && gameIsRunning)
        {
            EndGame();
        }

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)) && gameIsRunning) {
            Pause();
        }

    }

    public void StartGame()
    {
        if (!gameIsRunning)
        {
            gameIsRunning = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void Pause() {
        paused = !paused;
            
            if (paused) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                
            }
            else {
                Time.timeScale = 1;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            }

            pauseScreen.SetActive(paused);

    }
    private void EndGame()
    {

        GameOver.Play();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        gameIsRunning = false;
        endScreen.SetActive(true);
        finalScore.text = "final score: " + score.ToString();
        finalTargetsHit.text = "targets hit: " + totalNumTargetsHit.ToString();
        PlayerInput.GameOver();
        
    }

    public void AddScore(int amount)
    {
        if (gameIsRunning)
        {
            if (amount > 0)
            {
                Ding.Play();
                score += amount;
                numTargetsHit++;
                totalNumTargetsHit++;
                scoreText.text = score.ToString();

                if(numTargetsHit >= numTargetsPerWave && currentWave != waveParents.Length-1)
                {
                    numTargetsHit = 0;
                    NextWave();

                }
            }
        }
    }

    private void NextWave()
    {
        waveParents[currentWave].SetActive(false);
        currentWave++;
        numTargetsPerWave += waveIncrementAmount;
        waveParents[currentWave].SetActive(true);
        StartCoroutine(WaveText());
    }

    private IEnumerator WaveText()
    {
        if (currentWave > 0) Voice.Play();
        Debug.Log("text fade started");
        waveObject.SetActive(true);
        waveText.color = new Color(waveText.color.r, waveText.color.b, waveText.color.g, 1);
        waveText.text = "wave " + (currentWave +1)+ "/" + waveParents.Length;
        yield return firstDelay;
        for (float i = 1; i > 0; i -= 0.1f)
        {
            
            waveText.color = new Color(waveText.color.r, waveText.color.b, waveText.color.g, i);
            yield return textWait;
        }
        waveObject.SetActive(false);
    }
    public void EndButton()
    {
        Debug.Log("button");
        Music.Stop();
        SceneManager.LoadScene("Menu");
    }

    public void setSensX(float sens) {
        sensitivity.x = sens;
        
    }

    public void setSensY(float sens) {
        sensitivity.y = sens;
        
    }

    public void MasterVolume(float vol) {
        FMOD.Studio.Bus Bus = FMODUnity.RuntimeManager.GetBus("bus:/");
        Bus.setVolume(vol);
        Bus.setMute(vol == 0);
    }
    public void MusicVolume(float vol) {
        FMOD.Studio.Bus Bus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        Bus.setVolume(vol);
        Bus.setMute(vol == 0);
    }

    public void SFXVolume(float vol) {
        FMOD.Studio.Bus Bus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
        Bus.setVolume(vol);
        Bus.setMute(vol == 0);
    }

}
