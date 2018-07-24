using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SelfieManager : MonoBehaviour {
    public Image overlayImage;

    public RawImage cameraImage;
    public AspectRatioFitter rawImageFitter;

    public Button selfieButton;
    public Slider alphaSlider;

    public float overlayAlpha;

    WebCamTexture _webCamTexture;
    CaptureAndSave _captureAndSave;

    bool _webcamAvailable;
    float ratio = 1f / 0.5625f; // 0.52173913043f; // 1836f / 3264f; float ratio = 0.5346535f
    float sizeDeltaScaler = 1.27f;

    void Update() {
        if (_webcamAvailable) {
            _UpdateCameraOverlayAspectRatio();
            _UpdateCameraOverlaySize();
        }
    }

    void _UpdateCameraOverlayAspectRatio() {
        float scaleY = _webCamTexture.videoVerticallyMirrored ? -1f : 1f;
        cameraImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orientation = -_webCamTexture.videoRotationAngle;
        cameraImage.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    void _UpdateCameraOverlaySize() {
        int transparencyWidth = (int) overlayImage.rectTransform.rect.height;
        int transparencyHeight = (int) overlayImage.rectTransform.rect.width;
        rawImageFitter.aspectRatio = ratio;
        cameraImage.rectTransform.sizeDelta = new Vector2(transparencyHeight / sizeDeltaScaler, transparencyWidth / sizeDeltaScaler);
    }

    void Start() {
#if (UNITY_ANDROID && !UNITY_EDITOR)
        Screen.fullScreen = false;
#endif
        _captureAndSave = GameObject.FindObjectOfType<CaptureAndSave>();

        _SetupUI();
        _SetupWebcamTexture();
    }

    void _SetupWebcamTexture() {
        _webCamTexture = _GetWebcamTexture();
        if (_webCamTexture == null) {
            Debug.LogError("Couldn't find device camera!");
            return;
        }

        _webCamTexture.Play();
        cameraImage.texture = _webCamTexture;

        _webcamAvailable = true;
    }

    WebCamTexture _GetWebcamTexture() {
        string frontFacingCameraName = _GetFrontFacingCameraName();

        if (!string.IsNullOrEmpty(frontFacingCameraName))
            return new WebCamTexture(
                frontFacingCameraName,
                (int) ((RectTransform) overlayImage.transform).rect.width,
                (int) ((RectTransform) overlayImage.transform).rect.height
                );
        else {
            Debug.LogError("Couldn't find front facing camera!");
            return new WebCamTexture(Screen.width, Screen.height);
        }
    }

    string _GetFrontFacingCameraName() {
        foreach (WebCamDevice device in WebCamTexture.devices)
            if (device.isFrontFacing)
                return device.name;
        return null;
    }

    void _SetupUI() {
        _SetupSelfieButton();
        _SetupAlphaSlider();
        _RefreshOverlayImageAlpha();
    }

    void _SetupAlphaSlider() {
        alphaSlider.value = overlayAlpha;
        alphaSlider.onValueChanged.AddListener(alpha => {
            overlayAlpha = alpha;
            _RefreshOverlayImageAlpha();
        });
    }

    void _RefreshOverlayImageAlpha() {
        overlayImage.GetComponent<CanvasRenderer>().SetAlpha(overlayAlpha);
    }

    void _SetupSelfieButton() {
        Debug.Log("Setting Up Button");
        selfieButton.GetComponent<Button>().onClick.AddListener(_OnSelfieButtonPressed);
    }

    void _OnSelfieButtonPressed() {
        Debug.Log("Capturing");
        // _SaveSelfie();
        Debug.Log("Captured");
    }

    void _SaveSelfie() {
        // Texture2D selfieTexture = _GetSelfieTextureForSave();
        _captureAndSave.SaveTextureToGallery(cameraImage.texture as Texture2D);
    }

    //     Texture2D _GetSelfieTextureForSave() {
    //         Texture2D selfieTexture = new Texture2D(_webCamTexture.width, _webCamTexture.height);
    //         selfieTexture.SetPixels(_webCamTexture.GetPixels());
    //         selfieTexture.Apply();

    // #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
    //         return _GetRotatedTextureForMobile(selfieTexture);
    // #else
    //         return selfieTexture;
    // #endif
    //     }

    // /// Phone's front cameras returns rotated texture data for whatever reason
    // Texture2D _GetRotatedTextureForMobile(Texture2D selfieTexture) {
    //     Texture2D selfieTextureMobile = new Texture2D(_webCamTexture.height, _webCamTexture.width);
    //     for (int x = 0; x < _webCamTexture.width; ++x)
    //         for (int y = 0; y < _webCamTexture.height; ++y)
    //             selfieTextureMobile.SetPixel(y, x, selfieTexture.GetPixel(selfieTexture.width - x, selfieTexture.height - y));

    //     selfieTextureMobile.Apply();
    //     Destroy(selfieTexture); // Get rid of the old non-rotated one.

    //     return selfieTextureMobile;
    // }
}
