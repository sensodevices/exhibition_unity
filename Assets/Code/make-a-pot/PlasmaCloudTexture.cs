using UnityEngine;
using System;

public class PlasmaCloudTexture : MonoBehaviour {

    private PlasmaTextureGenerator textGen;
    void Start ()
    {
        textGen = new PlasmaTextureGenerator(8);
        Texture2D potTexture = textGen.Generate(100, 2.0f);
        GetComponent<Renderer>().material.SetTexture("_MainTexture", potTexture);
    }

}