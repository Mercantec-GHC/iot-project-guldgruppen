import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Login.css';

function Settings() {
    const [newEmail, setNewEmail] = useState('');
    const [currentPassword, setCurrentPassword] = useState('');
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [userEmail, setUserEmail] = useState('');
    const [userName, setUserName] = useState('');
    const [userId, setUserId] = useState('');
    const [sensorData, setSensorData] = useState(null);
    const [emailSending, setEmailSending] = useState(false);
    const [emailMessage, setEmailMessage] = useState('');
    const [emailError, setEmailError] = useState('');
    const [humidityThreshold, setHumidityThreshold] = useState(80);
    const [alertsEnabled, setAlertsEnabled] = useState(false);
    const [alertMessage, setAlertMessage] = useState('');
    const [alertError, setAlertError] = useState('');
    const [motionAlertEnabled, setMotionAlertEnabled] = useState(false);
    const [motionAlertMessage, setMotionAlertMessage] = useState('');
    const [motionAlertError, setMotionAlertError] = useState('');
    const [temperatureThreshold, setTemperatureThreshold] = useState(40);
    const [temperatureAlertsEnabled, setTemperatureAlertsEnabled] = useState(false);
    const [temperatureAlertMessage, setTemperatureAlertMessage] = useState('');
    const [temperatureAlertError, setTemperatureAlertError] = useState('');
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setMessage('');
        setError('');
        setIsLoading(true);

        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('You are not logged in');
            }

            const response = await fetch('http://localhost:5001/api/Auth/update-email', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ newEmail, currentPassword }),
            });

            const data = await response.json();

            if (response.ok) {
                setMessage('Email updated successfully!');
                setNewEmail('');
                setCurrentPassword('');
            } else {
                throw new Error(data.message || 'Failed to update email');
            }
        } catch (err) {
            setError(err.message || 'An error occurred while updating your email');
            console.error('Email update error:', err);
        } finally {
            setIsLoading(false);
        }
    };

    const handleBack = () => {
        navigate('/');
    };

    const handleSendSensorData = async () => {
        setEmailSending(true);
        setEmailMessage('');
        setEmailError('');

        try {
            if (!sensorData) {
                throw new Error('No sensor data available');
            }

            if (!userEmail) {
                throw new Error('User email not available');
            }

            // Formater sensor data til email body
            const emailBody = `
Latest Sensor Data:
------------------
Temperature: ${sensorData.temperature}°C
Humidity Level: ${sensorData.humidityLevel}%
Motion Detected: ${sensorData.motionDetected ? 'Yes' : 'No'}
Timestamp: ${new Date(sensorData.timestamp).toLocaleString()}
            `.trim();

            // Send email via API endpoint
            const response = await fetch('http://localhost:5001/Mail', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'accept': 'text/plain'
                },
                body: JSON.stringify({
                    emailToId: userEmail,
                    emailToName: userName,
                    emailSubject: 'Your Latest Sensor Data',
                    emailBody: emailBody
                })
            });

            if (response.ok) {
                setEmailMessage('Sensor data sent to your email successfully!');
            } else {
                const errorData = await response.text();
                throw new Error(errorData || 'Failed to send email');
            }
        } catch (err) {
            setEmailError(err.message || 'An error occurred while sending the email');
            console.error('Email sending error:', err);
        } finally {
            setEmailSending(false);
        }
    };

    // Funktion til at håndtere at gemme alert settings
    const handleSaveAlertSettings = async () => {
        setAlertMessage('');
        setAlertError('');

        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('You are not logged in');
            }

            if (!userId) {
                throw new Error('User ID not available');
            }

            const response = await fetch(`http://localhost:5001/api/Users/${userId}/set-humidity-alerts`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    sendHumidityAlert: alertsEnabled,
                    humidityThreshold: humidityThreshold
                }),
            });

            if (response.ok) {
                setAlertMessage('Alert settings saved successfully!');
            } else {
                const errorData = await response.text();
                throw new Error(errorData || 'Failed to update humidity alert settings');
            }
        } catch (error) {
            setAlertError(error.message || 'Failed to save alert settings');
            console.error('Error saving alert settings:', error);
        }
    };

    // Funktion til at håndtere at gemme temperature alert settings
    const handleSaveTemperatureAlertSettings = async () => {
        setTemperatureAlertMessage('');
        setTemperatureAlertError('');

        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('You are not logged in');
            }

            if (!userId) {
                throw new Error('User ID not available');
            }

            const response = await fetch(`http://localhost:5001/api/Users/${userId}/set-temperature-alerts`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    sendTemperatureAlert: temperatureAlertsEnabled,
                    temperatureThreshold: temperatureThreshold
                }),
            });

            if (response.ok) {
                setTemperatureAlertMessage('Temperature alert settings saved successfully!');
            } else {
                const errorData = await response.text();
                throw new Error(errorData || 'Failed to update temperature alert settings');
            }
        } catch (error) {
            setTemperatureAlertError(error.message || 'Failed to save temperature alert settings');
            console.error('Error saving temperature alert settings:', error);
        }
    };

    // Funktion til at håndtere toggle motion detect email alerts
    const handleToggleMotionAlert = async (enabled) => {
        setMotionAlertMessage('');
        setMotionAlertError('');

        try {
            const token = localStorage.getItem('token');
            if (!token) {
                throw new Error('You are not logged in');
            }

            if (!userId) {
                throw new Error('User ID not available');
            }

            const response = await fetch(`http://localhost:5001/api/Users/${userId}/set-alerts`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ sendEmailAlert: enabled }),
            });

            if (response.ok) {
                setMotionAlertEnabled(enabled);
                setMotionAlertMessage(`Motion detection email alerts ${enabled ? 'enabled' : 'disabled'} successfully!`);
            } else {
                const errorData = await response.text();
                throw new Error(errorData || 'Failed to update motion alert settings');
            }
        } catch (err) {
            setMotionAlertError(err.message || 'An error occurred while updating motion alert settings');
            console.error('Motion alert settings update error:', err);
        }
    };

    // Forskellige useEffect til at fetche user og sensor data
    useEffect(() => {
        const fetchUserAndSensorData = async () => {
            try {
                const token = localStorage.getItem('token');
                if (!token) {
                    throw new Error('No token found');
                }

                // Fetch user ID fra Auth endpoint
                const userIdRes = await fetch('http://localhost:5001/api/Auth/userid', {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    },
                    credentials: 'include'
                });

                if (userIdRes.status === 401) {
                    localStorage.removeItem('token');
                    navigate('/login');
                    return;
                }

                if (!userIdRes.ok) {
                    throw new Error(`Failed to fetch user ID: ${userIdRes.statusText}`);
                }

                const responseData = await userIdRes.json();
                const UserId = responseData.UserId || responseData.userId || responseData.Id;

                if (!UserId) {
                    throw new Error('User ID not found in response');
                }

                // Gem user ID til senere brug
                setUserId(UserId);

                // Fetch user detaljer ved hjælp af det fetchede user ID
                const userRes = await fetch(`http://localhost:5001/api/Users/${UserId}`);
                if (!userRes.ok) throw new Error('Failed to fetch user');
                const userData = await userRes.json();

                setUserEmail(userData.email);
                setUserName(userData.username || userData.name || 'User');

                // Fetch motion alert settings
                try {
                    const alertSettingsRes = await fetch(`http://localhost:5001/api/Users/${UserId}/alerts`, {
                        headers: {
                            'Authorization': `Bearer ${token}`
                        }
                    });

                    if (alertSettingsRes.ok) {
                        const alertSettings = await alertSettingsRes.json();
                        setMotionAlertEnabled(alertSettings.sendEmailAlert || false);
                    }
                } catch (alertError) {
                    console.error('Error fetching motion alert settings:', alertError);
                }

                // Fetch humidity alert settings
                try {
                    const humidityAlertSettingsRes = await fetch(`http://localhost:5001/api/Users/${UserId}/humidity-alerts`, {
                        headers: {
                            'Authorization': `Bearer ${token}`
                        }
                    });

                    if (humidityAlertSettingsRes.ok) {
                        const humidityAlertSettings = await humidityAlertSettingsRes.json();
                        setAlertsEnabled(humidityAlertSettings.sendHumidityAlert || false);
                        setHumidityThreshold(humidityAlertSettings.humidityThreshold || 80);
                    }
                } catch (humidityAlertError) {
                    console.error('Error fetching humidity alert settings:', humidityAlertError);
                }

                // Fetch temperature alert settings
                try {
                    const temperatureAlertSettingsRes = await fetch(`http://localhost:5001/api/Users/${UserId}/temperature-alerts`, {
                        headers: {
                            'Authorization': `Bearer ${token}`
                        }
                    });

                    if (temperatureAlertSettingsRes.ok) {
                        const temperatureAlertSettings = await temperatureAlertSettingsRes.json();
                        setTemperatureAlertsEnabled(temperatureAlertSettings.sendTemperatureAlert || false);
                        setTemperatureThreshold(temperatureAlertSettings.temperatureThreshold || 40);
                    }
                } catch (temperatureAlertError) {
                    console.error('Error fetching temperature alert settings:', temperatureAlertError);
                }

                const userArduinoId = userData.arduinoId;

                // Fetch sensor data ved hjælp af Arduino ID
                const sensorRes = await fetch(`http://localhost:5001/api/Sensor/${userArduinoId}`);
                if (!sensorRes.ok) throw new Error('Failed to fetch sensor data');
                const sensorArray = await sensorRes.json();

                if (Array.isArray(sensorArray) && sensorArray.length > 0) {
                    const newSensorData = sensorArray[0];
                    setSensorData(newSensorData);

                    // Just log the threshold checks without sending emails
                    const currentHumidityLevel = newSensorData ? Number(newSensorData.humidityLevel) : 0;
                    const humidityThresholdValue = Number(humidityThreshold);

                    console.log('Humidity Alert Check:', {
                        alertsEnabled: alertsEnabled,
                        currentHumidityLevel,
                        threshold: humidityThresholdValue,
                        exactMatch: currentHumidityLevel === humidityThresholdValue,
                        exceedsThreshold: currentHumidityLevel > humidityThresholdValue,
                        meetsOrExceedsThreshold: currentHumidityLevel >= humidityThresholdValue
                    });

                    // Just log the temperature checks without sending emails
                    const currentTemperature = newSensorData ? Number(newSensorData.temperature) : 0;
                    const temperatureThresholdValue = Number(temperatureThreshold);

                    console.log('Temperature Alert Check:', {
                        alertsEnabled: temperatureAlertsEnabled,
                        currentTemperature,
                        threshold: temperatureThresholdValue,
                        exactMatch: currentTemperature === temperatureThresholdValue,
                        exceedsThreshold: currentTemperature > temperatureThresholdValue,
                        meetsOrExceedsThreshold: currentTemperature >= temperatureThresholdValue
                    });
                }
            } catch (error) {
                console.error('Error in fetching process:', error);
                if (error.message.includes('401')) {
                    navigate('/login');
                }
            }
        };

        fetchUserAndSensorData();
        // Opsæt et interval til at fetche data hvert sekund
        const interval = setInterval(fetchUserAndSensorData, 1000);

        // Clear interval når alert slås fra
        return () => clearInterval(interval);
    }, [navigate, alertsEnabled, humidityThreshold, temperatureAlertsEnabled, temperatureThreshold]);

    return (
        <div className="login-container">
            <h2>Settings</h2>

            {/* Email Update Sektion */}
            <div className="settings-section">
                <h3>Update Email</h3>
                <form onSubmit={handleSubmit}>
                    <div className="form-group">
                        <label htmlFor="newEmail">New Email</label>
                        <input
                            id="newEmail"
                            type="email"
                            value={newEmail}
                            onChange={(e) => setNewEmail(e.target.value)}
                            required
                            disabled={isLoading}
                            placeholder="Enter your new email"
                        />
                    </div>
                    <div className="form-group">
                        <label htmlFor="currentPassword">Current Password</label>
                        <input
                            id="currentPassword"
                            type="password"
                            value={currentPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            required
                            disabled={isLoading}
                            placeholder="Enter your current password"
                        />
                    </div>
                    <button type="submit" disabled={isLoading}>
                        {isLoading ? 'Updating...' : 'Update Email'}
                    </button>
                </form>
                {message && <p className="success-message">{message}</p>}
                {error && <p className="error">{error}</p>}
            </div>

            {/* Sensor Data Email Sektion */}
            <div className="settings-section">
                <h3>Send Sensor Data to Email</h3>
                {sensorData ? (
                    <div className="sensor-data-preview">
                        <p><strong>Current Sensor Data:</strong></p>
                        <p>Temperature: {sensorData.temperature}°C</p>
                        <p>Humidity Level: {sensorData.humidityLevel}%</p>
                        <p>Motion Detected: {sensorData.motionDetected ? 'Yes' : 'No'}</p>
                        <p>Timestamp: {new Date(sensorData.timestamp).toLocaleString()}</p>

                        <button
                            onClick={handleSendSensorData}
                            disabled={emailSending || !userEmail}
                            className="send-email-button"
                        >
                            {emailSending ? 'Sending...' : 'Send to My Email'}
                        </button>

                        {emailMessage && <p className="success-message">{emailMessage}</p>}
                        {emailError && <p className="error">{emailError}</p>}
                        {!userEmail && <p className="error">User email not available</p>}
                    </div>
                ) : (
                    <p>Loading sensor data...</p>
                )}
            </div>

            {/* Humidity Alert Settings Sektion */}
            <div className="settings-section">
                <h3>Humidity Alert Settings</h3>
                {sensorData ? (
                    <div className="alert-settings">
                        <p>Configure alerts to be notified when humidity levels exceed a threshold.</p>

                        <div className="form-group">
                            <label htmlFor="humidityThreshold">
                                Humidity Threshold: <strong>{humidityThreshold}%</strong>
                            </label>
                            <input
                                id="humidityThreshold"
                                type="range"
                                min="0"
                                max="100"
                                value={humidityThreshold}
                                onChange={(e) => setHumidityThreshold(parseInt(e.target.value, 10))}
                                className="slider"
                            />
                            <div className="slider-labels">
                                <span>0%</span>
                                <span>50%</span>
                                <span>100%</span>
                            </div>
                        </div>

                        <div className="form-group">
                            <label className="toggle-label">
                                <input
                                    type="checkbox"
                                    checked={alertsEnabled}
                                    onChange={(e) => setAlertsEnabled(e.target.checked)}
                                />
                                <span className="toggle-text">
                                    {alertsEnabled ? 'Alerts Enabled' : 'Alerts Disabled'}
                                </span>
                            </label>
                        </div>

                        <p className="alert-info">
                            {alertsEnabled
                                ? `You will receive an email & SMS alert when humidity level exceeds ${humidityThreshold}%`
                                : 'Enable alerts to receive email & SMS notifications'}
                        </p>

                        <button
                            onClick={handleSaveAlertSettings}
                            className="save-settings-button"
                        >
                            Save Alert Settings
                        </button>

                        {alertMessage && <p className="success-message">{alertMessage}</p>}
                        {alertError && <p className="error">{alertError}</p>}
                    </div>
                ) : (
                    <p>Loading sensor data...</p>
                )}
            </div>

            {/* Temperature Alert Settings Sektion */}
            <div className="settings-section">
                <h3>Temperature Alert Settings</h3>
                {sensorData ? (
                    <div className="alert-settings">
                        <p>Configure alerts to be notified when temperature exceeds a threshold.</p>

                        <div className="form-group">
                            <label htmlFor="temperatureThreshold">
                                Temperature Threshold: <strong>{temperatureThreshold}°C</strong>
                            </label>
                            <input
                                id="temperatureThreshold"
                                type="range"
                                min="0"
                                max="100"
                                value={temperatureThreshold}
                                onChange={(e) => setTemperatureThreshold(parseInt(e.target.value, 10))}
                                className="slider"
                            />
                            <div className="slider-labels">
                                <span>0°C</span>
                                <span>50°C</span>
                                <span>100°C</span>
                            </div>
                        </div>

                        <div className="form-group">
                            <label className="toggle-label">
                                <input
                                    type="checkbox"
                                    checked={temperatureAlertsEnabled}
                                    onChange={(e) => setTemperatureAlertsEnabled(e.target.checked)}
                                />
                                <span className="toggle-text">
                                    {temperatureAlertsEnabled ? 'Alerts Enabled' : 'Alerts Disabled'}
                                </span>
                            </label>
                        </div>

                        <p className="alert-info">
                            {temperatureAlertsEnabled
                                ? `You will receive an email & SMS alert when temperature exceeds ${temperatureThreshold}°C`
                                : 'Enable alerts to receive email & SMS notifications'}
                        </p>

                        <button
                            onClick={handleSaveTemperatureAlertSettings}
                            className="save-settings-button"
                        >
                            Save Alert Settings
                        </button>

                        {temperatureAlertMessage && <p className="success-message">{temperatureAlertMessage}</p>}
                        {temperatureAlertError && <p className="error">{temperatureAlertError}</p>}
                    </div>
                ) : (
                    <p>Loading sensor data...</p>
                )}
            </div>

            {/* Motion Detection Email Alerts Sektion */}
            <div className="settings-section">
                <h3>Motion Detection Email & SMS Alerts</h3>
                <div className="alert-settings">
                    <p>Configure whether to receive email & SMS alerts when motion is detected.</p>

                    <div className="form-group">
                        <label className="toggle-label">
                            <input
                                type="checkbox"
                                checked={motionAlertEnabled}
                                onChange={(e) => handleToggleMotionAlert(e.target.checked)}
                            />
                            <span className="toggle-text">
                                {motionAlertEnabled ? 'Email & SMS Alerts Enabled' : 'Email & SMS Alerts Disabled'}
                            </span>
                        </label>
                    </div>

                    <p className="alert-info">
                        {motionAlertEnabled
                            ? 'You will receive an email & SMS alert when motion is detected'
                            : 'Enable alerts to receive email & SMS notifications when motion is detected'}
                    </p>

                    {motionAlertMessage && <p className="success-message">{motionAlertMessage}</p>}
                    {motionAlertError && <p className="error">{motionAlertError}</p>}
                </div>
            </div>

            <div className="back-link">
                <button onClick={handleBack} className="back-button">Back to Sensor Data</button>
            </div>
        </div>
    );
}

export default Settings;
