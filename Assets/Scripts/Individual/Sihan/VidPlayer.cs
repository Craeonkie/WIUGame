using UnityEngine;
using UnityEngine.Video;

public class VidPlayer : MonoBehaviour
{
    public string videoFileName = "with.mp4";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayVideo();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayVideo()
    {
        VideoPlayer vp = GetComponent<VideoPlayer>();

        if (vp != null)
        {
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            Debug.Log(videoPath);
            vp.url = videoPath;
            vp.Play();
        }
    }
}