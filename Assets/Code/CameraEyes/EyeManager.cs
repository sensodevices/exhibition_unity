using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EyeManager : MonoBehaviour {

    public string serverIpAddress;
    public Int32 serverPort;
    public GameObject[] sprites;

    private NetworkEyeConnection eyeConnection;

    private int curEye = 1;
    private Byte[] frameBuf;

    private Texture2D[] textures;
    private Color32[] colorsBuf;

    // Use this for initialization
    void Start() {
        frameBuf = new Byte[NetworkEyeConnection.RECV_BUFFER_SIZE];
        eyeConnection = new NetworkEyeConnection(serverIpAddress, serverPort);
        textures = new Texture2D[sprites.Length];
        colorsBuf = null;
	}

    private void Update()
    {
        UInt16[] dim = eyeConnection.GetFrame((EyeIndex)curEye, ref frameBuf);
        int ind = curEye - 1;
        if (dim[0] > 0 && dim[1] > 0)
        {
            int pixelsCnt = dim[0] * dim[1];
            int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
            int length = lengthOfColor32 * pixelsCnt;        
            
            // Initialize objects...
            if (colorsBuf == null) colorsBuf = new Color32[pixelsCnt];
            if (textures[ind] == null)
            {
                textures[ind] = new Texture2D(dim[0], dim[1], TextureFormat.RGBA32, false);
                var rend = sprites[ind].GetComponent<SpriteRenderer>();
                var aSprite = Sprite.Create(textures[ind], new Rect(0, 0, textures[ind].width, textures[ind].height), new Vector2(0.5f, 0.5f));
                rend.sprite = aSprite;
            }

            /*for (int y = 0; y < dim[1]; ++y)
            {
                for (int x = 0; x < dim[0]; ++x)
                {
                    int i = (y * dim[0] + x);
                    textures[ind].SetPixel(x, y, new Color32(frameBuf[i * 4 + 2], frameBuf[i * 4 + 1], frameBuf[i * 4], frameBuf[i * 4 + 3]));
                }
                //colorsBuf[i] = ;
            }*/
            
            GCHandle handle = default(GCHandle);
            try
            {
                handle = GCHandle.Alloc(colorsBuf, GCHandleType.Pinned);
                IntPtr colorsPtr = handle.AddrOfPinnedObject();
                Marshal.Copy(frameBuf, 0, colorsPtr, length);
            }
            finally
            {
                if (handle != default(GCHandle)) handle.Free();
            }
            textures[ind].SetPixels32(colorsBuf);
            textures[ind].Apply();
            var aRenderer = sprites[ind].GetComponent<SpriteRenderer>();
            //aRenderer.sprite.texture.SetPixels32(colorsBuf);
        }
    }

    // Update is called once per frame
    void OnDestroy () {
        eyeConnection = null;
	}
}
