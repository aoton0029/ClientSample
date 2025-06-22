�f�o�C�X�̏�Ԃ����j�^�����O����@�\���������邽�߂ɁA�܂��͌��݂̃v���W�F�N�g���̊֘A�R�[�h��T���Ă݂܂��傤�B
���ɁAPowerSupply�Ɋ֘A����C���^�[�t�F�[�X���m�F���܂��B
���āA���j�^�����O�@�\���������邽�߂̃t�H�[���ƃN���X���쐬���܂��傤�B�ȉ��ɒ�Ă���R�[�h�������܂��F

1. �܂��́A�f�o�C�X��Ԃ��Ď����邽�߂� `DeviceMonitor` �N���X���쐬���܂��F


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
    /// �f�o�C�X�X�e�[�^�X�p�C�x���g����
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
    /// �f�o�C�X�̃��j�^�����O���Ǘ�����N���X
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
        /// �f�o�C�X�̃X�e�[�^�X���X�V���ꂽ���ɔ�������C�x���g
        /// </summary>
        public event EventHandler<DeviceStatusEventArgs> DeviceStatusUpdated;

        /// <summary>
        /// ����l��臒l�𒴂����ꍇ�ɔ�������C�x���g
        /// </summary>
        public event EventHandler<MeasurementThresholdEventArgs> ThresholdExceeded;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="updateInterval">�X�V�Ԋu�i�~���b�j</param>
        public DeviceMonitor(int updateInterval = 1000)
        {
            _updateTimer = new System.Windows.Forms.Timer
            {
                Interval = updateInterval
            };
            _updateTimer.Tick += OnTimerTick;
        }

        /// <summary>
        /// ���j�^�����O���J�n���܂�
        /// </summary>
        public void StartMonitoring()
        {
            _updateTimer.Start();
        }

        /// <summary>
        /// ���j�^�����O���~���܂�
        /// </summary>
        public void StopMonitoring()
        {
            _updateTimer.Stop();
        }

        /// <summary>
        /// �d�����u��ǉ����܂�
        /// </summary>
        /// <param name="powerSupply">�d�����u</param>
        /// <param name="name">�d�����u�̎��ʖ�</param>
        public void AddPowerSupply(IPowerSupply powerSupply, string name)
        {
            if (powerSupply == null)
                throw new ArgumentNullException(nameof(powerSupply));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("���O���w�肳��Ă��܂���", nameof(name));

            _powerSupplies.Add(powerSupply);
            powerSupply.ConnectionStatusChanged += OnPowerSupplyConnectionChanged;
            powerSupply.ErrorOccurred += OnPowerSupplyErrorOccurred;

            // �f�[�^�v���Z�b�T���쐬
            var processor = new PowerSupplyDataProcessor(_processorKeyCounter++);
            processor.ThresholdExceeded += OnThresholdExceeded;
            _powerSupplyProcessors[name] = processor;

            // ���݂̏�Ԃ�ʒm
            NotifyPowerSupplyStatus(powerSupply, name);
        }

        /// <summary>
        /// �}���`���[�^�[��ǉ����܂�
        /// </summary>
        /// <param name="multimeter">�}���`���[�^�[</param>
        /// <param name="name">�}���`���[�^�[�̎��ʖ�</param>
        public void AddMultimeter(IMultimeter multimeter, string name)
        {
            if (multimeter == null)
                throw new ArgumentNullException(nameof(multimeter));

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("���O���w�肳��Ă��܂���", nameof(name));

            _multimeters.Add(multimeter);
            multimeter.ConnectionStatusChanged += OnMultimeterConnectionChanged;
            multimeter.ErrorOccurred += OnMultimeterErrorOccurred;

            // �f�[�^�v���Z�b�T���쐬
            var processor = new MultimeterDataProcessor(_processorKeyCounter++);
            processor.ThresholdExceeded += OnThresholdExceeded;
            _multimeterProcessors[name] = processor;

            // ���݂̏�Ԃ�ʒm
            NotifyMultimeterStatus(multimeter, name);
        }

        /// <summary>
        /// �d�����u��臒l��ݒ�
        /// </summary>
        /// <param name="deviceName">�f�o�C�X��</param>
        /// <param name="voltageUpperThreshold">�d�����臒l</param>
        /// <param name="voltageLowerThreshold">�d������臒l</param>
        /// <param name="currentUpperThreshold">�d�����臒l</param>
        /// <param name="currentLowerThreshold">�d������臒l</param>
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
        /// �}���`���[�^�[��臒l��ݒ�
        /// </summary>
        /// <param name="deviceName">�f�o�C�X��</param>
        /// <param name="upperThreshold">���臒l</param>
        /// <param name="lowerThreshold">����臒l</param>
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
            // �d�����u�̃X�e�[�^�X�X�V
            for (int i = 0; i < _powerSupplies.Count; i++)
            {
                var ps = _powerSupplies[i];
                var name = _powerSupplyProcessors.FirstOrDefault(p => p.Value.ProcessorKey == i + 1).Key;
                if (!string.IsNullOrEmpty(name))
                {
                    await NotifyPowerSupplyStatus(ps, name);
                }
            }

            // �}���`���[�^�[�̃X�e�[�^�X�X�V
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

                    // �f�t�H���g�`�����l�� (null) �̒l���擾
                    double voltage = await powerSupply.MeasureVoltageAsync();
                    double current = await powerSupply.MeasureCurrentAsync();

                    statusValues["Voltage"] = voltage;
                    statusValues["Current"] = current;
                    statusValues["Power"] = voltage * current; // �d�͌v�Z

                    // �v���Z�b�T�Ƀf�[�^�𑗐M
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

                    // �C�x���g����
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

                    // ���݂̑���@�\���擾
                    string function = await multimeter.GetFunctionAsync();
                    statusValues["Function"] = function;

                    // �@�\�ɉ����đ���
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
                        // ���̑��̋@�\�͈�ʓI�ȑ���l�Ƃ��Ĉ���
                        value = await multimeter.MeasureVoltageAsync(); // �ėp�I�ȑ���֐����Ȃ��ꍇ
                        statusValues["Value"] = value;
                    }

                    // �v���Z�b�T�Ƀf�[�^�𑗐M
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

                    // �C�x���g����
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
        /// ���\�[�X��������܂�
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

2. ���ɁA���j�^�����O����\������t�H�[�����쐬���܂��F


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

            // �C�x���g�n���h���̓o�^
            _deviceMonitor.DeviceStatusUpdated += OnDeviceStatusUpdated;
            _deviceMonitor.ThresholdExceeded += OnThresholdExceeded;

            // �t�H�[����������Ƃ��̏���
            FormClosing += (sender, e) => _deviceMonitor.StopMonitoring();
        }

        private void InitializeComponent()
        {
            this.Text = "�f�o�C�X���j�^�[";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // ���X�g�r���[�̐ݒ�
            ListView deviceListView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Dock = DockStyle.Top,
                Height = 200
            };
            deviceListView.Columns.Add("�f�o�C�X�^�C�v", 120);
            deviceListView.Columns.Add("�f�o�C�X��", 120);
            deviceListView.Columns.Add("�ڑ����", 80);
            deviceListView.Columns.Add("�X�e�[�^�X", 300);
            this.Controls.Add(deviceListView);

            // �X�e�[�^�X�p�l���̐ݒ�
            Panel statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(statusPanel);

            // �ڍ׃X�e�[�^�X�̃^�C�g��
            Label statusTitle = new Label
            {
                Text = "�ڍ׃X�e�[�^�X",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold)
            };
            statusPanel.Controls.Add(statusTitle);

            // ���O�G���A
            TextBox logTextBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Bottom,
                Height = 150
            };
            this.Controls.Add(logTextBox);

            // �f�o�C�X���X�g�r���[�̑I��ύX�C�x���g
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
        /// �f�o�C�X�̏ڍ׏���\��
        /// </summary>
        private void ShowDeviceDetails(string deviceType, string deviceName)
        {
            // �����̃R���g���[�����N���A
            Panel statusPanel = (Panel)Controls.Find("statusPanel", true)[0];
            foreach (Control control in statusPanel.Controls)
            {
                if (control is not Label titleLabel || titleLabel.Text != "�ڍ׃X�e�[�^�X")
                {
                    statusPanel.Controls.Remove(control);
                }
            }

            // �ڍ׃��x����ǉ�
            Label detailsLabel = new Label
            {
                Text = $"{deviceType}: {deviceName}",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold)
            };
            statusPanel.Controls.Add(detailsLabel);
            detailsLabel.BringToFront();

            // �X�e�[�^�X�\���p�̃��x����p��
            if (_deviceItems.TryGetValue($"{deviceType}:{deviceName}", out var item))
            {
                // �ڑ����
                bool isConnected = item.SubItems[2].Text == "�ڑ��ς�";
                
                Label connectionStatus = new Label
                {
                    Text = $"�ڑ����: {(isConnected ? "�ڑ��ς�" : "���ڑ�")}",
                    ForeColor = isConnected ? Color.Green : Color.Red,
                    Dock = DockStyle.Top,
                    Height = 25
                };
                statusPanel.Controls.Add(connectionStatus);
                connectionStatus.BringToFront();

                // �X�e�[�^�X���̕\��
                if (isConnected)
                {
                    // �����̃X�e�[�^�X�����擾
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

                            // �x���l�̐F�t��
                            if (trimmedPart.Contains("�x��") || trimmedPart.Contains("Error"))
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
        /// �f�o�C�X�̃X�e�[�^�X���X�V���ꂽ���̏���
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

            // �X�e�[�^�X���̕�����
            foreach (var pair in e.StatusValues)
            {
                // �G���[�̏ꍇ�͓��ʏ���
                if (pair.Key == "Error")
                {
                    statusText += $"Error: {pair.Value}, ";
                    continue;
                }

                // �l�̎�ނɂ���ĕ\����ς���
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

            // �g���[�����O�J���}���폜
            if (statusText.EndsWith(", "))
            {
                statusText = statusText.Substring(0, statusText.Length - 2);
            }

            // ���X�g�r���[�̃A�C�e���X�V���ǉ�
            if (_deviceItems.TryGetValue(deviceKey, out var item))
            {
                // �����̃A�C�e�����X�V
                item.SubItems[2].Text = e.IsConnected ? "�ڑ��ς�" : "���ڑ�";
                item.SubItems[3].Text = statusText;

                // �ڑ���Ԃɂ���ĕ����F��ύX
                item.ForeColor = e.IsConnected ? SystemColors.WindowText : Color.Gray;

                // �G���[������ꍇ�͐ԐF�\��
                if (e.StatusValues.ContainsKey("Error"))
                {
                    item.ForeColor = Color.Red;
                }
            }
            else
            {
                // �V�����A�C�e����ǉ�
                ListView deviceListView = (ListView)Controls[0];
                ListViewItem newItem = new ListViewItem(e.DeviceType);
                newItem.SubItems.Add(e.DeviceName);
                newItem.SubItems.Add(e.IsConnected ? "�ڑ��ς�" : "���ڑ�");
                newItem.SubItems.Add(statusText);

                // �ڑ���Ԃɂ���ĕ����F��ύX
                newItem.ForeColor = e.IsConnected ? SystemColors.WindowText : Color.Gray;

                // �G���[������ꍇ�͐ԐF�\��
                if (e.StatusValues.ContainsKey("Error"))
                {
                    newItem.ForeColor = Color.Red;
                }

                deviceListView.Items.Add(newItem);
                _deviceItems[deviceKey] = newItem;
            }

            // ���ݑI������Ă���A�C�e���Ɠ����f�o�C�X�Ȃ�ڍו\�����X�V
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
        /// 臒l�𒴂������̏���
        /// </summary>
        private void OnThresholdExceeded(object sender, MeasurementThresholdEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<object, MeasurementThresholdEventArgs>(OnThresholdExceeded), sender, e);
                return;
            }

            // ���O�ɋL�^
            TextBox logTextBox = (TextBox)Controls.Find("logTextBox", true)[0];
            string logMessage = $"[{DateTime.Now:HH:mm:ss}] �x��: {e.Message} - {e.Data.Source}, �l: {e.Data.Value:F3}";
            logTextBox.AppendText(logMessage + Environment.NewLine);

            // �����X�N���[��
            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.ScrollToCaret();
        }
    }
}

```

3. ���C���t�H�[���Ŏg�p�����F


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
            
            // �f�o�C�X���j�^�[�̍쐬�i�X�V�Ԋu1�b�j
            _deviceMonitor = new DeviceMonitor(1000);
            
            // �K�v�ȃf�o�C�X�������R�[�h...
            
            // �f�o�C�X���j�^�[�̃{�^���������̏���
            btnShowMonitor.Click += (sender, e) =>
            {
                var monitorForm = new DeviceMonitorForm(_deviceMonitor);
                monitorForm.Show();
                _deviceMonitor.StartMonitoring();
            };
        }

        // �f�o�C�X�̏�������
        private async void InitializeDevices()
        {
            // �d�����u�̏�������
            IPowerSupply powerSupply1 = new PowerSupplyCtrl2Adapter("COM1"); // �K�؂ȃR���X�g���N�^�������g�p
            await powerSupply1.ConnectAsync();
            if (powerSupply1.IsConnected)
            {
                await powerSupply1.InitializeAsync();
                _deviceMonitor.AddPowerSupply(powerSupply1, "�d�����u1");
                
                // 臒l�ݒ��
                _deviceMonitor.SetPowerSupplyThresholds(
                    "�d�����u1", 
                    voltageUpperThreshold: 12.5, 
                    voltageLowerThreshold: 11.5,
                    currentUpperThreshold: 2.0);
            }

            // �}���`���[�^�[�̏�������
            IMultimeter multimeter1 = new DMM6500CtrlAdapter("COM2"); // �K�؂ȃR���X�g���N�^�������g�p
            await multimeter1.ConnectAsync();
            if (multimeter1.IsConnected)
            {
                await multimeter1.InitializeAsync();
                await multimeter1.SetFunctionAsync("VOLT:DC");
                _deviceMonitor.AddMultimeter(multimeter1, "�}���`���[�^�[1");
                
                // 臒l�ݒ��
                _deviceMonitor.SetMultimeterThresholds("�}���`���[�^�[1", upperThreshold: 5.0, lowerThreshold: 0.0);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeDevices();
        }
    }
}

```

## �����̐���

1. **DeviceMonitor �N���X**:
   - �d�����u�ƃ}���`���[�^�[�̏�Ԃ����j�^�����O���钆�S�N���X
   - ����I�Ƀf�o�C�X���瑪��l���擾���A�C�x���g�Œʒm
   - �f�[�^�v���Z�b�T���g�p����臒l�Ď��Ə������s��
   - 臒l�𒴂����ꍇ�͓��ʂȃC�x���g�𔭉�

2. **DeviceMonitorForm �N���X**:
   - ���j�^�����O�������o�I�ɕ\������t�H�[��
   - ���X�g�r���[�Ńf�o�C�X�ꗗ�Ɗ�{����\��
   - �ڍ׃p�l���őI�������f�o�C�X�̏ڍ׏���\��
   - ���O�@�\��臒l���߂Ȃǂ̌x�����L�^

3. **�g�p���@**:
   - ���C���t�H�[����DeviceMonitor��������
   - �K�v�ȃf�o�C�X��ڑ��E���������Ă���DeviceMonitor�ɒǉ�
   - �K�v�ɉ�����臒l��ݒ�
   - ���j�^�����O�{�^���Ńt�H�[����\�����Ď����J�n

���̎����ɂ��A�d���̑���l��X�e�[�^�X�A�}���`���[�^�[�̑���l��X�e�[�^�X�ȂǁA�l�X�ȃf�o�C�X�̏�Ԃ����A���^�C���ŊĎ��ł���悤�ɂȂ�܂��B�܂��A臒l��ݒ肷�邱�Ƃňُ�l�̌��m���\�ł��B