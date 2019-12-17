using System;
using System.Collections.Generic;

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
        void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions, int difficulty);

        /// <summary>
        ///     Use this to send answers to the backend
        /// </summary>
        void Send();

        /// <summary>
        ///     Use this to shutdown the current MiniGame
        /// </summary>
        void Close();

        /// <summary>
        ///     Use this to shutdown the current MiniGame when Timer reached null
        /// </summary>
        void ForceQuit();
    }
}