using UnityEngine;

public class IceBullet : MonoBehaviour
{
    public float speed = 15f;                 
    public float timeToCreateTrack = 2;    
    public GameObject trackPrefab;            

    private Vector2 startPos;
    private float launchHeight;                
    private Vector2 direction;                
    public float elapsedTime = 0f;            
    public bool spawned;
    public GameObject TempLiftPrefab; 
    private Rigidbody2D rb;
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    

    void Start()
    {
        startPos = transform.position;
        launchHeight = transform.position.y;   
        elapsedTime = 0f;
    }

    void Update()
    {
        //if (direction == Vector2.zero) return;

       
        if (rb == null)
        {
            transform.Translate(direction * speed * Time.deltaTime);
        }

      
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= timeToCreateTrack && spawned == false)
        {

            CreateTrack();
            spawned = true;
        }
    }

    void CreateTrack()
    {
        if (trackPrefab == null) return;

       
        Vector2 trackPos = transform.position;
        //Debug.LogError(rb.linearVelocity.normalized);
        Instantiate(trackPrefab, trackPos, Quaternion.Euler(0,0, Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg));

    }

    private void OnDrawGizmosSelected()
    {
        if (trackPrefab == null) return;

        Vector2 trackPos = new Vector2(transform.position.x, launchHeight);
        Vector2 size = trackPrefab.GetComponent<BoxCollider2D>().size;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(trackPos, size);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;



        Destroy(gameObject);
    }


}
