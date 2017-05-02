using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Membros Públicos.
    [Header("Moviment Settings")] 
    [Space(5)]
    [Range(0f, 50f)] [Tooltip("Velocidade máxima do personagem caminhando.")]
    public float walkSpeed = 6.0f;                                  // Velocidade máxima do movimento caminhar.
    [Range(0f, 1f)] [Tooltip("Duração da interpolação Smooth para movimento.")]
    public float speedAccelerationTime = 0.1f;                      // Tempo de duração da interpolação Smooth para movimento.
    [Range(0f, 1f)] [Tooltip("Duração da interpolação Smooth para Rotação.")]
    public float turnAccelerationTime = 0.2f;                       // Tempo de duração da interpolação Smooth para rotação.

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


    public bool testLocalPlayer = true;

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    //  Métodos Privados.

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Inicializa referências de componentes, e Hashs para animação.
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        _mainCamera.GetComponent<CameraController>().target = this.transform;
        _animSpeedHash = Animator.StringToHash("Speed");
    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Update is called once per frame
    private void Update()
    {
        // TODO: Checar se é localPlayer
        if (testLocalPlayer)
        {
            // Recebe a input
            _input.Set(Input.GetAxis("Xbox LHorizontal"), Input.GetAxis("Xbox LVertical")); // Isso aqui precisa vir do GameManager.
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
        else
        {
            _input.Set(Input.GetAxis("Xbox LHorizontal"), Input.GetAxis("Xbox LVertical"));       // Vai sair no multiplayer                                                                                      
            _input = Vector2.ClampMagnitude(_input, 1f);                                          // Vai sair no multiplayer

            // Calcula a velocidade do movimento.
            float targetSpeed = this.walkSpeed * _input.magnitude;                                // Vai sair no multiplayer
            
            // Faz o tratamento de valores próximos a zero, pois a SmoothDamp da Unity não faz, provocando bugs na interpolação.
            if (_currentSpeed < 0.001f && targetSpeed < 0.001f)                                  // Vai sair no multiplayer
            {
                _currentSpeed = 0f;
                targetSpeed = 0f;
            }
            // Faz uma interpolação suavizada para o movimento, passando a ideia de aceleração.
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, targetSpeed, ref _speedSmoothVelocity, this.speedAccelerationTime);

            // TODO: APENAS ESTA PARTE DE BAIXO PERMANECE QUANDO PROJETO SE TORNAR MULTIPLAYER.

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
        // TODO: Checar se é localPlayer

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
            // Se não houver input, aplica apenas a gravidade no componente CharacterController.
            _controller.Move(Physics.gravity);
        }
    }
//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀

    // PARTE TEMPORÁRIA ATÉ A ELABORAÇÃO DO AUDIOCONTROLLER.
    public List<AudioSource> audioSources = new List<AudioSource>();
    private int _audioIndex = 0;

    private void PlayFootStepSound()
    {
        if (Hero.EntityClan == EntityClanType.AncientClan)
        {
            audioSources[_audioIndex].pitch = (_currentSpeed / 5f < 1f) ? 1f : Mathf.Max(_currentSpeed / 5f, 1.06f);
            audioSources[_audioIndex].Play();
            _audioIndex = ++_audioIndex & 2;
        }
    }

}


// TODO: Trabalhar com os eventos do Animator, e terminar de implementar isso depois!! 
// Mas temos que alterar porque o Animator é bugado pra caralho!!!!

[System.Serializable]
public class AudioController
{
    public delegate void CallBack();
    private float _play = 0.0f;
    private float _prevPlay = 0.0f;
    private float _animationEndTime = 0.0f;

    private List<AudioEvent> _audioEvents = new List<AudioEvent>();

    public void RegisterEvent(float time, CallBack callback)
    {
        _audioEvents.Add(new AudioEvent(time, callback));
        _audioEvents.Sort(delegate (AudioEvent ae1, AudioEvent ae2)
                          {
                              return (ae1.time.CompareTo(ae2.time));
                          });
    }

}



[System.Serializable]
public class AudioEvent
{    
    public float time = 0.0f;
    public AudioController.CallBack callback = null;
    public AudioEvent(float time, AudioController.CallBack callback) { this.time = time; this.callback = callback; }
}





//private float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed = Mathf.Infinity, float deltaTime = -1f)
//{
//    if (current < 0.001f && target < 0.001f)
//        return 0f;

//    deltaTime = (deltaTime == -1) ? Time.deltaTime : deltaTime;
//    smoothTime = Mathf.Max(0.0001f, smoothTime);
//    float num = 2f / smoothTime;
//    float num2 = num * deltaTime;
//    float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
//    float num4 = current - target;
//    float num5 = target;
//    float num6 = maxSpeed * smoothTime;
//    num4 = Mathf.Clamp(num4, -num6, num6);
//    target = current - num4;
//    float num7 = (currentVelocity + num * num4) * deltaTime;
//    currentVelocity = (currentVelocity - num * num7) * num3;
//    float num8 = target + (num4 + num7) * num3;
//    if (num5 - current > 0f == num8 > num5)
//    {
//        num8 = num5;
//        currentVelocity = (num8 - num5) / deltaTime;
//    }
//    return num8;
//}
