using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaybackItem : MonoBehaviour {

    [HideInInspector]
    public RecorderAlt.RecordedData[] preRecordedData;
    private Transform _thisTransform;
    private int _currFrame;
    private int _frameCount = 1;

    private int _lastIndex = 0;
    private RecorderAlt.typeOfLoop loopType;

    private void Start()
    {
        _thisTransform = this.gameObject.transform;
        _currFrame = 0;

        loopType = RecorderAlt.instance.loopType;
        for (int i = 0; i < preRecordedData.Length; i++)
        {
            if (preRecordedData[i].scale == Vector3.zero) break;
            _lastIndex++;
        }
    }

    public void Update()
    {
        if (_currFrame > _lastIndex)
        {
            if (loopType == RecorderAlt.typeOfLoop.JUMPTO) _currFrame = 0;
            else if (loopType == RecorderAlt.typeOfLoop.PINGPONG) _frameCount = -1;
        }
        else if (_currFrame < 0 && loopType == RecorderAlt.typeOfLoop.PINGPONG)
        {
            _currFrame = 0;
            _frameCount = 1;
        }

        _thisTransform.position = preRecordedData[_currFrame].position;
        _thisTransform.rotation = preRecordedData[_currFrame].rotation;
        _thisTransform.localScale = preRecordedData[_currFrame].scale;

        _currFrame += _frameCount;
    }

}
