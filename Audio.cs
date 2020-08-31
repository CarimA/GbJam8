using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using Cyotek.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace GBJamGame
{
    public class SoundEffect
    {
        private ISoundOut _soundOut;
        private IWaveSource _waveSource;
    }

    public class Audio
    {
        private readonly ObservableCollection<MMDevice> _devices;
        //private ISoundOut _soundOut;
        //private IWaveSource _waveSource;
        private Random _random;

        private CircularBuffer<SoundEffect> _soundEffects;

        private event EventHandler<PlaybackStoppedEventArgs> PlaybackStopped;

        public Audio()
        {
            _random = new Random();
            _devices = new ObservableCollection<MMDevice>();

            using var mmDeviceEnumerator = new MMDeviceEnumerator();
            using var mmDeviceCollection = mmDeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);
            _devices.Add(mmDeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console));
            foreach (var device in mmDeviceCollection)
                _devices.Add(device);
        }

        /*private void CleanupPlayback()
        {
            if (_soundOut != null)
            {
                _soundOut.Dispose();
                _soundOut = null;
            }

            if (_waveSource != null)
            {
                _waveSource.Dispose();
                _waveSource = null;
            }

        }*/

        public void PlaySfxPitched(string filename, float pitch)
        {
            var sample = CodecFactory.Instance.GetCodec(filename)
                .ChangeSampleRate(11025)
                .ToSampleSource();

            var ps = new PitchShifter(sample);
            ps.PitchShiftFactor = pitch;

            var waveSource = ps
                .ToMono()
                .ToWaveSource(8);


            var soundOut = new WasapiOut { Latency = 100, Device = _devices.First() };
            soundOut.Initialize(waveSource);
            soundOut.Volume = 1f;

            soundOut.Play();
        }

        public void PlaySfxHigh(string filename)
        {
            PlaySfxPitched(filename, 2f);
        }

        public void PlaySfxRandomPitched(string filename)
        {
            var min = 0.8f;
            var max = 1.15f;
            var diff = max - min;
            var num = min + (diff * (float)_random.NextDouble());

            PlaySfxPitched(filename, num);
        }

        public void PlaySfx(string filename)
        {
            var waveSource = CodecFactory.Instance.GetCodec(filename)
                .ChangeSampleRate(11025)
                .ToSampleSource()
                .ToMono()
                .ToWaveSource(8);

            var soundOut = new WasapiOut {Latency = 100, Device = _devices.First()};
            soundOut.Initialize(waveSource);
            soundOut.Volume = 1f;

            soundOut.Play();

            //if (PlaybackStopped != null)
            //    _soundOut.Stopped += PlaybackStopped;
        }
    }
}
