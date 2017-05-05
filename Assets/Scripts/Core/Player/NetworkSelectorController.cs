using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkSelectorController : NetworkBehaviour
{
//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Membros Públicos.
    [Header("Moviment Settings")]
    [Space(5)]
    [Range(0f, 70f)]
    [Tooltip("Velocidade do Seletor.")]
    public float movimentSpeed = 30.0f;

    [Header("Renderer Settings")]
    [Space(5)]
    [Range(0, 150)]
    [Tooltip("Quantidade de vértices do arco.")]
    public int arcResolution = 30;
    public Color validColor;
    public Color invalidColor;
    public Color opponetColor;

    [Header("Animation Settings")]
    [Space(5)]
    [Range(0f, 20f)]
    [Tooltip("Tempo da animação de rotação.")]
    public float targetAnimationTime = 4f;

    //  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Membros Privados.
    private Vector2 _input = Vector2.zero;                            // Vetor auxiliar para receber input.  

    // TODO: Isso sai daqui e vai para o input Manager
    private bool _leftTrigger = false;
    private bool _rightTrigger = true;

    private const float _selectorSize = 2.8f;                         // Constante para calculo do offset da tela em relação ao seletor.
    private float _elapsedTime = 0f;                                  // variável de auxilio para calculo do tempo de animação.

    private Transform _selector = null;                               // Referência do Transform pai dos componentes do Seletor.
    private Projector _projector = null;                              // Referência do componente Projector.
    private MeshRenderer _renderer = null;                            // Referência do componente MeshRenderer do efeito.
    private Transform _selectorOriginPoint = null;                    // Referência do Transform, posição de origem do arco do seletor.
    private LineRenderer _selectorArc = null;                         // Referência do LineRenderer responsável pelo arco do soletor
    private Camera _mainCamera = null;                                // Referência do componente Camera, na principal.


//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Use this for initialization
    void Start()
    {
        // Inicializa as referências
        _mainCamera = Camera.main;
        _selectorOriginPoint = this.transform.Find("Selector Origin Point");
        

        
    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Update is called once per frame
    void Update()
    {
        // Recebe a input
        _input.Set(/*Input.GetAxis("Xbox RHorizontal")*/ InputManager.GetSelectorHorizontalAxis(), 
            /*Input.GetAxis("Xbox RVertical")*/ InputManager.GetSelectorVerticalAxis()); // Isso aqui precisa vir do GameManager.
        if (_input.sqrMagnitude > 0.0f)
        {
            // Calcula a direção do input, considerando a perspectiva isométrica.
            _input = Vector2.ClampMagnitude(_input, 1f);

            // Calcula movimento e direção.
            Vector3 direction = Quaternion.Euler(30.0f, 45.0f, 0.0f) * new Vector3(_input.x, 0f, _input.y);
            direction *= Time.deltaTime * movimentSpeed;

            // TODO: Bug Conhecido.
            direction = ClampToScreen(direction); 

            direction.y = (NetworkSelectorRaycaster.surfaceHeight - direction.y);

            _selector.Translate(direction, Space.World);

            Vector3 temp = _selector.position;
            if (temp.y < 0f) temp.y = 0;
            _selector.position = temp;
        }
        _leftTrigger = /*Input.GetAxis("Xbox RLTrigger") < 0.0f*/ InputManager.GetCancelTrigger();
        _rightTrigger = /*Input.GetAxis("Xbox RLTrigger") > 0.0f*/ InputManager.GetActionTrigger();

        if (_leftTrigger)
        {
            NetworkHero.DeSelect();
        }
        if (_rightTrigger)
        {
            NetworkHero.Action(_selector.position);
        }

    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // This function is called every fixed framerate frame, if the MonoBehaviour is enabled
    private void FixedUpdate()
    {
        RenderConnectingArc(_selectorOriginPoint, _selector);
        AnimateSelector();
    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀

    // TODO: Precisa alterar, levando em conta a altura da merda!!!!
    // Limita o movimento do seletor à area da tela.
    private Vector3 ClampToScreen(Vector3 movement)
    {
        movement += _selector.position;
        Vector3 result = Vector3.zero;
        Vector3 SelectorScreen = _mainCamera.WorldToScreenPoint(movement);

        float offset = 0.0f; /*_selectorSize * _mainCamera.orthographicSize * _projector.orthographicSize;*/
        SelectorScreen.Set(Mathf.Clamp(SelectorScreen.x, 0f + offset, Screen.width - offset),
                           Mathf.Clamp(SelectorScreen.y, 0f + offset + Screen.height / 25f, Screen.height - offset - Screen.height / 25f),
                           SelectorScreen.z);

        result = _mainCamera.ScreenToWorldPoint(SelectorScreen);
        //result.y = SelectorRaycaster.height;

        result -= _selector.position;

        return result;
    }

    private Vector3 NewClampToScreen(Vector3 movement)
    {
        movement += Vector3.up * (NetworkSelectorRaycaster.surfaceHeight - _selector.position.y);
        Vector3 projectionPosition = _selector.position + _selector.TransformDirection(movement);

        Vector3 result = Vector3.zero;
        Vector3 SelectorScreen = _mainCamera.WorldToScreenPoint(projectionPosition);

        float offset = _selectorSize * _mainCamera.orthographicSize * _projector.orthographicSize;
        SelectorScreen.Set(Mathf.Clamp(SelectorScreen.x, 0f + offset, Screen.width - offset),
                           Mathf.Clamp(SelectorScreen.y, 0f + offset + Screen.height / 25f, Screen.height - offset - Screen.height / 25f),
                           SelectorScreen.z);

        result = _mainCamera.ScreenToWorldPoint(SelectorScreen);

        // result.y = (SelectorRaycaster.surfaceHeight - result.y); ;
        result -= _selector.position;
        result += _selector.TransformDirection(Vector3.up * (NetworkSelectorRaycaster.surfaceHeight - _selector.position.y));
        //result.y = (SelectorRaycaster.surfaceHeight - _selector.position.y);
        return result;
    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Renderiza o arco do seletor
    private void RenderConnectingArc(Transform origin, Transform destiny)
    {
        // Adquire os 3 pontos para o algorítimo de curva de Bezier.
        float distance = Vector3.Distance(origin.position, destiny.position);
        Vector3 controlPoint = Vector3.Lerp(origin.position, destiny.position, 0.5f);
        controlPoint += origin.up * (0.5f * distance);
        _selectorArc.positionCount = arcResolution;

        Vector3[] path = QuadraticBezierPoint(origin.position, destiny.position, controlPoint, arcResolution);
        _selectorArc.SetPositions(path);
    }

//  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀    
    // Seleciona a cor do Seletor.
    public void SetColor(SelectorStateType color)
    {
        Color newColor;
        switch (color)
        {
            case SelectorStateType.Valid:
                newColor = validColor;
                break;
            case SelectorStateType.Invalid:
                newColor = invalidColor;
                break;
            case SelectorStateType.Opponent:
                newColor = opponetColor;
                break;
            default:
                newColor = Color.black;
                break;
        }

        // TODO: Fazer interpolação de cores.
        _projector.material.color = newColor;
        Color startColor = new Color(newColor.r, newColor.g, newColor.b, 0f);
        Color endColor = new Color(newColor.r, newColor.g, newColor.b, 1f);
        _selectorArc.startColor = startColor;
        _selectorArc.endColor = endColor;
        Color colorRenderer = new Color(newColor.r, newColor.g, newColor.b, 0.5f);
        _renderer.material.SetColor("_TintColor", colorRenderer);
        _renderer.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", colorRenderer);

    }

    //  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Faz a animação de rotação do projetor do seletor.

    public float turnspeed = 5f;
    private void AnimateSelector()
    {
        _elapsedTime += Time.fixedDeltaTime;
        float time = _elapsedTime / targetAnimationTime;
        float newAngle = Mathf.Lerp(0, 360f, time);
        _projector.transform.rotation = Quaternion.Euler(_projector.transform.rotation.eulerAngles.x, newAngle, _projector.transform.rotation.eulerAngles.z);
        if (_elapsedTime >= targetAnimationTime)
        {
            _elapsedTime = 0f;
            _projector.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }

    }

    //  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Curva de Bezier - Algoritiom Cubic 
    private Vector3[] CubicBezierPoint(int resolution, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3[] vetor = new Vector3[resolution];
        Vector3 p = Vector3.zero;
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / resolution;
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;
            vetor[i] = p;
        }
        return vetor;
    }

    //  ▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀
    // Curva de Bezier - Algoritiom Quadratic
    private Vector3[] QuadraticBezierPoint(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint, int resolution)
    {
        Vector3[] vetor = new Vector3[resolution];
        Vector3 p = Vector3.zero;
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / resolution;
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float ut = u * t;

            p = uu * startPoint;
            p += 2f * ut * controlPoint;
            p += tt * endPoint;
            vetor[i] = p;
            //vetor[i] = Mathf.Pow((1 - t), 2) * startPoint + 2 * (1f - t) * t * controlPoint + Mathf.Pow(t, 2) * endPoint;
        }
        return vetor;
    }

    public void SetSelector(Transform selector)
    {
        _selector = selector;
        
        // Vamos pegar as coisas do seletor de outro lugar!!!!
        _projector = _selector.Find("Projector").GetComponent<Projector>();
        _selectorArc = _selector.GetComponent<LineRenderer>();
        _renderer = _selector.Find("Selector Mesh").GetComponent<MeshRenderer>();
        SetColor(SelectorStateType.Valid);
    }

}
