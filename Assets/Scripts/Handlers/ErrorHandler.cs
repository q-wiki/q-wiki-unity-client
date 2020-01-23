using System;
using UnityEngine;

namespace Handlers
{
    public class ErrorHandler : Singleton<ErrorHandler>
    {
        [SerializeField] private ErrorDialog _errorDialog;
        
        /// <summary>
        /// Add exception handling to application when enabled
        /// </summary>
        private void OnEnable()
        {
            Application.logMessageReceived += HandleException;
        }
        
        /// <summary>
        /// Remove exception handling from application when disabled
        /// </summary>
        private void OnDisable()
        {
            Application.logMessageReceived -= HandleException;
        }


        /// <summary>
        /// Show an error to the client by instantiating an error dialog.
        /// </summary>
        /// <param name="message">Message to be displayed to the user</param>
        /// <exception cref="Exception">Canvas could not be found properly</exception>
        public void Error(object message)
        {
            var canvases = GameObject.FindGameObjectsWithTag("Canvas");

            if(canvases == null)
                throw new Exception("Canvas cannot be null.");

            if (canvases.Length > 1)
                throw new Exception("There is more than one canvas tagged as such in the scene.");

            var canvas = canvases[0];

            var errorDialog = Instantiate(_errorDialog, canvas.transform, true);
            var t = errorDialog.transform;
            
            t.SetAsLastSibling();
            t.localScale = new Vector2(1f, 1f);
            t.localPosition = new Vector2(0f, 0f);
            
            errorDialog.Instantiate(message.ToString());
        }

        /// <summary>
        /// Handle all kinds of exceptions which are not handled otherwise and show them to the user
        /// </summary>
        /// <param name="logString">Log string of the exception</param>
        /// <param name="stackTrace">Stack trace of the exception</param>
        /// <param name="type">Type of log</param>
        private void HandleException(string logString, string stackTrace, LogType type)
        {
            
            if (type == LogType.Exception)
            {
                Error(logString);
            }
        }
    }
}