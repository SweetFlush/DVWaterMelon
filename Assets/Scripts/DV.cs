using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DV : MonoBehaviour
{
    public bool isDrag;
    public bool isMerge;
    public bool isAttach;
    public float deadTime;


    public int level;

    public GameManager gameManager;
    public ParticleSystem effect;

    Rigidbody2D rigid;
    CircleCollider2D circleCollider;
    SpriteRenderer spriteRenderer;
    Animator anim;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        circleCollider = GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        anim.SetInteger("Level", level);
        EffectPlay();
        rigid.mass = level+1;
    }

    private void Update()
    {
        if(isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float LeftBorder = -4.2f + transform.localScale.x / 2f;
            float RightBorder = 4.2f + transform.localScale.x / 2f;

            if(mousePos.x < LeftBorder)
            {
                mousePos.x = LeftBorder;
            }
            else if(mousePos.x > RightBorder)
            {
                mousePos.x = RightBorder;
            }

            transform.position = Vector3.Lerp(transform.position, new Vector3(mousePos.x, 7.8f, 0), 0.5f);
        }

    }

    public void Drag()
    {
        isDrag = true;
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall")
            StartCoroutine("AttachRoutine");
    }

    private IEnumerator AttachRoutine()
    {
        if(isAttach)
            yield break;    //return

        isAttach = true;
        gameManager.PlaySfx(GameManager.Sfx.Attach);

        yield return new WaitForSeconds(0.2f);
        isAttach = false;

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "DV")
        {
            DV other = collision.gameObject.GetComponent<DV>();
            if(level == other.level && !isMerge && !other.isMerge && level < 10) //합체
            {
                float myX = transform.position.x;
                float myY = transform.position.y;
                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                
                //내가 위에 있거나 혹은 같은 선상에서 오른쪽에 있을때
                if(myY < otherY || (myY == otherY && myX > otherX))    //내가 레벨업
                {
                    other.Hide(transform.position);
                    LevelUp();
                }
            }
        }

    }

    private void LevelUp()
    {
        isMerge = true;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0f;

        StartCoroutine("Merge");
    }

    private IEnumerator Merge()
    {
        yield return new WaitForSeconds(0.1f);
        anim.SetInteger("Level", level + 1);
        gameManager.PlaySfx(GameManager.Sfx.levelUp);
        EffectPlay();

        yield return new WaitForSeconds(0.2f);
        level++;
        gameManager.maxLevel = Mathf.Max(gameManager.maxLevel, level);
        gameManager.CheckTheScore();
        //rigid.mass = level + 1;
        isMerge = false;
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;
        rigid.simulated = false;
        circleCollider.enabled = false;

        StartCoroutine("HideRoutine", targetPos);

        //게임오버이펙트
        if (targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }
    }

    private IEnumerator HideRoutine(Vector3 targetPos)
    {
        int timeCount = 0;
        while(timeCount < 20)
        {
            timeCount++;
            if (targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            else if (targetPos == Vector3.up * 100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.25f);
            }

            yield return null;
        }
        gameManager.score += (int)Mathf.Pow(2, level);

        gameObject.SetActive(false);
        isMerge = false;
    }

    //뎌붕이 속성 초기화
    private void OnDisable()
    {
        level = 0;
        deadTime = 0;

        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        rigid.simulated = false;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        circleCollider.enabled = true;
    }

    private void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale * 10;
        if (effect.transform.localScale.x < 1)
            effect.transform.localScale = new Vector3(1, 1, 1);
        effect.Play();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;
            if (deadTime > 2)
            {
                spriteRenderer.color = new Color(0.9f, 0.2f, 0.2f);
            }
            if (deadTime > 5)
            {
                gameManager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        deadTime = 0;
        spriteRenderer.color = Color.white;
    }
}
