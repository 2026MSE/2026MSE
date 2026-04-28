using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class MainButtonUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Renderer targetRenderer;
    public Rooms gotoSceneName = Rooms.MainHall;
    private Material targetMaterial;
    private int intensityID;
    public float targetIntensity = 0.1f;

    private float currentIntensity = 0f;
    void Start()
    {
        targetMaterial = targetRenderer.material;
        intensityID = Shader.PropertyToID("_GlitchIntensity");
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        MainGameManager.instance.SetTurnInfo(new TurnInfo() { currentRoom = gotoSceneName});
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetMaterial.SetFloat(intensityID, targetIntensity);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        targetMaterial.SetFloat(intensityID, currentIntensity);
    }

}
