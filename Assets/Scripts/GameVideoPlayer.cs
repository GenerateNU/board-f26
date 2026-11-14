using UnityEngine;
using System.Threading.Tasks;

public class VideoPlayer : MonoBehaviour
{
    public static VideoPlayer Instance;
    public enum VideoType { EatenFish, EatenAlligator }
    public bool isVideoOpen { get; private set; }
    [SerializeField] private UnityEngine.Video.VideoClip eatenFishVideo;
    [SerializeField] private UnityEngine.Video.VideoClip eatenAlligatorVideo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Instance && Instance != this)
        {
            Debug.Log("already has an instance of Video Player");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => StopVideo());
    }

    public async Task PlayVideo(VideoType type)
    {
        var videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.frame = 0;
        switch(type)
        {
            case VideoType.EatenFish:
                videoPlayer.clip = eatenFishVideo;
                break;
            case VideoType.EatenAlligator:
                videoPlayer.clip = eatenAlligatorVideo;
                break;
        }
        gameObject.GetComponent<UnityEngine.UI.RawImage>().enabled = true;
        videoPlayer.Play();
        isVideoOpen = true;
        while(videoPlayer.isPlaying)
        {
            await Awaitable.WaitForSecondsAsync(0.1f);
        }   
    }

    private void StopVideo()
    {
        var videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        videoPlayer.Stop();
        isVideoOpen = false;
        gameObject.GetComponent<UnityEngine.UI.RawImage>().enabled = false;
        RenderTexture rt = videoPlayer.targetTexture;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.clear); // or Color.clear for transparency
        RenderTexture.active = null;
        videoPlayer.frame = 0;
        Debug.Log("Stopping video");
    }
}
