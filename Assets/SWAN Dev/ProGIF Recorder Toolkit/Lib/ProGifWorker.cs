// Created by SwanDEV 2019
using System;
using System.Collections;
using System.Collections.Generic;

#if !UNITY_WEBGL
using System.Threading;
using ThreadPriority = System.Threading.ThreadPriority;
#endif

internal sealed class ProGifWorker
{
    private int id;
    internal List<Frame> m_Frames;
    internal ProGifEncoder m_Encoder;
    internal Action<int> m_OnFileSaveProgress;

#if UNITY_WEBGL
    internal ProGifWorker(int workerId)
    {
        id = workerId;
    }

    /// <summary>
    /// Start corountine for WebGL 
    /// </summary>
    internal IEnumerator Start_Corountine()
    {
        m_Encoder.Start();
        for (int i = 0; i < m_Frames.Count; i++)
        {
            m_Encoder.AddFrame(m_Frames[i]);
            if (m_OnFileSaveProgress != null) m_OnFileSaveProgress(id);
            yield return 0;
        }
        m_Encoder.Finish();
    }
#else
    private Thread thread;

    internal ProGifWorker(int workerId, ThreadPriority priority)
    {
        id = workerId;
        thread = new Thread(Run);
        thread.Priority = priority;
    }

    /// <summary>
    /// Start method for threads supported platforms
    /// </summary>
    internal void Start()
    {
        thread.Start();
    }

    private void Run()
    {
        m_Encoder.Start();
        for (int i = 0; i < m_Frames.Count; i++)
        {
            m_Encoder.AddFrame(m_Frames[i]);
            if (m_OnFileSaveProgress != null) m_OnFileSaveProgress(id);
        }
        m_Encoder.Finish();
    }
#endif
}
