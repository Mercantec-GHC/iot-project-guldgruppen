import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './Login.css'; // Using the login CSS for consistent styling

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

            // Format the sensor data for the email body
            const emailBody = `
Latest Sensor Data:
------------------
Temperature: ${sensorData.temperature}°C
Moisture Level: ${sensorData.moistureLevel}%
Motion Detected: ${sensorData.motionDetected ? 'Yes' : 'No'}
Timestamp: ${new Date(sensorData.timestamp).toLocaleString()}
            `.trim();

            // Send the email using the provided API endpoint
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

    useEffect(() => {
        const fetchUserAndSensorData = async () => {
            try {
                const token = localStorage.getItem('token');
                if (!token) {
                    throw new Error('No token found');
                }

                // Fetch the user ID from the Auth endpoint
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

                // Fetch user details using the retrieved user ID
                const userRes = await fetch(`http://localhost:5001/api/Users/${UserId}`);
                if (!userRes.ok) throw new Error('Failed to fetch user');
                const userData = await userRes.json();

                setUserEmail(userData.email);
                setUserName(userData.username || userData.name || 'User');

                const userArduinoId = userData.arduinoId;

                // Fetch sensor data using the Arduino ID
                const sensorRes = await fetch(`http://localhost:5001/api/Sensor/${userArduinoId}`);
                if (!sensorRes.ok) throw new Error('Failed to fetch sensor data');
                const sensorArray = await sensorRes.json();

                if (Array.isArray(sensorArray) && sensorArray.length > 0) {
                    setSensorData(sensorArray[0]);
                }
            } catch (error) {
                console.error('Error in fetching process:', error);
                if (error.message.includes('401')) {
                    navigate('/login');
                }
            }
        };

        fetchUserAndSensorData();
    }, [navigate]);

    return (
        <div className="login-container">
            <h2>Settings</h2>

            {/* Email Update Section */}
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

            {/* Sensor Data Email Section */}
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

            <div className="back-link">
                <button onClick={handleBack} className="back-button">Back to Sensor Data</button>
            </div>
        </div>
    );
}

export default Settings;
