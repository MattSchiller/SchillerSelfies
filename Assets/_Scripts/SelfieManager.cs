using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class SelfieManager : MonoBehaviour
{
    public Image webcamImage;
    public AspectRatioFitter webcamFitter;
    public Image webcamImageAndroid;
    public AspectRatioFitter webcamAndroidFitter;

    public Image overlayImage;

    public Button selfieButton;
    public Slider alphaSlider;

    public float overlayAlpha;

    WebCamTexture _webCamTexture;
    CaptureAndSave _captureAndSave;

    bool _webcamAvailable;

    void Update() {
        if (_webcamAvailable) {
            _UpdateAspectRatioFitters();
        }
    }

    void _UpdateAspectRatioFitters() {
        float ratio = (float)_webCamTexture.width / (float)_webCamTexture.height;
        webcamFitter.aspectRatio = ratio;
        webcamAndroidFitter.aspectRatio = ratio;

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

        _SetWebcamImageTexture(_webCamTexture);
        _webCamTexture.Play();

        _webcamAvailable = true;
    }

    WebCamTexture _GetWebcamTexture() {
        string frontFacingCameraName = _GetFrontFacingCameraName();

        if (!string.IsNullOrEmpty(frontFacingCameraName))
            return new WebCamTexture(
                frontFacingCameraName,
                Screen.width,
                Screen.height
                // (int)((RectTransform)overlayImage.transform).rect.width,
                // (int)((RectTransform)overlayImage.transform).rect.height
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

    void _SetWebcamImageTexture(Texture webCamTexture) {
        _HideAllWebcamImages();

#if UNITY_ANDROID && !UNITY_EDITOR
        webcamImageAndroid.gameObject.SetActive(true);
        webcamImageAndroid.material.mainTexture = webCamTexture;
#else
        webcamImage.gameObject.SetActive(true);
        webcamImage.material.mainTexture = webCamTexture;
#endif
    }

    void _HideAllWebcamImages() {
        webcamImage.gameObject.SetActive(false);
        webcamImageAndroid.gameObject.SetActive(false);
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
        selfieButton.onClick.AddListener(_OnSelfieButtonPressed);
    }

    void _OnSelfieButtonPressed() => _SaveSelfie();

    void _SaveSelfie() {
        Texture2D selfieTexture = _GetSelfieTextureForSave();
        _captureAndSave.SaveTextureToGallery(selfieTexture);
        Destroy(selfieTexture);
    }

    Texture2D _GetSelfieTextureForSave() {
        Texture2D selfieTexture = new Texture2D(_webCamTexture.width, _webCamTexture.height);
        selfieTexture.SetPixels(_webCamTexture.GetPixels());
        selfieTexture.Apply();

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        return _GetRotatedTextureForMobile(selfieTexture);
#else
        return selfieTexture;
#endif
    }

    /// Phone's front cameras returns rotated texture data for whatever reason
    Texture2D _GetRotatedTextureForMobile(Texture2D selfieTexture) {
        Texture2D selfieTextureMobile = new Texture2D(_webCamTexture.height, _webCamTexture.width);
        for (int x = 0; x < _webCamTexture.width; ++x)
            for (int y = 0; y < _webCamTexture.height; ++y)
                selfieTextureMobile.SetPixel(y, x, selfieTexture.GetPixel(selfieTexture.width - x, selfieTexture.height - y));

        selfieTextureMobile.Apply();
        Destroy(selfieTexture); // Get rid of the old non-rotated one.

        return selfieTextureMobile;
    }
}
