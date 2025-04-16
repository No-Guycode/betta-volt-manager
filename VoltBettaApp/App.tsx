import React from 'react';
import { SafeAreaView, StyleSheet, StatusBar } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { Provider as PaperProvider, DefaultTheme, Appbar } from 'react-native-paper';

// Import screens
import HomeScreen from './screens/HomeScreen';
import TankLogScreen from './screens/TankLogScreen';
import MaintenanceScreen from './screens/MaintenanceScreen';
import PlantsScreen from './screens/PlantsScreen';
import TreatmentsScreen from './screens/TreatmentsScreen';
import GalleryScreen from './screens/GalleryScreen';
import NotesScreen from './screens/NotesScreen';

// Create the stack navigator
const Stack = createStackNavigator();

// Define the theme for the app
const theme = {
  ...DefaultTheme,
  colors: {
    ...DefaultTheme.colors,
    primary: '#2196F3', // Blue
    accent: '#03A9F4',  // Light Blue
    background: '#f5f5f5',
    surface: '#ffffff',
    text: '#212121',
    error: '#F44336',
    // Add more colors as needed
  },
};

// Custom header component
const CustomNavigationBar = ({ navigation, back, options }: any) => {
  return (
    <Appbar.Header>
      {back ? <Appbar.BackAction onPress={navigation.goBack} /> : null}
      <Appbar.Content title={options.title || 'Volt Betta Manager'} />
      {options.headerRight && options.headerRight()}
    </Appbar.Header>
  );
};

export default function App() {
  return (
    <PaperProvider theme={theme}>
      <SafeAreaView style={styles.container}>
        <StatusBar style="auto" />
        <NavigationContainer>
          <Stack.Navigator
            initialRouteName="Home"
            screenOptions={{
              header: (props) => <CustomNavigationBar {...props} />,
            }}
          >
            <Stack.Screen 
              name="Home" 
              component={HomeScreen} 
              options={{ title: 'Volt Betta Manager' }} 
            />
            <Stack.Screen 
              name="TankLog" 
              component={TankLogScreen} 
              options={{ title: 'Tank Log' }} 
            />
            <Stack.Screen 
              name="Maintenance" 
              component={MaintenanceScreen} 
              options={{ title: 'Maintenance' }} 
            />
            <Stack.Screen 
              name="Plants" 
              component={PlantsScreen} 
              options={{ title: 'Plants' }} 
            />
            <Stack.Screen 
              name="Treatments" 
              component={TreatmentsScreen} 
              options={{ title: 'Treatments' }} 
            />
            <Stack.Screen 
              name="Gallery" 
              component={GalleryScreen} 
              options={{ title: 'Photo Gallery' }} 
            />
            <Stack.Screen 
              name="Notes" 
              component={NotesScreen} 
              options={{ title: 'Notes' }} 
            />
          </Stack.Navigator>
        </NavigationContainer>
      </SafeAreaView>
    </PaperProvider>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
});