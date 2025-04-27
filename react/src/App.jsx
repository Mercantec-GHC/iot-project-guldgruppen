import { useEffect, useState } from 'react';
import './App.css';

function App() {
    const [sensorData, setSensorData] = useState({ temperature: 0, moisture: 0, motionDetected: false });

    useEffect(() => {
    }, []);

    return (
        <div>
            <h1>Room Sensor Data</h1>
            <p>Temperature: {sensorData.temperature} °C</p>
            <p>Moisture: {sensorData.moisture} %</p>
            <p>Motion: {sensorData.motionDetected ? "Detected" : "None"}</p>
        </div>
    );
}

export default App;
