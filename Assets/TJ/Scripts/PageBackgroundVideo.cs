using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class PageBackgroundVideo : MonoBehaviour
{
    public RawImage background;
    public bool useStreamingAssets = true;
    public string streamingFileName = "bg1.mp4";
    public VideoClip clipReference;
    public bool loop = true;
    public bool mute = true;
    public RenderTexture sharedRT;

    VideoPlayer _vp;
    RenderTexture _rt;

    void Awake()
    {
        _vp = GetComponent<VideoPlayer>();
        _vp.playOnAwake = false;
        _vp.waitForFirstFrame = true;
        _vp.skipOnDrop = true;
        _vp.isLooping = loop;
        _vp.audioOutputMode = mute ? VideoAudioOutputMode.None : VideoAudioOutputMode.Direct;

        if (!background) background = GetComponent<RawImage>();
        if (background) background.raycastTarget = false;
    }

    void OnEnable()  { StartCoroutine(PrepareAndPlay()); }
    void OnDisable() { StopVideo(); }

    IEnumerator PrepareAndPlay()
    {

        if (useStreamingAssets) {
            _vp.source = VideoSource.Url;
            _vp.url = System.IO.Path.Combine(Application.streamingAssetsPath, streamingFileName);
        } else {
            _vp.source = VideoSource.VideoClip;
            _vp.clip  = clipReference;
        }

        _vp.Prepare();
        while (!_vp.isPrepared) yield return null;

        int w = (int)_vp.width;
        int h = (int)_vp.height;

        if (sharedRT) {
            _vp.targetTexture = sharedRT;
            if (background) background.texture = sharedRT;
        } else {
            if (_rt == null || _rt.width != w || _rt.height != h) {
                if (_rt != null) _rt.Release();
                _rt = new RenderTexture(w>0?w:1920, h>0?h:1080, 0) { name = $"{name}_BG_RT" };
            }
            _vp.targetTexture = _rt;
            if (background) background.texture = _rt;
        }

        _vp.Play();
    }

    public void StopVideo()
    {
        if (_vp && _vp.isPlaying) _vp.Stop();
        if (background && !sharedRT) background.texture = null;
    }
}
