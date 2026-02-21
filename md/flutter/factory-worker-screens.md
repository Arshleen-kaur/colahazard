# Factory Worker - Flutter Screens & Workflows

## Overview
Flutter mobile app screens for factory workers with batch creation, quality control, and production tracking.

## Screen Structure

```
lib/
├── screens/
│   ├── factory/
│   │   ├── dashboard_screen.dart
│   │   ├── batch_creation_screen.dart
│   │   ├── quality_check_screen.dart
│   │   ├── qr_scanner_screen.dart
│   │   ├── defect_report_screen.dart
│   │   ├── task_list_screen.dart
│   │   ├── machine_monitoring_screen.dart
│   │   └── safety_checklist_screen.dart
```

---

## 1. Dashboard Screen

### File: `factory_dashboard_screen.dart`

```dart
import 'package:flutter/material.dart';
import '../services/factory_api_service.dart';
import '../models/factory_dashboard_model.dart';

class FactoryDashboardScreen extends StatefulWidget {
  final String workerId;

  const FactoryDashboardScreen({Key? key, required this.workerId}) : super(key: key);

  @override
  _FactoryDashboardScreenState createState() => _FactoryDashboardScreenState();
}

class _FactoryDashboardScreenState extends State<FactoryDashboardScreen> {
  final FactoryApiService _apiService = FactoryApiService();
  FactoryDashboard? _dashboard;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadDashboard();
  }

  Future<void> _loadDashboard() async {
    try {
      final dashboard = await _apiService.getDashboard(widget.workerId);
      setState(() {
        _dashboard = dashboard;
        _isLoading = false;
      });
    } catch (e) {
      setState(() => _isLoading = false);
      _showError('Failed to load dashboard: $e');
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading) {
      return Scaffold(body: Center(child: CircularProgressIndicator()));
    }

    return Scaffold(
      appBar: AppBar(
        title: Text('Production Dashboard'),
        actions: [
          IconButton(
            icon: Icon(Icons.refresh),
            onPressed: _loadDashboard,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _loadDashboard,
        child: SingleChildScrollView(
          padding: EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildWorkerInfo(),
              SizedBox(height: 20),
              _buildShiftInfo(),
              SizedBox(height: 20),
              _buildProductionStats(),
              SizedBox(height: 20),
              _buildActiveBatches(),
              SizedBox(height: 20),
              _buildPendingTasks(),
              SizedBox(height: 20),
              _buildQuickActions(),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildWorkerInfo() {
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Row(
          children: [
            CircleAvatar(
              radius: 30,
              child: Icon(Icons.person, size: 30),
            ),
            SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    _dashboard?.workerName ?? 'Worker',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                  ),
                  Text('ID: ${_dashboard?.workerId ?? 'N/A'}'),
                  Text('Plant: ${_dashboard?.plantName ?? 'N/A'}'),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildShiftInfo() {
    final shift = _dashboard?.currentShift;
    return Card(
      color: Colors.blue.shade50,
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(Icons.access_time, color: Colors.blue),
                SizedBox(width: 8),
                Text(
                  'Current Shift',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
              ],
            ),
            SizedBox(height: 12),
            Text('Shift: ${shift?.shiftCode ?? 'N/A'}'),
            Text('Line: ${shift?.productionLineId ?? 'N/A'}'),
            Text('Supervisor: ${shift?.supervisor ?? 'N/A'}'),
            Text('Time: ${_formatTime(shift?.startTime)} - ${_formatTime(shift?.endTime)}'),
          ],
        ),
      ),
    );
  }

  Widget _buildProductionStats() {
    final stats = _dashboard?.todayStats;
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Today\'s Production',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16),
            LinearProgressIndicator(
              value: (stats?.completionPercentage ?? 0) / 100,
              minHeight: 8,
              backgroundColor: Colors.grey.shade300,
              valueColor: AlwaysStoppedAnimation<Color>(Colors.green),
            ),
            SizedBox(height: 8),
            Text(
              '${stats?.unitsProduced ?? 0} / ${stats?.targetUnits ?? 0} units (${stats?.completionPercentage?.toStringAsFixed(1) ?? 0}%)',
              style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16),
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceAround,
              children: [
                _buildStatItem('QC Passed', '${stats?.qualityChecksPassed ?? 0}', Colors.green),
                _buildStatItem('QC Failed', '${stats?.qualityChecksFailed ?? 0}', Colors.red),
                _buildStatItem('Defects', '${stats?.defectsReported ?? 0}', Colors.orange),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildStatItem(String label, String value, Color color) {
    return Column(
      children: [
        Text(
          value,
          style: TextStyle(
            fontSize: 24,
            fontWeight: FontWeight.bold,
            color: color,
          ),
        ),
        Text(label, style: TextStyle(color: Colors.grey)),
      ],
    );
  }

  Widget _buildActiveBatches() {
    final batches = _dashboard?.activeBatches ?? [];
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Active Batches',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 12),
            if (batches.isEmpty)
              Center(child: Text('No active batches'))
            else
              ...batches.map((batch) => _buildBatchCard(batch)),
          ],
        ),
      ),
    );
  }

  Widget _buildBatchCard(dynamic batch) {
    return Card(
      margin: EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: CircleAvatar(
          child: Text('B'),
          backgroundColor: Colors.blue,
        ),
        title: Text(batch.batchCode),
        subtitle: Text('${batch.bottleType} ${batch.capacityML}ml'),
        trailing: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            Text(
              '${batch.producedUnits}/${batch.targetUnits}',
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
            Text(
              batch.status,
              style: TextStyle(color: Colors.orange, fontSize: 12),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildPendingTasks() {
    final tasks = _dashboard?.pendingTasks ?? [];
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Text(
                  'Pending Tasks',
                  style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
                ),
                TextButton(
                  onPressed: () => Navigator.pushNamed(context, '/tasks'),
                  child: Text('View All'),
                ),
              ],
            ),
            SizedBox(height: 12),
            if (tasks.isEmpty)
              Center(child: Text('No pending tasks'))
            else
              ...tasks.take(3).map((task) => _buildTaskCard(task)),
          ],
        ),
      ),
    );
  }

  Widget _buildTaskCard(dynamic task) {
    return Card(
      margin: EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: Icon(
          _getTaskIcon(task.taskType),
          color: task.priority == 'High' ? Colors.red : Colors.orange,
        ),
        title: Text(task.taskType),
        subtitle: Text('Batch: ${task.batchCode}'),
        trailing: Text(
          _formatTime(task.dueTime),
          style: TextStyle(fontSize: 12),
        ),
      ),
    );
  }

  Widget _buildQuickActions() {
    return Column(
      children: [
        Row(
          children: [
            Expanded(
              child: ElevatedButton.icon(
                onPressed: () => Navigator.pushNamed(context, '/create-batch'),
                icon: Icon(Icons.add_box),
                label: Text('Create Batch'),
                style: ElevatedButton.styleFrom(
                  padding: EdgeInsets.symmetric(vertical: 16),
                ),
              ),
            ),
            SizedBox(width: 12),
            Expanded(
              child: ElevatedButton.icon(
                onPressed: () => Navigator.pushNamed(context, '/qr-scanner'),
                icon: Icon(Icons.qr_code_scanner),
                label: Text('Scan QR'),
                style: ElevatedButton.styleFrom(
                  padding: EdgeInsets.symmetric(vertical: 16),
                ),
              ),
            ),
          ],
        ),
        SizedBox(height: 12),
        Row(
          children: [
            Expanded(
              child: ElevatedButton.icon(
                onPressed: () => Navigator.pushNamed(context, '/quality-check'),
                icon: Icon(Icons.verified),
                label: Text('Quality Check'),
                style: ElevatedButton.styleFrom(
                  padding: EdgeInsets.symmetric(vertical: 16),
                  backgroundColor: Colors.green,
                ),
              ),
            ),
            SizedBox(width: 12),
            Expanded(
              child: ElevatedButton.icon(
                onPressed: () => Navigator.pushNamed(context, '/report-defect'),
                icon: Icon(Icons.report_problem),
                label: Text('Report Defect'),
                style: ElevatedButton.styleFrom(
                  padding: EdgeInsets.symmetric(vertical: 16),
                  backgroundColor: Colors.orange,
                ),
              ),
            ),
          ],
        ),
      ],
    );
  }

  IconData _getTaskIcon(String taskType) {
    switch (taskType) {
      case 'QualityCheck':
        return Icons.verified;
      case 'Production':
        return Icons.precision_manufacturing;
      case 'Packaging':
        return Icons.inventory_2;
      default:
        return Icons.task;
    }
  }

  String _formatTime(dynamic time) {
    if (time == null) return 'N/A';
    // Format time logic here
    return time.toString();
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red),
    );
  }
}
```

---

## 2. Batch Creation Screen

### File: `batch_creation_screen.dart`

```dart
import 'package:flutter/material.dart';
import '../services/factory_api_service.dart';

class BatchCreationScreen extends StatefulWidget {
  @override
  _BatchCreationScreenState createState() => _BatchCreationScreenState();
}

class _BatchCreationScreenState extends State<BatchCreationScreen> {
  final _formKey = GlobalKey<FormState>();
  final FactoryApiService _apiService = FactoryApiService();
  
  String _bottleType = 'Thick';
  int _capacityML = 500;
  int _plannedUnits = 600;
  String _liquidType = 'Cola Classic';
  bool _isSubmitting = false;

  final List<String> _bottleTypes = ['Thick', 'Thin', 'rPET'];
  final List<int> _capacities = [250, 500, 1000];
  final List<String> _liquidTypes = ['Cola Classic', 'Cola Zero', 'Orange', 'Lemon'];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Create Production Batch'),
      ),
      body: Form(
        key: _formKey,
        child: ListView(
          padding: EdgeInsets.all(16),
          children: [
            _buildBottleSpecs(),
            SizedBox(height: 20),
            _buildLiquidSpecs(),
            SizedBox(height: 20),
            _buildProductionDetails(),
            SizedBox(height: 30),
            _buildSubmitButton(),
          ],
        ),
      ),
    );
  }

  Widget _buildBottleSpecs() {
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Bottle Specifications',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16),
            DropdownButtonFormField<String>(
              value: _bottleType,
              decoration: InputDecoration(
                labelText: 'Bottle Type',
                border: OutlineInputBorder(),
              ),
              items: _bottleTypes.map((type) {
                return DropdownMenuItem(value: type, child: Text(type));
              }).toList(),
              onChanged: (value) => setState(() => _bottleType = value!),
            ),
            SizedBox(height: 16),
            DropdownButtonFormField<int>(
              value: _capacityML,
              decoration: InputDecoration(
                labelText: 'Capacity (ML)',
                border: OutlineInputBorder(),
              ),
              items: _capacities.map((capacity) {
                return DropdownMenuItem(
                  value: capacity,
                  child: Text('$capacity ml'),
                );
              }).toList(),
              onChanged: (value) => setState(() => _capacityML = value!),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildLiquidSpecs() {
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Liquid Specifications',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16),
            DropdownButtonFormField<String>(
              value: _liquidType,
              decoration: InputDecoration(
                labelText: 'Liquid Type',
                border: OutlineInputBorder(),
              ),
              items: _liquidTypes.map((type) {
                return DropdownMenuItem(value: type, child: Text(type));
              }).toList(),
              onChanged: (value) => setState(() => _liquidType = value!),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildProductionDetails() {
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Production Details',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16),
            TextFormField(
              initialValue: _plannedUnits.toString(),
              decoration: InputDecoration(
                labelText: 'Planned Units',
                border: OutlineInputBorder(),
                suffixText: 'units',
              ),
              keyboardType: TextInputType.number,
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Please enter planned units';
                }
                if (int.tryParse(value) == null) {
                  return 'Please enter a valid number';
                }
                return null;
              },
              onChanged: (value) {
                final units = int.tryParse(value);
                if (units != null) {
                  setState(() => _plannedUnits = units);
                }
              },
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSubmitButton() {
    return ElevatedButton(
      onPressed: _isSubmitting ? null : _createBatch,
      style: ElevatedButton.styleFrom(
        padding: EdgeInsets.symmetric(vertical: 16),
        backgroundColor: Colors.blue,
      ),
      child: _isSubmitting
          ? CircularProgressIndicator(color: Colors.white)
          : Text(
              'Create Batch',
              style: TextStyle(fontSize: 18),
            ),
    );
  }

  Future<void> _createBatch() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isSubmitting = true);

    try {
      final result = await _apiService.createBatch(
        bottleType: _bottleType,
        capacityML: _capacityML,
        liquidType: _liquidType,
        plannedUnits: _plannedUnits,
      );

      if (result.success) {
        _showSuccessDialog(result);
      } else {
        _showError('Failed to create batch');
        setState(() => _isSubmitting = false);
      }
    } catch (e) {
      _showError('Error: $e');
      setState(() => _isSubmitting = false);
    }
  }

  void _showSuccessDialog(dynamic result) {
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: Row(
          children: [
            Icon(Icons.check_circle, color: Colors.green),
            SizedBox(width: 8),
            Text('Success'),
          ],
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Batch created successfully!'),
            SizedBox(height: 8),
            Text('Batch Code: ${result.batchCode}'),
            Text('QR Code: ${result.qrCode}'),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              Navigator.pop(context, result);
            },
            child: Text('Done'),
          ),
        ],
      ),
    );
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red),
    );
  }
}
```

---

## 3. Quality Check Screen

### File: `quality_check_screen.dart`

```dart
import 'package:flutter/material.dart';
import '../services/factory_api_service.dart';

class QualityCheckScreen extends StatefulWidget {
  final String batchCode;

  const QualityCheckScreen({Key? key, required this.batchCode}) : super(key: key);

  @override
  _QualityCheckScreenState createState() => _QualityCheckScreenState();
}

class _QualityCheckScreenState extends State<QualityCheckScreen> {
  final FactoryApiService _apiService = FactoryApiService();
  
  bool _visualInspection = true;
  bool _weightCheck = true;
  bool _sealIntegrity = true;
  bool _labelQuality = true;
  bool _carbonationLevel = true;
  bool _phLevel = true;
  
  int _sampleSize = 10;
  bool _isSubmitting = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('Quality Check'),
      ),
      body: ListView(
        padding: EdgeInsets.all(16),
        children: [
          _buildBatchInfo(),
          SizedBox(height: 20),
          _buildSampleSize(),
          SizedBox(height: 20),
          _buildCheckParameters(),
          SizedBox(height: 30),
          _buildSubmitButton(),
        ],
      ),
    );
  }

  Widget _buildBatchInfo() {
    return Card(
      color: Colors.blue.shade50,
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Batch Information',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 8),
            Text('Batch Code: ${widget.batchCode}'),
          ],
        ),
      ),
    );
  }

  Widget _buildSampleSize() {
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Sample Size',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16),
            Row(
              children: [
                Expanded(
                  child: Slider(
                    value: _sampleSize.toDouble(),
                    min: 5,
                    max: 50,
                    divisions: 9,
                    label: _sampleSize.toString(),
                    onChanged: (value) {
                      setState(() => _sampleSize = value.toInt());
                    },
                  ),
                ),
                Text(
                  '$_sampleSize units',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildCheckParameters() {
    return Card(
      child: Padding(
        padding: EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Check Parameters',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
            ),
            SizedBox(height: 16),
            _buildCheckItem('Visual Inspection', _visualInspection, (value) {
              setState(() => _visualInspection = value);
            }),
            _buildCheckItem('Weight Check', _weightCheck, (value) {
              setState(() => _weightCheck = value);
            }),
            _buildCheckItem('Seal Integrity', _sealIntegrity, (value) {
              setState(() => _sealIntegrity = value);
            }),
            _buildCheckItem('Label Quality', _labelQuality, (value) {
              setState(() => _labelQuality = value);
            }),
            _buildCheckItem('Carbonation Level', _carbonationLevel, (value) {
              setState(() => _carbonationLevel = value);
            }),
            _buildCheckItem('pH Level', _phLevel, (value) {
              setState(() => _phLevel = value);
            }),
          ],
        ),
      ),
    );
  }

  Widget _buildCheckItem(String label, bool value, Function(bool) onChanged) {
    return CheckboxListTile(
      title: Text(label),
      value: value,
      onChanged: (newValue) => onChanged(newValue ?? false),
      secondary: Icon(
        value ? Icons.check_circle : Icons.cancel,
        color: value ? Colors.green : Colors.red,
      ),
    );
  }

  Widget _buildSubmitButton() {
    return ElevatedButton(
      onPressed: _isSubmitting ? null : _submitQualityCheck,
      style: ElevatedButton.styleFrom(
        padding: EdgeInsets.symmetric(vertical: 16),
        backgroundColor: Colors.green,
      ),
      child: _isSubmitting
          ? CircularProgressIndicator(color: Colors.white)
          : Text(
              'Submit Quality Check',
              style: TextStyle(fontSize: 18),
            ),
    );
  }

  Future<void> _submitQualityCheck() async {
    setState(() => _isSubmitting = true);

    try {
      final result = await _apiService.performQualityCheck(
        batchCode: widget.batchCode,
        sampleSize: _sampleSize,
        visualInspection: _visualInspection,
        weightCheck: _weightCheck,
        sealIntegrity: _sealIntegrity,
        labelQuality: _labelQuality,
        carbonationLevel: _carbonationLevel,
        phLevel: _phLevel,
      );

      if (result.success) {
        _showResultDialog(result);
      } else {
        _showError('Failed to submit quality check');
        setState(() => _isSubmitting = false);
      }
    } catch (e) {
      _showError('Error: $e');
      setState(() => _isSubmitting = false);
    }
  }

  void _showResultDialog(dynamic result) {
    final passed = result.batchStatus == 'Approved';
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (context) => AlertDialog(
        title: Row(
          children: [
            Icon(
              passed ? Icons.check_circle : Icons.cancel,
              color: passed ? Colors.green : Colors.red,
            ),
            SizedBox(width: 8),
            Text(passed ? 'Passed' : 'Failed'),
          ],
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Quality Score: ${result.qualityScore}%'),
            Text('Status: ${result.batchStatus}'),
            Text('Next Action: ${result.nextAction}'),
          ],
        ),
        actions: [
          TextButton(
            onPressed: () {
              Navigator.pop(context);
              Navigator.pop(context, result);
            },
            child: Text('Done'),
          ),
        ],
      ),
    );
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red),
    );
  }
}
```

---

## Workflow Diagram

```
┌─────────────────┐
│  Login Screen   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Dashboard Screen│◄──────┐
└────────┬────────┘       │
         │                │
    ┌────┴────┬──────────┬┴──────────┬──────────┐
    │         │          │           │          │
    ▼         ▼          ▼           ▼          ▼
┌────────┐ ┌──────┐ ┌────────┐ ┌─────────┐ ┌────────┐
│Create  │ │QR    │ │Quality │ │Report   │ │Task    │
│Batch   │ │Scan  │ │Check   │ │Defect   │ │List    │
└────────┘ └──────┘ └────────┘ └─────────┘ └────────┘
```

---

## Dependencies (pubspec.yaml)

```yaml
dependencies:
  flutter:
    sdk: flutter
  http: ^1.1.0
  qr_code_scanner: ^1.0.1
  provider: ^6.1.1
  shared_preferences: ^2.2.2
  image_picker: ^1.0.4
```
