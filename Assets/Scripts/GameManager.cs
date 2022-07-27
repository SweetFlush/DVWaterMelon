using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const float musicVolume = 0.2f;
    private const float sfxVolume = 0.2f;

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

    private bool isMusicMuted = false;
    private bool isVolumeMuted = false;

    [Header("-----UI-----")]
    public GameObject StartGroup;
    public GameObject InGameGroup;
    public GameObject GameOverGroup;
    public GameObject line;

    public Text scoreText;
    public Text maxScoreText;

    public Text endScoreText;
    public Text endBestScoreText;

    public GameObject shakeSkillBtn;
    public Text itemNumber;
    public GameObject itemMessage;

    public GameObject ShakeModeText;
    public Text TimerText;

    public GameObject youDIdIt;


    [Header("-----Volume Options-----")]
    public GameObject musicOn;
    public GameObject musicOff;
    public GameObject volumeOn;
    public GameObject volumeOff;

    [Header("-----Shaking Systems-----")]
    public int shakeItem = 0;
    public int givenTime = 1;   //500�� �ѱ�鼭 ������ ���� ������ +1
    public bool isShakeMode = false;
    public float timer = 0f;
    public float shakeTime = 3.0f;

    public BoxCollider2D[] walls;
    public PhysicsMaterial2D physicsMaterial;
    public PhysicsController physicsController;


    private void Awake()
    {
        //������ ����
        Application.targetFrameRate = 60;
        dvPool = new List<DV>();
        effectPool = new List<ParticleSystem>();

        for (int i = 0; i < PoolSize; i++)
        {
            MakeDV(i);
        }

        //�ְ�����
        if(!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }

        //�Ҹ� ���ҰŰ� �����ϱ�
        if(!PlayerPrefs.HasKey("Volume"))
        {
            PlayerPrefs.SetInt("Volume", 1);
        }
        if (!PlayerPrefs.HasKey("Music"))
        {
            PlayerPrefs.SetInt("Music", 1);
        }

        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();

        //�Ҹ� ���ҰŰ� �ҷ�����
        if (PlayerPrefs.GetInt("Volume") == 1)
            isVolumeMuted = false;
        else
            isVolumeMuted = true;

        if (PlayerPrefs.GetInt("Music") == 1)
            isMusicMuted = false;
        else
            isMusicMuted = true;

        //�Ҹ� ���ҰŰ��� ���� ���� �� ���Ұ� �����ϱ�
        CheckVolume();
        CheckMusic();

        //����ũ ��� �ʱ�ȭ
        physicsController.GetComponent<ShakeDetector>().isShake = false;
        foreach(BoxCollider2D wall in walls)
        {
            wall.sharedMaterial = null;
        }

        //UI�ʱ�ȭ
        shakeSkillBtn.SetActive(false);
        ShakeModeText.SetActive(false);

        youDIdIt.SetActive(false);
    }

    public void Update()
    {
        if(isShakeMode)
        {
            timer -= Time.deltaTime;
            TimerText.text = timer.ToString("0.0") + "��";
            if (timer <= 0)
                DeactivateShakeMode();
        }
    }

    private void LateUpdate()
    {
        scoreText.text = score.ToString();
    }

    private DV MakeDV(int id)
    {
        //effect
        GameObject instantEffect = Instantiate(effectPrefab, effectParent);
        instantEffect.name = "Effect " + id;
        ParticleSystem instantEffectParticle = instantEffect.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffectParticle);

        //������Ʈ
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

        endScoreText.text = "���� : " + scoreText.text;

        if(score > int.Parse(maxScoreText.text))    //�ְ���
        {
            endBestScoreText.text = "�ְ� ���� : " + score.ToString();
            youDIdIt.SetActive(true);
        }
        else
            endBestScoreText.text = "�ְ� ���� : " + maxScoreText.text;

        //�ְ����� ����
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

    public void MusicOnOff()
    {
        //�Ҹ�����
        if(!isMusicMuted)
        {
            bgmPlayer.volume = 0f;
            musicOn.SetActive(false);
            musicOff.SetActive(true);
            PlayerPrefs.SetInt("Music", 0);
        }
        //�Ҹ�����
        else
        {
            bgmPlayer.volume = musicVolume;
            musicOn.SetActive(true);
            musicOff.SetActive(false);
            PlayerPrefs.SetInt("Music", 1);
        }
        isMusicMuted = !isMusicMuted;
    }

    public void VolumeOnOff()
    {
        //����
        if (!isVolumeMuted)
        {
            foreach(AudioSource audio in sfxPlayers)
            {
                audio.volume = 0f;
            }
            volumeOn.SetActive(false);
            volumeOff.SetActive(true);
            PlayerPrefs.SetInt("Music", 0);

            //���ǵ� ������
            if (!isMusicMuted)
            {
                MusicOnOff();
            }
        }
        //����
        else
        {
            foreach (AudioSource audio in sfxPlayers)
            {
                audio.volume = sfxVolume;
            }
            volumeOn.SetActive(true);
            volumeOff.SetActive(false);
            PlayerPrefs.SetInt("Music", 1);

            //���ǵ� �Ѿ���
            if (isMusicMuted)
            {
                MusicOnOff();
            }
        }
        isVolumeMuted = !isVolumeMuted;
    }

    private void CheckMusic()
    {
        if (isMusicMuted)
        {
            bgmPlayer.volume = 0f;
            musicOn.SetActive(false);
            musicOff.SetActive(true);
        }
        else
        {
            bgmPlayer.volume = musicVolume;
            musicOn.SetActive(true);
            musicOff.SetActive(false);
        }
    }

    private void CheckVolume()
    {
        if (isVolumeMuted)
        {
            foreach (AudioSource audio in sfxPlayers)
            {
                audio.volume = 0f;
            }
            volumeOn.SetActive(false);
            volumeOff.SetActive(true);

            //���ǵ� ������
            if (!isMusicMuted)
            {
                MusicOnOff();
            }
        }
        else
        {
            foreach (AudioSource audio in sfxPlayers)
            {
                audio.volume = sfxVolume;
            }
            volumeOn.SetActive(true);
            volumeOff.SetActive(false);

            //���ǵ� �Ѿ���
            if (isMusicMuted)
            {
                MusicOnOff();
            }
        }
    }

    //����ũ ��� �غ�
    public void ReadyToShake()
    {
        //���鿡�� Material ����
        foreach(BoxCollider2D wall in walls)
        {
            wall.sharedMaterial = physicsMaterial;
        }

        physicsController.AddRigidbody(dvPool);
    }

    //����
    public void ActivateShakeMode()
    {
        if(!isShakeMode)
        {
            ShakeModeText.SetActive(true);

            shakeItem--;
            isShakeMode = true;
            physicsController.GetComponent<ShakeDetector>().isShake = true;
            timer = shakeTime;

            itemNumber.text = shakeItem.ToString();
            if (shakeItem == 0)
                shakeSkillBtn.SetActive(false);
        }
    }

    //���� ��
    public void DeactivateShakeMode()
    {
        //���鿡�� Material ����
        foreach (BoxCollider2D wall in walls)
        {
            wall.sharedMaterial = null;
        }
        isShakeMode = false;
        physicsController.GetComponent<ShakeDetector>().isShake = false;
        ShakeModeText.SetActive(false);
    }

    //�� 500������ ���� ������ �ֱ�
    public void CheckTheScore()
    {
        if(score / 500 == givenTime)
        {
            shakeItem++;
            givenTime++;
            shakeSkillBtn.SetActive(true);
            itemNumber.text = shakeItem.ToString();
            StartCoroutine(ItemObtainMessage());
        }
    }

    //������ �־��ٰ� �˷��ֱ�
    public IEnumerator ItemObtainMessage() {
        itemMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.5f);
        itemMessage.gameObject.SetActive(false);
    }
}
