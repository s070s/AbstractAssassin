using UnityEngine;
using TMPro;
using System;
public class HealthTextUpdate : MonoBehaviour
{
    public TMP_Text healthText;
    public HealthManager manager;
    // Update is called once per frame
    void Update()
    {
        healthText.text = "Health:"+Convert.ToString(manager.currentHealth);
    }
}
