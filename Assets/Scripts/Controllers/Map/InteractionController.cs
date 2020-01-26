using System;
using System.Linq;
using Controllers.Web;
using Minigame;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers.Map
{
    public class InteractionController : Singleton<InteractionController>
    {
        [SerializeField] private GameObject[] miniGameCanvases;
        [SerializeField] private GameObject actionCanvas;
        [SerializeField] private GameObject attackButton;
        [SerializeField] private GameObject captureButton;
        [SerializeField] private GameObject categoryCanvas;
        [SerializeField] private GameObject levelUpButton;
        [SerializeField] private Text levelText;
        [SerializeField] private Button[] categoryButtons;

        [HideInInspector] public string CurrentMinigameId;
        
        private static GameManager GameManager => GameManager.Instance;

                
        /// <summary>
        ///     This is used to initialize a MiniGame and display it on-screen.
        /// </summary>
        /// <param name="categoryId">The ID of the chosen category</param>

        public async void StartMinigame(string categoryId)
        {
            LoadingIndicator.Instance.Show();
            categoryCanvas.GetComponent<CanvasGroup>().interactable = false;

            var selectedTile = GameManager.GridController().selectedTile;
            if (selectedTile == null)
                throw new Exception("There is no selected tile, therefore a MiniGame can not be instantiated.");
            
            Debug.Log($"Trying to initialize MiniGame for tile {selectedTile.id} with category {categoryId}");

            var miniGame = await GameManager.InitializeMinigame(selectedTile.id, categoryId);

            if (miniGame == null)
                throw new Exception("No MiniGame was returned from the backend.");
            
            if (miniGame.Type == null)
                throw new Exception($"The provided MiniGame {miniGame.Id} has no type.");

            CurrentMinigameId = miniGame.Id;

            /* if minigame has an image type, construct it from given values */

            MinigameImage image = null;
            if (miniGame.Type == 1)
            {
                var texture = await RemoteTextureHandler.GetRemoteTexture(miniGame.ImageUrl);
                var rect = new Rect(0, 0, texture.width, texture.height);
                var pivot = new Vector2(0.5f, 0.5f);
                var sprite = Sprite.Create(texture, rect, pivot);
                image = new MinigameImage(sprite, miniGame.LicenseInfo);
            }

            // using IMinigame interface to get miniGame depending on given type
            // 0: Sort, 1: Image, 2: Multiple Choice
            var miniGameCanvas = miniGameCanvases[miniGame.Type.Value];
            var miniGameInstance = miniGameCanvas.GetComponent<IMinigame>();

            /**
             * extended logging to check for string length issues
             */

            Debug.Log($"miniGame.task: {miniGame.TaskDescription}");
            foreach (var answer in miniGame.AnswerOptions) Debug.Log($"answerOption: {answer}");
            
            /**
             * get difficulty level from tile controller and initialize miniGame with it
             */
            
            miniGameInstance.Initialize(miniGame.Id, miniGame.TaskDescription, miniGame.AnswerOptions, selectedTile.difficulty, image);

            LoadingIndicator.Instance.Hide();
            CameraBehaviour.Instance.Toggle();
            ActionPointHandler.Instance.Hide();
            ScoreHandler.Instance.Hide();
            miniGameCanvas.SetActive(true);
            categoryCanvas.GetComponent<CanvasGroup>().interactable = true;
            categoryCanvas.SetActive(false);
        }
        
        /// <summary>
        ///     This function checks if there is currently a MiniGame displayed to the user
        /// </summary>
        /// <returns>if a MiniGame canvas is active</returns>
        public bool HasActiveMinigamePanel()
        {
            return miniGameCanvases.Any(canvas => canvas.activeSelf);
        }

        /// <summary>
        /// Handles tile selection if tile belongs to client.
        /// According Canvases are shown in the UI as a result.
        /// </summary>
        /// <param name="difficulty">The difficulty of the tile</param>
        public void HandleOwnTileSelected(int difficulty)
        {
            SetActiveAllChildren(actionCanvas.transform, true);
            actionCanvas.SetActive(true);
            if (captureButton.activeSelf &&
                attackButton.activeSelf)
            {
                captureButton.SetActive(false);
                attackButton.SetActive(false);
            }

            levelText.text = $"Tile Level: {difficulty + 1}";
        }

        /// <summary>
        /// Handles tile selection if tile belongs to opponent.
        /// According Canvases are shown in the UI as a result.
        /// </summary>
        /// <param name="difficulty">The difficulty of the tile</param>
        public void HandleOpponentTileSelected(int difficulty)
        {
            SetActiveAllChildren(actionCanvas.transform, true);
            actionCanvas.SetActive(true);
            if (captureButton.activeSelf &&
                levelUpButton.activeSelf)
            {
                captureButton.SetActive(false);
                levelUpButton.SetActive(false);
            }

            levelText.text = $"Tile Level: {difficulty + 1}";
        }

        /// <summary>
        /// Handles tile selection if tile belongs to no one.
        /// According Canvases are shown in the UI as a result.
        /// </summary>
        /// <param name="difficulty">The difficulty of the tile</param>
        public void HandleEmptyTileSelected(int difficulty)
        {
            categoryCanvas.SetActive(false);
            SetActiveAllChildren(actionCanvas.transform, true);
            actionCanvas.SetActive(true);
            if (attackButton.activeSelf &&
                levelUpButton.activeSelf)
            {
                attackButton.SetActive(false); 
                levelUpButton.SetActive(false);
            }
            
            levelText.text = $"Tile Level: {difficulty + 1}";
        }

        /// <summary>
        ///     This function is called when a tile is already occupied.
        ///     It distinguishes between an own tile and an opponent's tile.
        /// </summary>
        public void LevelUpOrAttackTile()
        {
            var gridController = GameManager.GridController();
            if(gridController == null)
                throw new Exception("GridController is not allowed to be null at this point.");
            
            var chosenCategoryId = gridController.selectedTile
                .chosenCategoryId;
            
            /* if the user selected their start tile, the category panel has to be shown to the client. */

            if (string.IsNullOrEmpty(chosenCategoryId)) ShowCategoryPanel();
            else
            {
                actionCanvas.SetActive(false);
                StartMinigame(chosenCategoryId);
            }
        }

        /// <summary>
        ///     This function is used to show the CategoryPanel.
        /// </summary>
        public void ShowCategoryPanel()
        {
            var gridController = GameManager.GridController();
            if(gridController == null)
                throw new Exception("GridController is not allowed to be null at this point.");
            
            actionCanvas.SetActive(false);
            categoryCanvas.SetActive(true);

            var availableCategories = gridController.selectedTile
                .availableCategories;

            for (var i = 0; i <= 2; i++)
            {
                var categoryButton = categoryButtons[i];
                var index = i;
                
                if(categoryButtons.Length != 3)
                    throw new Exception($"There is at least one category button missing / index: {i}");
                
                categoryButton.GetComponentInChildren<Text>().text = availableCategories[index].Title;
                categoryButton.onClick.RemoveAllListeners();
                categoryButton.onClick.AddListener(() =>
                {
                    StartMinigame(availableCategories[index].Id);
                });
            }

        }

        /// <summary>
        ///     This function is called to close the CategoryPanel and/or the ActionPanel.
        /// </summary>
        public void CloseCategoryAndActionPanel()
        {
            categoryCanvas.SetActive(false);
            actionCanvas.SetActive(false);
            
            GridController gridController = GameManager.GridController();
            if(gridController == null)
                throw new Exception("GridController is not allowed to be null at this point.");

            gridController.ClearSelection();

        }
        
        /// <summary>
        ///     This function is used to activate or deactivate all children of a Transform object.
        /// </summary>
        /// <param name="transform">The Transform object to be modified.</param>
        /// <param name="value">Should children be set active or inactive?</param>
        private static void SetActiveAllChildren(Transform transform, bool value)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(value);
                SetActiveAllChildren(child, value);
            }
        }
        
    }
}