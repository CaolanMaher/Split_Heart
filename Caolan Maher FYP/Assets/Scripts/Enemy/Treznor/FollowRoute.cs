using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowRoute : MonoBehaviour
{

    TreznorBossFight bossFight;

    Animator anim;

    // Different routes and their starting points
    [SerializeField] GameObject[] routesFrom1;
    [SerializeField] GameObject[] routesFrom2;
    [SerializeField] GameObject[] routesFrom3;
    [SerializeField] GameObject[] routesFrom4;
    [SerializeField] GameObject[] routesFrom5;
    [SerializeField] GameObject[] routeFromGP;

    [SerializeField] Transform control1_For_GP, control2_For_GP;

    public int startingPoint;

    private int routeToFollow;

    public float tParam;

    private Vector2 bossPosition;

    private float movementSpeed = 0.75f;

    [SerializeField] Transform rayCastPoint;
    bool groundPoundCheckDone;

    public bool shouldJump;
    private bool shouldShootWhileJumping;

    [SerializeField] bool shouldCheckForGroundPound;
    bool justGroundPounded;

    Rigidbody2D rb;

    AudioSource audioSource;
    [SerializeField] AudioClip groundPoundStartSound;

    GameObject dangerIcon;

    // Start is called before the first frame update
    void Start()
    {
        tParam = 0;
        shouldJump = true;
        shouldShootWhileJumping = false;

        rb = GetComponent<Rigidbody2D>();

        routesFrom1 = GameObject.FindGameObjectsWithTag("Route_From_1");
        routesFrom2 = GameObject.FindGameObjectsWithTag("Route_From_2");
        routesFrom3 = GameObject.FindGameObjectsWithTag("Route_From_3");
        routesFrom4 = GameObject.FindGameObjectsWithTag("Route_From_4");
        routesFrom5 = GameObject.FindGameObjectsWithTag("Route_From_5");
        routeFromGP = GameObject.FindGameObjectsWithTag("Route_From_GP");

        control1_For_GP = routeFromGP[0].transform.GetChild(0);
        control2_For_GP = routeFromGP[0].transform.GetChild(1);

        bossFight = GetComponent<TreznorBossFight>();

        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        dangerIcon = GameObject.FindGameObjectWithTag("DangerIcon");
    }

    // Update is called once per frame
    void Update()
    {

        if(!anim.GetBool("isInAir"))
        {
            control1_For_GP.position = transform.position;
            control2_For_GP.position = new Vector2(control1_For_GP.position.x, control1_For_GP.position.y + 5);
        }

        if (shouldJump && bossFight.bossFightStarted)
        {

            if (!justGroundPounded)
            {
                // pick between 1 of 2 routes
                routeToFollow = Random.Range(0, 2);
            }
            else
            {
                routeToFollow = 0;
                justGroundPounded = false;
            }

            int shouldShootInt = Random.Range(0, 11);
            if(shouldShootInt < 7)
            {
                shouldShootWhileJumping = true;
            }
            else
            {
                shouldCheckForGroundPound = true;
            }

            StartCoroutine(GoByRoute(routeToFollow));
        }
    }

    IEnumerator GoByRoute(int routeNumber)
    {
        rb.gravityScale = 0;

        anim.SetBool("isInAir", true);
        shouldJump = false;

        Vector2 p0 = Vector2.zero;
        Vector2 p1 = Vector2.zero;
        Vector2 p2 = Vector2.zero;
        Vector2 p3 = Vector2.zero;

        switch (startingPoint)
        {
            // ground pound
            case 0:
                p0 = routeFromGP[routeNumber].transform.GetChild(0).position;
                p1 = routeFromGP[routeNumber].transform.GetChild(1).position;
                p2 = routeFromGP[routeNumber].transform.GetChild(2).position;
                p3 = routeFromGP[routeNumber].transform.GetChild(3).position;
                break;

            case 1:
                p0 = routesFrom1[routeNumber].transform.GetChild(0).position;
                p1 = routesFrom1[routeNumber].transform.GetChild(1).position;
                p2 = routesFrom1[routeNumber].transform.GetChild(2).position;
                p3 = routesFrom1[routeNumber].transform.GetChild(3).position;
                break;
            case 2:
                p0 = routesFrom2[routeNumber].transform.GetChild(0).position;
                p1 = routesFrom2[routeNumber].transform.GetChild(1).position;
                p2 = routesFrom2[routeNumber].transform.GetChild(2).position;
                p3 = routesFrom2[routeNumber].transform.GetChild(3).position;
                break;
            case 3:
                p0 = routesFrom3[routeNumber].transform.GetChild(0).position;
                p1 = routesFrom3[routeNumber].transform.GetChild(1).position;
                p2 = routesFrom3[routeNumber].transform.GetChild(2).position;
                p3 = routesFrom3[routeNumber].transform.GetChild(3).position;
                break;
            case 4:
                p0 = routesFrom4[routeNumber].transform.GetChild(0).position;
                p1 = routesFrom4[routeNumber].transform.GetChild(1).position;
                p2 = routesFrom4[routeNumber].transform.GetChild(2).position;
                p3 = routesFrom4[routeNumber].transform.GetChild(3).position;
                break;
            case 5:
                p0 = routesFrom5[routeNumber].transform.GetChild(0).position;
                p1 = routesFrom5[routeNumber].transform.GetChild(1).position;
                p2 = routesFrom5[routeNumber].transform.GetChild(2).position;
                p3 = routesFrom5[routeNumber].transform.GetChild(3).position;
                break;
        }

        if (shouldShootWhileJumping)
        {
            anim.SetTrigger("shoot");
            shouldShootWhileJumping = false;
        }

        while (tParam < 1)
        {
            tParam += Time.deltaTime * movementSpeed;

            bossPosition = Mathf.Pow(1 - tParam, 3) * p0 +
                3 * Mathf.Pow(1 - tParam, 2) * tParam * p1 +
                3 * (1 - tParam) * Mathf.Pow(tParam, 2) * p2 +
                Mathf.Pow(tParam, 3) * p3;

            transform.position = bossPosition;

            if (shouldCheckForGroundPound)
            {
                if (!groundPoundCheckDone)
                {
                    RaycastHit2D hit = Physics2D.Raycast(rayCastPoint.position, Vector2.down, Mathf.Infinity);

                    if (hit.collider.CompareTag("Player"))
                    {
                        anim.SetTrigger("groundPound");
                        groundPoundCheckDone = true;
                        shouldCheckForGroundPound = false;

                        //bossFight.GroundPound();
                        justGroundPounded = true;

                        // because we are not starting from a pre-defined point
                        startingPoint = 0;

                        audioSource.clip = groundPoundStartSound;
                        audioSource.Play();

                        dangerIcon.GetComponent<SpriteRenderer>().enabled = true;
                        dangerIcon.transform.position = new Vector2(hit.point.x, hit.point.y + 0.75f);

                        yield return new WaitForSeconds(0.5f);

                        dangerIcon.GetComponent<SpriteRenderer>().enabled = false;

                        break;
                    }
                }
            }

            yield return new WaitForEndOfFrame();
        }

        if (!justGroundPounded)
        {
            anim.SetBool("isInAir", false);
        }

        rb.gravityScale = 5;

        shouldCheckForGroundPound = false;
        groundPoundCheckDone = false;

        ///////////////////////////////////////

        yield return new WaitForSeconds(2);

        //tParam = 0f;

        //shouldJump = true;

        /////////////////////////////////////////

        bossFight.PrepareToShoot();
    }

    public void ShouldJump()
    {
        tParam = 0f;

        shouldJump = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Tilemap") && justGroundPounded)
        {
            anim.SetBool("isInAir", false);
        }
        else if(collision.CompareTag("Player") && justGroundPounded)
        {
            collision.GetComponent<Player>().TakeDamage(20);
        }
    }
}
