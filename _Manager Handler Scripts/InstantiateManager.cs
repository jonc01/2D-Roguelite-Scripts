using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateManager : MonoBehaviour
{
    //Manages Instantiate related scripts or objects
    public static InstantiateManager Instance;

    [Header("References")]
    public IndicatorManager Indicator;
    public VFXManager VFX;
    public TextPopupsHandler TextPopups;
    public HitEffectsHandler HitEffects;
    public HitEffectsHandler ParryEffects;
    //public HitEffectsHandler OnKillEffects;

    public OrbManager XPOrbs;

    void Awake()
    {
        Instance = this;

        if(Indicator == null) Indicator = GetComponentInChildren<IndicatorManager>();
        if(VFX == null) VFX = GetComponentInChildren<VFXManager>();
        if(TextPopups == null) TextPopups = GameObject.FindGameObjectWithTag("TextPopupsHandler").GetComponent<TextPopupsHandler>();
        if(HitEffects == null) HitEffects = GetComponentInChildren<HitEffectsHandler>();
        //if(OnKillEffects == null) OnKillEffects = GameObject.Find("OnKillEffects").GetComponent<HitEffectsHandler>();
        if(XPOrbs == null) XPOrbs = GetComponentInChildren<OrbManager>();
    }
    
}
