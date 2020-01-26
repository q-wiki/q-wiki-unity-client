using TMPro;
using UnityEngine;


public class CreditsHandler : MonoBehaviour
 {
     private string _text;
     private TextMeshProUGUI m_TextMeshPro 
         => GetComponentInChildren<TextMeshProUGUI>();
 
     private void Awake()
     {
         var asset = Resources.Load<TextAsset>("Credits/credits");
         _text = asset.text;
         m_TextMeshPro.text = _text;
     }
 }