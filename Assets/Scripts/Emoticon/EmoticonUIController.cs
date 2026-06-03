using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class EmoticonUIController : MonoBehaviour
{
    [Header("UI 연결")]
    public Transform contentTransform;      // 버튼들이 들어갈 부모 (Scroll View -> Viewport -> Content)
    public GameObject emoticonButtonPrefab; // 아까 만든 버튼 원본 (Prefab)
    public GameObject emoticonPanel;        // 이모티콘 창 전체 (전송 후 창을 닫기 위해 연결)

    void Start()
    {
        // UI가 켜질 때 이모티콘 목록을 불러옵니다.
        LoadEmoticons().Forget();
    }

    private async UniTaskVoid LoadEmoticons()
    {
        // 1. ServerManager를 통해 Giphy 목록 가져오기 (12개)
        var giphyData = await ServerManager.instance.GetGiphyTrendingRequest(12);

        if (giphyData != null && giphyData.gifList != null)
        {
            foreach (var gif in giphyData.gifList)
            {
                // 원본 URL(보낼 용도)과 썸네일 URL(버튼에 띄울 용도) 가져오기
                string gifUrl = gif.images.original.url;
                string thumbnailUrl = gif.images.fixed_height.url;

                CreateButton(gifUrl, thumbnailUrl).Forget();
            }
        }
        else
        {
            Debug.LogWarning("이모티콘 목록을 불러오지 못했습니다.");
        }
    }

    private async UniTaskVoid CreateButton(string gifUrl, string thumbnailUrl)
    {
        // 2. 화면에 버튼 생성
        GameObject newBtn = Instantiate(emoticonButtonPrefab, contentTransform);
        Button btnComponent = newBtn.GetComponent<Button>();
        Image imgComponent = newBtn.GetComponent<Image>();

        // 3. 버튼 클릭 시 발생할 이벤트 연결
        btnComponent.onClick.AddListener(() => OnEmoticonClicked(gifUrl));

        // 4. ServerManager를 통해 썸네일 이미지 다운로드 후 버튼에 씌우기
        Texture2D texture = await ServerManager.instance.FetchTextureFromServer(thumbnailUrl);
        if (texture != null)
        {
            // Texture2D를 UI에서 쓸 수 있는 Sprite로 변환
            imgComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }

    private void OnEmoticonClicked(string gifUrl)
    {
        Debug.Log($"이모티콘 선택됨: {gifUrl}");
        SendAndClearEmoticon(gifUrl).Forget();
    }

    private async UniTaskVoid SendAndClearEmoticon(string gifUrl)
    {
        // 5. 서버로 이모티콘 URL 전송
        bool success = await ServerManager.instance.SendEmoticonRequest(gifUrl);

        if (success)
        {
            Debug.Log("서버 전송 성공! 캐릭터 머리 위에 뜹니다.");

            // (선택사항) 전송 후 이모티콘 창 숨기기
            if (emoticonPanel != null) emoticonPanel.SetActive(false);

            // 6. 3초 대기 후 서버에 빈 문자열("")을 보내서 이모티콘 지우기
            await UniTask.Delay(3000);
            await ServerManager.instance.SendEmoticonRequest("");
        }
    }
}