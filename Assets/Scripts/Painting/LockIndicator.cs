using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockIndicator : MonoBehaviour
{
    #region Inspector Fields

    [SerializeField] private Image lockIcon;
    [SerializeField] private Image unlockIcon;

    #endregion // Inspector Fields

    public void UpdateLockIcon(bool isLocked)
    {
        lockIcon.gameObject.SetActive(isLocked);
        unlockIcon.gameObject.SetActive(!isLocked);
    }
}
