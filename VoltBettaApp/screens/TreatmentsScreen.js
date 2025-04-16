import React, { useState } from 'react';
import { StyleSheet, View, ScrollView, FlatList } from 'react-native';
import { 
  Card, 
  Title, 
  Paragraph, 
  Button, 
  FAB, 
  Dialog, 
  Portal, 
  TextInput,
  Chip,
  Divider,
  List,
  Searchbar,
  SegmentedButtons,
  IconButton,
  Badge,
  TouchableRipple
} from 'react-native-paper';
import { StatusBar } from 'expo-status-bar';

// Treatment status types
const TREATMENT_STATUS = {
  ACTIVE: 'Active',
  COMPLETED: 'Completed',
  PLANNED: 'Planned'
};

// Symptom severity levels
const SYMPTOM_SEVERITY = {
  MILD: 'Mild',
  MODERATE: 'Moderate',
  SEVERE: 'Severe'
};

// Sample data - this would normally come from a database
const initialTreatments = [
  {
    id: 1,
    illnessName: 'Fin Rot',
    description: 'Minor fin rot on tail edges. Possibly caused by poor water quality.',
    startDate: new Date('2025-04-01'),
    endDate: null, // ongoing
    medicationNotes: 'Using API Fin & Body Cure as directed on package. Doing 25% water changes every other day.',
    status: TREATMENT_STATUS.ACTIVE,
    symptoms: [
      {
        name: 'Frayed fins',
        severity: SYMPTOM_SEVERITY.MODERATE,
        notes: 'Primarily on tail edges'
      },
      {
        name: 'Discoloration',
        severity: SYMPTOM_SEVERITY.MILD,
        notes: 'Slight darkening on edges of fins'
      }
    ],
    treatmentLogs: [
      {
        date: new Date('2025-04-01'),
        actions: 'Started treatment with API Fin & Body Cure. Added first dose.',
        notes: 'Removed carbon from filter. Tested water parameters: Ammonia 0, Nitrite 0, Nitrate 10ppm.'
      },
      {
        date: new Date('2025-04-03'),
        actions: '25% water change. Added second dose of medication.',
        notes: 'Slight improvement noticed. Fins look less ragged.'
      },
      {
        date: new Date('2025-04-05'),
        actions: '25% water change. Added third dose of medication.',
        notes: 'Continued improvement. Volt seems more active today.'
      }
    ]
  },
  {
    id: 2,
    illnessName: 'Swim Bladder Issue',
    description: 'Difficulty maintaining buoyancy, occasionally floating sideways.',
    startDate: new Date('2025-02-15'),
    endDate: new Date('2025-02-25'),
    medicationNotes: 'Fasted for 3 days, then fed daphnia to help with digestion. No medication used.',
    status: TREATMENT_STATUS.COMPLETED,
    symptoms: [
      {
        name: 'Floating sideways',
        severity: SYMPTOM_SEVERITY.MODERATE,
        notes: 'Most noticeable after feeding'
      },
      {
        name: 'Difficulty swimming',
        severity: SYMPTOM_SEVERITY.MILD,
        notes: 'Struggles to maintain normal position'
      }
    ],
    treatmentLogs: [
      {
        date: new Date('2025-02-15'),
        actions: 'Started 3-day fast to clear digestive system.',
        notes: 'Suspect overfeeding may be the cause.'
      },
      {
        date: new Date('2025-02-18'),
        actions: 'Fed small amount of daphnia to help with digestion.',
        notes: 'Swimming seems slightly improved today.'
      },
      {
        date: new Date('2025-02-20'),
        actions: 'Continued with small, controlled feedings of daphnia.',
        notes: 'Significant improvement in swimming and buoyancy.'
      },
      {
        date: new Date('2025-02-25'),
        actions: 'Returned to normal feeding schedule with reduced portions.',
        notes: 'Problem appears resolved. Will continue monitoring.'
      }
    ]
  },
  {
    id: 3,
    illnessName: 'Preventative Quarantine',
    description: 'Preventative quarantine after adding new plants to ensure no parasites or diseases are introduced.',
    startDate: new Date('2025-04-20'), // Future date
    endDate: null,
    medicationNotes: 'No medication planned unless symptoms appear.',
    status: TREATMENT_STATUS.PLANNED,
    symptoms: [],
    treatmentLogs: []
  }
];

const TreatmentsScreen = () => {
  const [treatments, setTreatments] = useState(initialTreatments);
  const [visible, setVisible] = useState(false);
  const [selectedTreatment, setSelectedTreatment] = useState(null);
  const [newTreatment, setNewTreatment] = useState({
    illnessName: '',
    description: '',
    startDate: new Date(),
    endDate: null,
    medicationNotes: '',
    status: TREATMENT_STATUS.ACTIVE,
    symptoms: [],
    treatmentLogs: []
  });
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedStatus, setSelectedStatus] = useState(null);
  const [detailVisible, setDetailVisible] = useState(false);
  
  // Treatment log management
  const [logDialogVisible, setLogDialogVisible] = useState(false);
  const [newLog, setNewLog] = useState({
    date: new Date(),
    actions: '',
    notes: ''
  });
  
  // Symptom management
  const [symptomDialogVisible, setSymptomDialogVisible] = useState(false);
  const [newSymptom, setNewSymptom] = useState({
    name: '',
    severity: SYMPTOM_SEVERITY.MILD,
    notes: ''
  });
  
  // Complete treatment management
  const [completeDialogVisible, setCompleteDialogVisible] = useState(false);
  const [outcomeNotes, setOutcomeNotes] = useState('');
  
  // Tab management for treatment details
  const [selectedTab, setSelectedTab] = useState('details');

  const showDialog = () => setVisible(true);
  const hideDialog = () => setVisible(false);
  
  const showDetailDialog = (treatment) => {
    setSelectedTreatment(treatment);
    setDetailVisible(true);
  };
  
  const hideDetailDialog = () => {
    setDetailVisible(false);
    setSelectedTreatment(null);
    setSelectedTab('details');
  };
  
  const showLogDialog = () => {
    setNewLog({
      date: new Date(),
      actions: '',
      notes: ''
    });
    setLogDialogVisible(true);
  };
  
  const hideLogDialog = () => {
    setLogDialogVisible(false);
  };
  
  const showSymptomDialog = () => {
    setNewSymptom({
      name: '',
      severity: SYMPTOM_SEVERITY.MILD,
      notes: ''
    });
    setSymptomDialogVisible(true);
  };
  
  const hideSymptomDialog = () => {
    setSymptomDialogVisible(false);
  };
  
  const showCompleteDialog = () => {
    setOutcomeNotes('');
    setCompleteDialogVisible(true);
  };
  
  const hideCompleteDialog = () => {
    setCompleteDialogVisible(false);
  };

  // Filter treatments based on search query and selected status
  const filteredTreatments = treatments.filter(treatment => {
    const matchesSearch = searchQuery === '' || 
      treatment.illnessName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      treatment.description?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      treatment.medicationNotes?.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesStatus = selectedStatus === null || 
      treatment.status === selectedStatus;
    
    return matchesSearch && matchesStatus;
  }).sort((a, b) => {
    // Sort by status priority (Active first, then Planned, then Completed)
    const statusOrder = {
      [TREATMENT_STATUS.ACTIVE]: 0,
      [TREATMENT_STATUS.PLANNED]: 1,
      [TREATMENT_STATUS.COMPLETED]: 2
    };
    
    if (statusOrder[a.status] !== statusOrder[b.status]) {
      return statusOrder[a.status] - statusOrder[b.status];
    }
    
    // If same status, sort by start date (newest first)
    return b.startDate - a.startDate;
  });

  const handleAddTreatment = () => {
    if (newTreatment.illnessName.trim() === '') {
      return;
    }
    
    const treatment = {
      id: treatments.length + 1,
      ...newTreatment,
      startDate: new Date(),
    };
    
    setTreatments([...treatments, treatment]);
    setNewTreatment({
      illnessName: '',
      description: '',
      startDate: new Date(),
      endDate: null,
      medicationNotes: '',
      status: TREATMENT_STATUS.ACTIVE,
      symptoms: [],
      treatmentLogs: []
    });
    hideDialog();
  };
  
  const handleAddLog = () => {
    if (newLog.actions.trim() === '' || !selectedTreatment) {
      return;
    }
    
    const updatedTreatments = treatments.map(treatment => {
      if (treatment.id === selectedTreatment.id) {
        return {
          ...treatment,
          treatmentLogs: [...treatment.treatmentLogs, newLog]
        };
      }
      return treatment;
    });
    
    setTreatments(updatedTreatments);
    
    // Update the selected treatment to reflect the changes
    const updatedTreatment = updatedTreatments.find(t => t.id === selectedTreatment.id);
    setSelectedTreatment(updatedTreatment);
    
    hideLogDialog();
  };
  
  const handleAddSymptom = () => {
    if (newSymptom.name.trim() === '' || !selectedTreatment) {
      return;
    }
    
    const updatedTreatments = treatments.map(treatment => {
      if (treatment.id === selectedTreatment.id) {
        return {
          ...treatment,
          symptoms: [...treatment.symptoms, newSymptom]
        };
      }
      return treatment;
    });
    
    setTreatments(updatedTreatments);
    
    // Update the selected treatment to reflect the changes
    const updatedTreatment = updatedTreatments.find(t => t.id === selectedTreatment.id);
    setSelectedTreatment(updatedTreatment);
    
    hideSymptomDialog();
  };
  
  const handleCompleteTreatment = () => {
    if (!selectedTreatment) {
      return;
    }
    
    const updatedTreatments = treatments.map(treatment => {
      if (treatment.id === selectedTreatment.id) {
        // Add a final treatment log for the outcome
        const finalLog = {
          date: new Date(),
          actions: 'Treatment completed',
          notes: outcomeNotes
        };
        
        return {
          ...treatment,
          status: TREATMENT_STATUS.COMPLETED,
          endDate: new Date(),
          treatmentLogs: [...treatment.treatmentLogs, finalLog]
        };
      }
      return treatment;
    });
    
    setTreatments(updatedTreatments);
    hideCompleteDialog();
    hideDetailDialog(); // Close the detail view too
  };

  const formatDate = (date) => {
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };
  
  const getStatusColor = (status) => {
    switch (status) {
      case TREATMENT_STATUS.ACTIVE:
        return '#2196F3'; // Blue
      case TREATMENT_STATUS.PLANNED:
        return '#9C27B0'; // Purple
      case TREATMENT_STATUS.COMPLETED:
        return '#4CAF50'; // Green
      default:
        return '#9E9E9E'; // Grey
    }
  };
  
  const getSeverityColor = (severity) => {
    switch (severity) {
      case SYMPTOM_SEVERITY.MILD:
        return '#4CAF50'; // Green
      case SYMPTOM_SEVERITY.MODERATE:
        return '#FF9800'; // Orange
      case SYMPTOM_SEVERITY.SEVERE:
        return '#F44336'; // Red
      default:
        return '#9E9E9E'; // Grey
    }
  };

  return (
    <View style={styles.container}>
      <StatusBar style="auto" />
      
      <View style={styles.searchContainer}>
        <Searchbar
          placeholder="Search treatments..."
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
        />
      </View>
      
      <ScrollView horizontal style={styles.filterContainer}>
        <Chip 
          selected={selectedStatus === null}
          onPress={() => setSelectedStatus(null)}
          style={styles.filterChip}
        >
          All
        </Chip>
        {Object.values(TREATMENT_STATUS).map((status, index) => (
          <Chip
            key={index}
            selected={selectedStatus === status}
            onPress={() => setSelectedStatus(status)}
            style={[
              styles.filterChip, 
              { backgroundColor: selectedStatus === status ? getStatusColor(status) : undefined }
            ]}
          >
            {status}
          </Chip>
        ))}
      </ScrollView>
      
      <FlatList
        data={filteredTreatments}
        keyExtractor={(item) => item.id.toString()}
        renderItem={({ item }) => (
          <Card style={styles.card} onPress={() => showDetailDialog(item)}>
            <Card.Content>
              <View style={styles.cardHeader}>
                <Title style={styles.cardTitle}>{item.illnessName}</Title>
                <Chip 
                  style={[styles.statusChip, { backgroundColor: getStatusColor(item.status) }]}
                >
                  {item.status}
                </Chip>
              </View>
              <Paragraph numberOfLines={2} style={styles.description}>{item.description}</Paragraph>
              <View style={styles.cardFooter}>
                <Paragraph style={styles.date}>
                  {`Started: ${formatDate(item.startDate)}`}
                </Paragraph>
                {item.status === TREATMENT_STATUS.COMPLETED && item.endDate && (
                  <Paragraph style={styles.date}>
                    {`Ended: ${formatDate(item.endDate)}`}
                  </Paragraph>
                )}
              </View>
              <View style={styles.symptomsContainer}>
                {item.symptoms.length > 0 && (
                  <>
                    <Paragraph style={styles.symptomsLabel}>Symptoms:</Paragraph>
                    <ScrollView horizontal>
                      {item.symptoms.map((symptom, index) => (
                        <Chip 
                          key={index}
                          style={[styles.symptomChip, { backgroundColor: getSeverityColor(symptom.severity) }]}
                        >
                          {symptom.name}
                        </Chip>
                      ))}
                    </ScrollView>
                  </>
                )}
              </View>
            </Card.Content>
          </Card>
        )}
        contentContainerStyle={styles.listContent}
      />
      
      <FAB
        style={styles.fab}
        icon="plus"
        onPress={showDialog}
        label="Add Treatment"
      />
      
      {/* Add New Treatment Dialog */}
      <Portal>
        <Dialog visible={visible} onDismiss={hideDialog}>
          <Dialog.Title>Add New Treatment Plan</Dialog.Title>
          <Dialog.ScrollArea>
            <ScrollView>
              <View style={styles.dialogContent}>
                <TextInput
                  label="Illness/Condition Name"
                  value={newTreatment.illnessName}
                  onChangeText={(text) => setNewTreatment({...newTreatment, illnessName: text})}
                  style={styles.input}
                />
                <TextInput
                  label="Description"
                  value={newTreatment.description}
                  onChangeText={(text) => setNewTreatment({...newTreatment, description: text})}
                  multiline
                  numberOfLines={3}
                  style={styles.input}
                />
                
                <Title style={styles.selectLabel}>Status</Title>
                <ScrollView horizontal style={styles.optionScroll}>
                  {Object.values(TREATMENT_STATUS).map((status, index) => (
                    <Chip
                      key={index}
                      selected={newTreatment.status === status}
                      onPress={() => setNewTreatment({...newTreatment, status})}
                      style={[
                        styles.optionChip, 
                        { backgroundColor: newTreatment.status === status ? getStatusColor(status) : undefined }
                      ]}
                    >
                      {status}
                    </Chip>
                  ))}
                </ScrollView>
                
                <TextInput
                  label="Medication Notes"
                  value={newTreatment.medicationNotes}
                  onChangeText={(text) => setNewTreatment({...newTreatment, medicationNotes: text})}
                  multiline
                  numberOfLines={3}
                  style={styles.input}
                />
              </View>
            </ScrollView>
          </Dialog.ScrollArea>
          <Dialog.Actions>
            <Button onPress={hideDialog}>Cancel</Button>
            <Button 
              onPress={handleAddTreatment} 
              disabled={!newTreatment.illnessName.trim()}
            >
              Add
            </Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>
      
      {/* Treatment Detail Dialog */}
      <Portal>
        <Dialog visible={detailVisible} onDismiss={hideDetailDialog} style={styles.detailDialog}>
          {selectedTreatment && (
            <>
              <Dialog.Title>{selectedTreatment.illnessName}</Dialog.Title>
              <Dialog.Content>
                <View style={styles.detailStatusRow}>
                  <Chip 
                    style={[styles.detailStatusChip, { backgroundColor: getStatusColor(selectedTreatment.status) }]}
                  >
                    {selectedTreatment.status}
                  </Chip>
                  <Paragraph style={styles.detailDate}>
                    {`Started: ${formatDate(selectedTreatment.startDate)}`}
                    {selectedTreatment.status === TREATMENT_STATUS.COMPLETED && selectedTreatment.endDate && 
                      ` • Ended: ${formatDate(selectedTreatment.endDate)}`}
                  </Paragraph>
                </View>
                
                <SegmentedButtons
                  value={selectedTab}
                  onValueChange={setSelectedTab}
                  buttons={[
                    { value: 'details', label: 'Details' },
                    { value: 'logs', label: 'Logs', badge: selectedTreatment.treatmentLogs.length > 0 },
                    { value: 'symptoms', label: 'Symptoms', badge: selectedTreatment.symptoms.length > 0 }
                  ]}
                  style={styles.tabButtons}
                />
                
                {selectedTab === 'details' && (
                  <View style={styles.tabContent}>
                    <Title style={styles.sectionTitle}>Description</Title>
                    <Paragraph style={styles.detailText}>
                      {selectedTreatment.description || 'No description provided.'}
                    </Paragraph>
                    
                    <Title style={styles.sectionTitle}>Medication Notes</Title>
                    <Paragraph style={styles.detailText}>
                      {selectedTreatment.medicationNotes || 'No medication notes provided.'}
                    </Paragraph>
                    
                    {selectedTreatment.status === TREATMENT_STATUS.ACTIVE && (
                      <Button 
                        mode="contained" 
                        onPress={showCompleteDialog}
                        style={styles.completeButton}
                      >
                        Mark as Completed
                      </Button>
                    )}
                  </View>
                )}
                
                {selectedTab === 'logs' && (
                  <View style={styles.tabContent}>
                    <Button 
                      mode="contained" 
                      icon="plus"
                      onPress={showLogDialog}
                      style={styles.addButton}
                      disabled={selectedTreatment.status === TREATMENT_STATUS.COMPLETED}
                    >
                      Add Treatment Log
                    </Button>
                    
                    {selectedTreatment.treatmentLogs.length === 0 ? (
                      <Paragraph style={styles.emptyMessage}>
                        No treatment logs recorded yet.
                      </Paragraph>
                    ) : (
                      <FlatList
                        data={selectedTreatment.treatmentLogs.sort((a, b) => new Date(b.date) - new Date(a.date))}
                        keyExtractor={(item, index) => index.toString()}
                        renderItem={({ item }) => (
                          <Card style={styles.logCard}>
                            <Card.Content>
                              <Paragraph style={styles.logDate}>{formatDate(new Date(item.date))}</Paragraph>
                              <Title style={styles.logTitle}>{item.actions}</Title>
                              {item.notes && (
                                <Paragraph style={styles.logNotes}>{item.notes}</Paragraph>
                              )}
                            </Card.Content>
                          </Card>
                        )}
                      />
                    )}
                  </View>
                )}
                
                {selectedTab === 'symptoms' && (
                  <View style={styles.tabContent}>
                    <Button 
                      mode="contained" 
                      icon="plus"
                      onPress={showSymptomDialog}
                      style={styles.addButton}
                      disabled={selectedTreatment.status === TREATMENT_STATUS.COMPLETED}
                    >
                      Add Symptom
                    </Button>
                    
                    {selectedTreatment.symptoms.length === 0 ? (
                      <Paragraph style={styles.emptyMessage}>
                        No symptoms recorded yet.
                      </Paragraph>
                    ) : (
                      <FlatList
                        data={selectedTreatment.symptoms}
                        keyExtractor={(item, index) => index.toString()}
                        renderItem={({ item }) => (
                          <Card style={styles.symptomCard}>
                            <Card.Content>
                              <View style={styles.symptomHeader}>
                                <Title style={styles.symptomName}>{item.name}</Title>
                                <Chip 
                                  style={[styles.severityChip, { backgroundColor: getSeverityColor(item.severity) }]}
                                >
                                  {item.severity}
                                </Chip>
                              </View>
                              {item.notes && (
                                <Paragraph style={styles.symptomNotes}>{item.notes}</Paragraph>
                              )}
                            </Card.Content>
                          </Card>
                        )}
                      />
                    )}
                  </View>
                )}
              </Dialog.Content>
              <Dialog.Actions>
                <Button onPress={hideDetailDialog}>Close</Button>
              </Dialog.Actions>
            </>
          )}
        </Dialog>
      </Portal>
      
      {/* Add Treatment Log Dialog */}
      <Portal>
        <Dialog visible={logDialogVisible} onDismiss={hideLogDialog}>
          <Dialog.Title>Add Treatment Log</Dialog.Title>
          <Dialog.Content>
            <TextInput
              label="Actions Taken"
              value={newLog.actions}
              onChangeText={(text) => setNewLog({...newLog, actions: text})}
              multiline
              numberOfLines={2}
              style={styles.input}
            />
            <TextInput
              label="Notes (optional)"
              value={newLog.notes}
              onChangeText={(text) => setNewLog({...newLog, notes: text})}
              multiline
              numberOfLines={3}
              style={styles.input}
            />
          </Dialog.Content>
          <Dialog.Actions>
            <Button onPress={hideLogDialog}>Cancel</Button>
            <Button 
              onPress={handleAddLog} 
              disabled={!newLog.actions.trim()}
            >
              Add
            </Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>
      
      {/* Add Symptom Dialog */}
      <Portal>
        <Dialog visible={symptomDialogVisible} onDismiss={hideSymptomDialog}>
          <Dialog.Title>Add Symptom</Dialog.Title>
          <Dialog.Content>
            <TextInput
              label="Symptom Name"
              value={newSymptom.name}
              onChangeText={(text) => setNewSymptom({...newSymptom, name: text})}
              style={styles.input}
            />
            
            <Title style={styles.selectLabel}>Severity</Title>
            <ScrollView horizontal style={styles.optionScroll}>
              {Object.values(SYMPTOM_SEVERITY).map((severity, index) => (
                <Chip
                  key={index}
                  selected={newSymptom.severity === severity}
                  onPress={() => setNewSymptom({...newSymptom, severity})}
                  style={[
                    styles.optionChip, 
                    { backgroundColor: newSymptom.severity === severity ? getSeverityColor(severity) : undefined }
                  ]}
                >
                  {severity}
                </Chip>
              ))}
            </ScrollView>
            
            <TextInput
              label="Notes (optional)"
              value={newSymptom.notes}
              onChangeText={(text) => setNewSymptom({...newSymptom, notes: text})}
              multiline
              numberOfLines={2}
              style={styles.input}
            />
          </Dialog.Content>
          <Dialog.Actions>
            <Button onPress={hideSymptomDialog}>Cancel</Button>
            <Button 
              onPress={handleAddSymptom} 
              disabled={!newSymptom.name.trim()}
            >
              Add
            </Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>
      
      {/* Complete Treatment Dialog */}
      <Portal>
        <Dialog visible={completeDialogVisible} onDismiss={hideCompleteDialog}>
          <Dialog.Title>Complete Treatment</Dialog.Title>
          <Dialog.Content>
            <Paragraph style={styles.completeMessage}>
              Are you sure you want to mark this treatment as completed?
            </Paragraph>
            <TextInput
              label="Outcome Notes"
              value={outcomeNotes}
              onChangeText={setOutcomeNotes}
              multiline
              numberOfLines={3}
              style={styles.input}
              placeholder="Describe the final outcome of the treatment"
            />
          </Dialog.Content>
          <Dialog.Actions>
            <Button onPress={hideCompleteDialog}>Cancel</Button>
            <Button 
              onPress={handleCompleteTreatment} 
              disabled={!outcomeNotes.trim()}
            >
              Complete Treatment
            </Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  searchContainer: {
    padding: 16,
    backgroundColor: '#fff',
  },
  searchbar: {
    elevation: 0,
  },
  filterContainer: {
    padding: 8,
    backgroundColor: '#fff',
    borderBottomWidth: 1,
    borderBottomColor: '#e0e0e0',
  },
  filterChip: {
    marginHorizontal: 4,
  },
  listContent: {
    padding: 16,
  },
  card: {
    marginBottom: 16,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  cardTitle: {
    flex: 1,
    marginRight: 8,
  },
  statusChip: {
    height: 24,
  },
  description: {
    marginBottom: 8,
  },
  cardFooter: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  date: {
    fontSize: 12,
    color: '#757575',
  },
  symptomsContainer: {
    marginTop: 4,
  },
  symptomsLabel: {
    fontSize: 12,
    marginBottom: 4,
  },
  symptomChip: {
    marginRight: 4,
    height: 24,
  },
  fab: {
    position: 'absolute',
    margin: 16,
    right: 0,
    bottom: 0,
  },
  dialogContent: {
    paddingVertical: 8,
  },
  input: {
    marginBottom: 16,
  },
  selectLabel: {
    fontSize: 16,
    marginTop: 8,
    marginBottom: 8,
  },
  optionScroll: {
    marginBottom: 16,
  },
  optionChip: {
    marginRight: 8,
  },
  detailDialog: {
    maxHeight: '80%',
  },
  detailStatusRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 16,
  },
  detailStatusChip: {
    marginRight: 8,
  },
  detailDate: {
    fontSize: 12,
    color: '#757575',
  },
  tabButtons: {
    marginBottom: 16,
  },
  tabContent: {
    flex: 1,
  },
  sectionTitle: {
    fontSize: 16,
    marginBottom: 8,
  },
  detailText: {
    fontSize: 14,
    lineHeight: 20,
    marginBottom: 16,
  },
  completeButton: {
    marginTop: 16,
  },
  addButton: {
    marginBottom: 16,
  },
  emptyMessage: {
    textAlign: 'center',
    marginTop: 24,
    color: '#757575',
  },
  logCard: {
    marginBottom: 12,
  },
  logDate: {
    fontSize: 12,
    color: '#757575',
    marginBottom: 4,
  },
  logTitle: {
    fontSize: 14,
    marginBottom: 8,
  },
  logNotes: {
    fontSize: 12,
    fontStyle: 'italic',
  },
  symptomCard: {
    marginBottom: 12,
  },
  symptomHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 8,
  },
  symptomName: {
    fontSize: 16,
  },
  severityChip: {
    height: 24,
  },
  symptomNotes: {
    fontSize: 12,
    fontStyle: 'italic',
  },
  completeMessage: {
    marginBottom: 16,
  }
});

export default TreatmentsScreen;