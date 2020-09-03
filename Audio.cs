using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using Cyotek.Collections.Generic;
using GBJamGame.Globals;
using Microsoft.Xna.Framework.Audio;

namespace GBJamGame
{
    public class Audio : IDisposable
    {
        //private ISoundOut _soundOut;
        //private IWaveSource _waveSource;

        private SoundMixer _mixer;
        private WasapiOut _soundOut;

        private event EventHandler<PlaybackStoppedEventArgs> PlaybackStopped;

        private readonly SharedMemoryStream _menu;
        private readonly SharedMemoryStream _fill;

        public Audio()
        {
            _mixer = new SoundMixer();
            _soundOut = new WasapiOut();

            _soundOut.Initialize(_mixer.ToWaveSource());
            _soundOut.Play();

            _menu = LoadSound(AppContext.BaseDirectory + "assets/sfx/menu.wav");
            _fill = LoadSound(AppContext.BaseDirectory + "assets/sfx/fill.wav");
        }

        private static SharedMemoryStream LoadSound(string filePath)
        {
            return new SharedMemoryStream(File.ReadAllBytes(filePath));
        }

        private void PlaySfx(SharedMemoryStream stream)
        {
            var sfx = new WaveFileReader(stream.MakeShared())
                .ChangeSampleRate(11025)
                .ToSampleSource()
                .ToMono();
            _mixer.AddSound(sfx);
        }

        public void PlayMenu() => PlaySfx(_menu);


        //public void PlayFill() => PlaySfx(_fill);

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
        public void Dispose()
        {
            _mixer?.Dispose();
            _soundOut?.Dispose();
        }
    }

    public class SoundMixer : ISampleSource
    {
        private readonly List<SoundSource> _soundSources = new List<SoundSource>();
        private readonly object _soundSourcesLock = new object();
        private bool _disposed;
        private float[] _internalBuffer;

        public SoundMixer()
        {
            var sampleRate = 11025;
            var bits = 8;
            var channels = 1;
            var audioEncoding = AudioEncoding.IeeeFloat;

            WaveFormat = new WaveFormat(sampleRate, bits, channels, audioEncoding);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var numberOfSamplesStoredInBuffer = 0;

            Array.Clear(buffer, offset, count);

            lock (_soundSourcesLock)
            {
                if (_disposed)
                    return 0;

                if (count > 0 && _soundSources.Count > 0)
                {
                    _internalBuffer = _internalBuffer.CheckBuffer(count);

                    for (var i = _soundSources.Count - 1; i >= 0; i--)
                    {
                        var soundSource = _soundSources[i];
                        soundSource.Read(_internalBuffer, count);

                        for (int j = offset, k = 0; k < soundSource.SamplesRead; j++, k++)
                        {
                            buffer[j] += _internalBuffer[k];
                        }

                        if (soundSource.SamplesRead > numberOfSamplesStoredInBuffer)
                            numberOfSamplesStoredInBuffer = soundSource.SamplesRead;

                        if (soundSource.SamplesRead == 0)
                        {
                            _soundSources.Remove(soundSource);
                            soundSource.Dispose();
                        }
                    }

                    // TODO Normalize!
                }
            }

            return count;
        }

        public void Dispose()
        {
            lock (_soundSourcesLock)
            {
                _disposed = true;

                foreach (var soundSource in _soundSources)
                {
                    soundSource.Dispose();
                }

                _soundSources.Clear();
            }
        }

        public bool CanSeek => !_disposed;
        public WaveFormat WaveFormat { get; }

        public long Position
        {
            get
            {
                CheckIfDisposed();
                return 0;
            }
            set => throw new NotSupportedException($"{nameof(SoundMixer)} does not support seeking.");
        }

        public long Length
        {
            get
            {
                CheckIfDisposed();
                return 0;
            }
        }

        public void AddSound(ISampleSource sound)
        {
            lock (_soundSourcesLock)
            {
                if (_disposed)
                    return;

                // TODO Check wave format compatibility?
                _soundSources.Add(new SoundSource(sound));
            }
        }

        private void CheckIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SoundMixer));
        }

        private class SoundSource : IDisposable
        {
            private readonly ISampleSource _sound;

            public SoundSource(ISampleSource sound)
            {
                _sound = sound;
            }

            public int SamplesRead { get; private set; }

            public void Dispose()
            {
                _sound.Dispose();
            }

            public void Read(float[] buffer, int count)
            {
                SamplesRead = _sound.Read(buffer, 0, count);
            }
        }
    }

    internal sealed class SharedMemoryStream : Stream
    {
        private readonly object _lock;
        private readonly RefCounter _refCounter;
        private readonly MemoryStream _sourceMemoryStream;
        private bool _disposed;
        private long _position;

        public SharedMemoryStream(byte[] buffer) : this(new object(), new RefCounter(), new MemoryStream(buffer))
        {
        }

        private SharedMemoryStream(object @lock, RefCounter refCounter, MemoryStream sourceMemoryStream)
        {
            _lock = @lock;

            lock (_lock)
            {
                _refCounter = refCounter;
                _sourceMemoryStream = sourceMemoryStream;

                _refCounter.Count++;
            }
        }

        public override bool CanRead
        {
            get
            {
                lock (_lock)
                {
                    return !_disposed;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                lock (_lock)
                {
                    return !_disposed;
                }
            }
        }

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                lock (_lock)
                {
                    CheckIfDisposed();
                    return _sourceMemoryStream.Length;
                }
            }
        }

        public override long Position
        {
            get
            {
                lock (_lock)
                {
                    CheckIfDisposed();
                    return _position;
                }
            }
            set
            {
                lock (_lock)
                {
                    CheckIfDisposed();
                    _position = value;
                }
            }
        }

        // Creates another shallow copy of stream that uses the same underlying MemoryStream
        public SharedMemoryStream MakeShared()
        {
            lock (_lock)
            {
                CheckIfDisposed();
                return new SharedMemoryStream(_lock, _refCounter, _sourceMemoryStream);
            }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (_lock)
            {
                CheckIfDisposed();

                _sourceMemoryStream.Position = Position;
                var seek = _sourceMemoryStream.Seek(offset, origin);
                Position = _sourceMemoryStream.Position;

                return seek;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException($"{nameof(SharedMemoryStream)} is read only stream.");
        }

        // Uses position that is unique for each copy of shared stream
        // to read underlying MemoryStream that is common for all shared copies
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                CheckIfDisposed();

                _sourceMemoryStream.Position = Position;
                var read = _sourceMemoryStream.Read(buffer, offset, count);
                Position = _sourceMemoryStream.Position;

                return read;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException($"{nameof(SharedMemoryStream)} is read only stream.");
        }

        // Reference counting to dispose underlying MemoryStream when all shared copies are disposed
        protected override void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (disposing)
                {
                    _disposed = true;
                    _refCounter.Count--;
                    if (_refCounter.Count == 0)
                        _sourceMemoryStream?.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        private void CheckIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SharedMemoryStream));
        }

        private class RefCounter
        {
            public int Count;
        }
    }
}