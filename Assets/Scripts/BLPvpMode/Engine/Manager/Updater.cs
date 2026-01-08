using System;
using System.Collections.Generic;

using Engine.Entities;

using UnityEditor;

using Engine.Utils;

namespace Engine.Components
{
    using BeginCallback = Action;
    using EndCallback = Action;
    using PauseCallback = Action;
    using ResumeCallback = Action;
    using UpdateCallback = Action<float>;

    public class Updater : EntityComponentV2
    {
        private readonly List<BeginCallback> beginCallbacks = new List<BeginCallback>();
        private readonly List<EndCallback> endCallbacks = new List<EndCallback>();
        private readonly List<PauseCallback> pauseCallbacks = new List<BeginCallback>();
        private readonly List<ResumeCallback> resumeCallbacks = new List<BeginCallback>();
        private readonly List<UpdateCallback> updateCallbacks = new List<UpdateCallback>();

        public Updater OnBegin(BeginCallback callback)
        {
            beginCallbacks.Add(callback);
            return this;
        }

        public Updater OnEnd(EndCallback callback)
        {
            endCallbacks.Add(callback);
            return this;
        }

        public Updater OnPause(PauseCallback callback)
        {
            pauseCallbacks.Add(callback);
            return this;
        }

        public Updater OnResume(ResumeCallback callback)
        {
            resumeCallbacks.Add(callback);
            return this;
        }

        public Updater OnUpdate(UpdateCallback callback)
        {
            updateCallbacks.Add(callback);
            return this;
        }

        public void Begin()
        {
            for (var i = beginCallbacks.Count - 1; i >= 0; --i)
            {
                beginCallbacks[i]();
            }
        }

        public void End()
        {
            for (var i = endCallbacks.Count - 1; i >= 0; --i)
            {
                endCallbacks[i]();
            }
        }

        public void Pause()
        {
            for (var i = pauseCallbacks.Count - 1; i >= 0; --i)
            {
                pauseCallbacks[i]();
            }
        }

        public void Resume()
        {
            for (var i = resumeCallbacks.Count - 1; i >= 0; --i)
            {
                resumeCallbacks[i]();
            }
        }

        public void ProcessUpdate(float delta)
        {
            // Faster than foreach or ForEach (Linq).
            for (var i = updateCallbacks.Count - 1; i >= 0; --i)
            {
                updateCallbacks[i](delta);
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update += ProcessEditorUpdate;
            }
#endif // UNITY_EDITOR
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.update -= ProcessEditorUpdate;
            }
#endif // UNITY_EDITOR
        }

        private void ProcessEditorUpdate()
        {
            var delta = TimeUtils.GetDelta();
            ProcessUpdate(delta);
        }
    }
}
