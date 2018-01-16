using AutoUpdateClient.Common;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AutoUpdateClient
{
    public class MainWindowViewModel : BindableBase
    {
        private Timer _autoUpdateTimer;
        private Timer _clientMonitorTimer;
        private int _logLineCount = 0;
        private string _logText;
        private WindowState _myWindowState;
        public MainWindowViewModel()
        {
            Start();
        }
        public WindowState MyWindowState
        {
            get
            {
                return _myWindowState;
            }
            set
            {
                _myWindowState = value;
                RaisePropertyChanged();
            }
        }
        public string LogText
        {
            get
            {
                return _logText;
            }
            set
            {
                _logText = value;
                RaisePropertyChanged();
            }
        }

        private void Start()
        {
            LimitOneAutoUpdaterClient();
            SetStartOnPowerOn();

            SelfUpdater.UpdateSelfDelegate = UpdateContents;
            Modifier.ModifierDelegate = UpdateContents;
            Monitor.MonitorDelegate = UpdateContents;
            Monitor.MinimizedDelegate = () => {  MyWindowState = WindowState.Minimized;};
            Monitor.MaxmizedDelegate = () => { MyWindowState = WindowState.Normal; };

            LogText += "【启动成功】";
            _autoUpdateTimer = new Timer(Modifier.CheckUpdate, null, 0, int.Parse(UserConfigInstance.Instance.Config.CheckUpdateTime));
            _clientMonitorTimer = new Timer(Monitor.CheckClientAlive, null, 0, int.Parse(UserConfigInstance.Instance.Config.CheckClientAliveTime));
        }

        private static void SetStartOnPowerOn()
        {
            if (UserConfigInstance.Instance.Config.StartOnPowerOn)
            {
                var name = "远图自动更新客户端";
                SystemHelper.CreateShortcut(name);
                SystemHelper.StartOnPowerOn(name);
            }
        }

        private void LimitOneAutoUpdaterClient()
        {
            var arr = Process.GetProcessesByName("AutoUpdateClient");
            if (arr.Length < 2) return;
            LogText = "【关闭原先AutoUpdateClient】\n";
            var myProcessId = Process.GetCurrentProcess().Id;
            foreach (var p in arr)
            {
                if (p.Id != myProcessId)
                {
                    p.Kill();
                }
            }
        }

        public void UpdateContents(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_logLineCount >= 100)
                {
                    LogText = string.Empty;
                    _logLineCount = 0;
                }
                LogText = LogText.Insert(0, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {msg}" + "\n");
                _logLineCount++;
            });
        }
    }
}
