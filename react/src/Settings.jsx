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
    const [sensorData, setSensorData] = useState(null);
    const [emailSending, setEmailSending] = useState(false);
    const [emailMessage, setEmailMessage] = useState('');
    const [emailError, setEmailError] = useState('');
    const [moistureThreshold, setMoistureThreshold] = useState(() => {
        const savedThreshold = localStorage.getItem('moistureThreshold');
        return savedThreshold ? parseInt(savedThreshold, 10) : 80;
    });
    const [alertsEnabled, setAlertsEnabled] = useState(() => {
        const savedAlertsEnabled = localStorage.getItem('alertsEnabled');
        return savedAlertsEnabled === 'true';
    });
    const [alertMessage, setAlertMessage] = useState('');
    const [alertError, setAlertError] = useState('');
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
Moisture Level: ${sensorData.moistureLevel}%
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
    const handleSaveAlertSettings = () => {
        setAlertMessage('');
        setAlertError('');

        try {
            // Gem settings i localStorage
            localStorage.setItem('moistureThreshold', moistureThreshold);
            localStorage.setItem('alertsEnabled', alertsEnabled.toString());

            setAlertMessage('Alert settings saved successfully!');
        } catch (error) {
            setAlertError('Failed to save alert settings');
            console.error('Error saving alert settings:', error);
        }
    };

    // Funktion til at sende alert email
    const sendAlertEmail = async (currentMoistureLevel, email, name) => {
        try {
            if (!email) {
                throw new Error('User email not available');
            }

            const numericMoistureLevel = Number(currentMoistureLevel);
            // Læs threshold direkte fra localStorage for at sikre, det er den nyeste værdi
            const savedThreshold = localStorage.getItem('moistureThreshold');
            const numericThreshold = Number(savedThreshold ? savedThreshold : moistureThreshold);

            console.log('Sending alert with values:', {
                moistureLevel: numericMoistureLevel,
                threshold: numericThreshold
            });

            // Formater alert email body
            const emailBody = `
Alert: Moisture Level Threshold Exceeded!
------------------------------------------
Current Moisture Level: ${numericMoistureLevel}%
Your Threshold Setting: ${numericThreshold}%

This is an automated alert from your sensor monitoring system.
            `.trim();

            // Send email via API endpoint
            const response = await fetch('http://localhost:5001/Mail', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'accept': 'text/plain'
                },
                body: JSON.stringify({
                    emailToId: email,
                    emailToName: name,
                    emailSubject: 'ALERT: Moisture Level Threshold Exceeded',
                    emailBody: emailBody
                })
            });

            if (!response.ok) {
                const errorData = await response.text();
                throw new Error(errorData || 'Failed to send alert email');
            }

            console.log('Alert email sent successfully');
        } catch (err) {
            console.error('Alert email sending error:', err);
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

                // Fetch user detaljer ved hjælp af det fetchede user ID
                const userRes = await fetch(`http://localhost:5001/api/Users/${UserId}`);
                if (!userRes.ok) throw new Error('Failed to fetch user');
                const userData = await userRes.json();

                setUserEmail(userData.email);
                setUserName(userData.username || userData.name || 'User');

                const userArduinoId = userData.arduinoId;

                // Fetch sensor data ved hjælp af Arduino ID
                const sensorRes = await fetch(`http://localhost:5001/api/Sensor/${userArduinoId}`);
                if (!sensorRes.ok) throw new Error('Failed to fetch sensor data');
                const sensorArray = await sensorRes.json();

                if (Array.isArray(sensorArray) && sensorArray.length > 0) {
                    const newSensorData = sensorArray[0];
                    setSensorData(newSensorData);

                    // Tjek om alerts er slået til og moisture threshold er nået
                    // Konverter værdier til numre for at sikre korrekt sammenligning
                    const currentMoistureLevel = newSensorData ? Number(newSensorData.moistureLevel) : 0;
                    // Læs threshold direkte fra localStorage for at sikre den nyeste værdi
                    const savedThreshold = localStorage.getItem('moistureThreshold');
                    const threshold = Number(savedThreshold ? savedThreshold : moistureThreshold);

                    // Læs alertsEnabled direkte fra localStorage for at sikre den nyeste værdi
                    const savedAlertsEnabled = localStorage.getItem('alertsEnabled');
                    const currentAlertsEnabled = savedAlertsEnabled === 'true';

                    console.log('Alert Check:', {
                        alertsEnabled: currentAlertsEnabled,
                        currentMoistureLevel,
                        threshold,
                        exactMatch: currentMoistureLevel === threshold,
                        exceedsThreshold: currentMoistureLevel > threshold,
                        meetsOrExceedsThreshold: currentMoistureLevel >= threshold
                    });

                    // Tjek om alerts er slået til og moisture level når eller overstiger threshold
                    if (currentAlertsEnabled && newSensorData &&
                        (currentMoistureLevel >= threshold)) {
                        console.log('Sending alert email!');
                        // Send en alert email hver gang threshold nås eller overskrider
                        sendAlertEmail(
                            currentMoistureLevel,
                            userData.email,
                            userData.username || userData.name || 'User'
                        );
                    }
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
    }, [navigate, alertsEnabled, moistureThreshold]);

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
                        <p>Moisture Level: {sensorData.moistureLevel}%</p>
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

            {/* Moisture Alert Settings Sektion */}
            <div className="settings-section">
                <h3>Moisture Alert Settings</h3>
                {sensorData ? (
                    <div className="alert-settings">
                        <p>Configure alerts to be notified when moisture levels exceed a threshold.</p>

                        <div className="form-group">
                            <label htmlFor="moistureThreshold">
                                Moisture Threshold: <strong>{moistureThreshold}%</strong>
                            </label>
                            <input
                                id="moistureThreshold"
                                type="range"
                                min="0"
                                max="100"
                                value={moistureThreshold}
                                onChange={(e) => setMoistureThreshold(parseInt(e.target.value, 10))}
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
                                ? `You will receive an email alert when moisture level exceeds ${moistureThreshold}%`
                                : 'Enable alerts to receive email notifications'}
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

            <div className="back-link">
                <button onClick={handleBack} className="back-button">Back to Sensor Data</button>
            </div>
        </div>
    );
}

export default Settings;
