using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Deterministic;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ResultItemController : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _positionText;
    [SerializeField] private TMPro.TextMeshProUGUI _nameText;
    [SerializeField] private TMPro.TextMeshProUGUI _timeText;

    [SerializeField] private GameObject _playerDot;

    [SerializeField] private Color _otherTimerColor;
    [SerializeField] private Color _playerTimerColor;

    [SerializeField] private Color _otherTextColor;
    [SerializeField] private Color _playerTextColor;

    [SerializeField] private Color _otherBgColor;
    [SerializeField] private Color _playerBgColor;

    [SerializeField] private Image _bgImage;

    public void SetResults(int position, string name, FP time, bool isPlayer)
    {
        _positionText.text = position.ToString();
        _nameText.text = name;

        _playerDot.SetActive(isPlayer);
        _bgImage.color = isPlayer ? _playerBgColor : _otherBgColor;
        _timeText.color = isPlayer ? _playerTimerColor : _otherTimerColor;
        _nameText.color = isPlayer ? _playerTextColor : _otherTextColor;
        _positionText.color = isPlayer ? _playerTextColor : _otherTextColor;

        if (time > 1)
        {
            int minutes = FPMath.FloorToInt(time / 60);
            int seconds = FPMath.FloorToInt(time) % 60;
            int milliSeconds = FPMath.FloorToInt(time % 1 * 100);

            _timeText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliSeconds);
        }
        else
        {
            _positionText.text = "-";
            _timeText.text = "DNF";
        }
    }
}
