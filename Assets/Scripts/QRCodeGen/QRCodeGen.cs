using QRCoder;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class QRCodeGen : MonoBehaviour
{
    public Image qrCodeImage;

    QRCodeGenerator qrGenerator;

    //public Spr
    void Start()
    {
        // QR 코드 생성기 초기화
        qrGenerator = new QRCodeGenerator();
    }

    public void OnBtnCreate()
    {
        int aaa = UnityEngine.Random.Range(0, 100);
        CreateQRCode("QRCodeGen_" + aaa.ToString());
    }

    void CreateQRCode(string key)
    {
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(key, 
            // "https://unity.com", 
            QRCodeGenerator.ECCLevel.Q);

        // Unity용 QR 코드 생성기 사용
        UnityQRCode qrCode = new UnityQRCode(qrCodeData);
        Texture2D texture = qrCode.GetGraphic(20);

        // Texture2D → Sprite 변환
        Sprite sprite = Sprite.Create(texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)); // 중심 pivot

        // UI Image에 설정
        qrCodeImage.sprite = sprite;

        SaveQRAsPNG(texture, key);
    }

    void SaveQRAsPNG(Texture2D qrTexture, string key)
    {
        if (qrTexture == null)
        {
            Debug.LogError("QR Texture is null");
            return;
        }

        // 1. encode to PNG type
        byte[] bytes = qrTexture.EncodeToPNG(); // 또는 .EncodeToJPG() 도 가능

        // 2. set path
        string path = Path.Combine(Application.persistentDataPath, $"QRCode_{key}.png");

        // 3. save to file.
        File.WriteAllBytes(path, bytes);

        Debug.Log("QR Code saved to: " + path);
     }


    public void OnBtnCreateQRCode(string key)
    {
        CreateQRCode(key);
    }
}