using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using WikidataGame.Models;

namespace Controllers.Map
{
    /// <summary>
    ///     This class handles the behaviour of a single tile.
    ///     Several values from the backend are stored and updated in place to ensure accessibility.
    /// </summary>
    public class TileController : MonoBehaviour
    {
        /**
        * private fields
        */
    
        private MeshRenderer _circleRenderer;
        private bool _direction;
        private Game _game;
        private bool _isHighlight;
        private string _myId;
    
        /**
        * public fields
        */
        
        public IList<Category> availableCategories;
        public string chosenCategoryId;
        public int difficulty;
        public string id;
        public long internalId;
        public string ownerId;
        public Material[] tileMaterials;

        private static GameManager GameManager => GameManager.Instance;

        /// <summary>
        ///     When a TileController is instantiated, its material is set.
        /// </summary>
        private void Start()
        {
            _myId = GameManager.PlayerId();
            _circleRenderer = transform
                .Find("TileBase/OuterCircle")
                .GetComponent<MeshRenderer>();

            if (ownerId == _myId)
                _circleRenderer.material = tileMaterials[0];
            else if (ownerId != _myId && ownerId != null)
                _circleRenderer.material = tileMaterials[1];
        }

        /// <summary>
        ///     The update function checks if a tile needs to be highlighted.
        ///     It is highlighted if its a possible move for the current user.
        ///     The highlighting looks like a glow.
        ///     Directions of alpha value: true = up, false = down
        /// </summary>
        private void Update()
        {
            if (_isHighlight)
            {
                var color = _circleRenderer.material.color;
                _direction = Math.Abs(color.a) <= 0.1f || Math.Abs(color.a) >= 0.3f ? !_direction : _direction;
                color.a += _direction ? 0.02f : -0.02f;
                _circleRenderer.material.color = color;
            }
        }

        /// <summary>
        ///     This function is used to activate or deactivate all children of a Transform object.
        /// </summary>
        /// <param name="transform">The Transform object to be modified.</param>
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
        ///     When the user clicks on a tile that's accessible and a possible move,
        ///     a canvas is shown depending on the state of the tile.
        /// </summary>
        private void OnMouseDown()
        {
            if (IsPointerOverUIObject())
                return;

            var gridController = GameManager.GridController();
            if(gridController == null)
                throw new Exception("GridController is not allowed to be null at this point.");

            var interactionController = InteractionController.Instance;
            if(interactionController == null)
                throw new Exception("InteractionController is not allowed to be null at this point.");

            /* don't do anything if tile is not a possible move for the client */
            if (!gridController.IsPossibleMove(this))
                return;
        
            var previousTile = gridController.selectedTile;
            gridController.selectedTile = this;

            if (previousTile != null)
            {
                /* reset tile material of the previously selected tile */
                var tile = previousTile.GetComponent<TileController>();
                tile.Deselect();
            }

            gameObject.transform
                .Find("TileBase/OuterCircle")
                .GetComponent<MeshRenderer>()
                .material = tileMaterials[2];

            /* Tile is owned */
            if (ownerId == _myId)
            {
                interactionController.HandleOwnTileSelected(difficulty);
            }
        
            /* Tile belongs to opponent */
            else if (!string.IsNullOrEmpty(ownerId) && ownerId != _myId)
            {
                interactionController.HandleOpponentTileSelected(difficulty);
            }
        
            /* Tile is empty */
            else if (string.IsNullOrEmpty(ownerId))
            {
                interactionController.HandleEmptyTileSelected(difficulty);
            }
            else
            {
                Debug.LogError($"Tile {id} seems to be broken - no actions can be done here.");
            }
        }

        /// <summary>
        /// Use this function to deselect a tile and restore its circle material accordingly.
        /// </summary>
        public void Deselect()
        {
            var circleRenderer = transform
                .Find("TileBase/OuterCircle")
                .GetComponent<MeshRenderer>();
        
            if (string.IsNullOrEmpty(ownerId))
                circleRenderer.material = tileMaterials[3];
            else if (ownerId == _myId)
                circleRenderer.material = tileMaterials[0];
            else
                circleRenderer.material = tileMaterials[1];
        }

        /// <summary>
        ///     This function is used to check if there are any UI objects showing above the tile.
        /// </summary>
        /// <returns></returns>
        private bool IsPointerOverUIObject()
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        /// <summary>
        ///     This function is used to set whether a tile should be highlighted.
        /// </summary>
        /// <param name="isHighlight">Should a tile be highlighted?</param>
        public void SetHighlight(bool isHighlight)
        {
            _isHighlight = isHighlight;
        }
    }
}