using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ground Check")]
    public Transform groundCheck;               //objeto que serve como referência pra fazer checagem com o chão
    public float footOffest = 0.4f;             //distância até o pé do personagem
    public float groundDistance = 0.1f;         //distância com que faz a checagem com o chão
    public LayerMask groundLayer;               //máscara de camada do chão
    public bool onGround;                       //variável que identifica se jogador está no chão ou não

    [Header("Movement")]
    public float speed = 5;                     //velocidade de movimento do jogador
    public float jumpForce = 12;                //força do pulo
    public float horizontalJumpForce = 6;       //força do pulo na horizontal
    public float horizontal;                    //armazena o input no eixo da horizontal
    public bool jumpPressed;                    //identifica se o botão do pulo foi pressionado ou não
    public int direction = 1;                   //identifica a direção do jogador (1 direita, -1 esquerda)
    public bool canMove = true;                 //identifica se pode se movimentar ou não    

    [Header("Ladder")]
    public float climbSpeed = 3;                //velocidade de subida na escada
    public LayerMask ladderMask;                //máscara de camada da escada
    public float vertical;                      //armazena o input do eixo da vertical
    public bool climbing;                       //identifica se jogador está escalando a escada
    public float checkRadius = 0.3f;            //raio de checagem com a escada

    private bool clearInputs;                   //identifica quando pode fazer limpeza nos inputs
        
    private Rigidbody2D rb;                     //armazena rigidbody do jogador
    private Animator anim;                      //armazena animator do jogador
    private Collider2D col;                     //armazena collider do jogador

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();       //referencia o rigidbody do jogador na variável
        anim = GetComponent<Animator>();        //referencia o animator do jogador na variável
        col = GetComponent<Collider2D>();       //referencia o collider do jogador na variável
    }

    // Update is called once per frame
    void Update()
    {
        //Funções de checagem de inputs e checagens de física são chamadas no Update

        CheckInputs();
        PhysicsCheck();       

    }

    private void FixedUpdate()
    {
        //funções de movimento são chamadas no FixedUpdate

        GroundMovement();
        AirMovement();
        ClimbLadder();

        //toda vez que o FixedUpdate é chamado, informa que pode limpar os inputs
        clearInputs = true;
    }

    bool TouchingLadder()
    {
        //fun~ção que retorna se jogador está se colidindo com escada
        return col.IsTouchingLayers(ladderMask);
    }

    void ClimbLadder()
    {

        //Função para fazer a subida da escada

        if (TouchingLadder() && !climbing && onGround)
            UIManager.instance.SetKeyUp(true, transform.position);
        else
            UIManager.instance.SetKeyUp(false);

        //Up verifica se tem escada acima, e down se tem escada abaixo
        bool up = Physics2D.OverlapCircle(transform.position, checkRadius, ladderMask);
        bool down = Physics2D.OverlapCircle(transform.position + new Vector3(0, -1), checkRadius, ladderMask);

        if (vertical != 0 && TouchingLadder()) //se o input da vertical for pressionado e o jogador esiver se colidindo com escada
        {
            climbing = true;                //passa climbing pra verdadeiro                       
            rb.isKinematic = true;          //coloca o rigidbody como kinematic pra evitar interferência da física
            canMove = false;                //impede movimentação do jogador na horizontal


            //coloca o jogador centralizado no tile. Funciona nesse projeto em que cada tile está na posição 0.5f
            //0.5 1.5 2.5 etc
            float xPos = (int)transform.position.x;
            xPos = xPos + (0.5f * Mathf.Sign(xPos));

            transform.position = new Vector2(xPos, transform.position.y);

            UIManager.instance.SetKeyUp(false);

        }

        if (climbing)
        {

            //Quando climbing for verdadeiro
            //Se não estiver escada acima ou abaixo, termina a escalada

            if (!up && vertical >= 0)
            {
                FinishClimb();
                return;
            }

            if (!down && vertical <= 0)
            {
                FinishClimb();
                return;
            }


            float y = vertical * climbSpeed;            //armazena o input da vertical multiplicado pela velocidade de subida
            rb.velocity = new Vector2(0, y);            //atualiza velocidade do rigdbody de acordo com velocidade em y armazenada

            anim.SetFloat("ClimbSpeed", vertical);      //atualiza parâmetro de velocidade de subida no Animator

            //se o pulo for pressionado
            if (jumpPressed)
            {
                col.isTrigger = true;           //coloca o collider em trigger
                FinishClimb();                  //finaliza a escalada
                canMove = false;                //permanece sem poder se movimentar na horizontal

                float x = direction;            //armazena o valor da direção numa nova variável x
                if (horizontal != 0)            //se o input da horizontal for pressionado        
                    x = horizontal > 0 ? 1 : -1;    //se o valor da horizontal for maior que 0, valor de x = 1, senão, x = -1

                //Caso x seja diferente da direção previamente armazenada, faz o Flip do jogador

                if(x * direction < 0)
                {
                    Flip();
                }

                //adiciona uma força na horizontal e vertical pra fazer o pulo da escada
                rb.AddForce(new Vector2(horizontalJumpForce * direction, jumpForce / 2), ForceMode2D.Impulse);

               
            }

        }

        //atualiza o parâmetro Climbing do Animator com o valor de climbing
        anim.SetBool("Climbing", climbing);
    }

    void FinishClimb()
    {

        //Função que finaliza a escalada
        /*Coloca climbing pra falso
         * tira o rigidbody de kinematic
         * pode movimentar novamente o jogador na horizontal
         * Chama uma função pra resetar o Climbing depois de um décimo
         * atualiza o parâmetro Climbing do Animator pra falso
         */
        climbing = false;
        rb.isKinematic = false;
        canMove = true;
        Invoke("ResetClimbing", 0.1f);
        anim.SetBool("Climbing", false);
        UIManager.instance.SetKeyUp(false);
    }

    void ResetClimbing()
    {
        
        //Função serve para passar movimento para verdadeiro e tirar collider de trigger
        //Enquanto estiver se colidindo com o chão, permanece em trigger
        //Isso impede de ficar preso numa plataforma
        canMove = true;
        if (col.IsTouchingLayers(groundLayer))
        {
            Invoke("ResetClimbing", 0.1f);
        }
        else
        {
            col.isTrigger = false;
        }

        
    }

    void GroundMovement()
    {
        //Função para fazer movimento na horizontal

        //Se não puder se movimentar, retorna da função
        if (!canMove)
            return;

        float x = horizontal * speed;               //Armazena velocidade numa variável x. Multiplica valor do input da horizontal pela velocidade

        rb.velocity = new Vector2(x, rb.velocity.y);//Atualiza velocidade do rigidbody

        if (x * direction < 0f)                     //Se direção for diferente do input do jogador, faz o Flip
            Flip();

        anim.SetFloat("Speed", Mathf.Abs(horizontal));//Atualiza parâmetro Speed no Animator
        
    }
    
           
       
        

    void AirMovement()
    {
        //Função para executar o pulo do chão

        //Se estiver escalando, retorna da função
        if (climbing)
            return;

        //Se o pulo for pressionado e estiver no chão
        if (jumpPressed && onGround)
        {
           
            jumpPressed = false;        //passa o pulo pressionado pra falso

            rb.velocity = Vector2.zero; //primeiro zera a velocidade do jogador

            //Depois adiciona uma força pra cima
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

        }
       
       

       

    }

    void Flip()
    {
        //Função que executa o Flip do jogador

        direction *= -1;                        //Inverte o valor da direção     
        Vector3 scale = transform.localScale;   //Armazena a escala do jogador
        scale.x *= -1;                          //No valor armazena, inverte o valor da escala em X
        transform.localScale = scale;           //Atualiza a escala do jogador de acordo com o novo Vector3 armazenado
    }

    void CheckInputs()
    {

        //Função que serve para checar os inputs

        //Limpa os inputs, faz o botão de pulo voltar para o valor padrão de falso
        if (clearInputs)
        {
            jumpPressed = false;
            clearInputs = false;
        }

       
        //Armazena se pulo foi pressionado
        jumpPressed = jumpPressed || Input.GetButtonDown("Jump");

       
        //Armazena eixo da horizontal
        horizontal = Input.GetAxis("Horizontal");

        //Armazena eixo da vertical
        vertical = Input.GetAxis("Vertical");

    }

    void PhysicsCheck()
    {

        //Função que faz checagem com o chão

        //Assume que nochão está como falso
        onGround = false;      

        /*Dispara dois Raycasts, um na parte debaixo da esquerda e outro na parte debaixo da direita
         * Dessa forma tem uma precisão grande de quando jogador está no chão
         * O raycast é disparado numa função própria que retorna um raycasthit2D
         */

        RaycastHit2D leftFoot = Raycast(groundCheck.position + new Vector3(-footOffest, 0), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightFoot = Raycast(groundCheck.position + new Vector3(footOffest, 0), Vector2.down, groundDistance, groundLayer);
    
        //Se uma das checagens for verdadeira, passa nochão para verdadeiro
        if(leftFoot || rightFoot)
        {
            onGround = true;
        }       

        //Atualiza parâmetro OnGround no Animator de acordo com o valor de OnGround
        anim.SetBool("OnGround", onGround);
       

    }

    private void OnDrawGizmos()
    {
        //Função que mostra os raios de colisão que fazem a checagem com a escada

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, checkRadius);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0,-1), checkRadius);
    }
    public RaycastHit2D Raycast(Vector2 origin, Vector2 rayDirection, float length, LayerMask mask, bool drawRay = true)
    {
        //Função que retorna um RaycastHit2D

        //Envia o raycast e salva o resultado na variável hit
        RaycastHit2D hit = Physics2D.Raycast(origin, rayDirection, length, mask);


        //Se quisermos mostrar o raycast em cena
        if (drawRay)
        {
            Color color = hit ? Color.red : Color.green;
            
            Debug.DrawRay(origin, rayDirection * length, color);
        }
        //determina a cor baseado se o raycast se colidiu ou não

        //Retorna o resultado do hit
        return hit;
    }

    
}
