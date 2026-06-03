using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class PlayerEmoticonDisplay : MonoBehaviour
{
    public string ownerId; // 이 캐릭터의 주인 ID
    public Image emoticonImage; // 머리 위 이미지 UI

    private string lastEmoticonUrl = ""; // 마지막으로 띄운 이모티콘 주소 (중복 다운로드 방지)

    void Update()
    {
        // 아직 ID를 못 받았거나, 플레이어 목록이 없으면 무시
        if (string.IsNullOrEmpty(ownerId) || PlayerManager.instance.playerList == null) return;

        // 전체 플레이어 목록에서 '나'의 정보를 찾음
        PlayerInfo myInfo = PlayerManager.instance.playerList.Find(p => p.playerId == ownerId);

        if (myInfo != null)
        {
            // 서버의 이모티콘 상태가 내가 알고 있던 것과 달라졌다면? (새로 띄웠거나 껐거나)
            if (myInfo.currentEmoticon != lastEmoticonUrl)
            {
                lastEmoticonUrl = myInfo.currentEmoticon;

                if (string.IsNullOrEmpty(lastEmoticonUrl))
                {
                    // 빈 문자열이 오면 이모티콘 숨기기
                    emoticonImage.gameObject.SetActive(false);
                }
                else
                {
                    // 주소가 오면 이미지 다운로드 후 표시
                    DownloadAndShowEmoticon(lastEmoticonUrl).Forget();
                }
            }
        }
    }

    private async UniTaskVoid DownloadAndShowEmoticon(string url)
    {
        // ServerManager의 함수를 재활용하여 텍스처 다운로드
        Texture2D texture = await ServerManager.instance.FetchTextureFromServer(url);

        if (texture != null && emoticonImage != null)
        {
            emoticonImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            emoticonImage.gameObject.SetActive(true); // 다운로드가 끝나면 짠! 하고 켜기
        }
    }
}