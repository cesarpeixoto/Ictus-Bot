using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    private Vector3 _offset = Vector3.zero;
    public Transform target = null;
    public float movimentSpeed = 1f;
    Vector3 destination = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        _offset = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // LateUpdate será chamada a cada quadro, se o Comportamento estiver habilitado
    private void LateUpdate()
    {
        if (this.target != null)
        {
            destination = this.target.position + _offset;
            this.transform.position = Vector3.Lerp(this.transform.position, destination, movimentSpeed);
        }
    }



}
