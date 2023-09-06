
#if OVR_LIPSYNC

namespace CharismaSDK.PlugNPlay
{
    using UnityEngine;

    public class LipsyncHelperOVR : LipsyncHelper
    {
        [SerializeField]
        private AudioSource _headAudioSource;

        private OVRLipSyncContextMorphTarget _lipSyncContextMorphTarget;
        private OVRLipSyncContext _lipSyncContext;

        public override void ResetBlendshapes()
        {
            VisemeToBlendTargets.Clear();

            // This is the default Charisma NPC Viseme configuration
            // Feel free to change this if you desire to use your own NPC
            VisemeToBlendTargets.Add("");           // SIL
            VisemeToBlendTargets.Add("BS.M");       // PP
            VisemeToBlendTargets.Add("BS.F");       // FF
            VisemeToBlendTargets.Add("BS.S");       // TH
            VisemeToBlendTargets.Add("BS.K");       // DD
            VisemeToBlendTargets.Add("BS.K");       // kk
            VisemeToBlendTargets.Add("BS.SH");      // CH
            VisemeToBlendTargets.Add("BS.S");       // SS
            VisemeToBlendTargets.Add("BS.ER");      // nn
            VisemeToBlendTargets.Add("BS.T");       // RR
            VisemeToBlendTargets.Add("BS.AA");      // aa
            VisemeToBlendTargets.Add("BS.EE");      // E
            VisemeToBlendTargets.Add("BS.IH");      // I
            VisemeToBlendTargets.Add("BS.OW");      // O
            VisemeToBlendTargets.Add("BS.TH");      // U

        }

        public override void CreateLipsyncImplementation()
        {
            CreateMorphTarget();
            CreateContext();
        }

        // Creates and adds MorphTarget component to current GameObject
        private void CreateMorphTarget()
        {
            // adds ovr morphtarget component
            if (this.gameObject.TryGetComponent<OVRLipSyncContextMorphTarget>(out var morphTarget))
            {
                _lipSyncContextMorphTarget = morphTarget;
            }
            else
            {
                _lipSyncContextMorphTarget = this.gameObject.AddComponent<OVRLipSyncContextMorphTarget>();
            }

            // Sets head mesh
            _lipSyncContextMorphTarget.skinnedMeshRenderer = HeadMeshRenderer;

            // apply the visemes
            int[] visemeIds = new int[VisemeToBlendTargets.Count];
            int index = 0;

            var dictionary = BlendShapeUtils.GetBlendshapesDic(HeadMeshRenderer.sharedMesh);
            foreach (var viseme in VisemeToBlendTargets)
            {
                // get viseme index based on string passed in above
                if (dictionary.ContainsKey(viseme))
                {
                    visemeIds[index] = dictionary[viseme];
                }
                else
                {
                    visemeIds[index] = -1;
                }
                index++;
            }

            _lipSyncContextMorphTarget.visemeToBlendTargets = visemeIds;

        }

        private void CreateContext()
        {
            // add ovr lipsync component
            if (this.gameObject.TryGetComponent<OVRLipSyncContext>(out var context))
            {
                _lipSyncContext = context;
            }
            else
            {
                _lipSyncContext = this.gameObject.AddComponent<OVRLipSyncContext>();
            }
            _lipSyncContext.audioSource = _headAudioSource;
            _lipSyncContext.audioLoopback = true;
        }
    }
}
#endif
