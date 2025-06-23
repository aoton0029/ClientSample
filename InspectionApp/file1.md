+---------------------+       +---------------------------+
|       Program       |       |          Form1           |
+---------------------+       +---------------------------+
| +Main() static      |       | +Form1()                 |
+---------------------+       | -InitializeComponent()   |
                              +---------------------------+
                                      ^
                                      |
                                      | 継承
                                      |
+---------------------+       +---------------------------+
|     MainForm        |       |    DeviceMonitorForm     |
+---------------------+       +---------------------------+
| -_deviceMonitor     |       | -_deviceMonitor          |
| +MainForm()         |       | -_deviceItems            |
| -InitializeDevices()|       | -_statusLabels           |
| -MainForm_Load()    |       | +DeviceMonitorForm()     |
+---------------------+       | -InitializeComponent()   |
        |                     | -ShowDeviceDetails()     |
        | 所有                | -OnDeviceStatusUpdated() |
        |                     | -OnThresholdExceeded()   |
        v                     +---------------------------+
+---------------------+                |
|   DeviceMonitor     |                | 使用
+---------------------+                |
| -_powerSupplies     |                |
| -_multimeters       |<---------------+
| -_powerSupplyProcessors|
| -_multimeterProcessors |
| -_updateTimer       |
| -_disposed          |
| -_processorKeyCounter|
| +DeviceMonitor()    |
| +StartMonitoring()  |
| +StopMonitoring()   |
| +AddPowerSupply()   |
| +AddMultimeter()    |
| +SetPowerSupplyThresholds()|
| +SetMultimeterThresholds() |
| +Dispose()          |
+---------------------+
    |           |
    | 使用      | 使用
    |           |
    v           v
+------------+              +------------+
| IPowerSupply|             | IMultimeter|
+------------+              +------------+
| +IsConnected|             | +IsConnected|
| +IsInitialized|           | +IsInitialized|
| +ConnectAsync()|          | +ConnectAsync()|
| +DisconnectAsync()|       | +DisconnectAsync()|
| +InitializeAsync()|       | +InitializeAsync()|
| +MeasureVoltageAsync()|   | +MeasureVoltageAsync()|
| +MeasureCurrentAsync()|   | +MeasureCurrentAsync()|
+------------+              | +SetFunctionAsync()|
                            | +GetFunctionAsync()|
                            | +GetErrorMessageAsync()|
                            +------------+

+---------------------------+  +---------------------------+
| MultimeterMeasurementRunner|  | PowerSupplyMeasurementRunner|
+---------------------------+  +---------------------------+
| -_multimeter             |  | -_powerSupply            |
| -_dataProcessor          |  | -_dataProcessor          |
| -_cts                    |  | -_cts                    |
| -_measurementTask        |  | -_measurementTask        |
| -_disposed               |  | -_disposed               |
| -_activeSettings         |  | -_activeSettings         |
| -_lockObject             |  | -_lockObject             |
| +IsMeasuring             |  | +IsMeasuring             |
| +StartMeasurement()      |  | +StartMeasurement()      |
| +StopMeasurement()       |  | +StopMeasurement()       |
| +StopAllMeasurements()   |  | +StopAllMeasurements()   |
| +Dispose()               |  | +Dispose()               |
+---------------------------+  +---------------------------+
    |                            |
    | 所有                       | 所有
    |                            |
    v                            v
+------------+              +------------+
| IMultimeter|              | IPowerSupply|
+------------+              +------------+

+---------------------------+
|   MeasurementDataWithKey  |
+---------------------------+
| +Key                     |
| +Value                   |
| +Timestamp               |
| +Source                  |
+---------------------------+

+---------------------------+
|   MeasurementSettings     |
+---------------------------+
| +MeasurementKey          |
| +MeasurementType         |
| +Interval                |
| +PowerSupplyChannel      |
| +MultimeterFunction      |
| +SourceIdentifier        |
+---------------------------+

+---------------------------+
|   MeasurementType        |
+---------------------------+
| Voltage                  |
| Current                  |
| MultimeterValue          |
+---------------------------+

