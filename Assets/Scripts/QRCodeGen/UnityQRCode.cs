using QRCoder;
using UnityEngine;

public class UnityQRCode
{
    private readonly QRCodeData qrCodeData;

    public UnityQRCode(QRCodeData data)
    {
        qrCodeData = data;
    }

    public Texture2D GetGraphic(int pixelsPerModule)
    {
        int size = qrCodeData.ModuleMatrix.Count * pixelsPerModule;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color32 white = new Color32(255, 255, 255, 255);
        Color32 black = new Color32(0, 0, 0, 255);

        for (int y = 0; y < qrCodeData.ModuleMatrix.Count; y++)
        {
            for (int x = 0; x < qrCodeData.ModuleMatrix.Count; x++)
            {
                bool module = qrCodeData.ModuleMatrix[y][x];
                Color32 color = module ? black : white;

                // 각 모듈을 픽셀로 채움
                for (int i = 0; i < pixelsPerModule; i++)
                {
                    for (int j = 0; j < pixelsPerModule; j++)
                    {
                        int px = (x * pixelsPerModule) + i;
                        int py = (qrCodeData.ModuleMatrix.Count - 1 - y) * pixelsPerModule + j; // 상하 반전
                        texture.SetPixel(px, py, color);
                    }
                }
            }
        }

        texture.Apply();
        return texture;
    }
}
