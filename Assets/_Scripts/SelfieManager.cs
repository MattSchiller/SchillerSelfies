using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelfieManager : MonoBehaviour {
    public Image webcamImage;
    public Image overlayImage;
    public Button selfieButton;

    public float overlayAlpha;

    WebCamTexture _webCamTexture;

    void Start() {
        _SetupSelfieButton();
        _SetupOverlayImage();
        _SetupWebcamTexture();
    }

    void _SetupSelfieButton() {
        selfieButton.onClick.AddListener(_OnSelfieButtonPressed);
    }

    void _OnSelfieButtonPressed() {
        _SaveSelfie();
    }

    void _SaveSelfie() {
        string selfieImagePath = _GetSelfieImagePath();
        Texture2D selfieTexture = _GetSelfieTexture();

        _SaveSelfieToFile(selfieImagePath, selfieTexture);

        Destroy(selfieTexture);
    }

    void _SaveSelfieToFile(string selfieImagePath, Texture2D selfieTexture) {
        Debug.Log($"Writing selfie to '{selfieImagePath}'");
        System.IO.File.WriteAllBytes(selfieImagePath, selfieTexture.EncodeToPNG());
    }

    string _GetSelfieImagePath(string name = null) {
        if (name == null)
            name = _GetSelfieName();
        return Application.persistentDataPath + "/" + name;
    }

    Texture2D _GetSelfieTexture() {
        Texture2D selfieTexture = new Texture2D(_webCamTexture.width, _webCamTexture.height);
        selfieTexture.SetPixels(_webCamTexture.GetPixels());
        selfieTexture.Apply();

        return selfieTexture;
    }

    string _GetSelfieName() {
        string date = System.DateTime.Now.ToString("yyyy.MM.dd");
        string namePrefix = date + "_selfie_";
        int selfieNumber = _GetNextSelfieNumber(namePrefix);

        return $"{namePrefix}{selfieNumber}.png";
    }

    int _GetNextSelfieNumber(string namePrefix) {
        int count = -1;
        string name, path;

        do {
            count++;
            name = $"{namePrefix}{count}.png";
            path = _GetSelfieImagePath(name);
        } while (File.Exists(path));

        return count;
    }

    void _SetupOverlayImage() {
        overlayImage.GetComponent<CanvasRenderer>().SetAlpha(overlayAlpha);
    }

    void _SetupWebcamTexture() {
        _webCamTexture = _GetWebcamTexture();
        if (_webCamTexture == null) {
            Debug.LogError("Couldn't find device camera!");
            return;
        }

        webcamImage.material.mainTexture = _webCamTexture;
        _webCamTexture.Play();
    }

    string _GetFrontFacingCameraName() {
        foreach (WebCamDevice device in WebCamTexture.devices)
            if (device.isFrontFacing)
                return device.name;
        return null;
    }

    WebCamTexture _GetWebcamTexture() {
        Rect webcamImageRect = webcamImage.rectTransform.rect;
        string frontFacingCameraName = _GetFrontFacingCameraName();

        if (!string.IsNullOrEmpty(frontFacingCameraName))
            return new WebCamTexture(frontFacingCameraName, (int) webcamImageRect.width, (int) webcamImageRect.height);
        else {
            Debug.LogError("Couldn't find front facing camera!");
            return new WebCamTexture((int) webcamImageRect.width, (int) webcamImageRect.height);
        }
    }
}
