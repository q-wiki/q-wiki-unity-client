using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HyperlinkHandler : MonoBehaviour, IPointerClickHandler
{

    private TextMeshProUGUI pTextMeshPro;
    private Camera pCamera;

    private void Start()
    {
        pTextMeshPro = GetComponent<TextMeshProUGUI>();
        pCamera = Camera.current;
    }

    public void OnPointerClick(PointerEventData eventData) {

        var linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, null);
        if( linkIndex != -1 ) { // was a link clicked?
            var linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

            // open the link id as a url, which is the metadata we added in the text field
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}