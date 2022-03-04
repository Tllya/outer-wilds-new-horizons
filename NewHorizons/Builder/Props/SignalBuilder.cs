﻿using NewHorizons.Components;
using NewHorizons.External;
using NewHorizons.Handlers;
using NewHorizons.Utility;
using OWML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Logger = NewHorizons.Utility.Logger;

namespace NewHorizons.Builder.Props
{
    public static class SignalBuilder
    {
        private static AnimationCurve _customCurve = null;

        private static Dictionary<SignalName, string> _customSignalNames;
        private static Stack<SignalName> _availableSignalNames;
        private static int _nextCustomSignalName;

        private static Dictionary<SignalFrequency, string> _customFrequencyNames;
        private static int _nextCustomFrequencyName;

        public static int NumberOfFrequencies;

        public static void Init()
        {
            Logger.Log($"Initializing SignalBuilder");
            _customSignalNames = new Dictionary<SignalName, string>();
            _availableSignalNames = new Stack<SignalName> (new SignalName[]
            {
                (SignalName)17,
                (SignalName)18,
                (SignalName)19,
                (SignalName)26,
                (SignalName)27,
                (SignalName)28,
                (SignalName)29,
                (SignalName)33,
                (SignalName)34,
                (SignalName)35,
                (SignalName)36,
                (SignalName)37,
                (SignalName)38,
                (SignalName)39,
                (SignalName)50,
                (SignalName)51,
                (SignalName)52,
                (SignalName)53,
                (SignalName)54,
                (SignalName)55,
                (SignalName)56,
                (SignalName)57,
                (SignalName)58,
                (SignalName)59,
                SignalName.WhiteHole_WH,
                SignalName.WhiteHole_SS_Receiver,
                SignalName.WhiteHole_CT_Receiver,
                SignalName.WhiteHole_CT_Experiment,
                SignalName.WhiteHole_TT_Receiver,
                SignalName.WhiteHole_TT_TimeLoopCore,
                SignalName.WhiteHole_TH_Receiver,
                SignalName.WhiteHole_BH_NorthPoleReceiver,
                SignalName.WhiteHole_BH_ForgeReceiver,
                SignalName.WhiteHole_GD_Receiver,
            });
            _customFrequencyNames = new Dictionary<SignalFrequency, string>() {
                { SignalFrequency.Statue, "FREQ_STATUE" }, 
                { SignalFrequency.Default, "FREQ_UNKNOWN" }, 
                { SignalFrequency.WarpCore, "FREQ_WARP_CORE" } 
            };
            _nextCustomSignalName = 200;
            _nextCustomFrequencyName = 256;
            NumberOfFrequencies = 8;
        }

        public static SignalFrequency AddFrequency(string str)
        {
            Logger.Log($"Registering new frequency name [{str}]");

            if (NumberOfFrequencies == 31)
            {
                Logger.LogWarning($"Can't store any more frequencies, skipping [{str}]");
                return SignalFrequency.Default;
            }

            var freq = CollectionUtilities.KeyByValue(_customFrequencyNames, str);
            if (freq != default)
            {
                return freq;
            }

            freq = (SignalFrequency)_nextCustomFrequencyName;
            _nextCustomFrequencyName *= 2;
            _customFrequencyNames.Add(freq, str);

            NumberOfFrequencies++;

            // This stuff happens after the signalscope is Awake so we have to change the number of frequencies now
            GameObject.FindObjectOfType<Signalscope>()._strongestSignals = new AudioSignal[NumberOfFrequencies+1];

            return freq;
        }

        public static string GetCustomFrequencyName(SignalFrequency frequencyName)
        {
            _customFrequencyNames.TryGetValue(frequencyName, out string name);
            return name;
        }

        public static SignalName AddSignalName(string str)
        {
            Logger.Log($"Registering new signal name [{str}]");
            SignalName newName;

            if (_availableSignalNames.Count == 0) newName = (SignalName)_nextCustomSignalName++;
            else newName = _availableSignalNames.Pop();

            _customSignalNames.Add(newName, str);
            return newName;
        }

        public static string GetCustomSignalName(SignalName signalName)
        {
            _customSignalNames.TryGetValue(signalName, out string name);
            return name;
        }

        public static void Make(GameObject body, Sector sector, SignalModule module, IModBehaviour mod)
        {
            foreach(var info in module.Signals)
            {
                Make(body, sector, info, mod);
            }
        }

        public static void Make(GameObject body, Sector sector, SignalModule.SignalInfo info, IModBehaviour mod)
        {
            var signalGO = new GameObject($"Signal_{info.Name}");
            signalGO.SetActive(false);
            signalGO.transform.parent = body.transform;
            signalGO.transform.localPosition = info.Position != null ? (Vector3)info.Position : Vector3.zero;
            signalGO.layer = LayerMask.NameToLayer("AdvancedEffectVolume");

            var source = signalGO.AddComponent<AudioSource>();
            var owAudioSource = signalGO.AddComponent<OWAudioSource>();

            AudioSignal audioSignal;
            if (info.InsideCloak) audioSignal = signalGO.AddComponent<CloakedAudioSignal>();
            else audioSignal = signalGO.AddComponent<AudioSignal>();

            var frequency = StringToFrequency(info.Frequency);
            var name = StringToSignalName(info.Name);

            AudioClip clip = null;
            if(info.AudioClip != null) clip = SearchUtilities.FindResourceOfTypeAndName<AudioClip>(info.AudioClip);
            else if (info.AudioFilePath != null)
            {
                try
                {
                    clip = AudioUtility.LoadAudio(mod.ModHelper.Manifest.ModFolderPath + "/" + info.AudioFilePath);
                }
                catch(Exception e)
                {
                    Logger.LogError($"Couldn't load audio file {info.AudioFilePath} : {e.Message}");
                }
            }

            if (clip == null)
            {
                Logger.LogError($"Couldn't find AudioClip {info.AudioClip} or AudioFile {info.AudioFilePath}");
                return;
            }

            audioSignal.SetSector(sector);

            if(name == SignalName.Default) audioSignal._preventIdentification = true;

            audioSignal._frequency = frequency;
            audioSignal._name = name;
            audioSignal._sourceRadius = info.SourceRadius;
            audioSignal._revealFactID = info.Reveals;
            audioSignal._onlyAudibleToScope = info.OnlyAudibleToScope;
            audioSignal._identificationDistance = info.IdentificationRadius;
            audioSignal._canBePickedUpByScope = true;

            source.clip = clip;
            source.loop = true;
            source.minDistance = 0;
            source.maxDistance = 30;
            source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            source.rolloffMode = AudioRolloffMode.Custom;

            if(_customCurve == null)
                _customCurve = GameObject.Find("Moon_Body/Sector_THM/Characters_THM/Villager_HEA_Esker/Signal_Whistling").GetComponent<AudioSource>().GetCustomCurve(AudioSourceCurveType.CustomRolloff);

            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _customCurve);
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.volume = 0.5f;
            source.dopplerLevel = 0;

            owAudioSource.SetTrack(OWAudioMixer.TrackName.Signal);

            // Frequency detection trigger volume

            var signalDetectionGO = new GameObject($"SignalDetectionTrigger_{info.Name}");
            signalDetectionGO.SetActive(false);
            signalDetectionGO.transform.parent = body.transform;
            signalDetectionGO.transform.localPosition = info.Position != null ? (Vector3)info.Position : Vector3.zero;
            signalDetectionGO.layer = LayerMask.NameToLayer("AdvancedEffectVolume");

            var sphereShape = signalDetectionGO.AddComponent<SphereShape>();
            var owTriggerVolume = signalDetectionGO.AddComponent<OWTriggerVolume>();
            var audioSignalDetectionTrigger = signalDetectionGO.AddComponent<AudioSignalDetectionTrigger>();

            sphereShape.radius = info.DetectionRadius == 0 ? info.SourceRadius + 30 : info.DetectionRadius;
            audioSignalDetectionTrigger._signal = audioSignal;
            audioSignalDetectionTrigger._trigger = owTriggerVolume;

            signalGO.SetActive(true);
            signalDetectionGO.SetActive(true);                                         
        }

        private static SignalFrequency StringToFrequency(string str)
        {
            foreach(SignalFrequency freq in Enum.GetValues(typeof(SignalFrequency)))
            {
                if (str.Equals(freq.ToString())) return freq;
            }
            var customName = CollectionUtilities.KeyByValue(_customFrequencyNames, str);

            if (customName == default) customName = AddFrequency(str);

            return customName;
        }

        private static SignalName StringToSignalName(string str)
        {
            foreach (SignalName name in Enum.GetValues(typeof(SignalName)))
            {
                if (str.Equals(name.ToString())) return name;
            }
            var customName = CollectionUtilities.KeyByValue(_customSignalNames, str);
            if (customName == default) customName = AddSignalName(str);

            return customName;
        } 
    }
}