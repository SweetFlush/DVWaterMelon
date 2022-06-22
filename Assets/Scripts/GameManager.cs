using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("-----Object Pooling-----")]
    public Transform parent;
    public GameObject dvPrefab;
    public List<DV> dvPool;
    [Range(1, 30)]
    public int PoolSize;

    private int poolCursor;
    private DV lastDV;

    public Transform effectParent;
    public GameObject effectPrefab;
    public List<ParticleSystem> effectPool;

    [Header("-----Variables-----")]
    public int score;
    public int maxLevel;
    public int random;
    public bool isOver = false;

    [Header("-----Audio-----")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayers;
    public AudioClip[] sfxClips;
    int sfxCursor;
    public enum Sfx  { levelUp, Next, Attach, Button, GameOver }

    [Header("-----UI-----")]
    public GameObject StartGroup;
    public GameObject InGameGroup;
    public GameObject GameOverGroup;
    public GameObject line;

    public Text scoreText;
    public Text maxScoreText;

    public Text endScoreText;
    public Text endBestScoreText;
    public GameObject youDIdIt;


    private void Awake()
    {
        //프레임 설정
        Application.targetFrameRate = 60;
        dvPool = new List<DV>();
        effectPool = new List<ParticleSystem>();

        for (int i = 0; i < PoolSize; i++)
        {
            MakeDV(i);
        }

        //최고점수
        if(!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    private DV MakeDV(int id)
    {
        //effect
        GameObject instantEffect = Instantiate(effectPrefab, effectParent);
        instantEffect.name = "Effect " + id;
        ParticleSystem instantEffectParticle = instantEffect.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffectParticle);

        //오브젝트
        GameObject instant = Instantiate(dvPrefab, parent);
        DV instantDV = instant.GetComponent<DV>();
        instant.name = "DyeoV " + id;
        dvPool.Add(instantDV);

        instantDV.gameManager = this;
        instantDV.effect = instantEffectParticle;

        return instantDV;
    }

    private DV GetDV()
    {
        for (int i = poolCursor; i < dvPool.Count; i++)
        {
            poolCursor = (poolCursor + 1) % dvPool.Count;
            if (!dvPool[poolCursor].gameObject.activeSelf)
            {
                return dvPool[poolCursor];
            }
        }

        return MakeDV(dvPool.Count);
    }

    public void GameStart()
    {
        StartGroup.SetActive(false);
        InGameGroup.SetActive(true);
        line.SetActive(true);

        PlaySfx(Sfx.Button);
        bgmPlayer.Play();
        Invoke("NextDV", 1.5f);
    }

    private void NextDV()
    {
        if (isOver)
            return;
        lastDV = GetDV();
        lastDV.level = RandomLevel();
        lastDV.gameObject.SetActive(true);

        StartCoroutine("WaitForNextDV");

        PlaySfx(Sfx.Next);
    }

    private IEnumerator WaitForNextDV()
    {
        while (lastDV != null)
        {
            yield return null;

        }

        yield return new WaitForSeconds(0.5f);
        NextDV();
    }

    public void TouchDown()
    {
        if (lastDV == null)
            return;

        lastDV.Drag();
    }

    public void TouchUp()
    {
        if (lastDV == null)
            return;

        lastDV.Drop();
        lastDV = null;
    }

    private int RandomLevel()
    {
        random = 0;
        if (maxLevel < 3)
        {
            random = Random.Range(0, 2);
        }
        else if (maxLevel < 4 && maxLevel >= 3)
        {
            random = Random.Range(0, 3);
        }
        else if (maxLevel < 5 && maxLevel >= 4)
        {
            random = Random.Range(0, 4);
        }
        else if (maxLevel >= 5)
        {
            random = Random.Range(0, 5);
        }

        switch (random)
        {
            default:
                return 0;
            case 0:
                return 0;
            case 1: case 2:
                return 1;
            case 3: case 4:
                return 2;
            case 5:
                return 3;
        }
    }

    public void GameOver()
    {
        isOver = true;
        bgmPlayer.Stop();
        StartCoroutine("ResultRoutine");
    }

    private IEnumerator ResultRoutine()
    {
        for (int i = 0; i < dvPool.Count; i++)
        {
            if (dvPool[i].gameObject.activeSelf)
            {
                dvPool[i].Hide(Vector3.up * 100);
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(1f);

        endScoreText.text = "점수 : " + scoreText.text;

        if(score > int.Parse(maxScoreText.text))    //최고기록
        {
            endBestScoreText.text = "최고 점수 : " + score.ToString();
            youDIdIt.SetActive(true);
        }
        else
            endBestScoreText.text = "최고 점수 : " + maxScoreText.text;

        //최고점수 갱신
        int maxScore = Mathf.Max(PlayerPrefs.GetInt("MaxScore"), score);
        PlayerPrefs.SetInt("MaxScore", maxScore);

        GameOverGroup.SetActive(true);
        PlaySfx(Sfx.GameOver);
    }

    public void Reset()
    {
        PlaySfx(Sfx.Button);
        StartCoroutine("ResetRoutine");
    }

    private IEnumerator ResetRoutine()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(0);
    }

    public void PlaySfx(Sfx sfx)
    {
        sfxCursor = (sfxCursor + 1) % sfxPlayers.Length;
        switch (sfx) 
        {
            default:
                break;
            case Sfx.levelUp:
                sfxPlayers[sfxCursor].clip = sfxClips[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayers[sfxCursor].clip = sfxClips[3];
                break;
            case Sfx.Attach:
                sfxPlayers[sfxCursor].clip = sfxClips[4];
                break;
            case Sfx.Button:
                sfxPlayers[sfxCursor].clip = sfxClips[5];
                break;
            case Sfx.GameOver:
                sfxPlayers[sfxCursor].clip = sfxClips[6];
                break;
        }

        sfxPlayers[sfxCursor].Play();
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}
