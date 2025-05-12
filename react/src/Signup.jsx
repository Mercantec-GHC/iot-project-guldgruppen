import { useState } from 'react';
import './SignUp.css';
import { Link } from 'react-router-dom';

function SignUp() {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [region, setRegion] = useState('Copenhagen');
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();

        const arduinoId = region === 'Copenhagen'
            ? '123e4567-e89b-12d3-a456-426614174000'
            : region === 'Skive1'
                ? '123e4567-e89b-12d3-a456-426614174001'
                : region === 'Skive2'
                    ? '123e4567-e89b-12d3-a456-426614174002'
                    : region === 'Aalborg'
                        ? '123e4567-e89b-12d3-a456-426614174003'
                        : '';

        const userData = {
            username,
            email,
            password,
            arduinoId,
            phoneNumber
        };

        try {
            const response = await fetch('http://localhost:5001/api/Auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(userData),
            });

            const data = await response.json();
            if (response.ok) {
                setMessage('User created successfully! Redirecting to login...');
                setTimeout(() => {
                    window.location.href = '/login';
                }, 2000);
                // Reset form fields
                setUsername('');
                setEmail('');
                setPassword('');
                setPhoneNumber('');
            } else {
                setMessage(data.message || 'Error creating user. Please try again.');
            }
        } catch (error) {
            setMessage('Network error: ' + error.message);
        }
    };

    return (
        <div className="signup-container">
            <h2>Sign Up</h2>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label htmlFor="username">Username</label>
                    <input
                        id="username"
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        placeholder="Choose a username"
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="email">Email</label>
                    <input
                        id="email"
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        placeholder="Enter your email address"
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="password">Password</label>
                    <input
                        id="password"
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        placeholder="Create a secure password"
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="phoneNumber">Phone Number (with country code)</label>
                    <input
                        id="phoneNumber"
                        type="tel"
                        value={phoneNumber}
                        onChange={(e) => setPhoneNumber(e.target.value)}
                        placeholder="+4512345678"
                        required
                    />
                </div>

                <div className="form-group">
                    <label htmlFor="region">Select Office Location for Sensor Data</label>
                    <select
                        id="region"
                        value={region}
                        onChange={(e) => setRegion(e.target.value)}
                    >
                        <option value="Copenhagen">Copenhagen</option>
                        <option value="Skive1">Skive1</option>
                        <option value="Skive2">Skive2</option>
                        <option value="Aalborg">Aalborg</option>
                    </select>
                </div>

                <button type="submit">Sign Up</button>
            </form>

            {message && <p>{message}</p>}
            <div className="login-link">
                <p>Already have an account? <Link to="/login">Log in here</Link>.</p>
            </div>
        </div>
    );
}

export default SignUp;
