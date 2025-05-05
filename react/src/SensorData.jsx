import { useEffect, useState } from 'react';
import GaugeComponent from 'react-gauge-component';
import './SensorData.css';


function SensorData() {
    const [sensorData, setSensorData] = useState({ temperature: 0, moistureLevel: 0, motionDetected: false });
    const [arduinoId, setArduinoId] = useState(null);

    useEffect(() => {
        const fetchUserAndSensorData = async () => {
            try {
                // Fetch the user ID from the Auth endpoint
                const userIdRes = await fetch('http://176.9.37.136:5001/api/Auth/userid', {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${localStorage.getItem('token')}`, // Include the token in the Authorization header
                    },
                });
                if (!userIdRes.ok) throw new Error('Failed to fetch user ID');
                const { UserId } = await userIdRes.json();

                // Fetch user details using the retrieved user ID
                const userRes = await fetch(`http://176.9.37.136:5001/api/Users/${UserId}`);
                if (!userRes.ok) throw new Error('Failed to fetch user');
                const userData = await userRes.json();

                const userArduinoId = userData.arduinoId;
                setArduinoId(userArduinoId);

                // Fetch sensor data using the Arduino ID
                const sensorRes = await fetch(`http://176.9.37.136:5001/api/Sensor/${userArduinoId}`);
                if (!sensorRes.ok) throw new Error('Failed to fetch sensor data');
                const sensorArray = await sensorRes.json();

                if (Array.isArray(sensorArray) && sensorArray.length > 0) {
                    setSensorData(sensorArray[0]);
                }
            } catch (error) {
                console.error('Error in fetching process:', error);
            }
        };

        fetchUserAndSensorData();
        const interval = setInterval(fetchUserAndSensorData, 1000);

        return () => clearInterval(interval);
    }, []);

    return (
        <div>
            <h1>Room Sensor Data</h1>
            {arduinoId ? (
                <p>Fetching data for Arduino ID: <strong>{arduinoId}</strong></p>
            ) : (
                <p>Loading user and sensor data...</p>
            )}
            <div className="sensor-gauges">
                <div className="temperature-gauge">
                    <GaugeComponent
                        type="radial"
                        arc={{
                            width: 0.2,
                            padding: 0.005,
                            cornerRadius: 1,
                            subArcs: [
                                { limit: 15, color: '#2BCABB', showTick: true, tooltip: { text: 'Too Low Temperature!' } },
                                { limit: 17, color: '#2BCA8B', showTick: true, tooltip: { text: 'Low Temperature!' } },
                                { limit: 28, color: '#5BE12C', showTick: true, tooltip: { text: 'Optimal Temperature!' } },
                                { limit: 30, color: '#F5CD19', showTick: true, tooltip: { text: 'High Temperature!' } },
                                { color: '#EA4228', tooltip: { text: 'Too High Temperature!' } }
                            ]
                        }}
                        pointer={{ color: '#345243', length: 0.80, width: 15 }}
                        labels={{
                            valueLabel: { formatTextValue: value => value + 'ºC' },
                            tickLabels: {
                                type: 'outer',
                                defaultTickValueConfig: {
                                    formatTextValue: (value) => value + 'ºC',
                                    style: { fontSize: 10 }
                                },
                                ticks: [{ value: 13 }, { value: 22.5 }, { value: 32 }]
                            }
                        }}
                        value={sensorData.temperature}
                        minValue={10}
                        maxValue={35}
                    />
                </div>
                <div className="moisture-gauge">
                    <GaugeComponent
                        type="radial"
                        arc={{
                            width: 0.2,
                            padding: 0.005,
                            cornerRadius: 1,
                            subArcs: [
                                { limit: 20, color: '#EA4228', showTick: true, tooltip: { text: 'Too Low Moisture!' } },
                                { limit: 40, color: '#F5CD19', showTick: true, tooltip: { text: 'Low Moisture!' } },
                                { limit: 60, color: '#5BE12C', showTick: true, tooltip: { text: 'Optimal Moisture!' } },
                                { limit: 80, color: '#1e9bef', showTick: true, tooltip: { text: 'High Moisture!' } },
                                { limit: 100, color: '#154ad0', showTick: true, tooltip: { text: 'Too High Moisture!' } }
                            ]
                        }}
                        pointer={{ color: '#345243', length: 0.80, width: 15 }}
                        value={sensorData.moistureLevel}
                    />
                </div>
            </div>
            <p>Motion: {sensorData.motionDetected ? 'Yes' : 'No'}</p>
        </div>
    );
}

export default SensorData;