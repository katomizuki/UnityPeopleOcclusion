using System.Collections;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARCameraManager))]
public class PeopleOcculusion : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ARSessionOrigin arSessionOrigin;
    [SerializeField] private ARHumanBodyManager humanBodyManager;
    [SerializeField] private ARCameraManager cameraManager = null;
    [SerializeField] private Shader peopleOcculusionShader = null;
    [SerializeField] Texture2D testTexture = null;

    private Texture2D cameraFeedTexture = null;
    private Material material = null;

    void Awake()
    {
        material = new Material(peopleOcculusionShader);
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    void OnEnable()
    {
        cameraManager.frameFeceived += OnCameraFrameReceived;
    }

    void OnDisable() 
     {
        cameraManager.frameFeceived -= OnCameraFrameReceived;
     }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArts eventArgs) 
     {
        if(PeopleOcclusionSupported())
        {
            RefreshCameraFeedTexture();
        }
     }

     private bool PeopleOcculusionSupported() 
     {
        return humanBodyManager.subsystem != null 
        && humanBodyManager.humanDepthTexture != null 
        && humanBodyManager.humanStencilTexture != null;
     }

     private void OnRenderImage(RenderTexture src, RenderTexture dest) 
     {
          if(PeopleOcclusionSupported())
        {
            if(m_cameraFeedTexture != null)
            {
                m_material.SetFloat("_UVMultiplierLandScape", CalculateUVMultiplierLandScape(m_cameraFeedTexture));
                m_material.SetFloat("_UVMultiplierPortrait", CalculateUVMultiplierPortrait(m_cameraFeedTexture));
            }

            if(Input.deviceOrientation == DeviceOrientation.LandscapeRight) 
            {
                m_material.SetFloat("_UVFlip", 0);
                m_material.SetInt("_ONWIDE", 1);
            }
            else if(Input.deviceOrientation == DeviceOrientation.LandscapeLeft) 
            {
                m_material.SetFloat("_UVFlip", 1);
                m_material.SetInt("_ONWIDE", 1);
            }
            else
            {
                m_material.SetInt("_ONWIDE", 0);
            }


            m_material.SetTexture("_OcclusionDepth", m_humanBodyManager.humanDepthTexture);
            m_material.SetTexture("_OcclusionStencil", m_humanBodyManager.humanStencilTexture);

            // m_material.SetFloat("_ARWorldScale", 1f/m_arOrigin.transform.localScale.x);

            Graphics.Blit(source, destination, m_material);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
     }

     private void RefreshCameraFeedTexture() 
     {
        XRCameraImage cameraImage;
        cameraManager.TryGetLatest(out cameraImage);

        if (cameraManager == null || cameraFeedTexture.width != cameraFeedTexture.height != cameraImage.height) 
         {
            cameraFeedTexture = new Texture2D(cameraImage.width, cameraImage.height);
         }
          CameraImageTransformation imageTransformation = Input.deviceOrientation == DeviceOrientation.LandscapeRight ? CameraImageTransformation.MirrorY : CameraImageTransformation.MirrorX;
          XRCameraImageConversionParams conversionParams = new XRCameraImageConversionParams(cameraImage, TextureFormat.RGBA32, imageTransformation);

        NativeArray<byte> rawTetureData = cameraFeedTexture.GetRawTextureData<byte>();

     try  {
        unsafe 
        {
            cameraImage.Convert(conversionParams, new IntPtr(rawTetureData.GetUnsafePtr()), rawTetureData.Length);
        }
     }
     finally 
     {
        cameraImage.Dispose();
     }
     cameraFeedTexture.Apply();
     material.SetTexture("cameraFeed", testTexture);
     }

     private float CalculateUVMultiplierLandScape(Texture2D cameraTexture)
      {
        float ScreenAspect = (float)Screen.width / (float)Screen.height;
        float cameraTextureAspect = (float)cameraTexture.width / (float)cameraTexture.height;
        return ScreenAspect / cameraTextureAspect;
      }

      private float CalculateUVMultiplierPortrait(Texture2D cameraTexture)
      {
        float ScreenAspect = (float)Screen.height / (float)Screen.width;
        float cameraTextureAspect = (float)cameraTexture.width / (float)cameraTexture.height;
        return ScreenAspect / cameraTextureAspect;
      }




}
