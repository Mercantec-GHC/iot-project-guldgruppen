import { useEffect, useState } from 'react';
import GaugeComponent from 'react-gauge-component';
import './App.css';

function App() {
    const [sensorData, setSensorData] = useState({ temperature: 0, moistureLevel: 0, motionDetected: false });

    useEffect(() => {
        const fetchSensorData = async () => {
            try {
                const response = await fetch('https://localhost:44393/api/Sensor');
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                const data = await response.json();
                if (Array.isArray(data) && data.length > 0) {
                    setSensorData(data[0]);
                }
            } catch (error) {
                console.error('Error fetching sensor data', error);
            }
        };

        fetchSensorData();
        const interval = setInterval(fetchSensorData, 1000);

        return () => clearInterval(interval);
    }, []);

    return (
        <div>
            <h1>Room Sensor Data</h1>
            <div className="sensor-gauges">

                <div className="temperature-gauge">
                    <GaugeComponent
                        type="radial"
                        arc={{
                            width: 0.2,
                            padding: 0.005,
                            cornerRadius: 1,
                            // gradient: true,
                            subArcs: [
                                {
                                    limit: 15,
                                    color: '#2BCABB',
                                    showTick: true,
                                    tooltip: {
                                        text: 'Too Low Temperature!'
                                    }
                                },
                                {
                                    limit: 17,
                                    color: '#2BCA8B',
                                    showTick: true,
                                    tooltip: {
                                        text: 'Low temperature!'
                                    }
                                },
                                {
                                    limit: 28,
                                    color: '#5BE12C',
                                    showTick: true,
                                    tooltip: {
                                        text: 'Optimal Temperature!'
                                    }
                                },
                                {
                                    limit: 30, color: '#F5CD19', showTick: true,
                                    tooltip: {
                                        text: 'High Temperature!'
                                    }
                                },
                                {
                                    color: '#EA4228',
                                    tooltip: {
                                        text: 'Too High Temperature!'
                                    }
                                }
                            ]
                        }}
                        pointer={{
                            color: '#345243',
                            length: 0.80,
                            width: 15,
                            // elastic: true,
                        }}
                        labels={{
                            valueLabel: { formatTextValue: value => value + 'ºC' },
                            tickLabels: {
                                type: 'outer',
                                defaultTickValueConfig: {
                                    formatTextValue: (value) => value + 'ºC' ,
                                    style: {fontSize: 10}
                                },
                                ticks: [
                                    { value: 13 },
                                    { value: 22.5 },
                                    { value: 32 }
                                ],
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
                                {
                                    limit: 20,
                                    color: '#EA4228',
                                    showTick: true,
                                    tooltip: {
                                        text: 'Too Low Moisture!'
                                    }
                                },
                                {
                                    limit: 40,
                                    color: '#F5CD19',
                                    showTick: true,
                                    tooltip: {
                                        text: 'Low Moisture!'
                                    }
                                },
                                {
                                    limit: 60,
                                    color: '#5BE12C',
                                    showTick: true,
                                    tooltip: {
                                        text: 'Optimal Moisture!'
                                    }
                                },
                                {
                                    limit: 80,
                                    color: '#1e9bef',
                                    showTick: true,
                                    tooltip: {
                                        text: 'High Moisture!'
                                    }
                                },
                                {
                                    limit: 100,
                                    color: '#154ad0',
                                    showTick: true,
                                    tooltip: {
                                        text: 'Too High Moisture!'
                                    }
                                },
                            ]
                        }}
                        pointer={{
                            color: '#345243',
                            length: 0.80,
                            width: 15,
                            // elastic: true,
                        }}
                        value={sensorData.moistureLevel}
                    />
                </div>

            </div>
            <p>Motion: {sensorData.motionDetected ? 'Yes' : 'No'}</p>
        </div>
    );
}

export default App;
