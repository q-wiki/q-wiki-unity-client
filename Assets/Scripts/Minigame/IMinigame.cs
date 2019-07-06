using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minigame
{
    public interface IMinigame
    {
        void Initialize(string miniGameId, string taskDescription, IList<string> answerOptions, int difficulty);

        void Send();

        void Close();

        void SetTimer(float milliseconds);

        void UpdateTimer();
    }
}