using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;
    public float boostTimer; //austin
    public bool boosting; //austin
    
    public int maxHealth = 5;
    public int score;
    
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI loseText;
    public TextMeshProUGUI createdByText;
    public TextMeshProUGUI createdText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI restartText;

    public GameObject projectilePrefab;
    public GameObject pickupParticlePrefab; //austin
    public AudioClip collectedClip; //austin

    public ParticleSystem hitEffect;

    public bool isGameActive;
    
    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip winSound; //austin
    public AudioClip fixSound; //austin  
    
    public int health { get { return currentHealth; }}
    int currentHealth;
    
    public float timeInvincible = 2.0f;
    bool isInvincible;
    float invincibleTimer;
    
    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;
    
    Animator animator;
    Vector2 lookDirection = new Vector2(1,0);
    
    AudioSource audioSource;

     
    // Start is called before the first frame update
    void Start()
    {
        isGameActive = true;
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        currentHealth = maxHealth;

        speed = 3; //austin
        boostTimer = 0; //austin
        boosting = false; //austin

        audioSource = GetComponent<AudioSource>();
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        restartText.gameObject.SetActive(false);
        createdByText.gameObject.SetActive(false);
        createdText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(isGameActive)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            
            Vector2 move = new Vector2(horizontal, vertical);
            
            if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
            {
                lookDirection.Set(move.x, move.y);
                lookDirection.Normalize();
            }
        
            animator.SetFloat("Look X", lookDirection.x);
            animator.SetFloat("Look Y", lookDirection.y);
            animator.SetFloat("Speed", move.magnitude);
        }
        
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }
        
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(!isGameActive)
            {
                RestartGame();
            }
        } 

        if(Input.GetKeyDown(KeyCode.C))
        {
            if(isGameActive)
            {
                Launch();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                }
            }
        }
    }
    
    
    void FixedUpdate()
    {
        if(isGameActive)
        {
            Vector2 position = rigidbody2d.position;
            position.x = position.x + speed * horizontal * Time.deltaTime;
            position.y = position.y + speed * vertical * Time.deltaTime;

            rigidbody2d.MovePosition(position);

             // Speed Boosting Powerup made by Austin
            if(boosting)
            {
                boostTimer += Time.deltaTime;
                if(boostTimer >= 4)
                {
                    speed = 3;
                    boostTimer = 0;
                    boosting = false;
                } 
            }
            
        }
    }

     void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "SpeedBoost") //used a tag and made the effect in ruby controller script
        {
            boosting = true;
            speed = 6;
            Instantiate(pickupParticlePrefab, transform.position, Quaternion.identity);
            Destroy(other.gameObject);

            PlaySound(collectedClip);
        }
    }
        
    

    public void ChangeHealth(int amount)
    {
        if(isGameActive)
        {
            if (amount < 0)
            {
                if (isInvincible)
                    return;
                
                isInvincible = true;
                invincibleTimer = timeInvincible;
                // take damage effect
                animator.SetTrigger("Hit");
                hitEffect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                
                PlaySound(hitSound);
            }
        }
            
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
            
         UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

         if(currentHealth <= 0)
        {
            GameOver();
        }

        
        
    }
    
    public void ChangeScore(int scoreAmount)
    {
        score += scoreAmount;
        scoreText.text = "Fixed Robots: " + score.ToString();
        PlaySound(fixSound); // austin

        if(score >= 2)
      {
        WinGame();
      }
    }
    

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");
        
        PlaySound(throwSound);
    } 
    
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void GameOver()
    {
        isGameActive = false;
        loseText.gameObject.SetActive(true);
        createdByText.gameObject.SetActive(true);
        restartText.gameObject.SetActive(true);
    }
    
    public void WinGame()
    {
        isGameActive = false;
        winText.gameObject.SetActive(true);
        createdText.gameObject.SetActive(true);
        PlaySound(winSound); //austin
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    

}