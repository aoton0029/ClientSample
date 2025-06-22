デバイスの状態をモニタリングする機能を実装するために、まずは現在のプロジェクト内の関連コードを探してみましょう。
次に、PowerSupplyに関連するインターフェースを確認します。
さて、モニタリング機能を実装するためのフォームとクラスを作成しましょう。以下に提案するコードを示します：

1. まずは、デバイス状態を監視するための `DeviceMonitor` クラスを作成します：


```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using InspectionApp.DataProcessors;
using InspectionApp.PowerSupplys;
using InspectionApp.Multimeters;

namespace InspectionApp.Core
{
    /// <summary>
    /// デバイスステータス用イベント引数
    /// </summary>
    public class DeviceStatusEventArgs : EventArgs
    {
        public string DeviceType { get; }
        public string DeviceName { get; }
        public bool IsConnected { get; }
        public Dictionary<string, object> StatusValues { get; }

        public DeviceStatusEventArgs(string deviceType, string deviceName, bool isConnected, Dictionary<string, object> statusValues)
        {
            DeviceType = deviceType;
            DeviceName = deviceName;
            IsConnected = isConnected;
            StatusValues = statusValues;
        }
    }

    /// <summary>
    /// デバイスのモニタリングを管理するクラス
    /// </summary>
    public class DeviceMonitor : IDisposable
    {
        private readonly List<IPowerSupply> _powerSupplies = new();
        private readonly List<IMultimeter> _multimeters = new();
        private readonly Dictionary<string, PowerSupplyDataProcessor> _powerSupplyProcessors = new();
        private readonly Dictionary<string, MultimeterDataProcessor> _multimeterProcessors = new();
        private readonly System.Windows.Forms.Timer _updateTimer;
        private bool _disposed = false;
        private int _processorKeyCounter = 1;

        /// <summary>
        /// デバイスのステータスが更新された時に発生するイベント
        /// </summary>
        public event EventHandler<DeviceStatusEventArgs> DeviceStatusUpdated;

        /// <summary>
        /// 測定値が閾値を超えた場合に発生するイベント
        /// </summary>
        public event EventHandler<MeasurementThresholdEventArgs> ThresholdExceeded;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="updateInterval">更新間隔（ミリ秒）</param>
        public DeviceMonitor(int updateInterval = 1000)
        {
            _updateTimer = new System.Windows.Forms.Timer
            {
                Interval = updateInterval
            };
            _updateTimer.Tick += OnTimerTick;
        }

        /// <summary>
        /// モニタリングを開始します
        /// </summary>
        public void StartMonitoring()
        {
            _updateTimer.Start();
        }

        /// <summary>
        /// モニタリングを停止します
        /// </summary>
        public void StopMonitoring()
        {
            _updateTimer.Stop();
        }

        /// <summary>
        /// 電源装置を追加します
        /// </summary>
        /// <param name="powerSupply">電源装置</param>
        /// <param name="name">電源装置の識別名</param>
        public void AddPowerSupply(IPowerSupply powerSupply, string name)
        {
            if (powerSupply == null)
                throw new ArgumentNullException(nameof(powerSupply));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("名前が指定されていません", nameof(name));

            _powerSupplies.Add(powerSupply);
            powerSupply.ConnectionStatusChanged += OnPowerSupplyConnectionChanged;
            powerSupply.ErrorOccurred += OnPowerSupplyErrorOccurred;

            // データプロセッサを作成
            var processor = new PowerSupplyDataProcessor(_processorKeyCounter++);
            processor.ThresholdExceeded += OnThresholdExceeded;
            _powerSupplyProcessors[name] = processor;

            // 現在の状態を通知
            NotifyPowerSupplyStatus(powerSupply, name);
        }

        /// <summary>
        /// マルチメーターを追加します
        /// </summary>
        /// <param name="multimeter">マルチメーター</param>
        /// <param name="name">マルチメーターの識別名</param>
        public void AddMultimeter(IMultimeter multimeter, string name)
        {
            if (multimeter == null)
                throw new ArgumentNullException(nameof(multimeter));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("名前が指定されていません", nameof(name));

            _multimeters.Add(multimeter);
            multimeter.ConnectionStatusChanged += OnMultimeterConnectionChanged;
            multimeter.ErrorOccurred += OnMultimeterErrorOccurred;

            // データプロセッサを作成
            var processor = new MultimeterDataProcessor(_processorKeyCounter++);
            processor.ThresholdExceeded += OnThresholdExceeded;
            _multimeterProcessors[name] = processor;

            // 現在の状態を通知
            NotifyMultimeterStatus(multimeter, name);
        }

        /// <summary>
        /// 電源装置の閾値を設定
        /// </summary>
        /// <param name="deviceName">デバイス名</param>
        /// <param name="voltageUpperThreshold">電圧上限閾値</param>
        /// <param name="voltageLowerThreshold">電圧下限閾値</param>
        /// <param name="currentUpperThreshold">電流上限閾値</param>
        /// <param name="currentLowerThreshold">電流下限閾値</param>
        public void SetPowerSupplyThresholds(
            string deviceName,
            double? voltageUpperThreshold = null,
            double? voltageLowerThreshold = null,
            double? currentUpperThreshold = null,
            double? currentLowerThreshold = null)
        {
            if (_powerSupplyProcessors.TryGetValue(deviceName, out var processor))
            {
                processor.VoltageUpperThreshold = voltageUpperThreshold;
                processor.VoltageLowerThreshold = voltageLowerThreshold;
                processor.CurrentUpperThreshold = currentUpperThreshold;
                processor.CurrentLowerThreshold = currentLowerThreshold;
            }
        }

        /// <summary>
        /// マルチメーターの閾値を設定
        /// </summary>
        /// <param name="deviceName">デバイス名</param>
        /// <param name="upperThreshold">上限閾値</param>
        /// <param name="lowerThreshold">下限閾値</param>
        public void SetMultimeterThresholds(string deviceName, double? upperThreshold = null, double? lowerThreshold = null)
        {
            if (_multimeterProcessors.TryGetValue(deviceName, out var processor))
            {
                processor.UpperThreshold = upperThreshold;
                processor.LowerThreshold = lowerThreshold;
            }
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            await UpdateAllDeviceStatus();
        }

        private async Task UpdateAllDeviceStatus()
        {
            // 電源装置のステータス更新
            for (int i = 0; i < _powerSupplies.Count; i++)
            {
                var ps = _powerSupplies[i];
                var name = _powerSupplyProcessors.FirstOrDefault(p => p.Value.ProcessorKey == i + 1).Key;
                if (!string.IsNullOrEmpty(name))
                {
                    await NotifyPowerSupplyStatus(ps, name);
                }
            }

            // マルチメーターのステータス更新
            for (int i = 0; i < _multimeters.Count; i++)
            {
                var mm = _multimeters[i];
                var name = _multimeterProcessors.FirstOrDefault(p => p.Value.ProcessorKey == i + _powerSupplies.Count + 1).Key;
                if (!string.IsNullOrEmpty(name))
                {
                    await NotifyMultimeterStatus(mm, name);
                }
            }
        }

        private async Task NotifyPowerSupplyStatus(IPowerSupply powerSupply, string name)
        {
            if (powerSupply.IsConnected)
            {
                try
                {
                    var statusValues = new Dictionary<string, object>();

                    // デフォルトチャンネル (null) の値を取得
                    double voltage = await powerSupply.MeasureVoltageAsync();
                    double current = await powerSupply.MeasureCurrentAsync();

                    statusValues["Voltage"] = voltage;
                    statusValues["Current"] = current;
                    statusValues["Power"] = voltage * current; // 電力計算

                    // プロセッサにデータを送信
                    if (_powerSupplyProcessors.TryGetValue(name, out var processor))
                    {
                        await processor.ProcessDataAsync(new MeasurementDataWithKey
                        {
                            MeasurementKey = processor.ProcessorKey,
                            MeasurementType = MeasurementType.Voltage,
                            Value = voltage,
                            Source = $"{name}:Voltage",
                            Timestamp = DateTime.Now
                        });

                        await processor.ProcessDataAsync(new MeasurementDataWithKey
                        {
                            MeasurementKey = processor.ProcessorKey,
                            MeasurementType = MeasurementType.Current,
                            Value = current,
                            Source = $"{name}:Current",
                            Timestamp = DateTime.Now
                        });
                    }

                    // イベント発火
                    DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                        "PowerSupply",
                        name,
                        true,
                        statusValues
                    ));
                }
                catch (Exception ex)
                {
                    var statusValues = new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message
                    };

                    DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                        "PowerSupply",
                        name,
                        true,
                        statusValues
                    ));
                }
            }
            else
            {
                DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                    "PowerSupply",
                    name,
                    false,
                    new Dictionary<string, object>()
                ));
            }
        }

        private async Task NotifyMultimeterStatus(IMultimeter multimeter, string name)
        {
            if (multimeter.IsConnected)
            {
                try
                {
                    var statusValues = new Dictionary<string, object>();

                    // 現在の測定機能を取得
                    string function = await multimeter.GetFunctionAsync();
                    statusValues["Function"] = function;

                    // 機能に応じて測定
                    double value = 0;
                    string source = $"{name}:{function}";

                    if (function.Contains("VOLT", StringComparison.OrdinalIgnoreCase))
                    {
                        value = await multimeter.MeasureVoltageAsync();
                        statusValues["Voltage"] = value;
                    }
                    else if (function.Contains("CURR", StringComparison.OrdinalIgnoreCase))
                    {
                        value = await multimeter.MeasureCurrentAsync();
                        statusValues["Current"] = value;
                    }
                    else
                    {
                        // その他の機能は一般的な測定値として扱う
                        value = await multimeter.MeasureVoltageAsync(); // 汎用的な測定関数がない場合
                        statusValues["Value"] = value;
                    }

                    // プロセッサにデータを送信
                    if (_multimeterProcessors.TryGetValue(name, out var processor))
                    {
                        await processor.ProcessDataAsync(new MeasurementDataWithKey
                        {
                            MeasurementKey = processor.ProcessorKey,
                            MeasurementType = MeasurementType.MultimeterValue,
                            Value = value,
                            Source = source,
                            Timestamp = DateTime.Now
                        });
                    }

                    // イベント発火
                    DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                        "Multimeter",
                        name,
                        true,
                        statusValues
                    ));
                }
                catch (Exception ex)
                {
                    var statusValues = new Dictionary<string, object>
                    {
                        ["Error"] = ex.Message
                    };

                    DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                        "Multimeter",
                        name,
                        true,
                        statusValues
                    ));
                }
            }
            else
            {
                DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                    "Multimeter",
                    name,
                    false,
                    new Dictionary<string, object>()
                ));
            }
        }

        private void OnPowerSupplyConnectionChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            string name = _powerSupplies.IndexOf((IPowerSupply)sender).ToString();
            foreach (var pair in _powerSupplyProcessors)
            {
                if (pair.Value.ProcessorKey == _powerSupplies.IndexOf((IPowerSupply)sender) + 1)
                {
                    name = pair.Key;
                    break;
                }
            }

            DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                "PowerSupply",
                name,
                e.IsConnected,
                new Dictionary<string, object> { ["ConnectionMessage"] = e.Message ?? string.Empty }
            ));
        }

        private void OnMultimeterConnectionChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            string name = _multimeters.IndexOf((IMultimeter)sender).ToString();
            foreach (var pair in _multimeterProcessors)
            {
                if (pair.Value.ProcessorKey == _multimeters.IndexOf((IMultimeter)sender) + _powerSupplies.Count + 1)
                {
                    name = pair.Key;
                    break;
                }
            }

            DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                "Multimeter",
                name,
                e.IsConnected,
                new Dictionary<string, object> { ["ConnectionMessage"] = e.Message ?? string.Empty }
            ));
        }

        private void OnPowerSupplyErrorOccurred(object sender, PowerSupplyErrorEventArgs e)
        {
            string name = _powerSupplies.IndexOf((IPowerSupply)sender).ToString();
            foreach (var pair in _powerSupplyProcessors)
            {
                if (pair.Value.ProcessorKey == _powerSupplies.IndexOf((IPowerSupply)sender) + 1)
                {
                    name = pair.Key;
                    break;
                }
            }

            DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                "PowerSupply",
                name,
                true,
                new Dictionary<string, object> { ["Error"] = e.Message }
            ));
        }

        private void OnMultimeterErrorOccurred(object sender, MultimeterErrorEventArgs e)
        {
            string name = _multimeters.IndexOf((IMultimeter)sender).ToString();
            foreach (var pair in _multimeterProcessors)
            {
                if (pair.Value.ProcessorKey == _multimeters.IndexOf((IMultimeter)sender) + _powerSupplies.Count + 1)
                {
                    name = pair.Key;
                    break;
                }
            }

            DeviceStatusUpdated?.Invoke(this, new DeviceStatusEventArgs(
                "Multimeter",
                name,
                true,
                new Dictionary<string, object> { ["Error"] = e.Message }
            ));
        }

        private void OnThresholdExceeded(object sender, MeasurementThresholdEventArgs e)
        {
            ThresholdExceeded?.Invoke(this, e);
        }

        /// <summary>
        /// リソースを解放します
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _updateTimer.Stop();
                    _updateTimer.Dispose();
                    
                    foreach (var processor in _powerSupplyProcessors.Values)
                    {
                        processor.ThresholdExceeded -= OnThresholdExceeded;
                    }
                    
                    foreach (var processor in _multimeterProcessors.Values)
                    {
                        processor.ThresholdExceeded -= OnThresholdExceeded;
                    }
                }
                _disposed = true;
            }
        }
    }
}

```

2. 次に、モニタリング情報を表示するフォームを作成します：


```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InspectionApp.Core;
using InspectionApp.DataProcessors;

namespace InspectionApp
{
    public partial class DeviceMonitorForm : Form
    {
        private readonly DeviceMonitor _deviceMonitor;
        private readonly Dictionary<string, ListViewItem> _deviceItems = new();
        private readonly Dictionary<string, Label> _statusLabels = new();

        public DeviceMonitorForm(DeviceMonitor deviceMonitor)
        {
            InitializeComponent();
            _deviceMonitor = deviceMonitor ?? throw new ArgumentNullException(nameof(deviceMonitor));

            // イベントハンドラの登録
            _deviceMonitor.DeviceStatusUpdated += OnDeviceStatusUpdated;
            _deviceMonitor.ThresholdExceeded += OnThresholdExceeded;

            // フォームが閉じられるときの処理
            FormClosing += (sender, e) => _deviceMonitor.StopMonitoring();
        }

        private void InitializeComponent()
        {
            this.Text = "デバイスモニター";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // リストビューの設定
            ListView deviceListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Top,
                Height = 200
            };
            deviceListView.Columns.Add("デバイスタイプ", 120);
            deviceListView.Columns.Add("デバイス名", 120);
            deviceListView.Columns.Add("接続状態", 80);
            deviceListView.Columns.Add("ステータス", 300);
            this.Controls.Add(deviceListView);

            // ステータスパネルの設定
            Panel statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(statusPanel);

            // 詳細ステータスのタイトル
            Label statusTitle = new Label
            {
                Text = "詳細ステータス",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)
            };
            statusPanel.Controls.Add(statusTitle);

            // ログエリア
            TextBox logTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Bottom,
                Height = 150
            };
            this.Controls.Add(logTextBox);

            // デバイスリストビューの選択変更イベント
            deviceListView.SelectedIndexChanged += (sender, e) =>
            {
                if (deviceListView.SelectedItems.Count > 0)
                {
                    var item = deviceListView.SelectedItems[0];
                    string deviceType = item.SubItems[0].Text;
                    string deviceName = item.SubItems[1].Text;
                    ShowDeviceDetails(deviceType, deviceName);
                }
            };
        }

        /// <summary>
        /// デバイスの詳細情報を表示
        /// </summary>
        private void ShowDeviceDetails(string deviceType, string deviceName)
        {
            // 既存のコントロールをクリア
            Panel statusPanel = (Panel)Controls.Find("statusPanel", true)[0];
            foreach (Control control in statusPanel.Controls)
            {
                if (control is not Label titleLabel || titleLabel.Text != "詳細ステータス")
                {
                    statusPanel.Controls.Remove(control);
                }
            }

            // 詳細ラベルを追加
            Label detailsLabel = new Label
            {
                Text = $"{deviceType}: {deviceName}",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold)
            };
            statusPanel.Controls.Add(detailsLabel);
            detailsLabel.BringToFront();

            // ステータス表示用のラベルを用意
            if (_deviceItems.TryGetValue($"{deviceType}:{deviceName}", out var item))
            {
                // 接続状態
                bool isConnected = item.SubItems[2].Text == "接続済み";
                
                Label connectionStatus = new Label
                {
                    Text = $"接続状態: {(isConnected ? "接続済み" : "未接続")}",
                    ForeColor = isConnected ? Color.Green : Color.Red,
                    Dock = DockStyle.Top,
                    Height = 25
                };
                statusPanel.Controls.Add(connectionStatus);
                connectionStatus.BringToFront();

                // ステータス情報の表示
                if (isConnected)
                {
                    // 既存のステータス情報を取得
                    string statusText = item.SubItems[3].Text;
                    string[] statusParts = statusText.Split(',');
                    
                    foreach (string part in statusParts)
                    {
                        string trimmedPart = part.Trim();
                        if (!string.IsNullOrEmpty(trimmedPart))
                        {
                            Label statusLabel = new Label
                            {
                                Text = trimmedPart,
                                Dock = DockStyle.Top,
                                Height = 25
                            };
                            statusPanel.Controls.Add(statusLabel);
                            statusLabel.BringToFront();

                            // 警告値の色付け
                            if (trimmedPart.Contains("警告") || trimmedPart.Contains("Error"))
                            {
                                statusLabel.ForeColor = Color.Red;
                                statusLabel.Font = new Font(statusLabel.Font, FontStyle.Bold);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// デバイスのステータスが更新された時の処理
        /// </summary>
        private void OnDeviceStatusUpdated(object sender, DeviceStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, DeviceStatusEventArgs>(OnDeviceStatusUpdated), sender, e);
                return;
            }

            string deviceKey = $"{e.DeviceType}:{e.DeviceName}";
            string statusText = string.Empty;

            // ステータス情報の文字列化
            foreach (var pair in e.StatusValues)
            {
                // エラーの場合は特別処理
                if (pair.Key == "Error")
                {
                    statusText += $"Error: {pair.Value}, ";
                    continue;
                }

                // 値の種類によって表示を変える
                if (pair.Value is double doubleValue)
                {
                    string unit = string.Empty;
                    if (pair.Key == "Voltage") unit = "V";
                    else if (pair.Key == "Current") unit = "A";
                    else if (pair.Key == "Power") unit = "W";

                    statusText += $"{pair.Key}: {doubleValue:F3}{unit}, ";
                }
                else
                {
                    statusText += $"{pair.Key}: {pair.Value}, ";
                }
            }

            // トレーリングカンマを削除
            if (statusText.EndsWith(", "))
            {
                statusText = statusText.Substring(0, statusText.Length - 2);
            }

            // リストビューのアイテム更新か追加
            if (_deviceItems.TryGetValue(deviceKey, out var item))
            {
                // 既存のアイテムを更新
                item.SubItems[2].Text = e.IsConnected ? "接続済み" : "未接続";
                item.SubItems[3].Text = statusText;

                // 接続状態によって文字色を変更
                item.ForeColor = e.IsConnected ? SystemColors.WindowText : Color.Gray;

                // エラーがある場合は赤色表示
                if (e.StatusValues.ContainsKey("Error"))
                {
                    item.ForeColor = Color.Red;
                }
            }
            else
            {
                // 新しいアイテムを追加
                ListView deviceListView = (ListView)Controls[0];
                ListViewItem newItem = new ListViewItem(e.DeviceType);
                newItem.SubItems.Add(e.DeviceName);
                newItem.SubItems.Add(e.IsConnected ? "接続済み" : "未接続");
                newItem.SubItems.Add(statusText);

                // 接続状態によって文字色を変更
                newItem.ForeColor = e.IsConnected ? SystemColors.WindowText : Color.Gray;

                // エラーがある場合は赤色表示
                if (e.StatusValues.ContainsKey("Error"))
                {
                    newItem.ForeColor = Color.Red;
                }

                deviceListView.Items.Add(newItem);
                _deviceItems[deviceKey] = newItem;
            }

            // 現在選択されているアイテムと同じデバイスなら詳細表示も更新
            ListView listView = (ListView)Controls[0];
            if (listView.SelectedItems.Count > 0)
            {
                var selectedItem = listView.SelectedItems[0];
                if (selectedItem.SubItems[0].Text == e.DeviceType && selectedItem.SubItems[1].Text == e.DeviceName)
                {
                    ShowDeviceDetails(e.DeviceType, e.DeviceName);
                }
            }
        }

        /// <summary>
        /// 閾値を超えた時の処理
        /// </summary>
        private void OnThresholdExceeded(object sender, MeasurementThresholdEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, MeasurementThresholdEventArgs>(OnThresholdExceeded), sender, e);
                return;
            }

            // ログに記録
            TextBox logTextBox = (TextBox)Controls.Find("logTextBox", true)[0];
            string logMessage = $"[{DateTime.Now:HH:mm:ss}] 警告: {e.Message} - {e.Data.Source}, 値: {e.Data.Value:F3}";
            logTextBox.AppendText(logMessage + Environment.NewLine);

            // 自動スクロール
            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.ScrollToCaret();
        }
    }
}

```

3. メインフォームで使用する例：


```csharp
using System;
using System.Windows.Forms;
using InspectionApp.Core;
using InspectionApp.PowerSupplys;
using InspectionApp.Multimeters;

namespace InspectionApp
{
    public partial class MainForm : Form
    {
        private readonly DeviceMonitor _deviceMonitor;

        public MainForm()
        {
            InitializeComponent();
            
            // デバイスモニターの作成（更新間隔1秒）
            _deviceMonitor = new DeviceMonitor(1000);
            
            // 必要なデバイス初期化コード...
            
            // デバイスモニターのボタン押下時の処理
            btnShowMonitor.Click += (sender, e) =>
            {
                var monitorForm = new DeviceMonitorForm(_deviceMonitor);
                monitorForm.Show();
                _deviceMonitor.StartMonitoring();
            };
        }

        // デバイスの初期化例
        private async void InitializeDevices()
        {
            // 電源装置の初期化例
            IPowerSupply powerSupply1 = new PowerSupplyCtrl2Adapter("COM1"); // 適切なコンストラクタ引数を使用
            await powerSupply1.ConnectAsync();
            if (powerSupply1.IsConnected)
            {
                await powerSupply1.InitializeAsync();
                _deviceMonitor.AddPowerSupply(powerSupply1, "電源装置1");
                
                // 閾値設定例
                _deviceMonitor.SetPowerSupplyThresholds(
                    "電源装置1", 
                    voltageUpperThreshold: 12.5, 
                    voltageLowerThreshold: 11.5,
                    currentUpperThreshold: 2.0);
            }

            // マルチメーターの初期化例
            IMultimeter multimeter1 = new DMM6500CtrlAdapter("COM2"); // 適切なコンストラクタ引数を使用
            await multimeter1.ConnectAsync();
            if (multimeter1.IsConnected)
            {
                await multimeter1.InitializeAsync();
                await multimeter1.SetFunctionAsync("VOLT:DC");
                _deviceMonitor.AddMultimeter(multimeter1, "マルチメーター1");
                
                // 閾値設定例
                _deviceMonitor.SetMultimeterThresholds("マルチメーター1", upperThreshold: 5.0, lowerThreshold: 0.0);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeDevices();
        }
    }
}

```

## 実装の説明

1. **DeviceMonitor クラス**:
   - 電源装置とマルチメーターの状態をモニタリングする中心クラス
   - 定期的にデバイスから測定値を取得し、イベントで通知
   - データプロセッサを使用して閾値監視と処理を行う
   - 閾値を超えた場合は特別なイベントを発火

2. **DeviceMonitorForm クラス**:
   - モニタリング情報を視覚的に表示するフォーム
   - リストビューでデバイス一覧と基本情報を表示
   - 詳細パネルで選択したデバイスの詳細情報を表示
   - ログ機能で閾値超過などの警告を記録

3. **使用方法**:
   - メインフォームでDeviceMonitorを初期化
   - 必要なデバイスを接続・初期化してからDeviceMonitorに追加
   - 必要に応じて閾値を設定
   - モニタリングボタンでフォームを表示し監視を開始

この実装により、電源の測定値やステータス、マルチメーターの測定値やステータスなど、様々なデバイスの状態をリアルタイムで監視できるようになります。また、閾値を設定することで異常値の検知も可能です。