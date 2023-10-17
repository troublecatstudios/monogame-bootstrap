using GameBootstrap.Components;
using Troublecat.Core;
using Troublecat.Core.Assets.Sprites;
using Troublecat.Math;
using Troublecat.Utilities;

namespace GameBootstrap;

public class SpriteAnimator : IUpdateableComponent {

    private int _frameIndex = 0;
    private float _frameTime = 0f;
    private AsepriteAnimation? _animation;
    private bool _playing = false;
    private int _direction;

    private float _playbackSpeed = 1f;

    public Sprite? CurrentSprite => _animation?.Frames[_frameIndex].Sprite;

    public void Reset() {
        _frameTime = 0f;
        _frameIndex = 0;
        _playing = true;
        _direction = 1;
        if (_animation != null) {
            if (_animation.StartAtRandomFrame) {
                _frameIndex = Randoms.InRange(0, _animation.Frames.Count - 1);
            }
        }
    }

    public void SetPlayback(float speed){
        _playbackSpeed = 1f/speed;
    }

    public void SetAnimation(AsepriteAnimation animation) {
        _animation = animation;
        Reset();
    }


    public void Update(Timing time) {
        if (_animation == null || !_playing) return;
        _frameTime += time.Delta;
        var frameDuration = _animation.Frames[_frameIndex].FrameDuration * _playbackSpeed;
        if (_frameTime >= frameDuration) {
            _frameTime = 0f;
            _frameIndex += _direction;
        }
        if (_frameIndex < 0) {
            _direction = 1;
        }
        if (_frameIndex >= _animation.Frames.Count) {
            if (_animation.Method == AnimationType.Single) {
                _playing = false;
                return;
            }
            if (_animation.Method == AnimationType.PingPong) {
                _direction = -1;
                return;
            }
            if (_animation.Method == AnimationType.Looped) {
                _frameTime = 0f;
                _frameIndex = 0;
            }
        }
    }
}
