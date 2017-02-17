using System;
using System.Windows;
using HwndExtensions.Host;

namespace NutzCode.MPVPlayer.WPF.Wrapper
{
    public class NativePresenter : HwndHostPresenter
    {
        public NativePresenter()
        {
            NativePlayer player = new NativePlayer();
            HwndHost = player;
            Application.Current.Dispatcher.ShutdownStarted += OnShutdownStarted;

        }




        private void OnShutdownStarted(object sender, EventArgs e)
        {
            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var host = HwndHost;
                host?.Dispose();
                HwndHost = null;
            }
        }

    }
}
