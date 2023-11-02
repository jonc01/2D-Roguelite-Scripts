using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField] private float parallaxEffectMultiplier;
    [SerializeField] private float parallaxEffectMultiplierY;
    [SerializeField] private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    [SerializeField] private float textureUnitSizeX;
    [SerializeField] private float textureUnitSizeY;

    private void Start()
    {
        cameraTransform = Camera.main.transform; //TODO: make sure is getting reference from neverUnload scene
        lastCameraPosition = cameraTransform.position;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        textureUnitSizeY = .9f;
    }

    private void Update() //FixedUpdate prevents jittery effect in editor, but Update fixes in build
    {
        if(cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        // transform.position += deltaMovement * parallaxEffectMultiplier;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier, deltaMovement.y *parallaxEffectMultiplierY, 0);
        // transform.position.y += deltaMovement * parallaxEffectMultiplierY;
        lastCameraPosition = cameraTransform.position;

        if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
            //float offsetPositionY = (cameraTransform.position.y - transform.position.y) % textureUnitSizeY;
        }
    }
}
