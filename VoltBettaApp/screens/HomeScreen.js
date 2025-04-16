import React from 'react';
import { StyleSheet, View, ScrollView, Image } from 'react-native';
import { Card, Title, Paragraph, Button, Text, Avatar, Divider } from 'react-native-paper';
import { StatusBar } from 'expo-status-bar';

const HomeScreen = ({ navigation }) => {
  // Sample betta fish data
  const fishData = {
    name: 'Volt',
    species: 'Betta splendens',
    variant: 'Halfmoon',
    color: 'Blue/Red',
    age: '~1 year 3 months',
    tank: '5 gallon planted',
    acquisitionDate: 'January 15, 2024'
  };

  // Menu options for main features
  const menuOptions = [
    {
      title: 'Tank Log',
      icon: 'water',
      description: 'Record water parameters and tank maintenance',
      screen: 'TankLog'
    },
    {
      title: 'Maintenance',
      icon: 'calendar-check',
      description: 'Schedule and track tank maintenance tasks',
      screen: 'Maintenance'
    },
    {
      title: 'Plants',
      icon: 'leaf',
      description: 'Manage aquarium plants and care information',
      screen: 'Plants'
    },
    {
      title: 'Treatments',
      icon: 'medical-bag',
      description: 'Track medications and health treatments',
      screen: 'Treatments'
    },
    {
      title: 'Gallery',
      icon: 'image-multiple',
      description: 'Store and organize photos of your betta',
      screen: 'Gallery'
    },
    {
      title: 'Notes',
      icon: 'note-text',
      description: 'Keep journal entries and observations',
      screen: 'Notes'
    }
  ];

  return (
    <View style={styles.container}>
      <StatusBar style="auto" />
      <ScrollView>
        {/* Betta Profile Card */}
        <Card style={styles.profileCard}>
          <Card.Content>
            <View style={styles.profileHeader}>
              <Avatar.Icon size={60} icon="fish" style={styles.fishIcon} />
              <View style={styles.profileInfo}>
                <Title style={styles.fishName}>{fishData.name}</Title>
                <Paragraph style={styles.fishSpecies}>{fishData.species}</Paragraph>
              </View>
            </View>
            
            <Divider style={styles.divider} />
            
            <View style={styles.statsContainer}>
              <View style={styles.statItem}>
                <Text style={styles.statLabel}>Variant</Text>
                <Text style={styles.statValue}>{fishData.variant}</Text>
              </View>
              <View style={styles.statItem}>
                <Text style={styles.statLabel}>Color</Text>
                <Text style={styles.statValue}>{fishData.color}</Text>
              </View>
              <View style={styles.statItem}>
                <Text style={styles.statLabel}>Age</Text>
                <Text style={styles.statValue}>{fishData.age}</Text>
              </View>
            </View>
            
            <View style={styles.statsContainer}>
              <View style={styles.statItem}>
                <Text style={styles.statLabel}>Tank Size</Text>
                <Text style={styles.statValue}>{fishData.tank}</Text>
              </View>
              <View style={styles.statItem}>
                <Text style={styles.statLabel}>Acquired</Text>
                <Text style={styles.statValue}>{fishData.acquisitionDate}</Text>
              </View>
            </View>
          </Card.Content>
        </Card>
        
        {/* Feature Menu Cards */}
        <View style={styles.menuGrid}>
          {menuOptions.map((option, index) => (
            <Card 
              key={index}
              style={styles.menuCard}
              onPress={() => navigation.navigate(option.screen)}
            >
              <Card.Content style={styles.menuCardContent}>
                <Avatar.Icon size={48} icon={option.icon} style={styles.menuIcon} />
                <Title style={styles.menuTitle}>{option.title}</Title>
                <Paragraph style={styles.menuDescription} numberOfLines={2}>
                  {option.description}
                </Paragraph>
              </Card.Content>
            </Card>
          ))}
        </View>
        
        {/* Quick Stats Card */}
        <Card style={styles.statsCard}>
          <Card.Title title="Quick Stats" subtitle="Recent activity" />
          <Card.Content>
            <View style={styles.quickStatsContainer}>
              <View style={styles.quickStatItem}>
                <Avatar.Icon size={36} icon="water-percent" style={styles.quickStatIcon} />
                <View>
                  <Text style={styles.quickStatLabel}>Last Water Change</Text>
                  <Text style={styles.quickStatValue}>4 days ago</Text>
                </View>
              </View>
              
              <View style={styles.quickStatItem}>
                <Avatar.Icon size={36} icon="test-tube" style={styles.quickStatIcon} />
                <View>
                  <Text style={styles.quickStatLabel}>Last Water Test</Text>
                  <Text style={styles.quickStatValue}>1 week ago</Text>
                </View>
              </View>
              
              <View style={styles.quickStatItem}>
                <Avatar.Icon size={36} icon="image" style={styles.quickStatIcon} />
                <View>
                  <Text style={styles.quickStatLabel}>Photos Taken</Text>
                  <Text style={styles.quickStatValue}>28 total</Text>
                </View>
              </View>
            </View>
          </Card.Content>
          <Card.Actions>
            <Button onPress={() => navigation.navigate('TankLog')}>View Tank Log</Button>
          </Card.Actions>
        </Card>
        
        {/* Upcoming Maintenance Card */}
        <Card style={styles.maintenanceCard}>
          <Card.Title title="Upcoming Maintenance" />
          <Card.Content>
            <View style={styles.maintenanceItem}>
              <Avatar.Icon size={36} icon="water" style={styles.maintenanceIcon} />
              <View style={styles.maintenanceInfo}>
                <Text style={styles.maintenanceTitle}>Water Change (25%)</Text>
                <Text style={styles.maintenanceDate}>Due in 3 days</Text>
              </View>
            </View>
            
            <View style={styles.maintenanceItem}>
              <Avatar.Icon size={36} icon="filter" style={styles.maintenanceIcon} />
              <View style={styles.maintenanceInfo}>
                <Text style={styles.maintenanceTitle}>Replace Filter Media</Text>
                <Text style={styles.maintenanceDate}>Due in 1 week</Text>
              </View>
            </View>
          </Card.Content>
          <Card.Actions>
            <Button onPress={() => navigation.navigate('Maintenance')}>View All Tasks</Button>
          </Card.Actions>
        </Card>
        
        {/* About the App Card */}
        <Card style={styles.aboutCard}>
          <Card.Title title="About This App" />
          <Card.Content>
            <Paragraph>
              Volt Betta Manager is designed to help you track and manage all aspects of care for your betta fish.
              Record water parameters, maintenance schedules, plant care information, health treatments, and more.
            </Paragraph>
          </Card.Content>
        </Card>
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#f5f5f5',
  },
  profileCard: {
    margin: 16,
    elevation: 4,
  },
  profileHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 16,
  },
  fishIcon: {
    backgroundColor: '#2196F3',
  },
  profileInfo: {
    marginLeft: 16,
  },
  fishName: {
    fontSize: 24,
    fontWeight: 'bold',
  },
  fishSpecies: {
    fontSize: 16,
    color: '#757575',
  },
  divider: {
    marginBottom: 16,
  },
  statsContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    marginBottom: 8,
  },
  statItem: {
    flex: 1,
  },
  statLabel: {
    fontSize: 12,
    color: '#757575',
  },
  statValue: {
    fontSize: 16,
  },
  menuGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
    paddingHorizontal: 12,
  },
  menuCard: {
    width: '47%',
    marginHorizontal: 4,
    marginBottom: 16,
    elevation: 2,
  },
  menuCardContent: {
    alignItems: 'center',
    padding: 8,
  },
  menuIcon: {
    marginBottom: 8,
    backgroundColor: '#03A9F4',
  },
  menuTitle: {
    fontSize: 16,
    marginBottom: 4,
    textAlign: 'center',
  },
  menuDescription: {
    fontSize: 12,
    textAlign: 'center',
    color: '#757575',
  },
  statsCard: {
    margin: 16,
    marginBottom: 12,
    elevation: 3,
  },
  quickStatsContainer: {
    marginBottom: 8,
  },
  quickStatItem: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 12,
  },
  quickStatIcon: {
    backgroundColor: '#3F51B5',
    marginRight: 12,
  },
  quickStatLabel: {
    fontSize: 14,
    color: '#757575',
  },
  quickStatValue: {
    fontSize: 16,
  },
  maintenanceCard: {
    margin: 16,
    marginTop: 4,
    marginBottom: 12,
    elevation: 3,
  },
  maintenanceItem: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 12,
  },
  maintenanceIcon: {
    backgroundColor: '#4CAF50',
    marginRight: 12,
  },
  maintenanceInfo: {
    flex: 1,
  },
  maintenanceTitle: {
    fontSize: 16,
  },
  maintenanceDate: {
    fontSize: 14,
    color: '#757575',
  },
  aboutCard: {
    margin: 16,
    marginTop: 4,
    marginBottom: 24,
    elevation: 2,
  },
});

export default HomeScreen;