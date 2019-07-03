using System.Collections.Generic;

namespace Minigame
{
    public interface IMinigame
    {
        void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions);

        void Send();

        void Close();
    }
}