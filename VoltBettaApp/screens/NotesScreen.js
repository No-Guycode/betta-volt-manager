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
  Searchbar,
  Menu,
  IconButton
} from 'react-native-paper';
import { StatusBar } from 'expo-status-bar';

// Sample tag options
const TAG_OPTIONS = [
  'General',
  'Behavior',
  'Health',
  'Tank',
  'Food',
  'Equipment',
  'Ideas'
];

// Sample data - this would normally come from a database
const initialNotes = [
  {
    id: 1,
    title: 'Volt Color Change',
    content: 'Volt\'s blue coloration seems to be intensifying, especially on his fins. The red is also more vibrant when he\'s under the tank light. Likely a sign of good health and diet.',
    createdDateTime: new Date('2025-04-01T10:30:00'),
    modifiedDateTime: new Date('2025-04-01T10:30:00'),
    tags: 'Behavior,Health'
  },
  {
    id: 2,
    title: 'Food Preferences',
    content: 'Volt seems to prefer the New Life Spectrum pellets over the Fluval Bug Bites. He gets excited as soon as I approach the tank with the NLS container. Will continue to use these as his primary food.',
    createdDateTime: new Date('2025-03-25T15:45:00'),
    modifiedDateTime: new Date('2025-03-26T09:15:00'),
    tags: 'Food,Behavior'
  },
  {
    id: 3,
    title: 'Tank Decor Ideas',
    content: 'Thinking about adding a new piece of driftwood and maybe some red plants to complement Volt\'s colors. The Anubias is doing well, but some contrast would look nice. Consider getting some Alternanthera reineckii or Ludwigia.',
    createdDateTime: new Date('2025-03-15T18:20:00'),
    modifiedDateTime: new Date('2025-03-15T18:20:00'),
    tags: 'Tank,Ideas'
  },
  {
    id: 4,
    title: 'New Heater Considerations',
    content: 'The current heater seems to fluctuate a bit too much. Temperature varies between 77-82°F, which is a wider range than ideal. Researching more reliable options - the Eheim Jäger or Fluval E-Series look promising. Need to get a 50W for my 5 gallon tank.',
    createdDateTime: new Date('2025-03-10T12:10:00'),
    modifiedDateTime: new Date('2025-03-12T14:30:00'),
    tags: 'Equipment,Ideas'
  }
];

const NotesScreen = () => {
  const [notes, setNotes] = useState(initialNotes);
  const [visible, setVisible] = useState(false);
  const [selectedNote, setSelectedNote] = useState(null);
  const [editMode, setEditMode] = useState(false);
  const [newNote, setNewNote] = useState({
    title: '',
    content: '',
    tags: ''
  });
  const [editedNote, setEditedNote] = useState(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTag, setSelectedTag] = useState(null);
  const [menuVisible, setMenuVisible] = useState(false);
  const [confirmVisible, setConfirmVisible] = useState(false);

  const showDialog = () => setVisible(true);
  const hideDialog = () => setVisible(false);
  
  const showDeleteConfirm = () => setConfirmVisible(true);
  const hideDeleteConfirm = () => setConfirmVisible(false);
  
  const openMenu = () => setMenuVisible(true);
  const closeMenu = () => setMenuVisible(false);

  // Get all unique tags from notes
  const getAllTags = () => {
    const tagSet = new Set();
    notes.forEach(note => {
      if (note.tags) {
        note.tags.split(',').forEach(tag => {
          tagSet.add(tag.trim());
        });
      }
    });
    return Array.from(tagSet).sort();
  };

  // Filter notes based on search query and selected tag
  const filteredNotes = notes.filter(note => {
    const matchesSearch = searchQuery === '' || 
      note.title.toLowerCase().includes(searchQuery.toLowerCase()) ||
      note.content.toLowerCase().includes(searchQuery.toLowerCase()) ||
      note.tags.toLowerCase().includes(searchQuery.toLowerCase());
    
    const matchesTag = selectedTag === null || 
      (note.tags && note.tags.split(',').map(tag => tag.trim()).includes(selectedTag));
    
    return matchesSearch && matchesTag;
  }).sort((a, b) => b.modifiedDateTime - a.modifiedDateTime);

  const handleAddNote = () => {
    if (newNote.title.trim() === '' || newNote.content.trim() === '') {
      return;
    }
    
    const now = new Date();
    const note = {
      id: notes.length + 1,
      ...newNote,
      createdDateTime: now,
      modifiedDateTime: now
    };
    
    setNotes([...notes, note]);
    setNewNote({
      title: '',
      content: '',
      tags: ''
    });
    hideDialog();
  };
  
  const handleEditNote = () => {
    if (!selectedNote || !editedNote || editedNote.title.trim() === '') {
      return;
    }
    
    const updatedNotes = notes.map(note => {
      if (note.id === selectedNote.id) {
        return {
          ...editedNote,
          modifiedDateTime: new Date()
        };
      }
      return note;
    });
    
    setNotes(updatedNotes);
    setSelectedNote({
      ...editedNote,
      modifiedDateTime: new Date()
    });
    setEditMode(false);
  };
  
  const handleDeleteNote = () => {
    if (!selectedNote) return;
    
    const updatedNotes = notes.filter(note => note.id !== selectedNote.id);
    setNotes(updatedNotes);
    setSelectedNote(null);
    hideDeleteConfirm();
  };
  
  const formatDateTime = (date) => {
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };
  
  const selectNote = (note) => {
    setSelectedNote(note);
    setEditedNote({...note});
    setEditMode(false);
  };

  return (
    <View style={styles.container}>
      <StatusBar style="auto" />
      
      <View style={styles.searchContainer}>
        <Searchbar
          placeholder="Search notes..."
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
        />
      </View>
      
      <ScrollView horizontal style={styles.filterContainer}>
        <Chip 
          selected={selectedTag === null}
          onPress={() => setSelectedTag(null)}
          style={styles.filterChip}
        >
          All Tags
        </Chip>
        {getAllTags().map((tag, index) => (
          <Chip
            key={index}
            selected={selectedTag === tag}
            onPress={() => setSelectedTag(tag)}
            style={styles.filterChip}
          >
            {tag}
          </Chip>
        ))}
      </ScrollView>
      
      <View style={styles.contentContainer}>
        {/* Note list */}
        <View style={styles.listContainer}>
          <FlatList
            data={filteredNotes}
            keyExtractor={(item) => item.id.toString()}
            renderItem={({ item }) => (
              <Card 
                style={[styles.noteCard, selectedNote?.id === item.id && styles.selectedNoteCard]} 
                onPress={() => selectNote(item)}
              >
                <Card.Content>
                  <Title style={styles.noteTitle} numberOfLines={1}>{item.title}</Title>
                  <Paragraph style={styles.notePreview} numberOfLines={2}>{item.content}</Paragraph>
                  <View style={styles.noteFooter}>
                    <Paragraph style={styles.noteDate}>{formatDateTime(item.modifiedDateTime)}</Paragraph>
                    <View style={styles.tagContainer}>
                      {item.tags && item.tags.split(',').map((tag, index) => (
                        <Chip 
                          key={index} 
                          style={styles.tagChip}
                          textStyle={styles.tagChipText}
                        >
                          {tag.trim()}
                        </Chip>
                      ))}
                    </View>
                  </View>
                </Card.Content>
              </Card>
            )}
            contentContainerStyle={styles.notesListContent}
          />
        </View>
        
        {/* Note detail view */}
        <View style={styles.detailContainer}>
          {selectedNote ? (
            <Card style={styles.detailCard}>
              <Card.Content>
                <View style={styles.detailHeader}>
                  {editMode ? (
                    <TextInput
                      mode="outlined"
                      label="Title"
                      value={editedNote.title}
                      onChangeText={(text) => setEditedNote({...editedNote, title: text})}
                      style={styles.titleInput}
                    />
                  ) : (
                    <Title style={styles.detailTitle}>{selectedNote.title}</Title>
                  )}
                  
                  <Menu
                    visible={menuVisible}
                    onDismiss={closeMenu}
                    anchor={
                      <IconButton
                        icon="dots-vertical"
                        onPress={openMenu}
                      />
                    }
                  >
                    {!editMode && <Menu.Item onPress={() => {
                      setEditMode(true);
                      closeMenu();
                    }} title="Edit" />}
                    {editMode && <Menu.Item onPress={() => {
                      handleEditNote();
                      closeMenu();
                    }} title="Save" />}
                    {editMode && <Menu.Item onPress={() => {
                      setEditMode(false);
                      setEditedNote({...selectedNote});
                      closeMenu();
                    }} title="Cancel" />}
                    <Menu.Item 
                      onPress={() => {
                        showDeleteConfirm();
                        closeMenu();
                      }} 
                      title="Delete" 
                      titleStyle={{color: '#F44336'}}
                    />
                  </Menu>
                </View>
                
                <View style={styles.metaContainer}>
                  <Paragraph style={styles.dateInfo}>
                    Created: {formatDateTime(selectedNote.createdDateTime)}
                    {selectedNote.modifiedDateTime > selectedNote.createdDateTime && 
                      ` • Modified: ${formatDateTime(selectedNote.modifiedDateTime)}`}
                  </Paragraph>
                  
                  {editMode ? (
                    <TextInput
                      mode="outlined"
                      label="Tags (comma-separated)"
                      value={editedNote.tags}
                      onChangeText={(text) => setEditedNote({...editedNote, tags: text})}
                      style={styles.tagsInput}
                    />
                  ) : (
                    <View style={styles.detailTagContainer}>
                      {selectedNote.tags && selectedNote.tags.split(',').map((tag, index) => (
                        <Chip 
                          key={index} 
                          style={styles.detailTagChip}
                        >
                          {tag.trim()}
                        </Chip>
                      ))}
                    </View>
                  )}
                </View>
                
                <Divider style={styles.divider} />
                
                {editMode ? (
                  <TextInput
                    mode="outlined"
                    label="Content"
                    value={editedNote.content}
                    onChangeText={(text) => setEditedNote({...editedNote, content: text})}
                    multiline
                    numberOfLines={12}
                    style={styles.contentInput}
                  />
                ) : (
                  <ScrollView style={styles.contentScroll}>
                    <Paragraph style={styles.noteContent}>{selectedNote.content}</Paragraph>
                  </ScrollView>
                )}
                
                {editMode && (
                  <View style={styles.editButtonContainer}>
                    <Button 
                      mode="outlined" 
                      onPress={() => {
                        setEditMode(false);
                        setEditedNote({...selectedNote});
                      }}
                      style={styles.editButton}
                    >
                      Cancel
                    </Button>
                    <Button 
                      mode="contained" 
                      onPress={handleEditNote}
                      style={styles.editButton}
                    >
                      Save Changes
                    </Button>
                  </View>
                )}
              </Card.Content>
            </Card>
          ) : (
            <Card style={styles.emptyDetailCard}>
              <Card.Content style={styles.emptyDetailContent}>
                <Title style={styles.emptyDetailTitle}>No Note Selected</Title>
                <Paragraph style={styles.emptyDetailText}>
                  Select a note from the list or create a new one to view its details here.
                </Paragraph>
              </Card.Content>
            </Card>
          )}
        </View>
      </View>
      
      <FAB
        style={styles.fab}
        icon="plus"
        onPress={showDialog}
        label="Add Note"
      />
      
      {/* Add New Note Dialog */}
      <Portal>
        <Dialog visible={visible} onDismiss={hideDialog}>
          <Dialog.Title>Add New Note</Dialog.Title>
          <Dialog.ScrollArea>
            <ScrollView>
              <View style={styles.dialogContent}>
                <TextInput
                  label="Title"
                  value={newNote.title}
                  onChangeText={(text) => setNewNote({...newNote, title: text})}
                  style={styles.input}
                />
                
                <TextInput
                  label="Content"
                  value={newNote.content}
                  onChangeText={(text) => setNewNote({...newNote, content: text})}
                  multiline
                  numberOfLines={6}
                  style={styles.input}
                />
                
                <TextInput
                  label="Tags (comma-separated)"
                  value={newNote.tags}
                  onChangeText={(text) => setNewNote({...newNote, tags: text})}
                  placeholder="e.g. Behavior,Food,Health"
                  style={styles.input}
                />
                
                <Title style={styles.suggestedLabel}>Suggested Tags:</Title>
                <ScrollView horizontal style={styles.tagScroll}>
                  {TAG_OPTIONS.map((tag, index) => (
                    <Chip
                      key={index}
                      onPress={() => {
                        const currentTags = newNote.tags ? newNote.tags.split(',').map(t => t.trim()) : [];
                        if (!currentTags.includes(tag)) {
                          const updatedTags = [...currentTags, tag].filter(t => t !== '').join(', ');
                          setNewNote({...newNote, tags: updatedTags});
                        }
                      }}
                      style={styles.suggestedTagChip}
                    >
                      {tag}
                    </Chip>
                  ))}
                </ScrollView>
              </View>
            </ScrollView>
          </Dialog.ScrollArea>
          <Dialog.Actions>
            <Button onPress={hideDialog}>Cancel</Button>
            <Button 
              onPress={handleAddNote} 
              disabled={!newNote.title.trim() || !newNote.content.trim()}
            >
              Add
            </Button>
          </Dialog.Actions>
        </Dialog>
      </Portal>
      
      {/* Delete Confirmation Dialog */}
      <Portal>
        <Dialog visible={confirmVisible} onDismiss={hideDeleteConfirm}>
          <Dialog.Title>Delete Note</Dialog.Title>
          <Dialog.Content>
            <Paragraph>
              Are you sure you want to delete "{selectedNote?.title}"? This action cannot be undone.
            </Paragraph>
          </Dialog.Content>
          <Dialog.Actions>
            <Button onPress={hideDeleteConfirm}>Cancel</Button>
            <Button 
              onPress={handleDeleteNote} 
              textColor="#F44336"
            >
              Delete
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
  contentContainer: {
    flex: 1,
    flexDirection: 'row',
    padding: 8,
  },
  listContainer: {
    flex: 1,
    marginRight: 8,
  },
  detailContainer: {
    flex: 2,
    marginLeft: 8,
  },
  notesListContent: {
    paddingVertical: 8,
  },
  noteCard: {
    marginBottom: 12,
  },
  selectedNoteCard: {
    borderLeftWidth: 4,
    borderLeftColor: '#2196F3',
  },
  noteTitle: {
    fontSize: 16,
    marginBottom: 4,
  },
  notePreview: {
    fontSize: 14,
    color: '#757575',
    marginBottom: 8,
  },
  noteFooter: {
    flexDirection: 'column',
    alignItems: 'flex-start',
  },
  noteDate: {
    fontSize: 12,
    color: '#757575',
    marginBottom: 4,
  },
  tagContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
  },
  tagChip: {
    marginRight: 4,
    marginBottom: 4,
    height: 24,
  },
  tagChipText: {
    fontSize: 10,
  },
  detailCard: {
    flex: 1,
  },
  emptyDetailCard: {
    flex: 1,
    justifyContent: 'center',
  },
  emptyDetailContent: {
    alignItems: 'center',
    justifyContent: 'center',
    padding: 24,
  },
  emptyDetailTitle: {
    marginBottom: 8,
  },
  emptyDetailText: {
    textAlign: 'center',
    color: '#757575',
  },
  detailHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  detailTitle: {
    flex: 1,
    fontSize: 20,
    marginBottom: 8,
  },
  metaContainer: {
    marginBottom: 16,
  },
  dateInfo: {
    fontSize: 12,
    color: '#757575',
    marginBottom: 8,
  },
  detailTagContainer: {
    flexDirection: 'row',
    flexWrap: 'wrap',
  },
  detailTagChip: {
    marginRight: 6,
    marginBottom: 6,
  },
  divider: {
    marginBottom: 16,
  },
  contentScroll: {
    maxHeight: 300,
  },
  noteContent: {
    fontSize: 16,
    lineHeight: 24,
  },
  titleInput: {
    flex: 1,
    marginRight: 8,
    marginBottom: 8,
  },
  tagsInput: {
    marginBottom: 8,
  },
  contentInput: {
    marginBottom: 16,
  },
  editButtonContainer: {
    flexDirection: 'row',
    justifyContent: 'flex-end',
  },
  editButton: {
    marginLeft: 8,
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
  suggestedLabel: {
    fontSize: 14,
    marginBottom: 8,
  },
  tagScroll: {
    marginBottom: 16,
  },
  suggestedTagChip: {
    marginRight: 8,
  }
});

export default NotesScreen;