using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Controllers.Web
{
    public class RemoteTextureHandler : MonoBehaviour
    {
        
        public static async Task<Texture2D> GetRemoteTexture (string url)
        {
            using(var webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                var asyncOp = webRequest.SendWebRequest();

                //await until it's done: 
                while( asyncOp.isDone==false )
                {
                    await Task.Delay( 1000 / 30 );
                }

                //read results:
                if( webRequest.isNetworkError || webRequest.isHttpError )
                {
                    Debug.Log( $"{ webRequest.error }, URL:{ webRequest.url }" );
                    return null;
                }

                return DownloadHandlerTexture.GetContent(webRequest);
            }
        }
    }
}