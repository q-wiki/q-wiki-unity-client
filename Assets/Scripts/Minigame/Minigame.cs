using System.Collections.Generic;

namespace Minigame
{
    public interface Minigame
    {
        void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions);

        void Send();
    }
}