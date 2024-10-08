using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] [Range(0f, 10f)] private float defaultDistance = 6f;
    [SerializeField] [Range(0f, 10f)] private float minimumDistance = 1f;
    [SerializeField] [Range(0f, 10f)] private float maximumDistance = 6f;
    
    [SerializeField] [Range(0f, 10f)] private float smoothing = 4f;
    [SerializeField] [Range(0f, 10f)] private float zoomSensitivity = 1f;
    
}
