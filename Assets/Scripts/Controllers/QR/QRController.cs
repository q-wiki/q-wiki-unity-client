// Code used from: https://medium.com/@adrian.n/reading-and-generating-qr-codes-with-c-in-unity-3d-the-easy-way-a25e1d85ba51

using Controllers;
using Controllers.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Handlers;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using UnityEngine.Android;

public class QRController : MonoBehaviour {

    private StartUIController _uiController => (StartUIController)GameManager.Instance.UIController();

    private WebCamTexture camTexture;
    private Rect screenRect;

    [SerializeField] private Image qrCode;
    private bool displayCameraOverlay = false;
    int counter = 0;

    void Start() {
        screenRect = new Rect(0, 0, Screen.width, Screen.height);
        camTexture = new WebCamTexture();
        camTexture.requestedHeight = Screen.height;
        camTexture.requestedWidth = Screen.width;
    }
    async void OnGUI() {
        if (displayCameraOverlay) {
            if (GUI.Button(screenRect, "", new GUIStyle())) {
                stopQRReader();
            }

            // drawing the camera on screen
            GUI.DrawTexture(screenRect, camTexture, ScaleMode.ScaleToFit);
            
            counter++;
            //Read QR Codes every 15 Frames
            if (counter%15 == 0) {
                try
                {
                    IBarcodeReader barcodeReader = new BarcodeReader();
                    // decode the current frame
                    var result = barcodeReader.Decode(camTexture.GetPixels32(),
                        camTexture.width, camTexture.height);
                    if (result != null)
                    {
                        Debug.Log("DECODED TEXT FROM QR: " + result.Text);
                        try
                        {
                            //Challenge user via the user id from their QR code and open the game request view
                            await Communicator.ChallengeUser(result.Text);
                            stopQRReader();
                            _uiController.DisplayGameRequestView();
                        }
                        catch (Exception)
                        {
                            ErrorHandler.Instance.Error("Couldn't challenge user");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex.Message);
                }
            }

        }
    }

    /// <summary>
    /// Encode a string to a QR code
    /// </summary>
    private static Color32[] Encode(string textForEncoding, int width, int height) {
        var writer = new BarcodeWriter {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    /// <summary>
    /// Generate a QR code that returns the users player id
    /// </summary>
    public void generateChallengeQRCode() {
        Debug.Log("DEBUG ID: " + PlayerPrefs.GetString(Communicator.PLAYERPREFS_USER_ID));
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString(Communicator.PLAYERPREFS_USER_ID))) {
            Texture2D texture = generateQR(PlayerPrefs.GetString(Communicator.PLAYERPREFS_USER_ID)); ;
            qrCode.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    /// <summary>
    /// Generate a QR Code texture from a string
    /// </summary>
    private Texture2D generateQR(string text) {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }

    public void openQRReader() {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera)) {
            Permission.RequestUserPermission(Permission.Camera);
        }
#endif
        if (camTexture != null) {
            camTexture.Play();
            displayCameraOverlay = true;
        }
    }

    public void stopQRReader() {
        if (camTexture != null) {
            camTexture.Stop();
            displayCameraOverlay = false;
        }
    }


}
