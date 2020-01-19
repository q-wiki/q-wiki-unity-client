using System;
using System.Collections.Generic;
using UnityEngine;

namespace Minigame
{
    /// <summary>
    ///     Basic interface to structure MiniGame frontend implementation
    /// </summary>
    public interface IMinigame
    {
        /// <summary>
        ///     Initialize a MiniGame in the frontend by providing necessary values
        /// </summary>
        /// <param name="miniGameId">ID of the current MiniGame</param>
        /// <param name="taskDescription">Description of the task</param>
        /// <param name="answerOptions">Provided answer options</param>
        /// <param name="difficulty">Given difficulty</param>
        /// <param name="minigameImage">Provided minigame image (optional)</param>
        void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions, int difficulty, MinigameImage minigameImage = null);
        
        
        /// <summary>
        ///     Use this to send answers to the backend
        /// </summary>
        void Submit();

        /// <summary>
        ///     Use this to shutdown the current MiniGame
        /// </summary>
        void Close();

        /// <summary>
        ///     Use this to shutdown the current MiniGame when Timer reached null
        /// </summary>
        void ForceQuit();

        /// <summary>
        ///     Process selection of an answer option by the user
        /// </summary>
        /// <param name="selected">Selected answer option</param>
        void Process(GameObject selected);

        /// <summary>
        ///     Open the client's mobile browser to initialize feedback for the platform
        /// </summary>
        void SendFeedbackToPlatform();
    }
}