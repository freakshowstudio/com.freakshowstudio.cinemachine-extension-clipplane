
using System;
using Unity.Cinemachine;
using UnityEngine;


namespace FreakshowStudio.CinemachineClipPlaneExtension.Runtime
{
    /// <summary>
    /// An add-on module for Cm Camera that lets you adjust the near and far
    /// clip plane after composition.
    /// </summary>
    [AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Clip Plane")]
    [ExecuteAlways]
    [SaveDuringPlay]
    public class CinemachineClipPlane : CinemachineExtension
    {
        /// <summary>
        /// When to apply the adjustment
        /// </summary>
        [field: SerializeField]
        [field: Tooltip("When to apply the adjustment")]
        public CinemachineCore.Stage ApplyAfter { get; set; } =
            CinemachineCore.Stage.Finalize;

        /// <summary>
        /// Near (x) and far (y) clip plane distances, in relation to the
        /// target
        /// </summary>
        [field: SerializeField]
        [field: Tooltip("Near and far clip plane distances from target")]
        [field: MinMaxRangeSlider(-1000, 1000)]
        public Vector2 ClipRange { get; set; } = new (-100, 100);

        private void OnValidate()
        {
            ClipRange = new(
                ClipRange.x,
                Mathf.Max(ClipRange.x + 1e-3f, ClipRange.y)
            );
        }

        private void Reset()
        {
            ApplyAfter = CinemachineCore.Stage.Finalize;
            ClipRange = new (-100, 100);
        }

        /// <summary>Callback to tweak the settings</summary>
        /// <param name="vcam">The virtual camera being processed</param>
        /// <param name="stage">The current pipeline stage</param>
        /// <param name="state">The current virtual camera state</param>
        /// <param name="deltaTime">The current applicable deltaTime</param>
        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != ApplyAfter) return;
            if (!state.HasLookAt()) return;

            var lookAt = state.ReferenceLookAt;
            var camPos = state.GetCorrectedPosition();
            var distance = Vector3.Distance(camPos, lookAt);

            var near = Mathf.Max(distance + ClipRange.x, 1e-3f);
            var far = Mathf.Max(distance + ClipRange.y, near + 1e-3f);

            var lens = state.Lens;
            lens.NearClipPlane = near;
            lens.FarClipPlane = far;
            state.Lens = lens;
        }
    }
}
