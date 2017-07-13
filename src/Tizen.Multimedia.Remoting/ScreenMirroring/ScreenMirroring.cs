/*
 * Copyright (c) 2016 Samsung Electronics Co., Ltd All Rights Reserved
 *
 * Licensed under the Apache License, Version 2.0 (the License);
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an AS IS BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Native = Interop.ScreenMirroring;

namespace Tizen.Multimedia
{
    static internal class ScreenMirroringLog
    {
        internal const string LogTag = "Tizen.Multimedia.ScreenMirroring";
    }

    /// <summary>
    /// ScreenMirroring class provides methods to function as screen mirroring application as sink.
    /// It gives the ability to connect to and disconnect from a screen mirroring source, and
    /// start, pause, and resume the screen mirroring sink, set the resolution or display,
    /// register state change callback function.
    /// </summary>
    public class ScreenMirroring : IDisposable, IDisplayable<int>
    {
        internal VideoInformation _videoInfo;
        internal AudioInformation _audioInfo;
        internal IntPtr _handle;
        internal string _ip;
        internal string _port;

        private bool _disposed = false;
        private EventHandler<StateChangedEventArgs> _stateChanged;
        private Native.StateChangedCallback _stateChangedCallback;

        /// <summary>
        /// Initializes a new instance of the ScreenMirroring class with parameters Ip, Port and Display handle.
        /// Object should be created only when Ip and Port are available.
        /// Create(i.e constructor) api will create a new handle with the given parameters.
        /// </summary>
        /// <param name="display">Display.</param>
        /// <param name="ip">Ip.</param>
        /// <param name="port">Port.</param>
        /// <exception cref="ArgumentException">Thrown when method fail due to an invalid parameter</exception>
        public ScreenMirroring(Display display, string ip, string port)
        {
            int ret = Native.Create(out _handle);
            if (ret != (int)ScreenMirroringError.None)
            {
                ScreenMirroringErrorFactory.ThrowException(ret, "Failed to create Screen Mirroring Sink");
            }

            // initiate values
            _ip = ip;
            _port = port;

            // Set ip and port
            int ret1 = Native.SetIpAndPort(_handle, _ip, _port);
            if (ret1 != (int)ScreenMirroringError.None)
            {
                Log.Error(ScreenMirroringLog.LogTag, "Set ip and port failed" + (ScreenMirroringError)ret1);
                ScreenMirroringErrorFactory.ThrowException(ret, "set ip and port failed");
            }

            Display = display;

            // AudioInfo
            _audioInfo = new AudioInformation();
            _audioInfo._handle = _handle;
            // VideoInfo
            _videoInfo = new VideoInformation();
            _videoInfo._handle = _handle;

            Log.Debug(ScreenMirroringLog.LogTag, "screen mirroring sink created : " + _handle);
        }

        /// <summary>
        /// Screen Mirroring destructor.
        /// </summary>
        ~ScreenMirroring()
        {
            Dispose(false);
        }

        /// <summary>
        /// StateChanged event is raised when state change happens.
        /// Must be called after Create() API.
        /// </summary>
        public event EventHandler<StateChangedEventArgs> StateChanged
        {
            add
            {
                if (_stateChanged == null)
                {
                    RegisterStateChangedEvent();
                }

                _stateChanged += value;
            }

            remove
            {
                _stateChanged -= value;
                if (_stateChanged == null)
                {
                    UnregisterStateChangedEvent();
                }
            }
        }

        /// <summary>
        /// Sets the server ip and port.
        /// This must be called before connect() and after create().
        /// </summary>
        /// <example> If only one handle is used for toggling between more than two source devices,
        /// then this API ahould be used to assign the parameters to the handle.
        /// </example>
        /// <param name="ip">Ip.</param>
        /// <param name="port">Port.</param>
        /// <exception cref="ArgumentException">Thrown when method fail due to an invalid parameter</exception>
        public void SetIpAndPort(string ip, string port)
        {
            int ret = Native.SetIpAndPort(_handle, ip, port);
            if (ret != (int)ScreenMirroringError.None)
            {
                Log.Error(ScreenMirroringLog.LogTag, "Set ip and port failed" + (ScreenMirroringError)ret);
                ScreenMirroringErrorFactory.ThrowException(ret, "set ip and port failed");
            }
        }

        /// <summary>
        /// Set Resolution.
        /// valid state: NULL..
        /// </summary>
        /// <param name="resolution"> example: (R1920x1080P30 | R1280x720P30) </param>
        /// <exception cref="ArgumentException">Thrown when method fail due to an invalid parameter</exception>
        public void SetResolution(ResolutionType resolution)
        {
            int ret = Native.SetResolution(_handle, (int)resolution);
            if (ret != (int)ScreenMirroringError.None)
            {
                Log.Error(ScreenMirroringLog.LogTag, "Set resolution failed" + (ScreenMirroringError)ret);
                ScreenMirroringErrorFactory.ThrowException(ret, "set resolution failed");
            }
        }

        private Display _display;

        private int ApplyDisplay(Display display)
        {
            return display.ApplyTo(this);
        }

        private void ReplaceDisplay(Display newDisplay)
        {
            _display?.SetOwner(null);
            _display = newDisplay;
            _display?.SetOwner(this);
        }

        /// <summary>
        /// Sets the display.
        /// This must be called before prepare() and after create().
        /// </summary>
        /// <example> If only one handle is used for toggling between more than two source devices,
        /// then this API should be used to assign the parameters to the handle.
        /// </example>
        /// <exception cref="ArgumentException">Thrown when method fail due to an invalid parameter</exception>
        public Display Display
        {
            get
            {
                return _display;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Display));
                }

                int ret = ApplyDisplay(value);
                if (ret != (int)ScreenMirroringError.None)
                {
                    Log.Error(ScreenMirroringLog.LogTag, "Set display failed" + (ScreenMirroringError)ret);
                    ScreenMirroringErrorFactory.ThrowException(ret, "set display failed");
                }
            }
        }

        int IDisplayable<int>.ApplyEvasDisplay(DisplayType type, ElmSharp.EvasObject evasObject)
        {
            Debug.Assert(_disposed == false);

            Debug.Assert(Enum.IsDefined(typeof(DisplayType), type));

            return Native.SetDisplay(_handle, (int)type, evasObject);
        }

        /// <summary>
        /// Prepare this instance.
        /// This must be called after Create().
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        public void Prepare()
        {
            int ret = Native.Prepare(_handle);
            if (ret != (int)ScreenMirroringError.None)
            {
                ScreenMirroringErrorFactory.ThrowException(ret, "Failed to prepare sink for screen mirroring");
            }
        }

        /// <summary>
        /// Creates connection and prepare for receiving data from ScreenMirroring source.
        /// This must be called after prepare().
        /// </summary>
        /// <remarks> It will not give the current state. Need to subscribe for event to get the current state </remarks>
        /// <returns>bool value</returns>
        /// <privilege>http://tizen.org/privilege/internet</privilege>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        public Task<bool> ConnectAsync()
        {
            int ret = Native.ConnectAsync(_handle);
            var task = new TaskCompletionSource<bool>();

            Task.Factory.StartNew(() =>
                {
                    if (ret == (int)ScreenMirroringError.None)
                    {
                        task.SetResult(true);
                    }

                    else if (ret != (int)ScreenMirroringError.None)
                    {
                        Log.Error(ScreenMirroringLog.LogTag, "Failed to start screen mirroring" + (ScreenMirroringError)ret);
                        InvalidOperationException e = new InvalidOperationException("Operation Failed");
                        task.TrySetException(e);
                    }
                });

            return task.Task;
        }

        /// <summary>
        /// Get AudioInfo.
        /// This must be called after connectasync().
        /// valid states: connected/playback/paused.
        /// If audio file changes during playback again
        /// then the current info should be retrieved from the audio information class.
        /// </summary>
        /// <value> AudioInfo object </value>
        public AudioInformation AudioInfo
        {
            get
            {
                return _audioInfo;
            }
        }

        /// <summary>
        /// Get VideoInfo.
        /// This must be called after connectasync().
        /// valid states: connected/playback/paused.
        /// If video file changes during playback again
        /// then the current info should be retrieved from the video information class.
        /// </summary>
        /// <value> VideoInfo object </value>
        public VideoInformation VideoInfo
        {
            get
            {
                return _videoInfo;
            }
        }

        /// <summary>
        /// Start receiving data from the ScreenMirroring source and display it(Mirror).
        /// This must be called after connectasync().
        ///  </summary>
        /// <remarks> It will not give the current state. Need to subscribe for event to get the current state </remarks>
        /// <returns>bool value<returns>
        /// <privilege>http://tizen.org/privilege/internet</privilege>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        public Task<bool> StartAsync()
        {
            int ret = Native.StartAsync(_handle);
            var task = new TaskCompletionSource<bool>();

            Task.Factory.StartNew(() =>
                {
                    if (ret == (int)ScreenMirroringError.None)
                    {
                        task.SetResult(true);
                    }

                    else if (ret != (int)ScreenMirroringError.None)
                    {
                        Log.Error(ScreenMirroringLog.LogTag, "Failed to start screen mirroring" + (ScreenMirroringError)ret);
                        InvalidOperationException e = new InvalidOperationException("Operation Failed");
                        task.TrySetException(e);
                    }
                });

            return task.Task;
        }

        /// <summary>
        /// Pauses receiving data from the ScreenMirroring source.
        /// This must be called after startasync().
        /// </summary>
        /// <remarks> It will not give the current state. Need to subscribe for event to get the current state </remarks>
        /// <returns>bool value</returns>
        /// <privilege>http://tizen.org/privilege/internet</privilege>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        public Task<bool> PauseAsync()
        {
            int ret = Native.PauseAsync(_handle);
            var task = new TaskCompletionSource<bool>();

            Task.Factory.StartNew(() =>
                {
                    if (ret == (int)ScreenMirroringError.None)
                    {
                        task.SetResult(true);
                    }

                    else if (ret != (int)ScreenMirroringError.None)
                    {
                        Log.Error(ScreenMirroringLog.LogTag, "Failed to start screen mirroring" + (ScreenMirroringError)ret);
                        InvalidOperationException e = new InvalidOperationException("Operation Failed");
                        task.TrySetException(e);
                    }
                });

            return task.Task;
        }

        /// <summary>
        /// Resumes receiving data from the ScreenMirroring source.
        /// This must be called after pauseasync().
        /// </summary>
        /// <remarks> It will not give the current state. Need to subscribe for event to get the current state </remarks>
        /// <returns>bool value</returns>
        /// <privilege>http://tizen.org/privilege/internet</privilege>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        public Task<bool> ResumeAsync()
        {
            int ret = Native.ResumeAsync(_handle);
            var task = new TaskCompletionSource<bool>();

            Task.Factory.StartNew(() =>
                {
                    if (ret == (int)ScreenMirroringError.None)
                    {
                        task.SetResult(true);
                    }

                    else if (ret != (int)ScreenMirroringError.None)
                    {
                        Log.Error(ScreenMirroringLog.LogTag, "Failed to start screen mirroring" + (ScreenMirroringError)ret);
                        InvalidOperationException e = new InvalidOperationException("Operation Failed");
                        task.TrySetException(e);
                    }
                });

            return task.Task;
        }

        /// <summary>
        /// Disconnect this instance.
        /// valid states: connected/playing/paused
        /// </summary>
        /// <privilege>http://tizen.org/privilege/internet</privilege>
        /// <exception cref="ArgumentException">Thrown when method fail due to no connection between devices</exception>
        public void Disconnect()
        {
            int ret = Native.Disconnect(_handle);
            if (ret != (int)ScreenMirroringError.None)
            {
                ScreenMirroringErrorFactory.ThrowException(ret, "Failed to disconnect sink for screen mirroring");
            }
        }

        /// <summary>
        /// Unprepare this instance.
        /// valid states: prepared/disconnected.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        public void Unprepare()
        {
            int ret = Native.Unprepare(_handle);
            if (ret != (int)ScreenMirroringError.None)
            {
                ScreenMirroringErrorFactory.ThrowException(ret, "Failed to reset the screen mirroring sink");
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="Tizen.Multimedia.ScreenMirroring"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Tizen.Multimedia.ScreenMirroring"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="Tizen.Multimedia.ScreenMirroring"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="Tizen.Multimedia.ScreenMirroring"/> so the garbage collector can reclaim the memory that the
        /// <see cref="Tizen.Multimedia.ScreenMirroring"/> was occupying.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the specified handle.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c> disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // To be used if there are any other disposable objects
                }

                if (_handle != IntPtr.Zero)
                {
                    Native.Destroy(_handle);
                    _handle = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Invoke the event for state or error.
        /// </summary>
        /// <param name="state"> state </param>
        /// <param name="error"> error </param>
        private void StateError(int state, int error)
        {
            ///if _stateChanged is subscribe, this will be invoke.
            StateChangedEventArgs eventArgsState = new StateChangedEventArgs(state, error);
            _stateChanged?.Invoke(this, eventArgsState);
        }

        /// <summary>
        /// Registers the state changed event.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        private void RegisterStateChangedEvent()
        {
            _stateChangedCallback = (IntPtr userData, int state, int error) =>
                {
                    StateError(state, error);
                };

            int ret = Native.SetStateChangedCb(_handle, _stateChangedCallback, IntPtr.Zero);
            if (ret != (int)ScreenMirroringError.None)
            {
                Log.Error(ScreenMirroringLog.LogTag, "Setting StateChanged callback failed" + (ScreenMirroringError)ret);
                ScreenMirroringErrorFactory.ThrowException(ret, "Setting StateChanged callback failed");
            }
        }

        /// <summary>
        /// Unregisters the state changed event.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when method fail due to an internal error</exception>
        private void UnregisterStateChangedEvent()
        {
            int ret = Native.UnsetStateChangedCb(_handle);
            if (ret != (int)ScreenMirroringError.None)
            {
                Log.Error(ScreenMirroringLog.LogTag, "Unsetting StateChnaged callback failed" + (ScreenMirroringError)ret);
                ScreenMirroringErrorFactory.ThrowException(ret, "Unsetting StateChanged callback failed");
            }
        }
    }
}
