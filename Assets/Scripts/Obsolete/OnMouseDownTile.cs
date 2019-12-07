using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Obsolete
{
    /// <summary>
    ///     This classed is used to handle clicks on tiles of the grid while being in-game.
    /// </summary>
    [Obsolete("This class is currently not used anymore.", true)]
    public class OnMouseDownTile : MonoBehaviour
    {
        private Renderer _objectRenderer;
        public GameObject actionCanvas;
        public GameObject attackButton;
        public GameObject captureButton;
        public GameObject categoryCanvas;
        public GameObject levelUpButton;

        /// <summary>
        ///     Start is called before the first frame update.
        /// </summary>
        private void Start()
        {
            _objectRenderer = GetComponent<Renderer>();
        }


        /// <summary>
        ///     Set all children of a Transform active or inactive.
        /// </summary>
        /// <param name="transform">Transform to use.</param>
        /// <param name="value">Should children be set active or inactive?</param>
        private void SetActiveAllChildren(Transform transform, bool value)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(value);

                SetActiveAllChildren(child, value);
            }
        }

        /// <summary>
        ///     Handle a mouse down event.
        /// </summary>
        private void OnMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (_objectRenderer.sharedMaterial.name == "Red")
            {
                SetActiveAllChildren(actionCanvas.GetComponent<Transform>(), true);
                Debug.Log("Red");
                //Instantiate(actionPanelPrefab, GameObject.FindGameObjectWithTag("Canvas").transform);
                actionCanvas.SetActive(true);

                if (captureButton.activeSelf && attackButton.activeSelf)
                {
                    captureButton.SetActive(false);
                    attackButton.SetActive(false);
                }
            }
            else if (_objectRenderer.sharedMaterial.name == "Blue")
            {
                SetActiveAllChildren(actionCanvas.GetComponent<Transform>(), true);
                Debug.Log("Blue");
                actionCanvas.SetActive(true);

                if (captureButton.activeSelf && levelUpButton.activeSelf)
                {
                    captureButton.SetActive(false);
                    levelUpButton.SetActive(false);
                }
            }
            else if (_objectRenderer.sharedMaterial.name == "White")
            {
                categoryCanvas.SetActive(false);
                SetActiveAllChildren(actionCanvas.GetComponent<Transform>(), true);
                Debug.Log("White");
                actionCanvas.SetActive(true);

                if (attackButton.activeSelf && levelUpButton.activeSelf)
                {
                    attackButton.SetActive(false);
                    levelUpButton.SetActive(false);
                }
            }
            else
            {
                Debug.Log("Invalid");
            }
        }
    }
}