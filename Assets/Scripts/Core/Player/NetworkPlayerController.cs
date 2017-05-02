using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class NetworkPlayerController : NetworkBehaviour
{
    //  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Membros Públicos.
    [Header("Movement Settings")]
    [Space(5)]
    [Range(0f, 50f)]
    [Tooltip("Velocidade máxima do personagem caminhando.")]
    public float walkSpeed = 6.0f;                                  // Velocidade máxima do movimento caminhar.
    [Range(0f, 1f)]
    [Tooltip("Duração da interpolação Smooth para movimento.")]
    public float speedAccelerationTime = 0.1f;                      // Tempo de duração da interpolação Smooth para movimento.
    [Range(0f, 1f)]
    [Tooltip("Duração da interpolação Smooth para Rotação.")]
    public float turnAccelerationTime = 0.2f;                       // Tempo de duração da interpolação Smooth para rotação.

    public EntityClanType entityClan;

    //  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Membros Privados.
    private Vector2 _input = Vector2.zero;                          // Vetor auxiliar para receber input.    
    private Vector3 _currentVelocity = Vector3.zero;                // Vetor autilizar para calculo de velocidade atual.
    private float _currentSpeed = 0.0f;                             // Rapidez atual.
    private float _turnSmoothVelocity = 0.0f;                       // Velocidade atual de rotação na interpolação Smooth.
    private float _speedSmoothVelocity = 0.0f;                      // Velocidade atual de movimento na interpolação Smooth.
    private int _animSpeedHash = 0;                                 // ID Hash do parametro Speed no Animator.

    private Animator _animator = null;                              // Referência do componente Animator.
    private CharacterController _controller = null;                 // Referência do componente CharacterController.
    private Camera _mainCamera = null;                              // Referência do componente Camera Principal.

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    //  Métodos Privados.

    // Awake is called when the script instance is being loaded
    private void Start()
    {
        // Inicializa referências de componentes, e Hashs para animação.
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _animSpeedHash = Animator.StringToHash("Speed");
        if (isLocalPlayer)
        {
            _mainCamera = Camera.main;
            _mainCamera.GetComponent<CameraController>().target = this.transform;            
        }
    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Update is called once per frame
    private void Update()
    {
        // TODO: Checar se é localPlayer
        if (isLocalPlayer)
        {
            // Recebe a input
            _input.Set(/*Input.GetAxis("Xbox LHorizontal")*/ InputManager.GetPlayerHorizontalAxis(), 
                /*Input.GetAxis("Xbox LVertical")*/ InputManager.GetPlayerVerticalAxis()); // Isso aqui precisa vir do GameManager.
            // Normaliza preservando valores do input analógico inferiores a um.
            _input = Vector2.ClampMagnitude(_input, 1f);

            // Calcula a celeridade relativa ao input.
            float targetSpeed = this.walkSpeed * _input.magnitude;

            // Faz o tratamento de valores próximos a zero, pois a SmoothDamp da Unity não faz, provocando bugs na interpolação.
            if (_currentSpeed < 0.001f && targetSpeed < 0.001f)
                _currentSpeed = targetSpeed = 0f;

            // Faz uma interpolação suavizada para o movimento, passando a ideia de aceleração.
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, this.speedAccelerationTime);
            // Atualiza parâmetros ne animação.
            _animator.SetFloat(_animSpeedHash, _currentSpeed);
        }
        else // Se não for localplayer, aplica apenas os parâmetros de animação.
        {
            // Aplica a velocidade nos parâmetros da maquina de estados do Animator.
            _animator.SetFloat(_animSpeedHash, _controller.velocity.magnitude, 0.1f, Time.deltaTime);
            if (_animator.GetFloat(_animSpeedHash) < 0.001f)
                _animator.SetFloat(_animSpeedHash, 0f);
        }

    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled
    private void FixedUpdate()
    {
        // Não precisa chegar se é localplayer aqui, pois _input nunca será setado.
        // Checa se há input.
        if (_input.sqrMagnitude > 0f)
        {
            // Converte a direção da input em um ângulo, levando em consideração a orientação isométrica.
            float targetRotation = Mathf.Atan2(_input.x + _input.y, _input.y - _input.x) * Mathf.Rad2Deg;
            // Faz uma interpolação suavizada para a rotação, passando a ideia de aceleração.
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation,
                                                    ref _turnSmoothVelocity, this.turnAccelerationTime, Mathf.Infinity, Time.fixedDeltaTime);

            // Determina o Vetor de velocidade. 
            _currentVelocity = (transform.forward * _currentSpeed * Time.fixedDeltaTime) + Physics.gravity;
            // Aplica a velocidade encontrada no componente CharacterController.
            _controller.Move(_currentVelocity);
        }
        else
        {
            if (isLocalPlayer)
            {
                // Se não houver input, aplica apenas a gravidade no componente CharacterController.
                _controller.Move(Physics.gravity);
            }
        }
    }
//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀

    // PARTE TEMPORÁRIA ATÉ A ELABORAÇÃO DO AUDIOCONTROLLER.
    public List<AudioSource> audioSources = new List<AudioSource>();
    private int _audioIndex = 0;

    private void PlayFootStepSound()
    {
        if (entityClan == EntityClanType.AncientClan)
        {
            audioSources[_audioIndex].pitch = (_currentSpeed / 5f < 1f) ? 1f : Mathf.Max(_currentSpeed / 5f, 1.06f);
            audioSources[_audioIndex].Play();
            _audioIndex = ++_audioIndex & 2;
        }
    }

}
